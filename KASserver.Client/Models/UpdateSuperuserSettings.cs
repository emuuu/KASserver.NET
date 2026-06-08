namespace KASserver;

/// <summary>
/// Parameters for editing the superuser-controlled settings of a subaccount
/// (<c>update_superusersettings</c>) — currently the SSH access and SSH keys. May only be executed
/// by the main account. The subaccount is identified by its <c>account_login</c>, passed separately
/// to <see cref="IKasClient.UpdateSuperuserSettingsAsync"/>. At least one field must be set.
/// </summary>
public sealed class UpdateSuperuserSettings
{
    /// <summary>Enable SSH access for the subaccount (<c>ssh_access</c>): <c>Y</c>/<c>N</c>. Optional.</summary>
    public bool? SshAccess { get; set; }

    /// <summary>The SSH public keys for public-key authentication (<c>ssh_keys</c>). Optional.</summary>
    public string? SshKeys { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters(string accountLogin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountLogin);

        var parameters = new Dictionary<string, object?> { ["account_login"] = accountLogin };

        if (SshAccess is not null)
            parameters["ssh_access"] = SshAccess.Value ? "Y" : "N";

        if (SshKeys is not null)
            parameters["ssh_keys"] = SshKeys;

        // account_login alone is a no-op that KAS rejects with "nothing_to_do" — fail fast on the client.
        if (parameters.Count == 1)
            throw new ArgumentException($"At least one field on {nameof(UpdateSuperuserSettings)} must be set.");

        return parameters;
    }
}
