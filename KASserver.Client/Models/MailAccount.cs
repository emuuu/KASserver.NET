namespace KASserver;

/// <summary>
/// A mailbox as returned by <c>get_mailaccounts</c>. Typed convenience properties cover the most
/// common fields; <see cref="Raw"/> exposes the complete map for everything else.
/// </summary>
public sealed class MailAccount
{
    /// <summary>The complete raw field map as returned by the KAS API.</summary>
    public required IReadOnlyDictionary<string, object?> Raw { get; init; }

    /// <summary>The technical mailbox login (e.g. <c>m07f821c</c>). Used by update/delete actions.</summary>
    public string? MailLogin => Raw.GetValueOrDefault("mail_login") as string;

    /// <summary>The mail address(es) of the mailbox (KAS field <c>mail_adresses</c>).</summary>
    public string? Addresses => Raw.GetValueOrDefault("mail_adresses") as string;

    internal static MailAccount FromMap(IReadOnlyDictionary<string, object?> map) => new() { Raw = map };
}
