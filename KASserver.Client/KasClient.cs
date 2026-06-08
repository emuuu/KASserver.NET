using KASserver.Soap;

namespace KASserver;

/// <inheritdoc cref="IKasClient"/>
internal sealed class KasClient : IKasClient
{
    private readonly KasSoapTransport _transport;

    public KasClient(KasSoapTransport transport) => _transport = transport;

    /// <inheritdoc/>
    public Task<KasResponse> RequestAsync(
        string action,
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        return _transport.CallAsync(action, parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, object?>> GetAccountSettingsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _transport.CallAsync("get_accountsettings", null, cancellationToken).ConfigureAwait(false);

        // ReturnInfo is { "settings": { ... } }; unwrap the inner map for convenience.
        var map = response.AsMap();
        return map.TryGetValue("settings", out var settings)
            && settings is IReadOnlyDictionary<string, object?> inner
            ? inner
            : map;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, object?>> GetAccountResourcesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _transport.CallAsync("get_accountresources", null, cancellationToken).ConfigureAwait(false);
        return response.AsMap();
    }

    /// <inheritdoc/>
    public async Task<AccountSettings> GetAccountSettingsTypedAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetAccountSettingsAsync(cancellationToken).ConfigureAwait(false);
        return AccountSettings.FromMap(settings);
    }

    /// <inheritdoc/>
    public async Task<AccountResources> GetAccountResourcesTypedAsync(CancellationToken cancellationToken = default)
    {
        var resources = await GetAccountResourcesAsync(cancellationToken).ConfigureAwait(false);
        return AccountResources.FromMap(resources);
    }

    /// <inheritdoc/>
    public Task UpdateAccountSettingsAsync(UpdateAccountSettings changes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changes);

        return _transport.CallAsync("update_accountsettings", changes.ToParameters(), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string> AddAccountAsync(AddAccount account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        var response = await _transport.CallAsync("add_account", account.ToParameters(), cancellationToken).ConfigureAwait(false);
        return ExtractAccountLogin(response);
    }

    // add_account returns the generated account_login as the ReturnInfo scalar (verified live).
    // The map branch is a defensive fallback in case KAS ever wraps it in an "account_login" field.
    internal static string ExtractAccountLogin(KasResponse response)
    {
        var login = response.ReturnInfo switch
        {
            string scalar => scalar,
            IReadOnlyDictionary<string, object?> map => map.GetValueOrDefault("account_login") as string,
            _ => null,
        };

        return !string.IsNullOrWhiteSpace(login)
            ? login
            : throw new KasApiException("add_account did not return an account login.", action: "add_account");
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SubAccount>> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _transport.CallAsync("get_accounts", null, cancellationToken).ConfigureAwait(false);
        return response.AsList().Select(SubAccount.FromMap).ToList();
    }

    /// <inheritdoc/>
    public async Task<SubAccount?> GetAccountAsync(string accountLogin, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountLogin);

        var parameters = new Dictionary<string, object?> { ["account_login"] = accountLogin };
        var response = await _transport.CallAsync("get_accounts", parameters, cancellationToken).ConfigureAwait(false);
        return response.AsList().Select(SubAccount.FromMap).FirstOrDefault();
    }

    /// <inheritdoc/>
    public Task UpdateAccountAsync(string accountLogin, UpdateAccount changes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountLogin);
        ArgumentNullException.ThrowIfNull(changes);

        return _transport.CallAsync("update_account", changes.ToParameters(accountLogin), cancellationToken);
    }

    /// <inheritdoc/>
    public Task DeleteAccountAsync(string accountLogin, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountLogin);

        var parameters = new Dictionary<string, object?> { ["account_login"] = accountLogin };
        return _transport.CallAsync("delete_account", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public Task UpdateSuperuserSettingsAsync(string accountLogin, UpdateSuperuserSettings changes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountLogin);
        ArgumentNullException.ThrowIfNull(changes);

        return _transport.CallAsync("update_superusersettings", changes.ToParameters(accountLogin), cancellationToken);
    }

    /// <inheritdoc/>
    public Task UpdateChownAsync(UpdateChown chown, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chown);

        return _transport.CallAsync("update_chown", chown.ToParameters(), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> GetDomainsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _transport.CallAsync("get_domains", null, cancellationToken).ConfigureAwait(false);
        return response.AsList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MailAccount>> GetMailAccountsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _transport.CallAsync("get_mailaccounts", null, cancellationToken).ConfigureAwait(false);
        return response.AsList().Select(MailAccount.FromMap).ToList();
    }

    /// <inheritdoc/>
    public async Task<string> AddMailAccountAsync(AddMailAccount account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        var response = await _transport.CallAsync("add_mailaccount", account.ToParameters(), cancellationToken).ConfigureAwait(false);

        // add_mailaccount returns the generated mail_login as the ReturnInfo scalar.
        return response.ReturnInfo as string is { } login && !string.IsNullOrWhiteSpace(login)
            ? login
            : throw new KasApiException("add_mailaccount did not return a mail login.", action: "add_mailaccount");
    }

    /// <inheritdoc/>
    public Task UpdateMailAccountAsync(string mailLogin, UpdateMailAccount changes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailLogin);
        ArgumentNullException.ThrowIfNull(changes);

        return _transport.CallAsync("update_mailaccount", changes.ToParameters(mailLogin), cancellationToken);
    }

    /// <inheritdoc/>
    public Task UpdateMailAccountPasswordAsync(string mailLogin, string newPassword, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPassword);

        return UpdateMailAccountAsync(mailLogin, new UpdateMailAccount { NewPassword = newPassword }, cancellationToken);
    }

    /// <inheritdoc/>
    public Task DeleteMailAccountAsync(string mailLogin, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailLogin);

        var parameters = new Dictionary<string, object?> { ["mail_login"] = mailLogin };
        return _transport.CallAsync("delete_mailaccount", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MailForward>> GetMailForwardsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _transport.CallAsync("get_mailforwards", null, cancellationToken).ConfigureAwait(false);
        return response.AsList().Select(MailForward.FromMap).ToList();
    }

    /// <inheritdoc/>
    public Task AddMailForwardAsync(AddMailForward forward, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(forward);

        return _transport.CallAsync("add_mailforward", forward.ToParameters(), cancellationToken);
    }

    /// <inheritdoc/>
    public Task DeleteMailForwardAsync(string mailForward, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailForward);

        var parameters = new Dictionary<string, object?> { ["mail_forward"] = mailForward };
        return _transport.CallAsync("delete_mailforward", parameters, cancellationToken);
    }
}
