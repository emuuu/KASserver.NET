namespace KASserver;

/// <summary>
/// A subaccount as returned by <c>get_accounts</c>. Typed convenience properties cover the most
/// common fields; <see cref="Raw"/> exposes the complete map for everything else. Field shape
/// verified live against the KAS API.
/// </summary>
/// <remarks>
/// Note: in the <c>get_accounts</c> response the quota fields use names that differ from the
/// <c>add_account</c>/<c>update_account</c> request parameters — e.g. <c>max_databases</c> (vs.
/// request <c>max_database</c>) and <c>max_mail_list</c> (vs. request <c>max_mailinglist</c>).
/// Read those via <see cref="Raw"/>.
/// </remarks>
public sealed class SubAccount
{
    /// <summary>The complete raw field map as returned by the KAS API.</summary>
    public required IReadOnlyDictionary<string, object?> Raw { get; init; }

    /// <summary>The subaccount login (KAS field <c>account_login</c>). Used by update/delete actions.</summary>
    public string? AccountLogin => Raw.GetValueOrDefault("account_login") as string;

    /// <summary>The account comment (KAS field <c>account_comment</c>).</summary>
    public string? Comment => Raw.GetValueOrDefault("account_comment") as string;

    /// <summary>The contact mail address (KAS field <c>account_contact_mail</c>).</summary>
    public string? ContactMail => Raw.GetValueOrDefault("account_contact_mail") as string;

    /// <summary>Whether KAS login is forbidden (KAS field <c>kas_access_forbidden</c>): <c>N</c>/<c>Y</c>/<c>forbidden</c>.</summary>
    public string? AccessForbidden => Raw.GetValueOrDefault("kas_access_forbidden") as string;

    internal static SubAccount FromMap(IReadOnlyDictionary<string, object?> map) => new() { Raw = map };
}
