namespace KASserver.Soap;

/// <summary>
/// Transport seam for the KAS API: a single gateway that sends an action plus its parameter map and
/// returns the parsed <see cref="KasResponse"/>. Extracted from <see cref="KasSoapTransport"/> so the
/// typed <see cref="IKasClient"/> wrappers can be unit-tested for the action name and parameters they
/// send, without touching the real SOAP/session/flood machinery. Intentionally internal — it is not
/// part of the public surface.
/// </summary>
internal interface IKasTransport
{
    /// <inheritdoc cref="KasSoapTransport.CallAsync"/>
    Task<KasResponse> CallAsync(
        string action,
        IReadOnlyDictionary<string, object?>? parameters,
        CancellationToken cancellationToken);
}
