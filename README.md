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

## Status

Early scaffold. Implemented: authentication, session handling, automatic flood throttling, raw-SOAP transport with `Map` parsing, and the mailbox/forward read & write actions. The remaining KAS actions (DNS, databases, FTP, cronjobs, …) follow the same `RequestAsync(action, params)` mechanism and are being added incrementally.

## Documentation

- **API reference (GitHub Pages):** https://emuuu.github.io/KASserver.NET/
- KAS API reference: https://kasapi.kasserver.com/dokumentation/phpdoc/

## License

MIT
