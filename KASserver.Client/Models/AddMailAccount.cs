namespace KASserver;

/// <summary>
/// Parameters for creating a mailbox (<c>add_mailaccount</c>).
/// </summary>
public sealed class AddMailAccount
{
    /// <summary>The local part of the address (before the <c>@</c>). Required.</summary>
    public required string LocalPart { get; set; }

    /// <summary>The domain part of the address (an existing domain on the account). Required.</summary>
    public required string DomainPart { get; set; }

    /// <summary>The mailbox password. Required.</summary>
    public required string Password { get; set; }

    /// <summary>Allow automatic login from KAS into webmail (<c>webmail_autologin</c>). Default: <c>true</c>.</summary>
    public bool WebmailAutologin { get; set; } = true;

    /// <summary>Comma-separated copy recipients (<c>copy_adress</c>). Optional.</summary>
    public string? CopyAddress { get; set; }

    /// <summary>Allowed sender alias addresses (<c>mail_sender_alias</c>). Optional.</summary>
    public string? SenderAlias { get; set; }

    /// <summary>Restrict access to specific clients/nets (<c>mail_allow_nets</c>), e.g. <c>webmail</c> or <c>1.2.3.4</c>. Optional.</summary>
    public string? AllowNets { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(LocalPart);
        ArgumentException.ThrowIfNullOrWhiteSpace(DomainPart);
        ArgumentException.ThrowIfNullOrWhiteSpace(Password);

        var parameters = new Dictionary<string, object?>
        {
            ["mail_password"] = Password,
            ["local_part"] = LocalPart,
            ["domain_part"] = DomainPart,
            ["webmail_autologin"] = WebmailAutologin ? "Y" : "N",
        };

        if (!string.IsNullOrWhiteSpace(CopyAddress))
            parameters["copy_adress"] = CopyAddress;

        if (!string.IsNullOrWhiteSpace(SenderAlias))
            parameters["mail_sender_alias"] = SenderAlias;

        if (!string.IsNullOrWhiteSpace(AllowNets))
            parameters["mail_allow_nets"] = AllowNets;

        return parameters;
    }
}
