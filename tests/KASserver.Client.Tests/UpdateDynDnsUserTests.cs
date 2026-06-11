using KASserver;

namespace KASserver.Client.Tests;

public class UpdateDynDnsUserTests
{
    [Test]
    public void ToParameters_NoChangeFields_Throws()
    {
        var update = new UpdateDynDnsUser();
        Assert.Throws<ArgumentException>(() => update.ToParameters("dyn123"));
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankLogin_Throws(string login)
    {
        var update = new UpdateDynDnsUser { Comment = "x" };
        Assert.Throws<ArgumentException>(() => update.ToParameters(login));
    }

    [Test]
    public void ToParameters_OnlySetFieldsAreSent()
    {
        var update = new UpdateDynDnsUser { Comment = "office" };

        var parameters = update.ToParameters("dyn123");

        Assert.Multiple(() =>
        {
            Assert.That(parameters.Keys, Is.EquivalentTo(new[] { "dyndns_login", "dyndns_comment" }));
            Assert.That(parameters["dyndns_login"], Is.EqualTo("dyn123"));
            Assert.That(parameters["dyndns_comment"], Is.EqualTo("office"));
        });
    }

    [Test]
    [TestCase(true, "Y")]
    [TestCase(false, "N")]
    public void ToParameters_DualStack_MapsToKasValue(bool dualStack, string expected)
    {
        var update = new UpdateDynDnsUser { DualStack = dualStack };
        Assert.That(update.ToParameters("dyn123")["dyndns_dual_stack"], Is.EqualTo(expected));
    }

    [Test]
    public void ToParameters_BlankPassword_Throws()
    {
        var update = new UpdateDynDnsUser { Password = "   " };
        Assert.Throws<ArgumentException>(() => update.ToParameters("dyn123"));
    }
}
