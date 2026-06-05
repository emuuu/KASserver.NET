namespace KASserver;

/// <summary>
/// A mail forward as returned by <c>get_mailforwards</c>. Typed convenience properties cover the
/// most common fields; <see cref="Raw"/> exposes the complete map for everything else.
/// </summary>
public sealed class MailForward
{
    // KAS returns the targets comma-separated (verified live); semicolon is tolerated defensively.
    private static readonly char[] TargetSeparators = [',', ';'];

    /// <summary>The complete raw field map as returned by the KAS API.</summary>
    public required IReadOnlyDictionary<string, object?> Raw { get; init; }

    /// <summary>
    /// The forwarding address. KAS returns this under both <c>mail_forward_address</c> and the
    /// misspelled <c>mail_forward_adress</c>; the correctly spelled key is preferred.
    /// </summary>
    public string? Address =>
        Raw.GetValueOrDefault("mail_forward_address") as string
        ?? Raw.GetValueOrDefault("mail_forward_adress") as string;

    /// <summary>The raw, comma-separated target specification (KAS field <c>mail_forward_targets</c>).</summary>
    public string? RawTargets => Raw.GetValueOrDefault("mail_forward_targets") as string;

    /// <summary>
    /// The target addresses parsed from <see cref="RawTargets"/> (split on comma or semicolon).
    /// Empty when no raw targets are present. Mirrors <see cref="AddMailForward.Targets"/>.
    /// </summary>
    public IReadOnlyList<string> TargetAddresses =>
        RawTargets is { Length: > 0 } raw
            ? raw.Split(TargetSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : [];

    /// <summary>The forward's comment (KAS field <c>mail_forward_comment</c>).</summary>
    public string? Comment => Raw.GetValueOrDefault("mail_forward_comment") as string;

    /// <summary>The configured spam filter (KAS field <c>mail_forward_spamfilter</c>).</summary>
    public string? SpamFilter => Raw.GetValueOrDefault("mail_forward_spamfilter") as string;

    internal static MailForward FromMap(IReadOnlyDictionary<string, object?> map) => new() { Raw = map };
}
