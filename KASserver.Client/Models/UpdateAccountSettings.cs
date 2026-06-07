namespace KASserver;

/// <summary>
/// Parameters for editing your own account settings (<c>update_accountsettings</c>). Only properties
/// that are set (non-null) are sent; everything left <c>null</c> stays unchanged on the server.
/// At least one field must be set.
/// </summary>
public sealed class UpdateAccountSettings
{
    /// <summary>New account password (<c>account_password</c>). Optional.</summary>
    public string? Password { get; set; }

    /// <summary>Show the password in the KAS interface (<c>show_password</c>). Optional.</summary>
    public bool? ShowPassword { get; set; }

    /// <summary>The web server log mode (<c>logging</c>). Optional.</summary>
    public AccountLogging? Logging { get; set; }

    /// <summary>The log retention age in days (<c>logage</c>), 1–999. Optional.</summary>
    public int? LogAge { get; set; }

    /// <summary>The web statistics mode (<c>statistic</c>). Optional.</summary>
    public AccountStatistic? Statistic { get; set; }

    /// <summary>The account comment (<c>account_comment</c>). Optional.</summary>
    public string? Comment { get; set; }

    /// <summary>The contact mail address (<c>account_contact_mail</c>). Optional.</summary>
    public string? ContactMail { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters()
    {
        var parameters = new Dictionary<string, object?>();

        if (Password is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(Password);
            parameters["account_password"] = Password;
        }

        if (ShowPassword is not null)
            parameters["show_password"] = ShowPassword.Value ? "Y" : "N";

        if (Logging is not null)
            parameters["logging"] = Logging.Value.ToKasValue();

        if (LogAge is not null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(LogAge.Value, 1, nameof(LogAge));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(LogAge.Value, 999, nameof(LogAge));
            parameters["logage"] = LogAge.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        if (Statistic is not null)
            parameters["statistic"] = Statistic.Value.ToKasValue();

        if (Comment is not null)
            parameters["account_comment"] = Comment;

        if (ContactMail is not null)
            parameters["account_contact_mail"] = ContactMail;

        // An empty change set is a no-op that KAS rejects with "nothing_to_do" — fail fast on the client.
        if (parameters.Count == 0)
            throw new ArgumentException($"At least one field on {nameof(UpdateAccountSettings)} must be set.");

        return parameters;
    }
}
