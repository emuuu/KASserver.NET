# KASserver.NET

Unofficial .NET client for the [ALL-INKL.COM](https://all-inkl.com/) **KAS API** (Kunden-Administrations-System).

The KAS API is SOAP 1.1 and ships a faulty WSDL (response element declared `Result`, server returns `return`) that breaks strict SOAP/WCF clients. KASserver.NET talks raw SOAP and parses the response itself, and additionally handles KAS session authentication and the `KasFloodDelay` rate limit automatically.

```csharp
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

Account management (subaccounts, account/superuser settings, ownership) is available too — superuser-only actions require the main account:

```csharp
public class AccountProvisioning(IKasClient kas)
{
    public async Task Run()
    {
        var login = await kas.AddAccountAsync(new AddAccount
        {
            KasPassword = "Kas-Pw!",
            FtpPassword = "Ftp-Pw!",
            HostnameKind = AccountHostnameKind.Domain,
            HostnamePart1 = "example",
            HostnamePart2 = "com",
            Quota = new AccountQuota { MaxDomain = 1, MaxWebspace = 1024 },
        });

        await kas.UpdateSuperuserSettingsAsync(login, new UpdateSuperuserSettings { SshAccess = true });
        await kas.DeleteAccountAsync(login); // irreversible: removes all of the subaccount's resources
    }
}
```

> Not affiliated with or endorsed by Neue Medien Münnich / ALL-INKL.COM.

Full docs: https://github.com/emuuu/KASserver.NET
