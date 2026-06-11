using KASserver.Client.Tests.Fakes;

namespace KASserver.Client.Tests;

/// <summary>
/// Verifies the action name and parameter map that the DNS wrappers send through the transport seam.
/// Covers the KAS-specific quirks: <c>zone_host</c> trailing-dot normalization, the empty
/// <c>record_name</c> apex, <c>record_aux</c> as a single field, and the absence of <c>record_type</c>
/// on update/get.
/// </summary>
public class KasClientContractDnsTests
{
    [Test]
    public async Task GetDnsRecordsAsync_NormalizesZoneHostAndOmitsRecordType()
    {
        var fake = new RecordingKasTransport().EnqueueList(
            new Dictionary<string, object?> { ["record_id"] = "1", ["record_zone"] = "example.com" });
        var client = new KasClient(fake);

        var records = await client.GetDnsRecordsAsync("example.com");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_dns_settings"));
            Assert.That(fake.LastParameters!["zone_host"], Is.EqualTo("example.com."));
            Assert.That(fake.LastParameters!.ContainsKey("record_type"), Is.False);
            Assert.That(fake.LastParameters!.ContainsKey("nameserver"), Is.False);
            Assert.That(records[0].RecordZone, Is.EqualTo("example.com"));
        });
    }

    [Test]
    public async Task GetDnsRecordsAsync_ZoneHostWithTrailingDot_IsIdempotent()
    {
        var fake = new RecordingKasTransport().EnqueueList();
        var client = new KasClient(fake);

        await client.GetDnsRecordsAsync("example.com.");

        Assert.That(fake.LastParameters!["zone_host"], Is.EqualTo("example.com."));
    }

    [Test]
    public async Task GetDnsRecordAsync_SendsZoneHostAndRecordId()
    {
        var fake = new RecordingKasTransport().EnqueueList(
            new Dictionary<string, object?>
            {
                ["record_id"] = "115664578",
                ["record_type"] = "MX",
                ["record_name"] = "",
                ["record_aux"] = "10",
            });
        var client = new KasClient(fake);

        var record = await client.GetDnsRecordAsync("example.com", "115664578");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("get_dns_settings"));
            Assert.That(fake.LastParameters!["zone_host"], Is.EqualTo("example.com."));
            Assert.That(fake.LastParameters!["record_id"], Is.EqualTo("115664578"));
            Assert.That(record!.Type, Is.EqualTo(DnsRecordType.Mx));
            Assert.That(record.RecordName, Is.EqualTo(string.Empty));
            Assert.That(record.Aux, Is.EqualTo(10));
        });
    }

    [Test]
    public async Task AddDnsRecordAsync_SendsAllFieldsAndExtractsScalarRecordId()
    {
        var fake = new RecordingKasTransport().EnqueueScalar("115664578");
        var client = new KasClient(fake);

        var recordId = await client.AddDnsRecordAsync(new AddDnsRecord
        {
            ZoneHost = "example.com",
            Type = DnsRecordType.A,
            RecordName = "www",
            RecordData = "1.2.3.4",
            Aux = 0,
        });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("add_dns_settings"));
            Assert.That(fake.LastParameters!["zone_host"], Is.EqualTo("example.com."));
            Assert.That(fake.LastParameters!["record_type"], Is.EqualTo("A"));
            Assert.That(fake.LastParameters!["record_name"], Is.EqualTo("www"));
            Assert.That(fake.LastParameters!["record_data"], Is.EqualTo("1.2.3.4"));
            // record_aux is always sent as a single field, defaulting to "0".
            Assert.That(fake.LastParameters!["record_aux"], Is.EqualTo("0"));
            Assert.That(recordId, Is.EqualTo("115664578"));
        });
    }

    [Test]
    public async Task AddDnsRecordAsync_ApexRecordName_IsSentAsEmptyString()
    {
        var fake = new RecordingKasTransport().EnqueueScalar("1");
        var client = new KasClient(fake);

        await client.AddDnsRecordAsync(new AddDnsRecord
        {
            ZoneHost = "example.com",
            Type = DnsRecordType.Mx,
            RecordName = "",
            RecordData = "mail.example.com.",
            Aux = 10,
        });

        Assert.Multiple(() =>
        {
            // The empty apex record_name must be sent, not swallowed.
            Assert.That(fake.LastParameters!.ContainsKey("record_name"), Is.True);
            Assert.That(fake.LastParameters!["record_name"], Is.EqualTo(string.Empty));
            Assert.That(fake.LastParameters!["record_aux"], Is.EqualTo("10"));
        });
    }

    [Test]
    public async Task AddDnsRecordAsync_RawType_IsSentVerbatim()
    {
        var fake = new RecordingKasTransport().EnqueueScalar("1");
        var client = new KasClient(fake);

        await client.AddDnsRecordAsync(new AddDnsRecord
        {
            ZoneHost = "example.com",
            RawType = "DNSKEY",
            RecordName = "",
            RecordData = "data",
        });

        Assert.That(fake.LastParameters!["record_type"], Is.EqualTo("DNSKEY"));
    }

    [Test]
    public async Task UpdateDnsRecordAsync_SendsRecordIdAndFieldsWithoutType()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.UpdateDnsRecordAsync("115664578", new UpdateDnsRecord { RecordData = "5.6.7.8", Aux = 20 });

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("update_dns_settings"));
            Assert.That(fake.LastParameters!["record_id"], Is.EqualTo("115664578"));
            Assert.That(fake.LastParameters!["record_data"], Is.EqualTo("5.6.7.8"));
            Assert.That(fake.LastParameters!["record_aux"], Is.EqualTo("20"));
            Assert.That(fake.LastParameters!.ContainsKey("record_type"), Is.False);
            Assert.That(fake.LastParameters!.ContainsKey("nameserver"), Is.False);
        });
    }

    [Test]
    public async Task DeleteDnsRecordAsync_SendsActionAndRecordId()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.DeleteDnsRecordAsync("115664578");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("delete_dns_settings"));
            Assert.That(fake.LastParameters!["record_id"], Is.EqualTo("115664578"));
        });
    }

    [Test]
    public async Task ResetDnsSettingsAsync_SendsZoneHostOnlyWhenNameserverUnset()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.ResetDnsSettingsAsync("example.com");

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("reset_dns_settings"));
            Assert.That(fake.LastParameters!["zone_host"], Is.EqualTo("example.com."));
            Assert.That(fake.LastParameters!.ContainsKey("nameserver"), Is.False);
        });
    }

    [Test]
    public async Task ResetDnsSettingsAsync_SendsNameserverWhenSet()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.ResetDnsSettingsAsync("example.com", "ns5.kasserver.com");

        Assert.That(fake.LastParameters!["nameserver"], Is.EqualTo("ns5.kasserver.com"));
    }

    [Test]
    public async Task GetDnsRecordsAsync_MapsUnknownTypeToNullButKeepsRawType()
    {
        var fake = new RecordingKasTransport().EnqueueList(
            new Dictionary<string, object?> { ["record_type"] = "DNSKEY" });
        var client = new KasClient(fake);

        var records = await client.GetDnsRecordsAsync("example.com");

        Assert.Multiple(() =>
        {
            Assert.That(records[0].Type, Is.Null);
            Assert.That(records[0].RawType, Is.EqualTo("DNSKEY"));
        });
    }
}
