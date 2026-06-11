namespace KASserver;

/// <summary>
/// Parameters for editing a DynDNS user (<c>update_ddnsuser</c>). Only properties that are set
/// (non-null) are sent; everything left <c>null</c> stays unchanged on the server. The user is
/// identified by its <c>dyndns_login</c>, passed separately to
/// <see cref="IKasClient.UpdateDynDnsUserAsync"/>. At least one field must be set.
/// </summary>
public sealed class UpdateDynDnsUser
{
    /// <summary>New DynDNS update password (<c>dyndns_password</c>). Optional.</summary>
    public string? Password { get; set; }

    /// <summary>New comment (<c>dyndns_comment</c>). Optional.</summary>
    public string? Comment { get; set; }

    /// <summary>Dual-stack mode (<c>dyndns_dual_stack</c>), i.e. updating both IPv4 and IPv6. Optional.</summary>
    public bool? DualStack { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters(string dyndnsLogin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dyndnsLogin);

        var parameters = new Dictionary<string, object?> { ["dyndns_login"] = dyndnsLogin };

        if (Password is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(Password);
            parameters["dyndns_password"] = Password;
        }

        if (Comment is not null)
            parameters["dyndns_comment"] = Comment;

        if (DualStack is not null)
            parameters["dyndns_dual_stack"] = DualStack.Value ? "Y" : "N";

        // dyndns_login alone is a no-op that KAS rejects with "nothing_to_do" — fail fast on the client.
        if (parameters.Count == 1)
            throw new ArgumentException($"At least one field on {nameof(UpdateDynDnsUser)} must be set.");

        return parameters;
    }
}
