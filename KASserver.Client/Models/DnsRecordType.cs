namespace KASserver;

/// <summary>
/// A DNS resource-record type (KAS <c>record_type</c>). Covers the common types; the KAS API may
/// accept further types, which can be sent via <see cref="AddDnsRecord.RawType"/>.
/// </summary>
public enum DnsRecordType
{
    /// <summary>IPv4 address record (<c>A</c>).</summary>
    A,

    /// <summary>IPv6 address record (<c>AAAA</c>).</summary>
    Aaaa,

    /// <summary>Canonical name (alias) record (<c>CNAME</c>).</summary>
    Cname,

    /// <summary>Mail exchanger record (<c>MX</c>). Uses <c>record_aux</c> as the priority.</summary>
    Mx,

    /// <summary>Text record (<c>TXT</c>), e.g. SPF/DKIM/verification entries.</summary>
    Txt,

    /// <summary>Name server record (<c>NS</c>).</summary>
    Ns,

    /// <summary>Service locator record (<c>SRV</c>). Uses <c>record_aux</c> as the priority.</summary>
    Srv,

    /// <summary>Certification Authority Authorization record (<c>CAA</c>).</summary>
    Caa,

    /// <summary>Pointer record for reverse lookups (<c>PTR</c>).</summary>
    Ptr,

    /// <summary>Sender Policy Framework record (<c>SPF</c>).</summary>
    Spf,
}
