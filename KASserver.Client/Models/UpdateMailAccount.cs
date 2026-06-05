namespace KASserver;

/// <summary>
/// Parameters for editing a mailbox (<c>update_mailaccount</c>). Only properties that are set
/// (non-null) are sent; everything left <c>null</c> stays unchanged on the server.
/// The mailbox is identified by its technical <c>mail_login</c>, passed separately to
/// <see cref="IKasClient.UpdateMailAccountAsync"/>. At least one field must be set.
/// </summary>
public sealed class UpdateMailAccount
{
    /// <summary>New mailbox password (<c>mail_new_password</c>). Optional.</summary>
    public string? NewPassword { get; set; }

    /// <summary>
    /// Autoresponder state (<c>responder</c>): <c>N</c>/<c>Y</c>, or a <c>start|end</c> timestamp range.
    /// Optional.
    /// </summary>
    public string? Responder { get; set; }

    /// <summary>Autoresponder text (<c>responder_text</c>). Optional.</summary>
    public string? ResponderText { get; set; }

    /// <summary>Comma-separated copy recipients (<c>copy_adress</c>). Optional.</summary>
    public string? CopyAddress { get; set; }

    /// <summary>Allowed sender alias addresses (<c>mail_sender_alias</c>). Optional.</summary>
    public string? SenderAlias { get; set; }

    /// <summary>Mailbox activation state (<c>is_active</c>): <c>Y</c>/<c>N</c>/<c>forbidden</c>. Optional.</summary>
    public MailboxActiveState? IsActive { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters(string mailLogin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailLogin);

        var parameters = new Dictionary<string, object?> { ["mail_login"] = mailLogin };

        if (NewPassword is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(NewPassword);
            parameters["mail_new_password"] = NewPassword;
        }

        if (Responder is not null)
            parameters["responder"] = Responder;

        if (ResponderText is not null)
            parameters["responder_text"] = ResponderText;

        if (CopyAddress is not null)
            parameters["copy_adress"] = CopyAddress;

        if (SenderAlias is not null)
            parameters["mail_sender_alias"] = SenderAlias;

        if (IsActive is not null)
            parameters["is_active"] = IsActive.Value switch
            {
                MailboxActiveState.Active => "Y",
                MailboxActiveState.ReceiveDisabled => "N",
                MailboxActiveState.Forbidden => "forbidden",
                _ => throw new ArgumentOutOfRangeException(nameof(IsActive)),
            };

        // mail_login alone is a no-op that KAS rejects with "nothing_to_do" — fail fast on the client.
        if (parameters.Count == 1)
            throw new ArgumentException($"At least one field on {nameof(UpdateMailAccount)} must be set.");

        return parameters;
    }
}
