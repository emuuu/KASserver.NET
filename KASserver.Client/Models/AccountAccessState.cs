namespace KASserver;

/// <summary>
/// The KAS login access state of a subaccount (KAS <c>kas_access_forbidden</c>). While
/// <c>add_account</c> only distinguishes allowed/forbidden, <c>update_account</c> additionally
/// accepts the explicit <c>forbidden</c> value.
/// </summary>
public enum AccountAccessState
{
    /// <summary>KAS login is allowed (<c>N</c>).</summary>
    Allowed,

    /// <summary>KAS login is forbidden (<c>Y</c>).</summary>
    Forbidden,

    /// <summary>KAS login is explicitly forbidden (<c>forbidden</c>) — only valid for <c>update_account</c>.</summary>
    ForbiddenExplicit,
}
