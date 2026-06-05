# KASserver.NET

Unofficial .NET client for the [ALL-INKL.COM](https://all-inkl.com/) **KAS API** (Kunden-Administrations-System) — manage accounts, mailboxes, forwards, domains, DNS, databases, FTP and cronjobs from .NET. Targets .NET 8, 9, and 10.

> Not affiliated with or endorsed by Neue Medien Münnich / ALL-INKL.COM.

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
