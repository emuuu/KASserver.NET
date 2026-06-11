namespace KASserver;

/// <summary>
/// The webalizer statistics version of a subdomain (KAS <c>statistic_version</c>: <c>0|4|5|7</c>,
/// default <c>5</c>). <see cref="None"/> disables the statistics.
/// </summary>
public enum WebalizerVersion
{
    /// <summary>Statistics disabled (<c>0</c>).</summary>
    None,

    /// <summary>Webalizer version 4 (<c>4</c>).</summary>
    Version4,

    /// <summary>Webalizer version 5 (<c>5</c>), the KAS default.</summary>
    Version5,

    /// <summary>Webalizer version 7 (<c>7</c>).</summary>
    Version7,
}
