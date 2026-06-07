namespace KASserver;

/// <summary>
/// The web server log mode of an account (KAS <c>logging</c>).
/// </summary>
public enum AccountLogging
{
    /// <summary>Full logging including the visitor IP (<c>voll</c>).</summary>
    Full,

    /// <summary>Short logging (<c>kurz</c>).</summary>
    Short,

    /// <summary>Logging without the visitor IP (<c>ohneip</c>).</summary>
    WithoutIp,

    /// <summary>No logging (<c>keine</c>).</summary>
    None,
}
