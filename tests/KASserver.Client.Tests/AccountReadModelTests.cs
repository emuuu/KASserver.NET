using KASserver;

namespace KASserver.Client.Tests;

public class AccountReadModelTests
{
    // Field shape verified live against the KAS API (get_accounts, populated entry).
    [Test]
    public void SubAccount_ExposesConvenienceProperties()
    {
        var account = SubAccount.FromMap(new Dictionary<string, object?>
        {
            ["account_login"] = "w021ab88",
            ["account_comment"] = "shop",
            ["account_contact_mail"] = "admin@example.com",
            ["kas_access_forbidden"] = "N",
            // KAS returns the quota under names that differ from the request params:
            ["max_databases"] = "0",
            ["max_mail_list"] = "0",
        });

        Assert.Multiple(() =>
        {
            Assert.That(account.AccountLogin, Is.EqualTo("w021ab88"));
            Assert.That(account.Comment, Is.EqualTo("shop"));
            Assert.That(account.ContactMail, Is.EqualTo("admin@example.com"));
            Assert.That(account.AccessForbidden, Is.EqualTo("N"));
            Assert.That(account.Raw["max_databases"], Is.EqualTo("0"));
        });
    }

    [Test]
    public void SubAccount_MissingField_IsNull()
    {
        var account = SubAccount.FromMap(new Dictionary<string, object?>());
        Assert.That(account.AccountLogin, Is.Null);
    }

    // Field shape verified live against the KAS API (get_accountsettings → inner "settings" map).
    private static AccountSettings BuildSettings() => AccountSettings.FromMap(new Dictionary<string, object?>
    {
        ["account_login"] = "w021a9f9",
        ["account_comment"] = "test.example.com",
        ["account_contact_mail"] = "admin@example.com",
        ["is_superuser"] = "Y",
        ["show_password"] = "N",
        ["logging"] = "keine",
        ["logage"] = "7",
        ["statistic"] = "0",
        ["dns_settings"] = "N",
        ["inst_htaccess"] = "Y",
        ["inst_fpse"] = "Y",
        ["inst_software"] = "Y",
        ["ssh_access"] = "N",
        ["server"] = "dd6006",
    });

    [Test]
    public void AccountSettings_ExposesStringProperties()
    {
        var settings = BuildSettings();

        Assert.Multiple(() =>
        {
            Assert.That(settings.AccountLogin, Is.EqualTo("w021a9f9"));
            Assert.That(settings.Comment, Is.EqualTo("test.example.com"));
            Assert.That(settings.ContactMail, Is.EqualTo("admin@example.com"));
            Assert.That(settings.Logging, Is.EqualTo("keine"));
            Assert.That(settings.LogAge, Is.EqualTo("7"));
            Assert.That(settings.Statistic, Is.EqualTo("0"));
            Assert.That(settings.Server, Is.EqualTo("dd6006"));
        });
    }

    [Test]
    public void AccountSettings_MapsYesNoFlagsToBool()
    {
        var settings = BuildSettings();

        Assert.Multiple(() =>
        {
            Assert.That(settings.IsSuperuser, Is.True);
            Assert.That(settings.InstHtaccess, Is.True);
            Assert.That(settings.InstSoftware, Is.True);
            Assert.That(settings.DnsSettings, Is.False);
            Assert.That(settings.SshAccess, Is.False);
        });
    }

    // Field shape verified live against the KAS API (get_accountresources).
    private static AccountResources BuildResources() => AccountResources.FromMap(new Dictionary<string, object?>
    {
        ["max_webspace"] = new Dictionary<string, object?>
        {
            ["max"] = "2500", ["exceeded"] = "false", ["reserved"] = "0",
            ["created"] = "0", ["used"] = "0", ["free"] = "2500",
        },
        ["max_subdomain"] = new Dictionary<string, object?>
        {
            ["max"] = "5", ["exceeded"] = "false", ["reserved"] = "0",
            ["created"] = "1", ["used"] = "1", ["free"] = "4",
        },
    });

    [Test]
    public void AccountResources_ExposesTypedResource()
    {
        var webspace = BuildResources().Webspace;

        Assert.That(webspace, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(webspace!.Max, Is.EqualTo(2500));
            Assert.That(webspace.Free, Is.EqualTo(2500));
            Assert.That(webspace.Used, Is.EqualTo(0));
            Assert.That(webspace.Exceeded, Is.False);
        });
    }

    [Test]
    public void AccountResources_SubdomainUsage_IsParsed()
    {
        var sub = BuildResources().Subdomains;

        Assert.Multiple(() =>
        {
            Assert.That(sub!.Max, Is.EqualTo(5));
            Assert.That(sub.Used, Is.EqualTo(1));
            Assert.That(sub.Free, Is.EqualTo(4));
            Assert.That(sub.Created, Is.EqualTo(1));
        });
    }

    [Test]
    public void AccountResources_MissingResource_IsNull()
    {
        Assert.That(BuildResources().Databases, Is.Null);
    }

    [Test]
    public void AccountResource_ExceededTrue_IsParsed()
    {
        var resource = AccountResource.FromMap(new Dictionary<string, object?>
        {
            ["max"] = "5", ["used"] = "6", ["exceeded"] = "true",
        });

        Assert.That(resource.Exceeded, Is.True);
    }
}
