namespace KASserver;

/// <summary>
/// The web statistics mode of an account (KAS <c>statistic</c>). The mapping of the
/// <c>de</c>/<c>ne</c> values to the concrete statistics packages is to be confirmed live.
/// </summary>
public enum AccountStatistic
{
    /// <summary>No statistics (<c>0</c>).</summary>
    None,

    /// <summary>Statistics variant <c>de</c> (concrete package to be confirmed live).</summary>
    De,

    /// <summary>Statistics variant <c>ne</c> (concrete package to be confirmed live).</summary>
    Ne,
}
