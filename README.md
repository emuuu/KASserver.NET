<h1 align="center">KASserver.NET</h1>

.NET client for the [ALL-INKL.COM](https://all-inkl.com/) **KAS API** (Kunden-Administrations-System) — manage accounts, mailboxes, forwards, domains, DNS, databases, FTP and cronjobs from .NET. Targets .NET 8, 9, and 10.

[![NuGet](https://img.shields.io/nuget/v/KASserver.NET.svg?label=KASserver.NET)](https://www.nuget.org/packages/KASserver.NET)
[![CI](https://github.com/emuuu/KASserver.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/emuuu/KASserver.NET/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Docs](https://img.shields.io/badge/Docs-GitHub%20Pages-blue)](https://emuuu.github.io/KASserver.NET/)

> **Unofficial** community library. Not affiliated with or endorsed by Neue Medien Münnich / ALL-INKL.COM.

## Why

The KAS API is SOAP 1.1 and ships a **faulty WSDL** (the response element is declared `Result` but the server returns `return`), which makes strict SOAP clients — including WCF/`System.ServiceModel` — fail to deserialize responses. KASserver.NET sidesteps this by talking raw SOAP and parsing the Apache `Map` response itself, so you get a clean typed .NET API.

It also handles the two operational quirks of the KAS API for you:

- **Session auth** — `plain` login is exchanged for a session token that is reused and refreshed automatically.
- **Flood limiting** — every response carries a `KasFloodDelay`; the client honors it automatically so bulk operations don't trip the rate limit.

## Prerequisites

- An ALL-INKL.COM account with KAS API access (login `w00XXXXX` + password)
- .NET 8.0, 9.0, or 10.0

## Installation

```bash
dotnet add package KASserver.NET
```

## Quick Start

```csharp
// Program.cs — register the client
builder.Services.AddKasServer(options =>
{
    options.Login = "w00XXXXX";
    options.Password = "your-kas-password";
});
```

```csharp
// Inject and use
public class MailProvisioning(IKasClient kas)
{
    public async Task Example()
    {
        // List existing mailboxes
        var mailboxes = await kas.GetMailAccountsAsync();

        // Create a mailbox (KAS generates the technical mail_login, e.g. "m07f821c")
        var login = await kas.AddMailAccountAsync(new AddMailAccount
        {
            LocalPart = "info",
            DomainPart = "example.com",
            Password = "S3cure-Pw!"
        });

        // Delete it again — note: delete/update work on the mail_login, not the address
        await kas.DeleteMailAccountAsync(login);
    }
}
```

### Subaccounts (superuser only)

```csharp
public class AccountProvisioning(IKasClient kas)
{
    public async Task Example()
    {
        // Create a subaccount with an explicit quota
        var login = await kas.AddAccountAsync(new AddAccount
        {
            KasPassword = "Kas-Pw!",
            FtpPassword = "Ftp-Pw!",
            HostnameKind = AccountHostnameKind.Domain,
            HostnamePart1 = "example",
            HostnamePart2 = "com",
            Quota = new AccountQuota { MaxDomain = 1, MaxWebspace = 1024, MaxMailAccount = 25 },
        });

        // List subaccounts, then raise a single quota
        var accounts = await kas.GetAccountsAsync();
        await kas.UpdateAccountAsync(login, new UpdateAccount
        {
            Quota = new AccountQuota { MaxWebspace = 2048 },
        });

        // Enable SSH access (update_superusersettings)
        await kas.UpdateSuperuserSettingsAsync(login, new UpdateSuperuserSettings { SshAccess = true });

        // Remove the subaccount including all of its resources (irreversible)
        await kas.DeleteAccountAsync(login);
    }
}
```

### DNS records

```csharp
public class DnsProvisioning(IKasClient kas)
{
    public async Task Example()
    {
        // List the zone's resource records (zone_host is normalized to a trailing dot for you).
        // Built-in records report RecordId "0" and are neither Changeable nor Deleteable.
        var records = await kas.GetDnsRecordsAsync("example.com");

        // Add a TXT record (KAS generates the technical record_id, required for update/delete).
        // Use a known type via DnsRecordType, or AddDnsRecord.RawType for a type not in the enum.
        var recordId = await kas.AddDnsRecordAsync(new AddDnsRecord
        {
            ZoneHost = "example.com",
            Type = DnsRecordType.Txt,
            RecordName = "_acme-challenge",
            RecordData = "token-value",
            // Aux defaults to 0; set it as the priority for MX/SRV records.
        });

        // Change its data, then remove it again
        await kas.UpdateDnsRecordAsync(recordId, new UpdateDnsRecord { RecordData = "new-token" });
        await kas.DeleteDnsRecordAsync(recordId);
    }
}
```

> DNS write actions require the account's DNS-settings permission; without it KAS faults with `dns_settings_not_allowed`. `ResetDnsSettingsAsync` is destructive — it discards all custom records of the zone.

### DynDNS users

```csharp
public class DynDnsProvisioning(IKasClient kas)
{
    public async Task Example()
    {
        // Create a DynDNS user for home.example.com (KAS generates the technical dyndns_login)
        var login = await kas.AddDynDnsUserAsync(new AddDynDnsUser
        {
            Comment = "home router",
            Password = "Dyn-Pw!",
            Zone = "example.com",
            Label = "home",
            TargetIp = "203.0.113.10",
            DualStack = true, // update both IPv4 and IPv6
        });

        // List users, change the comment, then remove it again (update/delete work on the dyndns_login)
        var users = await kas.GetDynDnsUsersAsync();
        await kas.UpdateDynDnsUserAsync(login, new UpdateDynDnsUser { Comment = "office router" });
        await kas.DeleteDynDnsUserAsync(login);
    }
}
```

### Subdomains

```csharp
public class SubdomainProvisioning(IKasClient kas)
{
    public async Task Example()
    {
        // Create shop.example.com (returns the full host name, used to address it afterwards)
        var host = await kas.AddSubdomainAsync(new AddSubdomain
        {
            SubdomainName = "shop",
            DomainName = "example.com",
            SubdomainPath = "/shop/",
            PhpVersion = "8.3", // optional; KAS defaults apply when omitted
        });

        // Turn it into a 301 redirect to another host. Note: a freshly created subdomain is still
        // provisioning (Subdomain.InProgress) and rejects updates until that clears — in real code,
        // poll GetSubdomainAsync(host) until !InProgress before updating (see the note below).
        await kas.UpdateSubdomainAsync(host, new UpdateSubdomain
        {
            SubdomainPath = "https://www.example.org",
            Redirect = RedirectStatus.MovedPermanently,
        });

        // List subdomains, then remove it again (update/delete/get work on the host name)
        var subdomains = await kas.GetSubdomainsAsync();
        await kas.DeleteSubdomainAsync(host);
    }
}
```

> KAS provisions a new subdomain asynchronously: it reports `Subdomain.InProgress == true` for a short while after creation, and `UpdateSubdomainAsync` faults with `in_progress` until that clears.

## Status

Early scaffold. Implemented: authentication, session handling, automatic flood throttling, raw-SOAP transport with `Map` parsing, the mailbox/forward read & write actions, the account-management actions (subaccounts, account/superuser settings, ownership), the DNS zone-record actions, the DynDNS user actions, and the subdomain actions. The remaining KAS actions (databases, FTP, cronjobs, …) follow the same `RequestAsync(action, params)` mechanism and are being added incrementally.

## Documentation

- **API reference (GitHub Pages):** https://emuuu.github.io/KASserver.NET/
- KAS API reference: https://kasapi.kasserver.com/dokumentation/phpdoc/

## License

MIT
