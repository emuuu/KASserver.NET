namespace KASserver;

/// <summary>
/// The account settings as returned by <c>get_accountsettings</c> (the inner <c>settings</c> map).
/// Typed convenience properties cover the most common fields; <see cref="Raw"/> exposes the complete
/// map for everything else. Field shape verified live against the KAS API.
/// </summary>
public sealed class AccountSettings
{
    /// <summary>The complete raw field map as returned by the KAS API.</summary>
    public required IReadOnlyDictionary<string, object?> Raw { get; init; }

    /// <summary>The account login (KAS field <c>account_login</c>).</summary>
    public string? AccountLogin => Raw.GetValueOrDefault("account_login") as string;

    /// <summary>The account comment (KAS field <c>account_comment</c>).</summary>
    public string? Comment => Raw.GetValueOrDefault("account_comment") as string;

    /// <summary>The contact mail address (KAS field <c>account_contact_mail</c>).</summary>
    public string? ContactMail => Raw.GetValueOrDefault("account_contact_mail") as string;

    /// <summary>Whether this is the superuser/main account (KAS field <c>is_superuser</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool IsSuperuser => IsYes("is_superuser");

    /// <summary>The web server log mode (KAS field <c>logging</c>): <c>voll</c>/<c>kurz</c>/<c>ohneip</c>/<c>keine</c>.</summary>
    public string? Logging => Raw.GetValueOrDefault("logging") as string;

    /// <summary>The log retention age in days (KAS field <c>logage</c>).</summary>
    public string? LogAge => Raw.GetValueOrDefault("logage") as string;

    /// <summary>The web statistics mode (KAS field <c>statistic</c>): <c>0</c>/<c>de</c>/<c>ne</c>.</summary>
    public string? Statistic => Raw.GetValueOrDefault("statistic") as string;

    /// <summary>Whether DNS settings are allowed (KAS field <c>dns_settings</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool DnsSettings => IsYes("dns_settings");

    /// <summary>Whether <c>.htaccess</c> support is enabled (KAS field <c>inst_htaccess</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool InstHtaccess => IsYes("inst_htaccess");

    /// <summary>Whether software installation is enabled (KAS field <c>inst_software</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool InstSoftware => IsYes("inst_software");

    /// <summary>Whether SSH access is enabled (KAS field <c>ssh_access</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool SshAccess => IsYes("ssh_access");

    /// <summary>The server the account lives on (KAS field <c>server</c>).</summary>
    public string? Server => Raw.GetValueOrDefault("server") as string;

    private bool IsYes(string key) =>
        string.Equals(Raw.GetValueOrDefault(key) as string, "Y", StringComparison.OrdinalIgnoreCase);

    internal static AccountSettings FromMap(IReadOnlyDictionary<string, object?> map) => new() { Raw = map };
}
