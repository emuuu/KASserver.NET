using KASserver.Client.Tests.Fakes;

namespace KASserver.Client.Tests;

/// <summary>
/// Verifies the action name and parameter map that the mailbox/forward wrappers send through the
/// transport seam, plus how each maps the response back.
/// </summary>
public class KasClientContractMailTests
{
    [Test]
    public async Task GetMailAccountsAsync_SendsActionAndProjectsList()
    {
        var fake = new RecordingKasTransport().EnqueueList(
            new Dictionary<string, object?> { ["mail_login"] = "m07f821c", ["mail_adresses"] = "info@example.com" });
        var client = new KasClient(fake);

        var accounts = await client.GetMailAccountsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_mailaccounts"));
            Assert.That(fake.LastParameters, Is.Null);
            Assert.That(accounts[0].MailLogin, Is.EqualTo("m07f821c"));
            Assert.That(accounts[0].Addresses, Is.EqualTo("info@example.com"));
        });
    }

    [Test]
    public async Task AddMailAccountAsync_SendsActionDefaultsAndExtractsScalarLogin()
    {
        var fake = new RecordingKasTransport().EnqueueScalar("m07f821c");
        var client = new KasClient(fake);

        var login = await client.AddMailAccountAsync(new AddMailAccount
        {
            LocalPart = "info",
            DomainPart = "example.com",
            Password = "pw",
        });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("add_mailaccount"));
            Assert.That(fake.LastParameters!["local_part"], Is.EqualTo("info"));
            Assert.That(fake.LastParameters!["domain_part"], Is.EqualTo("example.com"));
            Assert.That(fake.LastParameters!["mail_password"], Is.EqualTo("pw"));
            Assert.That(fake.LastParameters!["webmail_autologin"], Is.EqualTo("Y"));
            // Optional fields are omitted when unset.
            Assert.That(fake.LastParameters!.ContainsKey("copy_adress"), Is.False);
            Assert.That(fake.LastParameters!.ContainsKey("mail_sender_alias"), Is.False);
            Assert.That(fake.LastParameters!.ContainsKey("mail_allow_nets"), Is.False);
            Assert.That(login, Is.EqualTo("m07f821c"));
        });
    }

    [Test]
    public async Task AddMailAccountAsync_OptionalFields_OnlySentWhenSet()
    {
        var fake = new RecordingKasTransport().EnqueueScalar("m07f821c");
        var client = new KasClient(fake);

        await client.AddMailAccountAsync(new AddMailAccount
        {
            LocalPart = "info",
            DomainPart = "example.com",
            Password = "pw",
            WebmailAutologin = false,
            CopyAddress = "copy@example.com",
            SenderAlias = "alias@example.com",
            AllowNets = "webmail",
        });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastParameters!["webmail_autologin"], Is.EqualTo("N"));
            Assert.That(fake.LastParameters!["copy_adress"], Is.EqualTo("copy@example.com"));
            Assert.That(fake.LastParameters!["mail_sender_alias"], Is.EqualTo("alias@example.com"));
            Assert.That(fake.LastParameters!["mail_allow_nets"], Is.EqualTo("webmail"));
        });
    }

    [Test]
    public async Task UpdateMailAccountAsync_SendsActionLoginAndFields()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.UpdateMailAccountAsync("m07f821c", new UpdateMailAccount { IsActive = MailboxActiveState.Active });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("update_mailaccount"));
            Assert.That(fake.LastParameters!["mail_login"], Is.EqualTo("m07f821c"));
            Assert.That(fake.LastParameters!["is_active"], Is.EqualTo("Y"));
        });
    }

    [Test]
    public async Task UpdateMailAccountPasswordAsync_SendsUpdateWithLoginAndNewPassword()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.UpdateMailAccountPasswordAsync("m07f821c", "new-pw");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("update_mailaccount"));
            Assert.That(fake.LastParameters!["mail_login"], Is.EqualTo("m07f821c"));
            Assert.That(fake.LastParameters!["mail_new_password"], Is.EqualTo("new-pw"));
        });
    }

    [Test]
    public async Task DeleteMailAccountAsync_SendsActionAndLogin()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.DeleteMailAccountAsync("m07f821c");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("delete_mailaccount"));
            Assert.That(fake.LastParameters!["mail_login"], Is.EqualTo("m07f821c"));
        });
    }

    [Test]
    public async Task GetMailForwardsAsync_SendsActionAndProjectsList()
    {
        var fake = new RecordingKasTransport().EnqueueList(
            new Dictionary<string, object?>
            {
                ["mail_forward_address"] = "info@example.com",
                ["mail_forward_targets"] = "a@x.de,b@y.de",
            });
        var client = new KasClient(fake);

        var forwards = await client.GetMailForwardsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_mailforwards"));
            Assert.That(fake.LastParameters, Is.Null);
            Assert.That(forwards[0].Address, Is.EqualTo("info@example.com"));
            Assert.That(forwards[0].TargetAddresses, Is.EqualTo(new[] { "a@x.de", "b@y.de" }));
        });
    }

    [Test]
    public async Task AddMailForwardAsync_SendsActionAndTargets()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.AddMailForwardAsync(new AddMailForward
        {
            LocalPart = "info",
            DomainPart = "example.com",
            Targets = new[] { "a@x.de", "b@y.de" },
        });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("add_mailforward"));
            Assert.That(fake.LastParameters!["local_part"], Is.EqualTo("info"));
            Assert.That(fake.LastParameters!["domain_part"], Is.EqualTo("example.com"));
            Assert.That(fake.LastParameters!["target_0"], Is.EqualTo("a@x.de"));
            Assert.That(fake.LastParameters!["target_1"], Is.EqualTo("b@y.de"));
        });
    }

    [Test]
    public async Task DeleteMailForwardAsync_SendsActionAndForward()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.DeleteMailForwardAsync("info@example.com");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("delete_mailforward"));
            Assert.That(fake.LastParameters!["mail_forward"], Is.EqualTo("info@example.com"));
        });
    }
}
