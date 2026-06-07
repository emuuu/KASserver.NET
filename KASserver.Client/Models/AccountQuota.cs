namespace KASserver;

/// <summary>
/// The resource quotas of a subaccount, shared by <c>add_account</c> and <c>update_account</c>.
/// Only properties that are set (non-null) are sent. On <c>add_account</c> an unset quota means the
/// KAS default (<c>0</c> = unlimited/none depending on the resource); on <c>update_account</c> it
/// means the value stays unchanged. All values must be non-negative.
/// </summary>
public sealed class AccountQuota
{
    /// <summary>Maximum number of subaccounts (<c>max_account</c>).</summary>
    public int? MaxAccount { get; set; }

    /// <summary>Maximum number of domains (<c>max_domain</c>).</summary>
    public int? MaxDomain { get; set; }

    /// <summary>Maximum number of subdomains (<c>max_subdomain</c>).</summary>
    public int? MaxSubdomain { get; set; }

    /// <summary>Maximum webspace in MB (<c>max_webspace</c>).</summary>
    public int? MaxWebspace { get; set; }

    /// <summary>Maximum number of mailboxes (<c>max_mail_account</c>).</summary>
    public int? MaxMailAccount { get; set; }

    /// <summary>Maximum number of mail forwards (<c>max_mail_forward</c>).</summary>
    public int? MaxMailForward { get; set; }

    /// <summary>Maximum number of mailing lists (<c>max_mailinglist</c>).</summary>
    public int? MaxMailinglist { get; set; }

    /// <summary>Maximum number of databases (<c>max_database</c>).</summary>
    public int? MaxDatabase { get; set; }

    /// <summary>Maximum number of FTP users (<c>max_ftpuser</c>).</summary>
    public int? MaxFtpUser { get; set; }

    /// <summary>Maximum number of Samba users (<c>max_sambauser</c>).</summary>
    public int? MaxSambaUser { get; set; }

    /// <summary>Maximum number of cron jobs (<c>max_cronjobs</c>).</summary>
    public int? MaxCronjobs { get; set; }

    /// <summary>Maximum number of WebFTP accounts (<c>max_wbk</c>).</summary>
    public int? MaxWbk { get; set; }

    /// <summary>Adds every set quota field to <paramref name="parameters"/> as a string value.</summary>
    /// <param name="parameters">The parameter map to populate.</param>
    /// <exception cref="ArgumentOutOfRangeException">A quota value is negative.</exception>
    internal void ApplyTo(IDictionary<string, object?> parameters)
    {
        Add(parameters, "max_account", MaxAccount);
        Add(parameters, "max_domain", MaxDomain);
        Add(parameters, "max_subdomain", MaxSubdomain);
        Add(parameters, "max_webspace", MaxWebspace);
        Add(parameters, "max_mail_account", MaxMailAccount);
        Add(parameters, "max_mail_forward", MaxMailForward);
        Add(parameters, "max_mailinglist", MaxMailinglist);
        Add(parameters, "max_database", MaxDatabase);
        Add(parameters, "max_ftpuser", MaxFtpUser);
        Add(parameters, "max_sambauser", MaxSambaUser);
        Add(parameters, "max_cronjobs", MaxCronjobs);
        Add(parameters, "max_wbk", MaxWbk);
    }

    private static void Add(IDictionary<string, object?> parameters, string key, int? value)
    {
        if (value is null)
            return;

        ArgumentOutOfRangeException.ThrowIfNegative(value.Value, key);
        parameters[key] = value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
