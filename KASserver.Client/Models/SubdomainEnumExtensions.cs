namespace KASserver;

/// <summary>
/// Maps the subdomain enums (<see cref="RedirectStatus"/>, <see cref="WebalizerVersion"/>,
/// <see cref="WebalizerLanguage"/>) to and from their KAS string values, kept in one place so the
/// <c>add_subdomain</c>/<c>update_subdomain</c> requests and the <see cref="Subdomain"/> read model
/// stay consistent.
/// </summary>
internal static class SubdomainEnumExtensions
{
    internal static string ToKasValue(this RedirectStatus status) => status switch
    {
        RedirectStatus.None => "0",
        RedirectStatus.MovedPermanently => "301",
        RedirectStatus.Found => "302",
        RedirectStatus.TemporaryRedirect => "307",
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    /// <summary>
    /// Parses a KAS redirect-status value back to a <see cref="RedirectStatus"/> (the request sends it
    /// as <c>redirect_status</c>, the <c>get_subdomains</c> response returns it as
    /// <c>subdomain_redirect_status</c>). Returns <c>null</c> for an unknown or missing value — read
    /// models must not throw on unexpected values.
    /// </summary>
    internal static RedirectStatus? TryParseRedirectStatus(string? raw) => raw?.Trim() switch
    {
        "0" => RedirectStatus.None,
        "301" => RedirectStatus.MovedPermanently,
        "302" => RedirectStatus.Found,
        "307" => RedirectStatus.TemporaryRedirect,
        _ => null,
    };

    internal static string ToKasValue(this WebalizerVersion version) => version switch
    {
        WebalizerVersion.None => "0",
        WebalizerVersion.Version4 => "4",
        WebalizerVersion.Version5 => "5",
        WebalizerVersion.Version7 => "7",
        _ => throw new ArgumentOutOfRangeException(nameof(version)),
    };

    /// <summary>
    /// Parses a KAS <c>statistic_version</c> string back to a <see cref="WebalizerVersion"/>. Returns
    /// <c>null</c> for an unknown or missing value.
    /// </summary>
    internal static WebalizerVersion? TryParseWebalizerVersion(string? raw) => raw?.Trim() switch
    {
        "0" => WebalizerVersion.None,
        "4" => WebalizerVersion.Version4,
        "5" => WebalizerVersion.Version5,
        "7" => WebalizerVersion.Version7,
        _ => null,
    };

    internal static string ToKasValue(this WebalizerLanguage language) => language switch
    {
        WebalizerLanguage.German => "de",
        WebalizerLanguage.English => "en",
        _ => throw new ArgumentOutOfRangeException(nameof(language)),
    };

    /// <summary>
    /// Parses a KAS <c>statistic_language</c> string back to a <see cref="WebalizerLanguage"/>. Returns
    /// <c>null</c> for an unknown or missing value.
    /// </summary>
    internal static WebalizerLanguage? TryParseWebalizerLanguage(string? raw) => raw?.Trim().ToLowerInvariant() switch
    {
        "de" => WebalizerLanguage.German,
        "en" => WebalizerLanguage.English,
        _ => null,
    };
}
