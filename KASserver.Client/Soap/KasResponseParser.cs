using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using KASserver;

namespace KASserver.Soap;

/// <summary>
/// Parses raw KAS SOAP responses. This is required because the official KAS WSDL declares the
/// response element as <c>Result</c> while the server actually returns <c>return</c>, which makes
/// strict SOAP/WCF clients fail. The response payload is an Apache <c>xml-soap</c> <c>Map</c>
/// structure of nested <c>&lt;item&gt;&lt;key/&gt;&lt;value/&gt;&lt;/item&gt;</c> entries.
/// </summary>
internal static class KasResponseParser
{
    private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";
    private static readonly XNamespace SoapEnc = "http://schemas.xmlsoap.org/soap/encoding/";

    // Defensive upper bound for the flood delay; a malformed/hostile value must never park a request indefinitely.
    private const double MaxFloodDelaySeconds = 60d;

    /// <summary>
    /// Extracts the bare text content of the <c>&lt;return&gt;</c> element (used by <c>KasAuth</c>,
    /// which returns the session token directly).
    /// </summary>
    public static string? ExtractReturnText(string xml)
    {
        var doc = ParseXml(xml, "KasAuth");
        ThrowIfFault(doc, action: "KasAuth");

        var ret = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "return");
        if (ret is null)
            return null;

        // The auth token is a plain scalar; a structured (Map/Array) return here is a protocol surprise.
        if (ret.Elements().Any())
            throw new KasApiException("KAS authentication returned a structured value where a token was expected.", action: "KasAuth");

        return ret.Value;
    }

    /// <summary>
    /// Parses a full <c>KasApi</c> response envelope into a <see cref="KasResponse"/>.
    /// </summary>
    public static KasResponse Parse(string xml, string? action = null)
    {
        var doc = ParseXml(xml, action);
        ThrowIfFault(doc, action);

        var ret = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "return")
            ?? throw new KasApiException("KAS response did not contain a 'return' element.", action: action);

        if (ParseValue(ret) is not IReadOnlyDictionary<string, object?> root)
            throw new KasApiException("KAS response 'return' was not a map (unexpected response shape).", action: action);

        if (!root.TryGetValue("Response", out var r) || r is not IReadOnlyDictionary<string, object?> response)
            throw new KasApiException("KAS response did not contain a 'Response' map.", action: action);

        return new KasResponse
        {
            ReturnString = response.TryGetValue("ReturnString", out var rs) ? rs as string : null,
            ReturnInfo = response.TryGetValue("ReturnInfo", out var ri) ? ri : null,
            FloodDelay = ParseFloodDelay(response.TryGetValue("KasFloodDelay", out var fd) ? fd : null),
        };
    }

    private static XDocument ParseXml(string xml, string? action)
    {
        try
        {
            return XDocument.Parse(xml);
        }
        catch (XmlException ex)
        {
            var snippet = string.IsNullOrEmpty(xml) ? "(empty)" : xml[..Math.Min(xml.Length, 200)];
            throw new KasApiException($"KAS response was not valid XML: {snippet}", ex, action: action);
        }
    }

    /// <summary>
    /// Returns <c>true</c> if the body is well-formed XML containing a SOAP <c>Fault</c> element.
    /// Used to decide, on a non-2xx HTTP response, whether to let the parser surface a KAS fault or
    /// to throw a transport-level error instead.
    /// </summary>
    public static bool LooksLikeSoapFault(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return false;

        try
        {
            return XDocument.Parse(xml).Descendants().Any(e => e.Name.LocalName == "Fault");
        }
        catch (XmlException)
        {
            return false;
        }
    }

    private static void ThrowIfFault(XDocument doc, string? action)
    {
        var fault = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fault");
        if (fault is null)
            return;

        var faultString = fault.Elements().FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value
            ?? "Unknown KAS fault.";

        throw new KasApiException(
            $"KAS API fault: {faultString}",
            faultCode: faultString,
            action: action);
    }

    /// <summary>
    /// Recursively converts a SOAP value element into a plain .NET object:
    /// a <c>Map</c> becomes a <see cref="Dictionary{TKey,TValue}"/>, an <c>Array</c> becomes a
    /// <see cref="List{T}"/>, and a scalar becomes a <see cref="string"/> (or <c>null</c> when
    /// explicitly <c>xsi:nil="true"</c>). Detection relies on <c>xsi:type</c> and the
    /// <c>SOAP-ENC:arrayType</c> attribute, falling back to the keyed/unkeyed shape of the items.
    /// </summary>
    private static object? ParseValue(XElement element)
    {
        // An explicit xsi:nil collapses to null regardless of declared type (Map/Array/scalar).
        if ((string?)element.Attribute(Xsi + "nil") is "true" or "1")
            return null;

        var localType = ((string?)element.Attribute(Xsi + "type"))?.Split(':').Last() ?? string.Empty;
        var hasArrayType = element.Attribute(SoapEnc + "arrayType") is not null;

        var items = element.Elements().Where(e => e.Name.LocalName == "item").ToList();
        var allItemsKeyed = items.Count > 0 && items.All(i => i.Elements().Any(e => e.Name.LocalName == "key"));
        var noItemsKeyed = items.Count > 0 && items.All(i => !i.Elements().Any(e => e.Name.LocalName == "key"));

        // Mixed keyed/unkeyed items are neither a clean Map nor a clean Array — surface, don't mask.
        if (items.Count > 0 && !allItemsKeyed && !noItemsKeyed)
            throw new KasApiException("KAS response contained mixed keyed/unkeyed items (unexpected shape).");

        // Explicit type signals take precedence over the keyed/unkeyed heuristic.
        if (localType == "Map")
            return ParseMap(items);

        if (localType == "Array" || hasArrayType)
            return items.Select(ParseValue).ToList();

        // Untyped: fall back to the item shape.
        if (allItemsKeyed)
            return ParseMap(items);

        if (noItemsKeyed)
            return items.Select(ParseValue).ToList();

        // Scalar leaf. An empty string stays "".
        return element.Value;
    }

    private static Dictionary<string, object?> ParseMap(List<XElement> items)
    {
        var map = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var item in items)
        {
            var key = item.Elements().FirstOrDefault(e => e.Name.LocalName == "key")
                ?? throw new KasApiException("KAS map item was missing a <key> (unexpected shape).");

            var value = item.Elements().FirstOrDefault(e => e.Name.LocalName == "value");
            map[key.Value] = value is null ? null : ParseValue(value);
        }

        return map;
    }

    private static double ParseFloodDelay(object? value)
    {
        if (value is string s
            && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
            && double.IsFinite(d)
            && d > 0)
        {
            return Math.Min(d, MaxFloodDelaySeconds);
        }

        return 0d;
    }
}
