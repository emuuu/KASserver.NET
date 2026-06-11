using KASserver.Client.Tests.Fakes;

namespace KASserver.Client.Tests;

/// <summary>
/// Verifies the action name and parameter map that the subdomain wrappers send through the transport
/// seam. Covers the KAS-specific quirks: <c>get_subdomains</c> with/without the <c>subdomain_name</c>
/// filter, <c>add_subdomain</c> returning no id (addressed by host name), and the <c>move_subdomain</c>
/// source/target accounts.
/// </summary>
public class KasClientContractSubdomainTests
{
    [Test]
    public async Task GetSubdomainsAsync_SendsActionAndProjectsList()
    {
        var fake = new RecordingKasTransport().EnqueueList(
            new Dictionary<string, object?> { ["subdomain_name"] = "shop.example.com", ["php_version"] = "8.1" });
        var client = new KasClient(fake);

        var subdomains = await client.GetSubdomainsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_subdomains"));
            Assert.That(fake.LastParameters, Is.Null);
            Assert.That(subdomains[0].SubdomainName, Is.EqualTo("shop.example.com"));
            Assert.That(subdomains[0].PhpVersion, Is.EqualTo("8.1"));
        });
    }

    [Test]
    public async Task GetSubdomainAsync_SendsSubdomainNameFilter()
    {
        var fake = new RecordingKasTransport().EnqueueList(
            new Dictionary<string, object?> { ["subdomain_name"] = "shop.example.com" });
        var client = new KasClient(fake);

        var subdomain = await client.GetSubdomainAsync("shop.example.com");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_subdomains"));
            Assert.That(fake.LastParameters!["subdomain_name"], Is.EqualTo("shop.example.com"));
            Assert.That(subdomain!.SubdomainName, Is.EqualTo("shop.example.com"));
        });
    }

    [Test]
    public async Task AddSubdomainAsync_SendsRequiredFieldsAndExtractsHostName()
    {
        var fake = new RecordingKasTransport().EnqueueScalar("shop.example.com");
        var client = new KasClient(fake);

        var host = await client.AddSubdomainAsync(new AddSubdomain
        {
            SubdomainName = "shop",
            DomainName = "example.com",
        });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("add_subdomain"));
            Assert.That(fake.LastParameters!["subdomain_name"], Is.EqualTo("shop"));
            Assert.That(fake.LastParameters!["domain_name"], Is.EqualTo("example.com"));
            // Every unset optional is omitted entirely, not sent as a default/empty value.
            Assert.That(fake.LastParameters!.ContainsKey("subdomain_path"), Is.False);
            Assert.That(fake.LastParameters!.ContainsKey("redirect_status"), Is.False);
            Assert.That(fake.LastParameters!.ContainsKey("statistic_version"), Is.False);
            Assert.That(fake.LastParameters!.ContainsKey("statistic_language"), Is.False);
            Assert.That(fake.LastParameters!.ContainsKey("php_version"), Is.False);
            // add_subdomain returns the full host name as the ReturnInfo scalar (verified live).
            Assert.That(host, Is.EqualTo("shop.example.com"));
        });
    }

    [Test]
    public async Task AddSubdomainAsync_SendsOptionalFieldsWhenSet()
    {
        var fake = new RecordingKasTransport().EnqueueScalar("shop.example.com");
        var client = new KasClient(fake);

        await client.AddSubdomainAsync(new AddSubdomain
        {
            SubdomainName = "shop",
            DomainName = "example.com",
            SubdomainPath = "http://target.tld",
            Redirect = RedirectStatus.MovedPermanently,
            StatisticVersion = WebalizerVersion.Version7,
            StatisticLanguage = WebalizerLanguage.English,
            PhpVersion = "8.1",
        });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastParameters!["subdomain_path"], Is.EqualTo("http://target.tld"));
            Assert.That(fake.LastParameters!["redirect_status"], Is.EqualTo("301"));
            Assert.That(fake.LastParameters!["statistic_version"], Is.EqualTo("7"));
            Assert.That(fake.LastParameters!["statistic_language"], Is.EqualTo("en"));
            Assert.That(fake.LastParameters!["php_version"], Is.EqualTo("8.1"));
        });
    }

    [Test]
    public async Task UpdateSubdomainAsync_SendsSubdomainNameAndFields()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.UpdateSubdomainAsync("shop.example.com", new UpdateSubdomain { Redirect = RedirectStatus.Found });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("update_subdomain"));
            Assert.That(fake.LastParameters!["subdomain_name"], Is.EqualTo("shop.example.com"));
            Assert.That(fake.LastParameters!["redirect_status"], Is.EqualTo("302"));
            // Unset fields are omitted; update_subdomain never carries the add-only statistic fields.
            Assert.That(fake.LastParameters!.ContainsKey("subdomain_path"), Is.False);
            Assert.That(fake.LastParameters!.ContainsKey("php_version"), Is.False);
            Assert.That(fake.LastParameters!.ContainsKey("statistic_version"), Is.False);
            Assert.That(fake.LastParameters!.ContainsKey("statistic_language"), Is.False);
        });
    }

    [Test]
    public async Task DeleteSubdomainAsync_SendsActionAndSubdomainName()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.DeleteSubdomainAsync("shop.example.com");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("delete_subdomain"));
            Assert.That(fake.LastParameters!["subdomain_name"], Is.EqualTo("shop.example.com"));
        });
    }

    [Test]
    public async Task MoveSubdomainAsync_SendsSourceAndTargetAccounts()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.MoveSubdomainAsync("shop.example.com", "w00aaaaa", "w00bbbbb");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("move_subdomain"));
            Assert.That(fake.LastParameters!["subdomain_name"], Is.EqualTo("shop.example.com"));
            Assert.That(fake.LastParameters!["source_account"], Is.EqualTo("w00aaaaa"));
            Assert.That(fake.LastParameters!["target_account"], Is.EqualTo("w00bbbbb"));
        });
    }
}
