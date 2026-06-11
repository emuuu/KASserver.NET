using KASserver;

namespace KASserver.Client.Tests;

public class AddDynDnsUserTests
{
    private static AddDynDnsUser Valid() => new()
    {
        Comment = "home",
        Password = "Dyn-Pw!",
        Zone = "example.com",
        Label = "home",
        TargetIp = "192.0.2.1",
    };

    [Test]
    public void ToParameters_RequiredFieldsAreSent()
    {
        var parameters = Valid().ToParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters["dyndns_comment"], Is.EqualTo("home"));
            Assert.That(parameters["dyndns_password"], Is.EqualTo("Dyn-Pw!"));
            Assert.That(parameters["dyndns_zone"], Is.EqualTo("example.com"));
            Assert.That(parameters["dyndns_label"], Is.EqualTo("home"));
            Assert.That(parameters["dyndns_target_ip"], Is.EqualTo("192.0.2.1"));
        });
    }

    [Test]
    public void ToParameters_DualStackUnset_IsNotSent()
    {
        Assert.That(Valid().ToParameters().ContainsKey("dyndns_dual_stack"), Is.False);
    }

    [Test]
    [TestCase(true, "Y")]
    [TestCase(false, "N")]
    public void ToParameters_DualStack_MapsToKasValue(bool dualStack, string expected)
    {
        var user = Valid();
        user.DualStack = dualStack;
        Assert.That(user.ToParameters()["dyndns_dual_stack"], Is.EqualTo(expected));
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankComment_Throws(string comment)
    {
        var user = Valid();
        user.Comment = comment;
        Assert.Throws<ArgumentException>(() => user.ToParameters());
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankZone_Throws(string zone)
    {
        var user = Valid();
        user.Zone = zone;
        Assert.Throws<ArgumentException>(() => user.ToParameters());
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankTargetIp_Throws(string ip)
    {
        var user = Valid();
        user.TargetIp = ip;
        Assert.Throws<ArgumentException>(() => user.ToParameters());
    }
}
