namespace KASserver;

/// <summary>
/// The redirect behaviour of a subdomain (KAS <c>redirect_status</c>). When <see cref="None"/> the
/// subdomain serves the host path; otherwise it issues an HTTP redirect to the FQDN/WBK given as the
/// subdomain path.
/// </summary>
public enum RedirectStatus
{
    /// <summary>No redirect (<c>0</c>); the subdomain serves its host path.</summary>
    None,

    /// <summary>Permanent redirect, HTTP <c>301</c>.</summary>
    MovedPermanently,

    /// <summary>Temporary redirect, HTTP <c>302</c>.</summary>
    Found,

    /// <summary>Temporary redirect, HTTP <c>307</c>.</summary>
    TemporaryRedirect,
}
