using System.Globalization;

namespace KASserver;

/// <summary>
/// The usage figures of a single account resource (one entry of <c>get_accountresources</c>), e.g.
/// webspace or mailboxes. Field shape verified live against the KAS API.
/// </summary>
public sealed class AccountResource
{
    /// <summary>The complete raw field map as returned by the KAS API.</summary>
    public required IReadOnlyDictionary<string, object?> Raw { get; init; }

    /// <summary>The maximum allowed amount (KAS field <c>max</c>).</summary>
    public int? Max => ParseInt("max");

    /// <summary>The amount currently in use (KAS field <c>used</c>).</summary>
    public int? Used => ParseInt("used");

    /// <summary>The amount still free (KAS field <c>free</c>).</summary>
    public int? Free => ParseInt("free");

    /// <summary>The amount reserved (KAS field <c>reserved</c>).</summary>
    public int? Reserved => ParseInt("reserved");

    /// <summary>The amount already created (KAS field <c>created</c>).</summary>
    public int? Created => ParseInt("created");

    /// <summary>Whether the limit is exceeded (KAS field <c>exceeded</c>, <c>true</c>/<c>false</c>).</summary>
    public bool Exceeded =>
        string.Equals(Raw.GetValueOrDefault("exceeded") as string, "true", StringComparison.OrdinalIgnoreCase);

    private int? ParseInt(string key) =>
        int.TryParse(Raw.GetValueOrDefault(key) as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v
            : null;

    internal static AccountResource FromMap(IReadOnlyDictionary<string, object?> map) => new() { Raw = map };
}
