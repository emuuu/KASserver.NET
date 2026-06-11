namespace KASserver;

/// <summary>
/// Parameters for creating a DynDNS user (<c>add_ddnsuser</c>). The user updates the A/AAAA record
/// of <c>{Label}.{Zone}</c> via DynDNS; KAS generates the technical DynDNS login.
/// </summary>
public sealed class AddDynDnsUser
{
    /// <summary>A comment for the DynDNS user (<c>dyndns_comment</c>). Required by the KAS API.</summary>
    public required string Comment { get; set; }

    /// <summary>The DynDNS update password (<c>dyndns_password</c>). Required.</summary>
    public required string Password { get; set; }

    /// <summary>The zone the host belongs to (<c>dyndns_zone</c>), e.g. <c>example.com</c>. Required.</summary>
    public required string Zone { get; set; }

    /// <summary>The host label within the zone (<c>dyndns_label</c>), e.g. <c>home</c>. Required.</summary>
    public required string Label { get; set; }

    /// <summary>The initial target IP address (<c>dyndns_target_ip</c>). Required.</summary>
    public required string TargetIp { get; set; }

    /// <summary>
    /// Enable dual-stack mode (<c>dyndns_dual_stack</c>), i.e. updating both IPv4 and IPv6. Optional;
    /// only sent when set.
    /// </summary>
    public bool? DualStack { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Comment);
        ArgumentException.ThrowIfNullOrWhiteSpace(Password);
        ArgumentException.ThrowIfNullOrWhiteSpace(Zone);
        ArgumentException.ThrowIfNullOrWhiteSpace(Label);
        ArgumentException.ThrowIfNullOrWhiteSpace(TargetIp);

        var parameters = new Dictionary<string, object?>
        {
            ["dyndns_comment"] = Comment,
            ["dyndns_password"] = Password,
            ["dyndns_zone"] = Zone,
            ["dyndns_label"] = Label,
            ["dyndns_target_ip"] = TargetIp,
        };

        if (DualStack is not null)
            parameters["dyndns_dual_stack"] = DualStack.Value ? "Y" : "N";

        return parameters;
    }
}
