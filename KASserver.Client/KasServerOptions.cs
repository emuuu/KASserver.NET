namespace KASserver;

/// <summary>
/// Configuration options for the KAS API client.
/// </summary>
public class KasServerOptions
{
    /// <summary>
    /// The KAS login, e.g. <c>w00XXXXX</c>. Required.
    /// </summary>
    public string? Login { get; set; }

    /// <summary>
    /// The KAS password. Required.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Lifetime of the session token in seconds. Default: 1800 (30 minutes).
    /// </summary>
    public int SessionLifetime { get; set; } = 1800;

    /// <summary>
    /// When <c>true</c>, the session lifetime is extended with every request
    /// (KAS parameter <c>session_update_lifetime=Y</c>). Default: <c>true</c>.
    /// </summary>
    public bool UpdateSessionLifetime { get; set; } = true;

    /// <summary>
    /// Optional callback that supplies the current two-factor one-time PIN
    /// (KAS parameter <c>session_2fa</c>). Only needed when 2FA is enabled for the account.
    /// </summary>
    /// <remarks>
    /// Invoked on every (re-)authentication while the single request lock is held, so all other
    /// calls block until it completes. Keep it fast and non-interactive.
    /// </remarks>
    public Func<CancellationToken, Task<string>>? TwoFactorPinProvider { get; set; }

    /// <summary>
    /// HTTP request timeout. Default: 30 seconds. Use <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>
    /// to disable the timeout; any other non-positive value is rejected.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(Login))
            throw new InvalidOperationException($"{nameof(KasServerOptions)}.{nameof(Login)} must be set.");

        if (string.IsNullOrWhiteSpace(Password))
            throw new InvalidOperationException($"{nameof(KasServerOptions)}.{nameof(Password)} must be set.");

        if (SessionLifetime <= 0)
            throw new InvalidOperationException($"{nameof(KasServerOptions)}.{nameof(SessionLifetime)} must be greater than zero.");

        if (Timeout != System.Threading.Timeout.InfiniteTimeSpan)
        {
            if (Timeout <= TimeSpan.Zero)
                throw new InvalidOperationException(
                    $"{nameof(KasServerOptions)}.{nameof(Timeout)} must be positive or System.Threading.Timeout.InfiniteTimeSpan.");

            if (Timeout.TotalMilliseconds > int.MaxValue)
                throw new InvalidOperationException(
                    $"{nameof(KasServerOptions)}.{nameof(Timeout)} must not exceed {int.MaxValue} milliseconds (HttpClient limit).");
        }
    }
}
