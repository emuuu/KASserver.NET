namespace KASserver;

/// <summary>
/// Helpers for the KAS <c>zone_host</c> parameter, which the KAS API keeps with a trailing dot
/// (e.g. <c>example.com.</c>). Kept in one place so add/get/reset normalize identically.
/// </summary>
internal static class DnsZoneHost
{
    /// <summary>
    /// Normalizes a zone host to the trailing-dot form KAS expects. Appends a dot when missing;
    /// idempotent for values that already end in one.
    /// </summary>
    /// <param name="zoneHost">The zone host, with or without a trailing dot.</param>
    /// <returns>The zone host with a guaranteed trailing dot.</returns>
    /// <exception cref="ArgumentException"><paramref name="zoneHost"/> is null, empty or whitespace.</exception>
    internal static string Normalize(string zoneHost)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(zoneHost);

        var trimmed = zoneHost.Trim();
        return trimmed.EndsWith('.') ? trimmed : trimmed + ".";
    }
}
