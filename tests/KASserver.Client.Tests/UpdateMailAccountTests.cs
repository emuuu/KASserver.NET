using KASserver;

namespace KASserver.Client.Tests;

public class UpdateMailAccountTests
{
    [Test]
    public void ToParameters_NoChangeFields_Throws()
    {
        var update = new UpdateMailAccount();
        Assert.Throws<ArgumentException>(() => update.ToParameters("m07f821c"));
    }

    [Test]
    public void ToParameters_WhitespacePassword_Throws()
    {
        var update = new UpdateMailAccount { NewPassword = "   " };
        Assert.Throws<ArgumentException>(() => update.ToParameters("m07f821c"));
    }

    [Test]
    public void ToParameters_BlankMailLogin_Throws()
    {
        var update = new UpdateMailAccount { NewPassword = "secret" };
        Assert.Throws<ArgumentException>(() => update.ToParameters(" "));
    }

    [Test]
    [TestCase(MailboxActiveState.Active, "Y")]
    [TestCase(MailboxActiveState.ReceiveDisabled, "N")]
    [TestCase(MailboxActiveState.Forbidden, "forbidden")]
    public void ToParameters_IsActive_MapsToKasValue(MailboxActiveState state, string expected)
    {
        var update = new UpdateMailAccount { IsActive = state };

        var parameters = update.ToParameters("m07f821c");

        Assert.That(parameters["is_active"], Is.EqualTo(expected));
    }

    [Test]
    public void ToParameters_OnlySetFieldsAreSent()
    {
        var update = new UpdateMailAccount { CopyAddress = "copy@example.com" };

        var parameters = update.ToParameters("m07f821c");

        Assert.Multiple(() =>
        {
            Assert.That(parameters.Keys, Is.EquivalentTo(new[] { "mail_login", "copy_adress" }));
            Assert.That(parameters["copy_adress"], Is.EqualTo("copy@example.com"));
        });
    }
}
