using KASserver.Client.Tests.Fakes;

namespace KASserver.Client.Tests;

/// <summary>
/// Verifies the action name and parameter map that the account/subaccount/settings/superuser/chown
/// wrappers send through the transport seam, plus how each maps the response back.
/// </summary>
public class KasClientContractAccountTests
{
    private static AddAccount ValidAccount() => new()
    {
        KasPassword = "kas-pw",
        FtpPassword = "ftp-pw",
        HostnameKind = AccountHostnameKind.Domain,
        HostnamePart1 = "example",
        HostnamePart2 = "com",
    };

    [Test]
    public async Task GetAccountSettingsAsync_SendsActionAndUnwrapsSettings()
    {
        var inner = new Dictionary<string, object?> { ["account_login"] = "w01abcde", ["logging"] = "keine" };
        var fake = new RecordingKasTransport().EnqueueMap(new Dictionary<string, object?> { ["settings"] = inner });
        var client = new KasClient(fake);

        var result = await client.GetAccountSettingsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_accountsettings"));
            Assert.That(fake.LastParameters, Is.Null);
            Assert.That(result, Is.SameAs(inner));
        });
    }

    [Test]
    public async Task GetAccountResourcesAsync_SendsActionAndReturnsMap()
    {
        var map = new Dictionary<string, object?> { ["max_webspace"] = "2500" };
        var fake = new RecordingKasTransport().EnqueueMap(map);
        var client = new KasClient(fake);

        var result = await client.GetAccountResourcesAsync();

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_accountresources"));
            Assert.That(fake.LastParameters, Is.Null);
            Assert.That(result["max_webspace"], Is.EqualTo("2500"));
        });
    }

    [Test]
    public async Task GetAccountSettingsTypedAsync_SendsActionAndMapsModel()
    {
        var settings = new Dictionary<string, object?> { ["is_superuser"] = "Y", ["dns_settings"] = "N" };
        var fake = new RecordingKasTransport().EnqueueMap(new Dictionary<string, object?> { ["settings"] = settings });
        var client = new KasClient(fake);

        var result = await client.GetAccountSettingsTypedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_accountsettings"));
            Assert.That(result.IsSuperuser, Is.True);
            Assert.That(result.DnsSettings, Is.False);
        });
    }

    [Test]
    public async Task GetAccountResourcesTypedAsync_SendsActionAndMapsModel()
    {
        var resources = new Dictionary<string, object?>
        {
            ["max_webspace"] = new Dictionary<string, object?>
            {
                ["max"] = "2500", ["exceeded"] = "false", ["reserved"] = "0",
                ["created"] = "0", ["used"] = "0", ["free"] = "2500",
            },
        };
        var fake = new RecordingKasTransport().EnqueueMap(resources);
        var client = new KasClient(fake);

        var result = await client.GetAccountResourcesTypedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_accountresources"));
            Assert.That(result.Webspace, Is.Not.Null);
            Assert.That(result.Webspace!.Max, Is.EqualTo(2500));
        });
    }

    [Test]
    public async Task UpdateAccountSettingsAsync_SendsActionAndParameters()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.UpdateAccountSettingsAsync(new UpdateAccountSettings { Comment = "renamed", ShowPassword = true });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("update_accountsettings"));
            Assert.That(fake.LastParameters!["account_comment"], Is.EqualTo("renamed"));
            Assert.That(fake.LastParameters!["show_password"], Is.EqualTo("Y"));
            Assert.That(fake.LastParameters!.ContainsKey("account_password"), Is.False);
        });
    }

    [Test]
    public async Task AddAccountAsync_SendsActionAndExtractsScalarLogin()
    {
        var fake = new RecordingKasTransport().EnqueueScalar("w0123abc");
        var client = new KasClient(fake);

        var login = await client.AddAccountAsync(ValidAccount());

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("add_account"));
            Assert.That(fake.LastParameters!["hostname_part1"], Is.EqualTo("example"));
            Assert.That(login, Is.EqualTo("w0123abc"));
        });
    }

    [Test]
    public async Task AddAccountAsync_MapReturnInfo_ExtractsWrappedLogin()
    {
        var fake = new RecordingKasTransport().EnqueueMap(new Dictionary<string, object?> { ["account_login"] = "w0999zzz" });
        var client = new KasClient(fake);

        var login = await client.AddAccountAsync(ValidAccount());

        Assert.That(login, Is.EqualTo("w0999zzz"));
    }

    [Test]
    public async Task GetAccountsAsync_SendsActionAndProjectsList()
    {
        var fake = new RecordingKasTransport().EnqueueList(
            new Dictionary<string, object?> { ["account_login"] = "w01aaaaa" },
            new Dictionary<string, object?> { ["account_login"] = "w01bbbbb" });
        var client = new KasClient(fake);

        var accounts = await client.GetAccountsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_accounts"));
            Assert.That(fake.LastParameters, Is.Null);
            Assert.That(accounts.Select(a => a.AccountLogin), Is.EqualTo(new[] { "w01aaaaa", "w01bbbbb" }));
        });
    }

    [Test]
    public async Task GetAccountAsync_SendsLoginFilterAndReturnsFirst()
    {
        var fake = new RecordingKasTransport().EnqueueList(
            new Dictionary<string, object?> { ["account_login"] = "w01aaaaa", ["account_comment"] = "shop" });
        var client = new KasClient(fake);

        var account = await client.GetAccountAsync("w01aaaaa");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_accounts"));
            Assert.That(fake.LastParameters!["account_login"], Is.EqualTo("w01aaaaa"));
            Assert.That(account!.Comment, Is.EqualTo("shop"));
        });
    }

    [Test]
    public async Task GetAccountAsync_EmptyList_ReturnsNull()
    {
        var fake = new RecordingKasTransport().EnqueueList();
        var client = new KasClient(fake);

        var account = await client.GetAccountAsync("w01aaaaa");

        Assert.That(account, Is.Null);
    }

    [Test]
    public async Task UpdateAccountAsync_SendsActionLoginAndChangedFields()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.UpdateAccountAsync("w01abcde", new UpdateAccount { Comment = "renamed", DnsSettings = true });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("update_account"));
            Assert.That(fake.LastParameters!["account_login"], Is.EqualTo("w01abcde"));
            Assert.That(fake.LastParameters!["account_comment"], Is.EqualTo("renamed"));
            Assert.That(fake.LastParameters!["dns_settings"], Is.EqualTo("Y"));
        });
    }

    [Test]
    [TestCase(AccountAccessState.Allowed, "N")]
    [TestCase(AccountAccessState.Forbidden, "Y")]
    [TestCase(AccountAccessState.ForbiddenExplicit, "forbidden")]
    public async Task UpdateAccountAsync_AccessForbidden_SendsTriStateValue(AccountAccessState state, string expected)
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.UpdateAccountAsync("w01abcde", new UpdateAccount { AccessForbidden = state });

        Assert.That(fake.LastParameters!["kas_access_forbidden"], Is.EqualTo(expected));
    }

    [Test]
    public async Task DeleteAccountAsync_SendsActionAndLogin()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.DeleteAccountAsync("w01abcde");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("delete_account"));
            Assert.That(fake.LastParameters!["account_login"], Is.EqualTo("w01abcde"));
        });
    }

    [Test]
    public async Task UpdateSuperuserSettingsAsync_SendsActionLoginAndFields()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.UpdateSuperuserSettingsAsync("w01abcde", new UpdateSuperuserSettings { SshAccess = true });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("update_superusersettings"));
            Assert.That(fake.LastParameters!["account_login"], Is.EqualTo("w01abcde"));
            Assert.That(fake.LastParameters!["ssh_access"], Is.EqualTo("Y"));
        });
    }

    [Test]
    public async Task UpdateChownAsync_SendsActionAndParameters()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.UpdateChownAsync(new UpdateChown { Path = "/www/htdocs", User = "ssh-user", Recursive = true });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("update_chown"));
            Assert.That(fake.LastParameters!["chown_path"], Is.EqualTo("/www/htdocs"));
            Assert.That(fake.LastParameters!["chown_user"], Is.EqualTo("ssh-user"));
            Assert.That(fake.LastParameters!["recursive"], Is.EqualTo("Y"));
        });
    }

    [Test]
    public async Task GetDomainsAsync_SendsActionAndReturnsRawMaps()
    {
        var fake = new RecordingKasTransport().EnqueueList(
            new Dictionary<string, object?> { ["domain_name"] = "example.com" });
        var client = new KasClient(fake);

        var domains = await client.GetDomainsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_domains"));
            Assert.That(fake.LastParameters, Is.Null);
            Assert.That(domains[0]["domain_name"], Is.EqualTo("example.com"));
        });
    }
}
