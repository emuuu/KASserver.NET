namespace KASserver;

/// <summary>
/// The activation state of a mailbox (KAS <c>is_active</c>).
/// </summary>
public enum MailboxActiveState
{
    /// <summary>The mailbox is fully active (<c>Y</c>).</summary>
    Active,

    /// <summary>Receiving is disabled, but existing mail can still be retrieved (<c>N</c>).</summary>
    ReceiveDisabled,

    /// <summary>The mailbox is forbidden — no receiving and no retrieval (<c>forbidden</c>).</summary>
    Forbidden,
}
