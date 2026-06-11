using KASserver.Client.Tests.Fakes;

namespace KASserver.Client.Tests;

/// <summary>
/// Verifies the action name and parameter map that the DynDNS wrappers send through the transport
/// seam. Covers the login field-name quirk: <c>get_ddnsusers</c> filters on <c>ddns_login</c> while
/// update/delete use <c>dyndns_login</c>, and the add response scalar is the generated login.
/// </summary>
public class KasClientContractDynDnsTests
{
    [Test]
    public async Task AddDynDnsUserAsync_SendsActionFieldsAndExtractsScalarLogin()
    {
        var fake = new RecordingKasTransport().EnqueueScalar("dd12abc");
        var client = new KasClient(fake);

        var login = await client.AddDynDnsUserAsync(new AddDynDnsUser
        {
            Comment = "home",
            Password = "pw",
            Zone = "example.com",
            Label = "home",
            TargetIp = "1.2.3.4",
        });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("add_ddnsuser"));
            Assert.That(fake.LastParameters!["dyndns_zone"], Is.EqualTo("example.com"));
            Assert.That(fake.LastParameters!["dyndns_label"], Is.EqualTo("home"));
            Assert.That(fake.LastParameters!["dyndns_target_ip"], Is.EqualTo("1.2.3.4"));
            // dyndns_dual_stack is optional and omitted when unset.
            Assert.That(fake.LastParameters!.ContainsKey("dyndns_dual_stack"), Is.False);
            Assert.That(login, Is.EqualTo("dd12abc"));
        });
    }

    [Test]
    public async Task AddDynDnsUserAsync_MapReturnInfo_ExtractsDdnsLoginFallback()
    {
        var fake = new RecordingKasTransport().EnqueueMap(new Dictionary<string, object?> { ["ddns_login"] = "dd99zzz" });
        var client = new KasClient(fake);

        var login = await client.AddDynDnsUserAsync(new AddDynDnsUser
        {
            Comment = "home",
            Password = "pw",
            Zone = "example.com",
            Label = "home",
            TargetIp = "1.2.3.4",
        });

        Assert.That(login, Is.EqualTo("dd99zzz"));
    }

    [Test]
    public async Task GetDynDnsUsersAsync_SendsActionAndProjectsList()
    {
        var fake = new RecordingKasTransport().EnqueueList(
            new Dictionary<string, object?> { ["dyndns_login"] = "dd12abc", ["dyndns_zone"] = "example.com" });
        var client = new KasClient(fake);

        var users = await client.GetDynDnsUsersAsync();

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_ddnsusers"));
            Assert.That(fake.LastParameters, Is.Null);
            Assert.That(users[0].Login, Is.EqualTo("dd12abc"));
            Assert.That(users[0].Zone, Is.EqualTo("example.com"));
        });
    }

    [Test]
    public async Task GetDynDnsUserAsync_FiltersOnDdnsLogin()
    {
        var fake = new RecordingKasTransport().EnqueueList(
            new Dictionary<string, object?> { ["dyndns_login"] = "dd12abc" });
        var client = new KasClient(fake);

        var user = await client.GetDynDnsUserAsync("dd12abc");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_ddnsusers"));
            // get_ddnsusers filters on ddns_login (not dyndns_login like update/delete).
            Assert.That(fake.LastParameters!["ddns_login"], Is.EqualTo("dd12abc"));
            Assert.That(fake.LastParameters!.ContainsKey("dyndns_login"), Is.False);
            Assert.That(user!.Login, Is.EqualTo("dd12abc"));
        });
    }

    [Test]
    public async Task UpdateDynDnsUserAsync_FiltersOnDyndnsLoginAndSendsFields()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.UpdateDynDnsUserAsync("dd12abc", new UpdateDynDnsUser { Comment = "renamed", DualStack = true });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("update_ddnsuser"));
            Assert.That(fake.LastParameters!["dyndns_login"], Is.EqualTo("dd12abc"));
            Assert.That(fake.LastParameters!["dyndns_comment"], Is.EqualTo("renamed"));
            Assert.That(fake.LastParameters!["dyndns_dual_stack"], Is.EqualTo("Y"));
        });
    }

    [Test]
    public async Task DeleteDynDnsUserAsync_SendsActionAndDyndnsLogin()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.DeleteDynDnsUserAsync("dd12abc");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("delete_ddnsuser"));
            Assert.That(fake.LastParameters!["dyndns_login"], Is.EqualTo("dd12abc"));
        });
    }
}
