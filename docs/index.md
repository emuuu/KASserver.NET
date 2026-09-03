# KASserver.NET

Unofficial .NET client for the [ALL-INKL.COM](https://all-inkl.com/) **KAS API** (Kunden-Administrations-System) — manage accounts, mailboxes, forwards, domains, DNS, databases, FTP and cronjobs from .NET. Targets .NET 8, 9, and 10.

> **Unofficial project.** KASserver.NET is an independent community library. It is not affiliated with, endorsed by, or supported by Neue Medien Münnich GmbH (ALL-INKL.COM). See the trademark notice at the bottom of this page.

## Install

```bash
dotnet add package KASserver.NET
```

## Quick start

```csharp
// Program.cs
builder.Services.AddKasServer(options =>
{
    options.Login = "w00XXXXX";
    options.Password = "your-kas-password";
});
```

```csharp
public class MailProvisioning(IKasClient kas)
{
    public async Task Run()
    {
        var login = await kas.AddMailAccountAsync(new AddMailAccount
        {
            LocalPart = "info",
            DomainPart = "example.com",
            Password = "S3cure-Pw!"
        });

        await kas.DeleteMailAccountAsync(login); // delete works on the mail_login, not the address
    }
}
```

The client handles the KAS quirks for you: session authentication, automatic flood throttling
(`KasFloodDelay`), and the raw-SOAP workaround for the faulty KAS WSDL.

## Reference

- **[API Reference](api/index.md)** — generated from the library's XML documentation.
- [Official KAS API documentation](https://kasapi.kasserver.com/dokumentation/phpdoc/)
- [Source on GitHub](https://github.com/emuuu/KASserver.NET)

## Trademarks and disclaimer

KASserver.NET is an independent, community-maintained open-source project. It is not an official ALL-INKL.COM product and is not affiliated with, authorized by, endorsed by, or sponsored by Neue Medien Münnich GmbH (ALL-INKL.COM).

"KAS", "KASserver", "ALL-INKL" and "ALL-INKL.COM" are trademarks or registered trademarks of Neue Medien Münnich GmbH (ALL-INKL.COM). All other product names, logos and brands are the property of their respective owners. They are used in this project, in its documentation and in its package names solely to identify the third-party APIs this library communicates with; such descriptive use implies no endorsement, sponsorship or business relationship.

The library is provided under the MIT license, as-is and without warranty of any kind. It carries no support agreement or service level of any kind from ALL-INKL.COM, and using it does not change your obligations under ALL-INKL.COM's own terms of service — you still need your own ALL-INKL.COM account with KAS API access. Questions about the APIs themselves belong to their respective providers; questions about this library belong in its [issue tracker](https://github.com/emuuu/KASserver.NET/issues).

If you hold rights to any of the marks named above and object to their use here, please open an issue and it will be addressed.
