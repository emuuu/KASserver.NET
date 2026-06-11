namespace KASserver;

/// <summary>
/// Parameters for editing a subdomain (<c>update_subdomain</c>). Only properties that are set
/// (non-null) are sent; everything left <c>null</c> stays unchanged on the server. The subdomain is
/// identified by its host name, passed separately to <see cref="IKasClient.UpdateSubdomainAsync"/>.
/// At least one field must be set. <c>update_subdomain</c> only accepts <c>subdomain_path</c>,
/// <c>redirect_status</c> and <c>php_version</c> — unlike <see cref="AddSubdomain"/>, the webalizer
/// statistics fields cannot be changed here.
/// </summary>
public sealed class UpdateSubdomain
{
    /// <summary>
    /// The host path within the account, or — when a redirect is set — a target FQDN or WBK
    /// (<c>subdomain_path</c>). Examples: <c>/path/</c>, <c>http://domain.tld</c>, <c>wbk:wbk000001</c>.
    /// Optional.
    /// </summary>
    public string? SubdomainPath { get; set; }

    /// <summary>The redirect behaviour (<c>redirect_status</c>). Optional.</summary>
    public RedirectStatus? Redirect { get; set; }

    /// <summary>
    /// The PHP version (<c>php_version</c>), a raw KAS version string such as <c>8.3</c>. The
    /// documented <c>5.X|7.X</c> format is outdated — current accounts also accept <c>8.x</c>. Optional.
    /// </summary>
    public string? PhpVersion { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters(string subdomainName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subdomainName);

        var parameters = new Dictionary<string, object?> { ["subdomain_name"] = subdomainName };

        if (SubdomainPath is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(SubdomainPath);
            parameters["subdomain_path"] = SubdomainPath;
        }

        if (Redirect is not null)
            parameters["redirect_status"] = Redirect.Value.ToKasValue();

        if (PhpVersion is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(PhpVersion);
            parameters["php_version"] = PhpVersion;
        }

        // subdomain_name alone is a no-op that KAS rejects with "nothing_to_do" — fail fast on the client.
        if (parameters.Count == 1)
            throw new ArgumentException($"At least one field on {nameof(UpdateSubdomain)} must be set.");

        return parameters;
    }
}
