using System.Globalization;

namespace KASserver;

/// <summary>
/// A DNS resource record as returned by <c>get_dns_settings</c>. Typed convenience properties cover
/// the common fields; <see cref="Raw"/> exposes the complete map for everything else.
/// </summary>
/// <remarks>
/// Field shape verified live against the KAS API (<c>get_dns_settings</c>). Note that the response
/// field names differ from the request parameter names: the zone is returned as <c>record_zone</c>
/// (the request uses <c>zone_host</c>). Built-in records carry <c>record_id = 0</c> and are neither
/// changeable nor deleteable.
/// </remarks>
public sealed class DnsRecord
{
    /// <summary>The complete raw field map as returned by the KAS API.</summary>
    public required IReadOnlyDictionary<string, object?> Raw { get; init; }

    /// <summary>
    /// The technical record ID (KAS field <c>record_id</c>), used by update/delete actions. Built-in
    /// records report <c>0</c> and cannot be edited or deleted (see <see cref="Changeable"/>/<see cref="Deleteable"/>).
    /// </summary>
    public string? RecordId => Raw.GetValueOrDefault("record_id") as string;

    /// <summary>The raw record type string as returned by KAS (KAS field <c>record_type</c>).</summary>
    public string? RawType => Raw.GetValueOrDefault("record_type") as string;

    /// <summary>
    /// The record type as a <see cref="DnsRecordType"/>, or <c>null</c> when KAS returned a type not
    /// covered by the enum (the raw string is then available via <see cref="RawType"/>).
    /// </summary>
    public DnsRecordType? Type => DnsEnumExtensions.TryParseKas(RawType);

    /// <summary>The record name (KAS field <c>record_name</c>); empty for the zone apex.</summary>
    public string? RecordName => Raw.GetValueOrDefault("record_name") as string;

    /// <summary>The record data (KAS field <c>record_data</c>).</summary>
    public string? RecordData => Raw.GetValueOrDefault("record_data") as string;

    /// <summary>The AUX value / priority (KAS field <c>record_aux</c>), relevant for <c>MX</c>/<c>SRV</c>.</summary>
    public int? Aux => ParseInt("record_aux");

    /// <summary>The zone this record belongs to (KAS field <c>record_zone</c>).</summary>
    public string? RecordZone => Raw.GetValueOrDefault("record_zone") as string;

    /// <summary>Whether the record may be edited (KAS field <c>record_changeable</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool Changeable => IsYes("record_changeable");

    /// <summary>Whether the record may be deleted (KAS field <c>record_deleteable</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool Deleteable => IsYes("record_deleteable");

    private bool IsYes(string key) =>
        string.Equals(Raw.GetValueOrDefault(key) as string, "Y", StringComparison.OrdinalIgnoreCase);

    private int? ParseInt(string key) =>
        int.TryParse(Raw.GetValueOrDefault(key) as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v
            : null;

    internal static DnsRecord FromMap(IReadOnlyDictionary<string, object?> map) => new() { Raw = map };
}
