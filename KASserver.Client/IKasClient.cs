namespace KASserver;

/// <summary>
/// High-level client for the ALL-INKL.COM KAS API. Authentication, session handling and flood
/// throttling are handled transparently.
/// </summary>
public interface IKasClient
{
    /// <summary>
    /// Invokes any KAS action directly and returns the parsed response. Use this as an escape hatch
    /// for actions that do not yet have a typed wrapper.
    /// </summary>
    /// <param name="action">The KAS action name, e.g. <c>get_dns_settings</c>.</param>
    /// <param name="parameters">The action parameters.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<KasResponse> RequestAsync(
        string action,
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reads the account settings (<c>get_accountsettings</c>).</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<IReadOnlyDictionary<string, object?>> GetAccountSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>Reads the account resource quotas (<c>get_accountresources</c>).</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<IReadOnlyDictionary<string, object?>> GetAccountResourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>Reads the account settings as a typed model (<c>get_accountsettings</c>).</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<AccountSettings> GetAccountSettingsTypedAsync(CancellationToken cancellationToken = default);

    /// <summary>Reads the account resource quotas as a typed model (<c>get_accountresources</c>).</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<AccountResources> GetAccountResourcesTypedAsync(CancellationToken cancellationToken = default);

    /// <summary>Edits your own account settings (<c>update_accountsettings</c>).</summary>
    /// <param name="changes">The fields to change; unset fields are left unchanged.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task UpdateAccountSettingsAsync(UpdateAccountSettings changes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a subaccount (<c>add_account</c>). Requires a superuser login.
    /// </summary>
    /// <param name="account">The subaccount to create.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The <c>account_login</c> of the created subaccount, as returned by KAS.</returns>
    /// <exception cref="KasApiException">The KAS API returned a fault or did not return an account login.</exception>
    Task<string> AddAccountAsync(AddAccount account, CancellationToken cancellationToken = default);

    /// <summary>Lists the subaccounts (<c>get_accounts</c>). Requires a superuser login.</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<IReadOnlyList<SubAccount>> GetAccountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a single subaccount by its login (<c>get_accounts</c> with an <c>account_login</c> filter).
    /// Requires a superuser login.
    /// </summary>
    /// <param name="accountLogin">The subaccount login (e.g. <c>w01abcde</c>).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The subaccount, or <c>null</c> when no account matches the login.</returns>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<SubAccount?> GetAccountAsync(string accountLogin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edits a subaccount (<c>update_account</c>). Identified by its <c>account_login</c>. Requires a superuser login.
    /// </summary>
    /// <param name="accountLogin">The subaccount login (e.g. <c>w01abcde</c>).</param>
    /// <param name="changes">The fields to change; unset fields are left unchanged.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task UpdateAccountAsync(string accountLogin, UpdateAccount changes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a subaccount (<c>delete_account</c>). Requires a superuser login.
    /// <b>Irreversible</b>: removes the subaccount including all of its resources.
    /// </summary>
    /// <param name="accountLogin">The subaccount login (e.g. <c>w01abcde</c>).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task DeleteAccountAsync(string accountLogin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edits the superuser-controlled settings (SSH access/keys) of a subaccount
    /// (<c>update_superusersettings</c>). May only be executed by the main account.
    /// </summary>
    /// <param name="accountLogin">The subaccount login to edit (e.g. <c>w01abcde</c>).</param>
    /// <param name="changes">The fields to change; unset fields are left unchanged.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task UpdateSuperuserSettingsAsync(string accountLogin, UpdateSuperuserSettings changes, CancellationToken cancellationToken = default);

    /// <summary>Changes ownership of a path (<c>update_chown</c>). Requires a superuser login.</summary>
    /// <param name="chown">The path, new owner and recursion flag.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task UpdateChownAsync(UpdateChown chown, CancellationToken cancellationToken = default);

    /// <summary>Lists the domains on the account (<c>get_domains</c>).</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> GetDomainsAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists the mailboxes on the account (<c>get_mailaccounts</c>).</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<IReadOnlyList<MailAccount>> GetMailAccountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a mailbox (<c>add_mailaccount</c>).
    /// </summary>
    /// <param name="account">The mailbox to create.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The technical mail login generated by KAS (e.g. <c>m07f821c</c>), required for subsequent update/delete calls.</returns>
    /// <exception cref="KasApiException">The KAS API returned a fault or did not return a mail login.</exception>
    Task<string> AddMailAccountAsync(AddMailAccount account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edits a mailbox (<c>update_mailaccount</c>). Identified by the technical <c>mail_login</c>.
    /// </summary>
    /// <param name="mailLogin">The technical mail login (e.g. <c>m07f821c</c>), as returned by <see cref="AddMailAccountAsync"/>.</param>
    /// <param name="changes">The fields to change; unset fields are left unchanged.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task UpdateMailAccountAsync(string mailLogin, UpdateMailAccount changes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes a mailbox password (<c>update_mailaccount</c>) — a convenience wrapper over
    /// <see cref="UpdateMailAccountAsync"/>. Identified by the technical <c>mail_login</c>.
    /// </summary>
    /// <param name="mailLogin">The technical mail login (e.g. <c>m07f821c</c>).</param>
    /// <param name="newPassword">The new mailbox password.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task UpdateMailAccountPasswordAsync(string mailLogin, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>Deletes a mailbox (<c>delete_mailaccount</c>). Identified by the <c>mail_login</c>, not the address.</summary>
    /// <param name="mailLogin">The technical mail login (e.g. <c>m07f821c</c>).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task DeleteMailAccountAsync(string mailLogin, CancellationToken cancellationToken = default);

    /// <summary>Lists the mail forwards on the account (<c>get_mailforwards</c>).</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<IReadOnlyList<MailForward>> GetMailForwardsAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a mail forward (<c>add_mailforward</c>).</summary>
    /// <param name="forward">The forward to create.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task AddMailForwardAsync(AddMailForward forward, CancellationToken cancellationToken = default);

    /// <summary>Deletes a mail forward (<c>delete_mailforward</c>). Identified by the full forwarding address.</summary>
    /// <param name="mailForward">The full forwarding address, e.g. <c>info@example.com</c>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task DeleteMailForwardAsync(string mailForward, CancellationToken cancellationToken = default);
}
