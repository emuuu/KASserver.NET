namespace KASserver;

/// <summary>
/// A DynDNS user as returned by <c>get_ddnsusers</c>. Typed convenience properties cover the common
/// fields; <see cref="Raw"/> exposes the complete map for everything else.
/// </summary>
/// <remarks>
/// Field shape verified live against the KAS API (<c>get_ddnsusers</c>).
/// </remarks>
public sealed class DynDnsUser
{
    /// <summary>The complete raw field map as returned by the KAS API.</summary>
    public required IReadOnlyDictionary<string, object?> Raw { get; init; }

    /// <summary>The technical DynDNS login (KAS field <c>dyndns_login</c>), used by update/delete actions.</summary>
    public string? Login => Raw.GetValueOrDefault("dyndns_login") as string;

    /// <summary>The zone the host belongs to (KAS field <c>dyndns_zone</c>).</summary>
    public string? Zone => Raw.GetValueOrDefault("dyndns_zone") as string;

    /// <summary>The host label within the zone (KAS field <c>dyndns_label</c>).</summary>
    public string? Label => Raw.GetValueOrDefault("dyndns_label") as string;

    /// <summary>The comment (KAS field <c>dyndns_comment</c>).</summary>
    public string? Comment => Raw.GetValueOrDefault("dyndns_comment") as string;

    /// <summary>The current target IP (KAS field <c>dyndns_target_ip</c>).</summary>
    public string? TargetIp => Raw.GetValueOrDefault("dyndns_target_ip") as string;

    /// <summary>The current IPv4 target (KAS field <c>dyndns_target_ipv4</c>); empty when unset.</summary>
    public string? TargetIpv4 => Raw.GetValueOrDefault("dyndns_target_ipv4") as string;

    /// <summary>The current IPv6 target (KAS field <c>dyndns_target_ipv6</c>); empty when unset.</summary>
    public string? TargetIpv6 => Raw.GetValueOrDefault("dyndns_target_ipv6") as string;

    /// <summary>Whether dual-stack mode is enabled (KAS field <c>dyndns_dual_stack</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool DualStack => IsYes("dyndns_dual_stack");

    private bool IsYes(string key) =>
        string.Equals(Raw.GetValueOrDefault(key) as string, "Y", StringComparison.OrdinalIgnoreCase);

    internal static DynDnsUser FromMap(IReadOnlyDictionary<string, object?> map) => new() { Raw = map };
}
