namespace KASserver;

/// <summary>
/// A subdomain as returned by <c>get_subdomains</c>. Typed convenience properties cover the common
/// fields; <see cref="Raw"/> exposes the complete map for everything else.
/// </summary>
/// <remarks>
/// Field shape verified live against the KAS API (<c>get_subdomains</c> on famgmbh.de). Note that the
/// response field names differ from the request parameter names: the redirect status is returned as
/// <c>subdomain_redirect_status</c> (the request uses <c>redirect_status</c>), while
/// <c>subdomain_name</c> is the full host name, e.g. <c>shop.example.com</c>. Freshly created
/// subdomains report <c>in_progress = TRUE</c> while KAS provisions them; updates are rejected with an
/// <c>in_progress</c> fault until that clears.
/// </remarks>
public sealed class Subdomain
{
    /// <summary>The complete raw field map as returned by the KAS API.</summary>
    public required IReadOnlyDictionary<string, object?> Raw { get; init; }

    /// <summary>The subdomain host name (KAS field <c>subdomain_name</c>), e.g. <c>shop.example.com</c>.</summary>
    public string? SubdomainName => Raw.GetValueOrDefault("subdomain_name") as string;

    /// <summary>The host path or redirect target (KAS field <c>subdomain_path</c>).</summary>
    public string? SubdomainPath => Raw.GetValueOrDefault("subdomain_path") as string;

    /// <summary>The raw redirect-status string as returned by KAS (KAS field <c>subdomain_redirect_status</c>).</summary>
    public string? RawRedirectStatus => Raw.GetValueOrDefault("subdomain_redirect_status") as string;

    /// <summary>
    /// The redirect behaviour as a <see cref="RedirectStatus"/>, or <c>null</c> when KAS returned a
    /// value not covered by the enum (the raw string is then available via <see cref="RawRedirectStatus"/>).
    /// </summary>
    public RedirectStatus? Redirect => SubdomainEnumExtensions.TryParseRedirectStatus(RawRedirectStatus);

    /// <summary>The PHP version (KAS field <c>php_version</c>), a raw version string such as <c>8.5</c>.</summary>
    public string? PhpVersion => Raw.GetValueOrDefault("php_version") as string;

    /// <summary>Whether the configured PHP version is deprecated (KAS field <c>php_deprecated</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool PhpDeprecated => IsYes("php_deprecated");

    /// <summary>The raw webalizer-version string as returned by KAS (KAS field <c>statistic_version</c>).</summary>
    public string? RawStatisticVersion => Raw.GetValueOrDefault("statistic_version") as string;

    /// <summary>The webalizer statistics version as a <see cref="WebalizerVersion"/>, or <c>null</c> for an unknown/missing value.</summary>
    public WebalizerVersion? StatisticVersion => SubdomainEnumExtensions.TryParseWebalizerVersion(RawStatisticVersion);

    /// <summary>The raw webalizer-language string as returned by KAS (KAS field <c>statistic_language</c>).</summary>
    public string? RawStatisticLanguage => Raw.GetValueOrDefault("statistic_language") as string;

    /// <summary>The webalizer statistics language as a <see cref="WebalizerLanguage"/>, or <c>null</c> for an unknown/missing value.</summary>
    public WebalizerLanguage? StatisticLanguage => SubdomainEnumExtensions.TryParseWebalizerLanguage(RawStatisticLanguage);

    /// <summary>The account the subdomain belongs to (KAS field <c>subdomain_account</c>).</summary>
    public string? Account => Raw.GetValueOrDefault("subdomain_account") as string;

    /// <summary>The server hosting the subdomain (KAS field <c>subdomain_server</c>).</summary>
    public string? Server => Raw.GetValueOrDefault("subdomain_server") as string;

    /// <summary>Whether the subdomain is active (KAS field <c>is_active</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool IsActive => IsYes("is_active");

    /// <summary>
    /// Whether KAS is still provisioning the subdomain (KAS field <c>in_progress</c>, <c>TRUE</c>/<c>FALSE</c>).
    /// Updates are rejected with an <c>in_progress</c> fault while this is <c>true</c>.
    /// </summary>
    public bool InProgress => string.Equals(Raw.GetValueOrDefault("in_progress") as string, "TRUE", StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether FrontPage Server Extensions are active (KAS field <c>fpse_active</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool FpseActive => IsYes("fpse_active");

    /// <summary>Whether the SSL proxy is enabled (KAS field <c>ssl_proxy</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool SslProxy => IsYes("ssl_proxy");

    /// <summary>Whether an IP-based SSL certificate is present (KAS field <c>ssl_certificate_ip</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool SslCertificateIp => IsYes("ssl_certificate_ip");

    /// <summary>Whether an SNI-based SSL certificate is present (KAS field <c>ssl_certificate_sni</c>, <c>Y</c>/<c>N</c>).</summary>
    public bool SslCertificateSni => IsYes("ssl_certificate_sni");

    private bool IsYes(string key) =>
        string.Equals(Raw.GetValueOrDefault(key) as string, "Y", StringComparison.OrdinalIgnoreCase);

    internal static Subdomain FromMap(IReadOnlyDictionary<string, object?> map) => new() { Raw = map };
}
