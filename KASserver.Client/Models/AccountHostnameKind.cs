namespace KASserver;

/// <summary>
/// The kind of hostname created for a subaccount (KAS <c>hostname_art</c>).
/// </summary>
public enum AccountHostnameKind
{
    /// <summary>The hostname is a domain (<c>domain</c>).</summary>
    Domain,

    /// <summary>The hostname is a subdomain (<c>subdomain</c>).</summary>
    Subdomain,

    /// <summary>No hostname is created (empty value).</summary>
    None,
}
