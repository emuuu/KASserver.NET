# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Raw-SOAP transport for the KAS API that works around the faulty `Result`/`return` WSDL declaration
- Session authentication (`plain` login → session token) with automatic reuse and refresh
- Automatic flood throttling based on the `KasFloodDelay` returned by each call
- `Map`/`Array` response parser into plain .NET objects
- Mailbox actions: `GetMailAccountsAsync`, `AddMailAccountAsync`, `UpdateMailAccountAsync`, `DeleteMailAccountAsync`
- Mail-forward actions: `GetMailForwardsAsync`, `AddMailForwardAsync`, `DeleteMailForwardAsync`
- Account read actions: `GetAccountSettingsAsync`, `GetAccountResourcesAsync`, `GetDomainsAsync`
- Typed account read actions: `GetAccountSettingsTypedAsync`, `GetAccountResourcesTypedAsync`
- Subaccount actions: `AddAccountAsync`, `GetAccountsAsync`, `GetAccountAsync`, `UpdateAccountAsync`, `DeleteAccountAsync`
- Account settings/superuser actions: `UpdateAccountSettingsAsync`, `UpdateSuperuserSettingsAsync`, `UpdateChownAsync`
- DNS zone-record actions: `GetDnsRecordsAsync`, `GetDnsRecordAsync`, `AddDnsRecordAsync`, `UpdateDnsRecordAsync`, `DeleteDnsRecordAsync`, `ResetDnsSettingsAsync` — with the `DnsRecord` read model, the `DnsRecordType` enum plus a raw-type escape, and automatic `zone_host` trailing-dot normalization. Note: `ResetDnsSettingsAsync` is destructive — it discards all custom records of the zone
- DynDNS user actions: `GetDynDnsUsersAsync`, `GetDynDnsUserAsync`, `AddDynDnsUserAsync`, `UpdateDynDnsUserAsync`, `DeleteDynDnsUserAsync` — with the `DynDnsUser` read model (`add_ddnsuser`/`get_ddnsusers`/`update_ddnsuser`/`delete_ddnsuser`)
- Subdomain actions: `GetSubdomainsAsync`, `GetSubdomainAsync`, `AddSubdomainAsync`, `UpdateSubdomainAsync`, `DeleteSubdomainAsync`, `MoveSubdomainAsync` — with the `Subdomain` read model and the `RedirectStatus`/`WebalizerVersion`/`WebalizerLanguage` enums (`add_subdomain`/`get_subdomains`/`update_subdomain`/`delete_subdomain`/`move_subdomain`). `AddSubdomainAsync` returns the created host name. Note: KAS provisions a new subdomain asynchronously (`in_progress = TRUE`) and rejects updates until that clears
- Generic escape hatch `RequestAsync(action, params)` for any KAS action not yet wrapped
- Dependency-injection integration via `AddKasServer(...)`
- Multi-target support for net8.0, net9.0, and net10.0

For releases, see [GitHub Releases](https://github.com/emuuu/KASserver.NET/releases).
