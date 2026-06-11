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

    /// <summary>
    /// Lists the DNS resource records of a zone (<c>get_dns_settings</c>). A missing trailing dot on
    /// <paramref name="zoneHost"/> is added automatically.
    /// </summary>
    /// <param name="zoneHost">The DNS zone, e.g. <c>example.com</c> (or <c>example.com.</c>).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The resource records of the zone.</returns>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<IReadOnlyList<DnsRecord>> GetDnsRecordsAsync(string zoneHost, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a single DNS resource record by its id (<c>get_dns_settings</c> with a <c>record_id</c>
    /// filter). A missing trailing dot on <paramref name="zoneHost"/> is added automatically.
    /// </summary>
    /// <param name="zoneHost">The DNS zone the record belongs to, e.g. <c>example.com</c>.</param>
    /// <param name="recordId">The technical record id to look up.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The matching record, or <c>null</c> when no record matches the id.</returns>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<DnsRecord?> GetDnsRecordAsync(string zoneHost, string recordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a DNS resource record (<c>add_dns_settings</c>).
    /// </summary>
    /// <remarks>
    /// Requires the account's DNS-settings permission; without it KAS faults with
    /// <c>dns_settings_not_allowed</c>. The same applies to <see cref="UpdateDnsRecordAsync"/>,
    /// <see cref="DeleteDnsRecordAsync"/> and <see cref="ResetDnsSettingsAsync"/>.
    /// </remarks>
    /// <param name="record">The record to create.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The technical <c>record_id</c> generated by KAS, required for subsequent update/delete calls.</returns>
    /// <exception cref="KasApiException">The KAS API returned a fault or did not return a record id.</exception>
    Task<string> AddDnsRecordAsync(AddDnsRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edits a DNS resource record (<c>update_dns_settings</c>). Identified by its <c>record_id</c>.
    /// The record type cannot be changed via this action.
    /// </summary>
    /// <param name="recordId">The technical record id, as returned by <see cref="AddDnsRecordAsync"/> or <see cref="GetDnsRecordsAsync"/>.</param>
    /// <param name="changes">The fields to change; unset fields are left unchanged.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task UpdateDnsRecordAsync(string recordId, UpdateDnsRecord changes, CancellationToken cancellationToken = default);

    /// <summary>Deletes a DNS resource record (<c>delete_dns_settings</c>). Identified by its <c>record_id</c>.</summary>
    /// <param name="recordId">The technical record id, as returned by <see cref="AddDnsRecordAsync"/> or <see cref="GetDnsRecordsAsync"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task DeleteDnsRecordAsync(string recordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the DNS settings of a whole zone to the KAS defaults (<c>reset_dns_settings</c>).
    /// <b>Destructive</b>: discards all custom resource records of the zone. A missing trailing dot on
    /// <paramref name="zoneHost"/> is added automatically.
    /// </summary>
    /// <param name="zoneHost">The DNS zone to reset, e.g. <c>example.com</c>.</param>
    /// <param name="nameserver">The name server to apply (<c>nameserver</c>); when <c>null</c> the KAS default <c>ns5.kasserver.com</c> is used.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task ResetDnsSettingsAsync(string zoneHost, string? nameserver = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a DynDNS user (<c>add_ddnsuser</c>) that can update the A/AAAA record of
    /// <c>{Label}.{Zone}</c> via DynDNS.
    /// </summary>
    /// <param name="user">The DynDNS user to create.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The technical DynDNS login generated by KAS, required for subsequent update/delete calls.</returns>
    /// <exception cref="KasApiException">The KAS API returned a fault or did not return a DynDNS login.</exception>
    Task<string> AddDynDnsUserAsync(AddDynDnsUser user, CancellationToken cancellationToken = default);

    /// <summary>Lists the DynDNS users on the account (<c>get_ddnsusers</c>).</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The DynDNS users on the account.</returns>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<IReadOnlyList<DynDnsUser>> GetDynDnsUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a single DynDNS user by its login (<c>get_ddnsusers</c> with a <c>ddns_login</c> filter).
    /// </summary>
    /// <param name="dyndnsLogin">The DynDNS login.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The DynDNS user, or <c>null</c> when no user matches the login.</returns>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<DynDnsUser?> GetDynDnsUserAsync(string dyndnsLogin, CancellationToken cancellationToken = default);

    /// <summary>Edits a DynDNS user (<c>update_ddnsuser</c>). Identified by its <c>dyndns_login</c>.</summary>
    /// <param name="dyndnsLogin">The DynDNS login, as returned by <see cref="AddDynDnsUserAsync"/> or <see cref="GetDynDnsUsersAsync"/>.</param>
    /// <param name="changes">The fields to change; unset fields are left unchanged.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task UpdateDynDnsUserAsync(string dyndnsLogin, UpdateDynDnsUser changes, CancellationToken cancellationToken = default);

    /// <summary>Deletes a DynDNS user (<c>delete_ddnsuser</c>). Identified by its <c>dyndns_login</c>.</summary>
    /// <param name="dyndnsLogin">The DynDNS login.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task DeleteDynDnsUserAsync(string dyndnsLogin, CancellationToken cancellationToken = default);

    /// <summary>Lists the subdomains on the account (<c>get_subdomains</c>).</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The subdomains on the account.</returns>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<IReadOnlyList<Subdomain>> GetSubdomainsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a single subdomain by its host name (<c>get_subdomains</c> with a <c>subdomain_name</c> filter).
    /// </summary>
    /// <param name="subdomainName">The subdomain host name, e.g. <c>shop.example.com</c>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The matching subdomain, or <c>null</c> when no subdomain matches the name.</returns>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task<Subdomain?> GetSubdomainAsync(string subdomainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a subdomain (<c>add_subdomain</c>). The new host is <c>{SubdomainName}.{DomainName}</c>.
    /// </summary>
    /// <remarks>
    /// KAS provisions the subdomain asynchronously: it reports <c>in_progress = TRUE</c> for a short
    /// while after creation, and <see cref="UpdateSubdomainAsync"/> is rejected with an
    /// <c>in_progress</c> fault until that clears.
    /// </remarks>
    /// <param name="subdomain">The subdomain to create.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The full host name of the created subdomain (<c>{SubdomainName}.{DomainName}</c>), used to address it in subsequent update/delete/get calls.</returns>
    /// <exception cref="KasApiException">The KAS API returned a fault or did not return a subdomain name.</exception>
    Task<string> AddSubdomainAsync(AddSubdomain subdomain, CancellationToken cancellationToken = default);

    /// <summary>Edits a subdomain (<c>update_subdomain</c>). Identified by its host name.</summary>
    /// <param name="subdomainName">The subdomain host name, e.g. <c>shop.example.com</c>.</param>
    /// <param name="changes">The fields to change; unset fields are left unchanged.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task UpdateSubdomainAsync(string subdomainName, UpdateSubdomain changes, CancellationToken cancellationToken = default);

    /// <summary>Deletes a subdomain (<c>delete_subdomain</c>). Identified by its host name.</summary>
    /// <param name="subdomainName">The subdomain host name, e.g. <c>shop.example.com</c>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task DeleteSubdomainAsync(string subdomainName, CancellationToken cancellationToken = default);

    /// <summary>Moves a subdomain between accounts (<c>move_subdomain</c>). Identified by its host name.</summary>
    /// <param name="subdomainName">The subdomain host name, e.g. <c>shop.example.com</c>.</param>
    /// <param name="sourceAccount">The source account the subdomain currently belongs to (<c>source_account</c>).</param>
    /// <param name="targetAccount">The target account the subdomain is moved to (<c>target_account</c>).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="KasApiException">The KAS API returned a fault or an uninterpretable response.</exception>
    Task MoveSubdomainAsync(string subdomainName, string sourceAccount, string targetAccount, CancellationToken cancellationToken = default);
}
