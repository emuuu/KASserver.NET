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
- Generic escape hatch `RequestAsync(action, params)` for any KAS action not yet wrapped
- Dependency-injection integration via `AddKasServer(...)`
- Multi-target support for net8.0, net9.0, and net10.0

For releases, see [GitHub Releases](https://github.com/emuuu/KASserver.NET/releases).
