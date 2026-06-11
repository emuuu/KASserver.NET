using KASserver.Soap;

namespace KASserver.Client.Tests.Fakes;

/// <summary>
/// A test double for <see cref="IKasTransport"/> that records every call (action + parameters) the
/// typed <see cref="KasClient"/> wrappers make and returns pre-staged <see cref="KasResponse"/>s.
/// Lets the contract tests assert exactly which KAS action and parameter map leaves the client,
/// without touching the real SOAP/session/flood machinery.
/// </summary>
internal sealed class RecordingKasTransport : IKasTransport
{
    private readonly Queue<KasResponse> _responses = new();

    /// <summary>Every call made, in order. Each entry is the action and the parameter map sent.</summary>
    public List<(string Action, IReadOnlyDictionary<string, object?>? Parameters)> Calls { get; } = new();

    /// <summary>The action of the most recent call.</summary>
    public string LastAction => Calls[^1].Action;

    /// <summary>The parameter map of the most recent call (may be <c>null</c> for parameterless actions).</summary>
    public IReadOnlyDictionary<string, object?>? LastParameters => Calls[^1].Parameters;

    /// <summary>The cancellation token forwarded on the most recent call.</summary>
    public CancellationToken LastCancellationToken { get; private set; }

    /// <summary>Stages a raw response for the next call.</summary>
    public RecordingKasTransport Enqueue(KasResponse response)
    {
        _responses.Enqueue(response);
        return this;
    }

    /// <summary>Stages an empty successful response, for the <c>update_*</c>/<c>delete_*</c> wrappers that ignore the payload.</summary>
    public RecordingKasTransport EnqueueSuccess() =>
        Enqueue(new KasResponse { ReturnString = "TRUE" });

    /// <summary>Stages a scalar <c>ReturnInfo</c> response (e.g. the generated id of an <c>add_*</c> action).</summary>
    public RecordingKasTransport EnqueueScalar(string returnInfo) =>
        Enqueue(new KasResponse { ReturnString = "TRUE", ReturnInfo = returnInfo });

    /// <summary>Stages a list <c>ReturnInfo</c> response (e.g. for the <c>get_*</c> list actions).</summary>
    public RecordingKasTransport EnqueueList(params IReadOnlyDictionary<string, object?>[] items) =>
        Enqueue(new KasResponse { ReturnString = "TRUE", ReturnInfo = items.Cast<object?>().ToList() });

    /// <summary>Stages a map <c>ReturnInfo</c> response (e.g. for <c>get_accountsettings</c>/<c>get_accountresources</c>).</summary>
    public RecordingKasTransport EnqueueMap(IReadOnlyDictionary<string, object?> map) =>
        Enqueue(new KasResponse { ReturnString = "TRUE", ReturnInfo = map });

    public Task<KasResponse> CallAsync(
        string action,
        IReadOnlyDictionary<string, object?>? parameters,
        CancellationToken cancellationToken)
    {
        Calls.Add((action, parameters));
        LastCancellationToken = cancellationToken;

        // Deliberately strict: one staged response per expected call. An unstaged call is either a
        // forgotten Enqueue or an unexpected extra transport call — both should fail the test loudly
        // rather than silently succeed and mask the wrong contract.
        if (_responses.Count == 0)
            throw new InvalidOperationException(
                $"No response was staged for call #{Calls.Count} to '{action}'. Stage one response per expected transport call.");

        return Task.FromResult(_responses.Dequeue());
    }
}
