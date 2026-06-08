namespace KASserver;

/// <summary>
/// Parameters for editing a subaccount (<c>update_account</c>). Only properties that are set
/// (non-null) are sent; everything left <c>null</c> stays unchanged on the server. The subaccount
/// is identified by its <c>account_login</c>, passed separately to
/// <see cref="IKasClient.UpdateAccountAsync"/>. At least one field must be set. Requires a superuser login.
/// </summary>
public sealed class UpdateAccount
{
    /// <summary>New KAS password (<c>account_kas_password</c>). Optional.</summary>
    public string? KasPassword { get; set; }

    /// <summary>The resource quotas to change; unset quotas stay unchanged. Optional.</summary>
    public AccountQuota? Quota { get; set; }

    /// <summary>Install <c>.htaccess</c> support (<c>inst_htaccess</c>). Optional.</summary>
    public bool? InstHtaccess { get; set; }

    /// <summary>Install FrontPage server extensions (<c>inst_fpse</c>). Optional.</summary>
    public bool? InstFpse { get; set; }

    /// <summary>Install the software set (<c>inst_software</c>). Optional.</summary>
    public bool? InstSoftware { get; set; }

    /// <summary>KAS login access state (<c>kas_access_forbidden</c>): <c>N</c>/<c>Y</c>/<c>forbidden</c>. Optional.</summary>
    public AccountAccessState? AccessForbidden { get; set; }

    /// <summary>Show the password in the KAS interface (<c>show_password</c>). Optional.</summary>
    public bool? ShowPassword { get; set; }

    /// <summary>The web server log mode (<c>logging</c>). Optional.</summary>
    public AccountLogging? Logging { get; set; }

    /// <summary>The log retention age in days (<c>logage</c>), 1–999. Optional.</summary>
    public int? LogAge { get; set; }

    /// <summary>The web statistics mode (<c>statistic</c>). Optional.</summary>
    public AccountStatistic? Statistic { get; set; }

    /// <summary>Allow DNS settings (<c>dns_settings</c>). Optional.</summary>
    public bool? DnsSettings { get; set; }

    /// <summary>The account comment (<c>account_comment</c>). Optional.</summary>
    public string? Comment { get; set; }

    /// <summary>The contact mail address (<c>account_contact_mail</c>). Optional.</summary>
    public string? ContactMail { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters(string accountLogin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountLogin);

        var parameters = new Dictionary<string, object?> { ["account_login"] = accountLogin };

        if (KasPassword is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(KasPassword);
            parameters["account_kas_password"] = KasPassword;
        }

        Quota?.ApplyTo(parameters);

        if (InstHtaccess is not null)
            parameters["inst_htaccess"] = InstHtaccess.Value ? "Y" : "N";

        if (InstFpse is not null)
            parameters["inst_fpse"] = InstFpse.Value ? "Y" : "N";

        if (InstSoftware is not null)
            parameters["inst_software"] = InstSoftware.Value ? "Y" : "N";

        if (AccessForbidden is not null)
            parameters["kas_access_forbidden"] = AccessForbidden.Value.ToKasValue();

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

        if (DnsSettings is not null)
            parameters["dns_settings"] = DnsSettings.Value ? "Y" : "N";

        if (Comment is not null)
            parameters["account_comment"] = Comment;

        if (ContactMail is not null)
            parameters["account_contact_mail"] = ContactMail;

        // account_login alone is a no-op that KAS rejects with "nothing_to_do" — fail fast on the client.
        if (parameters.Count == 1)
            throw new ArgumentException($"At least one field on {nameof(UpdateAccount)} must be set.");

        return parameters;
    }
}
