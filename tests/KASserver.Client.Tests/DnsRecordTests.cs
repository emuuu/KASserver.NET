using KASserver;

namespace KASserver.Client.Tests;

public class DnsRecordTests
{
    // Field shape verified live against the KAS API (get_dns_settings on famgmbh.de):
    // record_zone / record_name / record_type / record_data / record_aux / record_id /
    // record_changeable / record_deleteable (the response uses record_zone, not zone_host).
    [Test]
    public void FromMap_ExposesConvenienceProperties()
    {
        var record = DnsRecord.FromMap(new Dictionary<string, object?>
        {
            ["record_zone"] = "famgmbh.de",
            ["record_name"] = "",
            ["record_type"] = "MX",
            ["record_data"] = "w021a9f9.kasserver.com.",
            ["record_aux"] = "10",
            ["record_id"] = "115664578",
            ["record_changeable"] = "Y",
            ["record_deleteable"] = "Y",
        });

        Assert.Multiple(() =>
        {
            Assert.That(record.RecordId, Is.EqualTo("115664578"));
            Assert.That(record.RawType, Is.EqualTo("MX"));
            Assert.That(record.Type, Is.EqualTo(DnsRecordType.Mx));
            Assert.That(record.RecordName, Is.EqualTo(string.Empty));
            Assert.That(record.RecordData, Is.EqualTo("w021a9f9.kasserver.com."));
            Assert.That(record.Aux, Is.EqualTo(10));
            Assert.That(record.RecordZone, Is.EqualTo("famgmbh.de"));
            Assert.That(record.Changeable, Is.True);
            Assert.That(record.Deleteable, Is.True);
        });
    }

    // Built-in records report record_id = 0 and are neither changeable nor deleteable (verified live).
    [Test]
    public void FromMap_BuiltInRecord_IsLocked()
    {
        var record = DnsRecord.FromMap(new Dictionary<string, object?>
        {
            ["record_zone"] = "famgmbh.de",
            ["record_name"] = "",
            ["record_type"] = "A",
            ["record_data"] = "85.13.130.218",
            ["record_aux"] = "0",
            ["record_id"] = "0",
            ["record_changeable"] = "N",
            ["record_deleteable"] = "N",
        });

        Assert.Multiple(() =>
        {
            Assert.That(record.RecordId, Is.EqualTo("0"));
            Assert.That(record.Changeable, Is.False);
            Assert.That(record.Deleteable, Is.False);
        });
    }

    [Test]
    public void Type_UnknownRecordType_IsNullButRawTypeKept()
    {
        var record = DnsRecord.FromMap(new Dictionary<string, object?> { ["record_type"] = "DNSKEY" });

        Assert.Multiple(() =>
        {
            Assert.That(record.Type, Is.Null);
            Assert.That(record.RawType, Is.EqualTo("DNSKEY"));
        });
    }

    [Test]
    public void MissingFields_ReadAsNull()
    {
        var record = DnsRecord.FromMap(new Dictionary<string, object?>());

        Assert.Multiple(() =>
        {
            Assert.That(record.RecordId, Is.Null);
            Assert.That(record.Type, Is.Null);
            Assert.That(record.Aux, Is.Null);
        });
    }

    [Test]
    public void Aux_NonNumeric_ReadsAsNull()
    {
        var record = DnsRecord.FromMap(new Dictionary<string, object?> { ["record_aux"] = "n/a" });
        Assert.That(record.Aux, Is.Null);
    }

    [Test]
    public void ExtractRecordId_ScalarReturnInfo_ReturnsId()
    {
        var response = new KasResponse { ReturnInfo = "1234" };
        Assert.That(KasClient.ExtractRecordId(response), Is.EqualTo("1234"));
    }

    [Test]
    public void ExtractRecordId_MapWithRecordId_ReturnsId()
    {
        var response = new KasResponse
        {
            ReturnInfo = new Dictionary<string, object?> { ["record_id"] = "1234" },
        };
        Assert.That(KasClient.ExtractRecordId(response), Is.EqualTo("1234"));
    }

    [Test]
    public void ExtractRecordId_NullReturnInfo_Throws()
    {
        var response = new KasResponse { ReturnInfo = null };
        Assert.Throws<KasApiException>(() => KasClient.ExtractRecordId(response));
    }

    [Test]
    public void ExtractRecordId_WhitespaceScalar_Throws()
    {
        var response = new KasResponse { ReturnInfo = "   " };
        Assert.Throws<KasApiException>(() => KasClient.ExtractRecordId(response));
    }

    [Test]
    public void ExtractRecordId_MapWithoutRecordId_Throws()
    {
        var response = new KasResponse
        {
            ReturnInfo = new Dictionary<string, object?> { ["something_else"] = "x" },
        };
        Assert.Throws<KasApiException>(() => KasClient.ExtractRecordId(response));
    }
}
