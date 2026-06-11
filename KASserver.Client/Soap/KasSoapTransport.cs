using System.Text;
using System.Text.Json;
using KASserver;
using Microsoft.Extensions.Options;

namespace KASserver.Soap;

/// <summary>
/// Low-level transport for the KAS API. Handles session authentication, automatic token refresh,
/// flood throttling (<c>KasFloodDelay</c>) and the raw-SOAP workaround for the faulty KAS WSDL.
/// Thread-safe; calls are serialized so the shared session token and flood window stay consistent.
/// Register as a singleton (see <see cref="ServiceCollectionExtensions.AddKasServer"/>) so the
/// flood window and session token are shared across the process for a given account.
/// </summary>
internal sealed class KasSoapTransport : IKasTransport
{
    private const string AuthUrl = "https://kasapi.kasserver.com/soap/KasAuth.php";
    private const string ApiUrl = "https://kasapi.kasserver.com/soap/KasApi.php";

    // The auth and API endpoints use distinct SOAP namespaces (confirmed against the official WSDLs).
    private const string AuthNamespace = "urn:xmethodsKasApiAuthentication";
    private const string ApiNamespace = "urn:xmethodsKasApi";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly KasServerOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private string? _token;

    // Monotonic deadline (Environment.TickCount64 milliseconds) before which the next request must wait.
    private long _floodReadyTick;

    public KasSoapTransport(IHttpClientFactory httpClientFactory, IOptions<KasServerOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _options.Validate();
    }

    /// <summary>
    /// Invokes a KAS action and returns the parsed response. Authentication, token refresh and
    /// flood throttling are handled transparently.
    /// </summary>
    public async Task<KasResponse> CallAsync(
        string action,
        IReadOnlyDictionary<string, object?>? parameters,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            try
            {
                return await CallCoreAsync(action, parameters, cancellationToken).ConfigureAwait(false);
            }
            catch (KasApiException ex) when (ex.Action != "KasAuth" && IsSessionFault(ex.FaultCode))
            {
                // Session expired or was invalidated on an API call — re-authenticate once and retry.
                // Auth faults (Action == "KasAuth") are excluded so a bad login does not loop.
                _token = null;
                return await CallCoreAsync(action, parameters, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<KasResponse> CallCoreAsync(
        string action,
        IReadOnlyDictionary<string, object?>? parameters,
        CancellationToken cancellationToken)
    {
        await EnsureTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["kas_login"] = _options.Login,
            ["kas_auth_type"] = "session",
            ["kas_auth_data"] = _token,
            ["kas_action"] = action,
            ["KasRequestParams"] = parameters ?? new Dictionary<string, object?>(),
        }, JsonOptions);

        var xml = await PostAsync(ApiUrl, "KasApi", ApiNamespace, payload, cancellationToken).ConfigureAwait(false);
        var response = KasResponseParser.Parse(xml, action);

        ScheduleFlood(response.FloodDelay);
        return response;
    }

    private async Task EnsureTokenAsync(CancellationToken cancellationToken)
    {
        if (_token is not null)
            return;

        var authParameters = new Dictionary<string, object?>
        {
            ["kas_login"] = _options.Login,
            ["kas_auth_type"] = "plain",
            ["kas_auth_data"] = _options.Password,
            ["session_lifetime"] = _options.SessionLifetime,
            ["session_update_lifetime"] = _options.UpdateSessionLifetime ? "Y" : "N",
        };

        if (_options.TwoFactorPinProvider is not null)
        {
            var pin = await _options.TwoFactorPinProvider(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(pin))
                throw new KasApiException("TwoFactorPinProvider returned an empty 2FA PIN.", action: "KasAuth");

            authParameters["session_2fa"] = pin;
        }

        var payload = JsonSerializer.Serialize(authParameters, JsonOptions);
        var xml = await PostAsync(AuthUrl, "KasAuth", AuthNamespace, payload, cancellationToken).ConfigureAwait(false);

        _token = KasResponseParser.ExtractReturnText(xml);
        if (string.IsNullOrEmpty(_token))
            throw new KasApiException("KAS authentication did not return a session token.", action: "KasAuth");
    }

    private async Task<string> PostAsync(
        string url,
        string operation,
        string soapNamespace,
        string jsonPayload,
        CancellationToken cancellationToken)
    {
        // Honor the flood window before EVERY outbound request, including auth and the retry path.
        await ThrottleAsync(cancellationToken).ConfigureAwait(false);

        var envelope = BuildEnvelope(operation, soapNamespace, jsonPayload);

        using var content = new StringContent(envelope, Encoding.UTF8, "text/xml");
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        request.Headers.TryAddWithoutValidation("SOAPAction", $"{soapNamespace}#{operation}");

        using var client = _httpClientFactory.CreateClient(KasServerDefaults.HttpClientName);
        client.Timeout = _options.Timeout;

        using var httpResponse = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!httpResponse.IsSuccessStatusCode && !KasResponseParser.LooksLikeSoapFault(body))
        {
            // KAS faults arrive as HTTP 500 with a SOAP fault body (let the parser surface those with
            // their fault code). Any other non-2xx (proxy HTML error pages, empty/non-fault bodies, or a
            // success-shaped body on an error status) is surfaced here with the HTTP status for context.
            var snippet = string.IsNullOrEmpty(body) ? "(empty body)" : body![..Math.Min(body.Length, 200)];
            throw new KasApiException(
                $"KAS request failed with HTTP {(int)httpResponse.StatusCode}: {snippet}",
                action: operation);
        }

        return body ?? string.Empty;
    }

    private static string BuildEnvelope(string operation, string soapNamespace, string jsonPayload)
    {
        var escaped = System.Security.SecurityElement.Escape(jsonPayload);
        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
            $"xmlns:ns1=\"{soapNamespace}\" " +
            "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<SOAP-ENV:Body>" +
            $"<ns1:{operation}><Params xsi:type=\"xsd:string\">{escaped}</Params></ns1:{operation}>" +
            "</SOAP-ENV:Body></SOAP-ENV:Envelope>";
    }

    private void ScheduleFlood(double floodDelaySeconds)
    {
        if (floodDelaySeconds > 0)
            _floodReadyTick = Environment.TickCount64 + (long)(floodDelaySeconds * 1000);
    }

    private async Task ThrottleAsync(CancellationToken cancellationToken)
    {
        var waitMs = _floodReadyTick - Environment.TickCount64;
        if (waitMs > 0)
            await Task.Delay((int)waitMs, cancellationToken).ConfigureAwait(false);
    }

    // Exact allowlist of transient session faults that warrant a single re-authentication.
    // KAS conveys the error code verbatim in the SOAP faultstring.
    private static bool IsSessionFault(string? faultCode)
        => faultCode is "kas_session_invalid" or "kas_session_expired" or "session_invalid" or "session_expired";
}
