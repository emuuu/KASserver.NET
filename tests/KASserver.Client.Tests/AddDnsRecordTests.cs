using KASserver;

namespace KASserver.Client.Tests;

public class AddDnsRecordTests
{
    private static AddDnsRecord Valid() => new()
    {
        ZoneHost = "example.com",
        Type = DnsRecordType.A,
        RecordName = "www",
        RecordData = "192.0.2.1",
    };

    [Test]
    public void ToParameters_RequiredFieldsAreSent()
    {
        var parameters = Valid().ToParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters["zone_host"], Is.EqualTo("example.com."));
            Assert.That(parameters["record_type"], Is.EqualTo("A"));
            Assert.That(parameters["record_name"], Is.EqualTo("www"));
            Assert.That(parameters["record_data"], Is.EqualTo("192.0.2.1"));
            Assert.That(parameters["record_aux"], Is.EqualTo("0"));
        });
    }

    [Test]
    [TestCase(DnsRecordType.A, "A")]
    [TestCase(DnsRecordType.Aaaa, "AAAA")]
    [TestCase(DnsRecordType.Cname, "CNAME")]
    [TestCase(DnsRecordType.Mx, "MX")]
    [TestCase(DnsRecordType.Txt, "TXT")]
    [TestCase(DnsRecordType.Ns, "NS")]
    [TestCase(DnsRecordType.Srv, "SRV")]
    [TestCase(DnsRecordType.Caa, "CAA")]
    [TestCase(DnsRecordType.Ptr, "PTR")]
    [TestCase(DnsRecordType.Spf, "SPF")]
    public void ToParameters_Type_MapsToKasValue(DnsRecordType type, string expected)
    {
        var record = Valid();
        record.Type = type;
        Assert.That(record.ToParameters()["record_type"], Is.EqualTo(expected));
    }

    [Test]
    public void ToParameters_RawType_IsSentAndTrimmed()
    {
        var record = Valid();
        record.Type = null;
        record.RawType = "  DNSKEY  ";
        Assert.That(record.ToParameters()["record_type"], Is.EqualTo("DNSKEY"));
    }

    [Test]
    public void ToParameters_NeitherTypeNorRawType_Throws()
    {
        var record = Valid();
        record.Type = null;
        record.RawType = null;
        Assert.Throws<ArgumentException>(() => record.ToParameters());
    }

    [Test]
    public void ToParameters_BothTypeAndRawType_Throws()
    {
        var record = Valid();
        record.Type = DnsRecordType.A;
        record.RawType = "A";
        Assert.Throws<ArgumentException>(() => record.ToParameters());
    }

    [Test]
    [TestCase("example.com", "example.com.")]
    [TestCase("example.com.", "example.com.")]
    [TestCase("  sub.example.com  ", "sub.example.com.")]
    public void ToParameters_ZoneHost_IsNormalizedWithTrailingDot(string input, string expected)
    {
        var record = Valid();
        record.ZoneHost = input;
        Assert.That(record.ToParameters()["zone_host"], Is.EqualTo(expected));
    }

    [Test]
    public void ToParameters_Aux_IsSentAsString()
    {
        var record = Valid();
        record.Type = DnsRecordType.Mx;
        record.Aux = 10;
        Assert.That(record.ToParameters()["record_aux"], Is.EqualTo("10"));
    }

    [Test]
    public void ToParameters_NegativeAux_Throws()
    {
        var record = Valid();
        record.Aux = -1;
        Assert.Throws<ArgumentOutOfRangeException>(() => record.ToParameters());
    }

    [Test]
    public void ToParameters_EmptyRecordName_IsSentAsApex()
    {
        var record = Valid();
        record.RecordName = string.Empty;
        Assert.That(record.ToParameters()["record_name"], Is.EqualTo(string.Empty));
    }

    [Test]
    [TestCase("   ")]
    [TestCase("\t")]
    public void ToParameters_WhitespaceRecordName_Throws(string name)
    {
        var record = Valid();
        record.RecordName = name;
        Assert.Throws<ArgumentException>(() => record.ToParameters());
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankRecordData_Throws(string data)
    {
        var record = Valid();
        record.RecordData = data;
        Assert.Throws<ArgumentException>(() => record.ToParameters());
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankZoneHost_Throws(string zone)
    {
        var record = Valid();
        record.ZoneHost = zone;
        Assert.Throws<ArgumentException>(() => record.ToParameters());
    }
}
