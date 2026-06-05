namespace KASserver;

/// <summary>
/// Thrown when the KAS API returns a SOAP fault (e.g. <c>kas_session_invalid</c>,
/// <c>email_already_exists</c>, <c>missing_parameter</c>) or when a response cannot be
/// interpreted (non-XML body, unexpected response shape).
/// </summary>
public class KasApiException : Exception
{
    /// <summary>
    /// The raw KAS fault string, e.g. <c>email_already_exists</c>.
    /// Useful for programmatic handling. May be <c>null</c> if the error did not originate from a SOAP fault.
    /// </summary>
    public string? FaultCode { get; }

    /// <summary>
    /// The KAS action that triggered the error, if known.
    /// </summary>
    public string? Action { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="KasApiException"/> class.
    /// </summary>
    public KasApiException(string message, string? faultCode = null, string? action = null)
        : base(message)
    {
        FaultCode = faultCode;
        Action = action;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KasApiException"/> class wrapping an underlying error.
    /// </summary>
    public KasApiException(string message, Exception innerException, string? faultCode = null, string? action = null)
        : base(message, innerException)
    {
        FaultCode = faultCode;
        Action = action;
    }
}
