namespace KASserver;

/// <summary>
/// Maps <see cref="DnsRecordType"/> to and from its KAS <c>record_type</c> string, kept in one place
/// so <c>add_dns_settings</c> and the <see cref="DnsRecord"/> read model stay consistent.
/// </summary>
internal static class DnsEnumExtensions
{
    internal static string ToKasValue(this DnsRecordType type) => type switch
    {
        DnsRecordType.A => "A",
        DnsRecordType.Aaaa => "AAAA",
        DnsRecordType.Cname => "CNAME",
        DnsRecordType.Mx => "MX",
        DnsRecordType.Txt => "TXT",
        DnsRecordType.Ns => "NS",
        DnsRecordType.Srv => "SRV",
        DnsRecordType.Caa => "CAA",
        DnsRecordType.Ptr => "PTR",
        DnsRecordType.Spf => "SPF",
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    /// <summary>
    /// Parses a KAS <c>record_type</c> string back to a <see cref="DnsRecordType"/>. Returns
    /// <c>null</c> for an unknown or missing type — read models must not throw on unexpected values.
    /// </summary>
    internal static DnsRecordType? TryParseKas(string? raw) => raw?.Trim().ToUpperInvariant() switch
    {
        "A" => DnsRecordType.A,
        "AAAA" => DnsRecordType.Aaaa,
        "CNAME" => DnsRecordType.Cname,
        "MX" => DnsRecordType.Mx,
        "TXT" => DnsRecordType.Txt,
        "NS" => DnsRecordType.Ns,
        "SRV" => DnsRecordType.Srv,
        "CAA" => DnsRecordType.Caa,
        "PTR" => DnsRecordType.Ptr,
        "SPF" => DnsRecordType.Spf,
        _ => null,
    };
}
