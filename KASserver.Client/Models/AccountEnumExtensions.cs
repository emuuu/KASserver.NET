namespace KASserver;

/// <summary>
/// Maps the account enums to their KAS string values, kept in one place so
/// <c>add_account</c>, <c>update_account</c> and <c>update_accountsettings</c> stay consistent.
/// </summary>
internal static class AccountEnumExtensions
{
    internal static string ToKasValue(this AccountHostnameKind kind) => kind switch
    {
        AccountHostnameKind.Domain => "domain",
        AccountHostnameKind.Subdomain => "subdomain",
        AccountHostnameKind.None => string.Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    internal static string ToKasValue(this AccountLogging logging) => logging switch
    {
        AccountLogging.Full => "voll",
        AccountLogging.Short => "kurz",
        AccountLogging.WithoutIp => "ohneip",
        AccountLogging.None => "keine",
        _ => throw new ArgumentOutOfRangeException(nameof(logging)),
    };

    internal static string ToKasValue(this AccountStatistic statistic) => statistic switch
    {
        AccountStatistic.None => "0",
        AccountStatistic.De => "de",
        AccountStatistic.Ne => "ne",
        _ => throw new ArgumentOutOfRangeException(nameof(statistic)),
    };

    internal static string ToKasValue(this AccountAccessState state) => state switch
    {
        AccountAccessState.Allowed => "N",
        AccountAccessState.Forbidden => "Y",
        AccountAccessState.ForbiddenExplicit => "forbidden",
        _ => throw new ArgumentOutOfRangeException(nameof(state)),
    };
}
