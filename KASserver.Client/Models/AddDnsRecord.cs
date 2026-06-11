using System.Globalization;

namespace KASserver;

/// <summary>
/// Parameters for creating a DNS resource record (<c>add_dns_settings</c>). The record type can be
/// given either as a <see cref="DnsRecordType"/> via <see cref="Type"/> or as a raw KAS string via
/// <see cref="RawType"/> — exactly one of the two must be set.
/// </summary>
public sealed class AddDnsRecord
{
    /// <summary>
    /// The DNS zone the record belongs to (<c>zone_host</c>), e.g. <c>example.com</c>. A missing
    /// trailing dot is added automatically. Required.
    /// </summary>
    public required string ZoneHost { get; set; }

    /// <summary>
    /// The record type (<c>record_type</c>) as a known enum value. Set either this or
    /// <see cref="RawType"/>, not both.
    /// </summary>
    public DnsRecordType? Type { get; set; }

    /// <summary>
    /// The record type (<c>record_type</c>) as a raw KAS string, for types not covered by
    /// <see cref="DnsRecordType"/>. Set either this or <see cref="Type"/>, not both.
    /// </summary>
    public string? RawType { get; set; }

    /// <summary>
    /// The record name (<c>record_name</c>), e.g. <c>www</c>. Use an empty string for the zone apex —
    /// that is how <c>get_dns_settings</c> returns it. Required (may be empty, but not whitespace-only).
    /// </summary>
    public required string RecordName { get; set; }

    /// <summary>The record data (<c>record_data</c>), e.g. an IP, a target host or a TXT value. Required.</summary>
    public required string RecordData { get; set; }

    /// <summary>
    /// The AUX value (<c>record_aux</c>), used as the priority for <c>MX</c>/<c>SRV</c> records and
    /// <c>0</c> otherwise. Always sent because the KAS API requires it. Default: <c>0</c>.
    /// </summary>
    public int Aux { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters()
    {
        // An empty record_name is the zone apex (as get_dns_settings returns it), so allow "" — but
        // reject null and whitespace-only values.
        ArgumentNullException.ThrowIfNull(RecordName);
        if (RecordName.Length > 0 && string.IsNullOrWhiteSpace(RecordName))
            throw new ArgumentException("RecordName must be empty (zone apex) or a non-whitespace label.", nameof(RecordName));
        ArgumentException.ThrowIfNullOrWhiteSpace(RecordData);
        ArgumentOutOfRangeException.ThrowIfNegative(Aux, nameof(Aux));

        return new Dictionary<string, object?>
        {
            ["zone_host"] = DnsZoneHost.Normalize(ZoneHost),
            ["record_type"] = ResolveRecordType(),
            ["record_name"] = RecordName,
            ["record_data"] = RecordData,
            ["record_aux"] = Aux.ToString(CultureInfo.InvariantCulture),
        };
    }

    private string ResolveRecordType()
    {
        var hasRawType = !string.IsNullOrWhiteSpace(RawType);

        if (Type is not null && hasRawType)
            throw new ArgumentException($"Set either {nameof(Type)} or {nameof(RawType)} on {nameof(AddDnsRecord)}, not both.");

        if (Type is not null)
            return Type.Value.ToKasValue();

        if (hasRawType)
            return RawType!.Trim();

        throw new ArgumentException($"Either {nameof(Type)} or {nameof(RawType)} must be set on {nameof(AddDnsRecord)}.");
    }
}
