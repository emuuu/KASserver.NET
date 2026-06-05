namespace KASserver;

/// <summary>
/// A parsed KAS API response (the <c>Response</c> map of a successful call).
/// </summary>
public sealed class KasResponse
{
    /// <summary>
    /// The KAS status string, normally <c>TRUE</c> for a successful call.
    /// </summary>
    public string? ReturnString { get; init; }

    /// <summary>
    /// The action-specific payload (<c>ReturnInfo</c>). Depending on the action this is a
    /// <see cref="IReadOnlyDictionary{TKey,TValue}"/>, a <see cref="IReadOnlyList{T}"/>,
    /// a scalar string, or <c>null</c>.
    /// </summary>
    public object? ReturnInfo { get; init; }

    /// <summary>
    /// The server-advised delay before the next request, in seconds (KAS <c>KasFloodDelay</c>).
    /// The transport honors this automatically.
    /// </summary>
    public double FloodDelay { get; init; }

    /// <summary>
    /// <see cref="ReturnInfo"/> as a map. Returns an empty map when <see cref="ReturnInfo"/> is
    /// <c>null</c>. Throws <see cref="KasApiException"/> when it is present but not a map, so a
    /// shape mismatch is never silently masked as "empty".
    /// </summary>
    public IReadOnlyDictionary<string, object?> AsMap()
    {
        return ReturnInfo switch
        {
            null => new Dictionary<string, object?>(),
            IReadOnlyDictionary<string, object?> map => map,
            _ => throw new KasApiException(
                $"Expected a map in ReturnInfo but got {ReturnInfo.GetType().Name}."),
        };
    }

    /// <summary>
    /// <see cref="ReturnInfo"/> as a list of maps. Returns an empty list when <see cref="ReturnInfo"/>
    /// is <c>null</c> or an empty list (a legitimate "no results" response). Throws
    /// <see cref="KasApiException"/> when it is a non-list scalar, so a shape mismatch is not masked.
    /// </summary>
    public IReadOnlyList<IReadOnlyDictionary<string, object?>> AsList()
    {
        if (ReturnInfo is null)
            return [];

        if (ReturnInfo is not IReadOnlyList<object?> list)
            throw new KasApiException($"Expected a list in ReturnInfo but got {ReturnInfo.GetType().Name}.");

        var result = new List<IReadOnlyDictionary<string, object?>>(list.Count);
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i] is not IReadOnlyDictionary<string, object?> map)
                throw new KasApiException(
                    $"Expected a map at ReturnInfo[{i}] but got {list[i]?.GetType().Name ?? "null"}.");

            result.Add(map);
        }

        return result;
    }
}
