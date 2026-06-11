using KASserver;

namespace KASserver.Client.Tests;

public class UpdateDnsRecordTests
{
    [Test]
    public void ToParameters_NoChangeFields_Throws()
    {
        var update = new UpdateDnsRecord();
        Assert.Throws<ArgumentException>(() => update.ToParameters("1234"));
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankRecordId_Throws(string recordId)
    {
        var update = new UpdateDnsRecord { RecordData = "192.0.2.9" };
        Assert.Throws<ArgumentException>(() => update.ToParameters(recordId));
    }

    [Test]
    public void ToParameters_OnlySetFieldsAreSent()
    {
        var update = new UpdateDnsRecord { RecordData = "192.0.2.9" };

        var parameters = update.ToParameters("1234");

        Assert.Multiple(() =>
        {
            Assert.That(parameters.Keys, Is.EquivalentTo(new[] { "record_id", "record_data" }));
            Assert.That(parameters["record_id"], Is.EqualTo("1234"));
            Assert.That(parameters["record_data"], Is.EqualTo("192.0.2.9"));
        });
    }

    [Test]
    public void ToParameters_AllFields_AreSent()
    {
        var update = new UpdateDnsRecord { RecordName = "mail", RecordData = "mail.example.com", Aux = 10 };

        var parameters = update.ToParameters("1234");

        Assert.Multiple(() =>
        {
            Assert.That(parameters["record_name"], Is.EqualTo("mail"));
            Assert.That(parameters["record_data"], Is.EqualTo("mail.example.com"));
            Assert.That(parameters["record_aux"], Is.EqualTo("10"));
        });
    }

    [Test]
    public void ToParameters_EmptyRecordName_IsSent()
    {
        // An empty record_name is a legitimate change (apex), so it is forwarded — only null is "unset".
        var update = new UpdateDnsRecord { RecordName = string.Empty };

        var parameters = update.ToParameters("1234");

        Assert.That(parameters["record_name"], Is.EqualTo(string.Empty));
    }

    [Test]
    [TestCase("   ")]
    [TestCase("\t")]
    public void ToParameters_WhitespaceRecordName_Throws(string name)
    {
        var update = new UpdateDnsRecord { RecordName = name };
        Assert.Throws<ArgumentException>(() => update.ToParameters("1234"));
    }

    [Test]
    public void ToParameters_BlankRecordData_Throws()
    {
        var update = new UpdateDnsRecord { RecordData = "   " };
        Assert.Throws<ArgumentException>(() => update.ToParameters("1234"));
    }

    [Test]
    public void ToParameters_NegativeAux_Throws()
    {
        var update = new UpdateDnsRecord { Aux = -1 };
        Assert.Throws<ArgumentOutOfRangeException>(() => update.ToParameters("1234"));
    }
}
