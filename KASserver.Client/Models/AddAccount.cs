namespace KASserver;

/// <summary>
/// Parameters for creating a subaccount (<c>add_account</c>). Requires a superuser login.
/// </summary>
public sealed class AddAccount
{
    /// <summary>The KAS password of the new subaccount (<c>account_kas_password</c>). Required.</summary>
    public required string KasPassword { get; set; }

    /// <summary>The FTP password of the new subaccount (<c>account_ftp_password</c>). Required.</summary>
    public required string FtpPassword { get; set; }

    /// <summary>The kind of hostname to create (<c>hostname_art</c>). Required.</summary>
    public required AccountHostnameKind HostnameKind { get; set; }

    /// <summary>The first hostname part (<c>hostname_part1</c>), e.g. the domain/subdomain label. Required.</summary>
    public required string HostnamePart1 { get; set; }

    /// <summary>The second hostname part (<c>hostname_part2</c>), e.g. the TLD or parent domain. Required.</summary>
    public required string HostnamePart2 { get; set; }

    /// <summary>The resource quotas. Unset quotas use the KAS default. Optional.</summary>
    public AccountQuota? Quota { get; set; }

    /// <summary>The account comment (<c>account_comment</c>). Optional; KAS defaults to the hostname.</summary>
    public string? Comment { get; set; }

    /// <summary>The contact mail address (<c>account_contact_mail</c>). Optional.</summary>
    public string? ContactMail { get; set; }

    /// <summary>The hostname path (<c>hostname_path</c>). Optional; KAS defaults to the hostname.</summary>
    public string? HostnamePath { get; set; }

    /// <summary>Install <c>.htaccess</c> support (<c>inst_htaccess</c>). Default: <c>true</c> (<c>Y</c>).</summary>
    public bool InstHtaccess { get; set; } = true;

    /// <summary>Install the software set (<c>inst_software</c>). Default: <c>true</c> (<c>Y</c>).</summary>
    public bool InstSoftware { get; set; } = true;

    /// <summary>Forbid KAS login for the subaccount (<c>kas_access_forbidden</c>). Default: <c>false</c> (<c>N</c>).</summary>
    public bool AccessForbidden { get; set; }

    /// <summary>The web server log mode (<c>logging</c>). Default: <see cref="AccountLogging.None"/> (<c>keine</c>).</summary>
    public AccountLogging Logging { get; set; } = AccountLogging.None;

    /// <summary>The log retention age in days (<c>logage</c>), 1–999. Default: <c>190</c>.</summary>
    public int LogAge { get; set; } = 190;

    /// <summary>The web statistics mode (<c>statistic</c>). Default: <see cref="AccountStatistic.None"/> (<c>0</c>).</summary>
    public AccountStatistic Statistic { get; set; } = AccountStatistic.None;

    /// <summary>Allow DNS settings (<c>dns_settings</c>). Default: <c>false</c> (<c>N</c>).</summary>
    public bool DnsSettings { get; set; }

    /// <summary>Show the password in the KAS interface (<c>show_password</c>). Default: <c>false</c> (<c>N</c>).</summary>
    public bool ShowPassword { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(KasPassword);
        ArgumentException.ThrowIfNullOrWhiteSpace(FtpPassword);
        ArgumentException.ThrowIfNullOrWhiteSpace(HostnamePart1);
        ArgumentException.ThrowIfNullOrWhiteSpace(HostnamePart2);
        ArgumentOutOfRangeException.ThrowIfLessThan(LogAge, 1, nameof(LogAge));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(LogAge, 999, nameof(LogAge));

        var parameters = new Dictionary<string, object?>
        {
            ["account_kas_password"] = KasPassword,
            ["account_ftp_password"] = FtpPassword,
            ["hostname_art"] = HostnameKind.ToKasValue(),
            ["hostname_part1"] = HostnamePart1,
            ["hostname_part2"] = HostnamePart2,
            ["inst_htaccess"] = InstHtaccess ? "Y" : "N",
            ["inst_software"] = InstSoftware ? "Y" : "N",
            ["kas_access_forbidden"] = AccessForbidden ? "Y" : "N",
            ["logging"] = Logging.ToKasValue(),
            ["logage"] = LogAge.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["statistic"] = Statistic.ToKasValue(),
            ["dns_settings"] = DnsSettings ? "Y" : "N",
            ["show_password"] = ShowPassword ? "Y" : "N",
        };

        Quota?.ApplyTo(parameters);

        if (!string.IsNullOrWhiteSpace(Comment))
            parameters["account_comment"] = Comment;

        if (!string.IsNullOrWhiteSpace(ContactMail))
            parameters["account_contact_mail"] = ContactMail;

        if (!string.IsNullOrWhiteSpace(HostnamePath))
            parameters["hostname_path"] = HostnamePath;

        return parameters;
    }
}
