namespace KASserver;

/// <summary>
/// The account resource quotas and usage as returned by <c>get_accountresources</c>. The KAS API
/// returns a map keyed by resource name (<c>max_webspace</c>, <c>max_domain</c>, …) where each value
/// holds the <c>max</c>/<c>used</c>/<c>free</c>/… figures. Field shape verified live against the KAS API.
/// </summary>
public sealed class AccountResources
{
    /// <summary>The complete raw field map as returned by the KAS API.</summary>
    public required IReadOnlyDictionary<string, object?> Raw { get; init; }

    /// <summary>Subaccounts (KAS field <c>max_account</c>).</summary>
    public AccountResource? Accounts => Get("max_account");

    /// <summary>Domains (KAS field <c>max_domain</c>).</summary>
    public AccountResource? Domains => Get("max_domain");

    /// <summary>Subdomains (KAS field <c>max_subdomain</c>).</summary>
    public AccountResource? Subdomains => Get("max_subdomain");

    /// <summary>Webspace in MB (KAS field <c>max_webspace</c>).</summary>
    public AccountResource? Webspace => Get("max_webspace");

    /// <summary>Mailboxes (KAS field <c>max_mail_account</c>).</summary>
    public AccountResource? MailAccounts => Get("max_mail_account");

    /// <summary>Mail forwards (KAS field <c>max_mail_forward</c>).</summary>
    public AccountResource? MailForwards => Get("max_mail_forward");

    /// <summary>Mailing lists (KAS field <c>max_mailinglist</c>).</summary>
    public AccountResource? Mailinglists => Get("max_mailinglist");

    /// <summary>Databases (KAS field <c>max_database</c>).</summary>
    public AccountResource? Databases => Get("max_database");

    /// <summary>FTP users (KAS field <c>max_ftpuser</c>).</summary>
    public AccountResource? FtpUsers => Get("max_ftpuser");

    /// <summary>Samba users (KAS field <c>max_sambauser</c>).</summary>
    public AccountResource? SambaUsers => Get("max_sambauser");

    /// <summary>Cron jobs (KAS field <c>max_cronjobs</c>).</summary>
    public AccountResource? Cronjobs => Get("max_cronjobs");

    /// <summary>WebFTP accounts (KAS field <c>max_wbk</c>).</summary>
    public AccountResource? Wbk => Get("max_wbk");

    /// <summary>Returns the resource stored under <paramref name="key"/>, or <c>null</c> when absent.</summary>
    /// <param name="key">The KAS resource key, e.g. <c>max_webspace</c>.</param>
    public AccountResource? Get(string key) =>
        Raw.GetValueOrDefault(key) is IReadOnlyDictionary<string, object?> map
            ? AccountResource.FromMap(map)
            : null;

    internal static AccountResources FromMap(IReadOnlyDictionary<string, object?> map) => new() { Raw = map };
}
