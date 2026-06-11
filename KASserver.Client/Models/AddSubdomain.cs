namespace KASserver;

/// <summary>
/// Parameters for creating a subdomain (<c>add_subdomain</c>). The new host is
/// <c>{SubdomainName}.{DomainName}</c>. Only the two name parts are required; every other property is
/// optional and only sent when set, leaving the KAS defaults in place
/// (<c>redirect_status</c> = no redirect, <c>statistic_version</c> = 5, <c>statistic_language</c> = de,
/// <c>php_version</c> = 7.1).
/// </summary>
public sealed class AddSubdomain
{
    /// <summary>The subdomain label (<c>subdomain_name</c>), e.g. <c>shop</c> for <c>shop.example.com</c>. Required.</summary>
    public required string SubdomainName { get; set; }

    /// <summary>The domain the label is added to (<c>domain_name</c>), e.g. <c>example.com</c>. Required.</summary>
    public required string DomainName { get; set; }

    /// <summary>
    /// The host path within the account, or — when a redirect is set — a target FQDN or WBK
    /// (<c>subdomain_path</c>). Examples: <c>/path/</c>, <c>http://domain.tld</c>, <c>wbk:wbk000001</c>.
    /// Optional.
    /// </summary>
    public string? SubdomainPath { get; set; }

    /// <summary>The redirect behaviour (<c>redirect_status</c>); defaults to no redirect on the server. Optional.</summary>
    public RedirectStatus? Redirect { get; set; }

    /// <summary>The webalizer statistics version (<c>statistic_version</c>); defaults to <c>5</c> on the server. Optional.</summary>
    public WebalizerVersion? StatisticVersion { get; set; }

    /// <summary>The webalizer statistics language (<c>statistic_language</c>); defaults to <c>de</c> on the server. Optional.</summary>
    public WebalizerLanguage? StatisticLanguage { get; set; }

    /// <summary>
    /// The PHP version (<c>php_version</c>), a raw KAS version string such as <c>8.3</c>; defaults to
    /// <c>7.1</c> on the server. The documented <c>5.X|7.X</c> format is outdated — current accounts
    /// also accept <c>8.x</c>. Optional.
    /// </summary>
    public string? PhpVersion { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(SubdomainName);
        ArgumentException.ThrowIfNullOrWhiteSpace(DomainName);

        var parameters = new Dictionary<string, object?>
        {
            ["subdomain_name"] = SubdomainName,
            ["domain_name"] = DomainName,
        };

        if (SubdomainPath is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(SubdomainPath);
            parameters["subdomain_path"] = SubdomainPath;
        }

        if (Redirect is not null)
            parameters["redirect_status"] = Redirect.Value.ToKasValue();

        if (StatisticVersion is not null)
            parameters["statistic_version"] = StatisticVersion.Value.ToKasValue();

        if (StatisticLanguage is not null)
            parameters["statistic_language"] = StatisticLanguage.Value.ToKasValue();

        if (PhpVersion is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(PhpVersion);
            parameters["php_version"] = PhpVersion;
        }

        return parameters;
    }
}
