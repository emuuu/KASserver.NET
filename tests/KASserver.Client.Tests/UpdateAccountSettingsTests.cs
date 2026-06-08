using KASserver;

namespace KASserver.Client.Tests;

public class UpdateAccountSettingsTests
{
    [Test]
    public void ToParameters_NoChangeFields_Throws()
    {
        var update = new UpdateAccountSettings();
        Assert.Throws<ArgumentException>(() => update.ToParameters());
    }

    [Test]
    public void ToParameters_WhitespacePassword_Throws()
    {
        var update = new UpdateAccountSettings { Password = "   " };
        Assert.Throws<ArgumentException>(() => update.ToParameters());
    }

    [Test]
    [TestCase(0)]
    [TestCase(1000)]
    public void ToParameters_LogAgeOutOfRange_Throws(int logAge)
    {
        var update = new UpdateAccountSettings { LogAge = logAge };
        Assert.Throws<ArgumentOutOfRangeException>(() => update.ToParameters());
    }

    [Test]
    public void ToParameters_OnlySetFieldsAreSent()
    {
        var update = new UpdateAccountSettings { Logging = AccountLogging.Short, ShowPassword = true };

        var parameters = update.ToParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters.Keys, Is.EquivalentTo(new[] { "logging", "show_password" }));
            Assert.That(parameters["logging"], Is.EqualTo("kurz"));
            Assert.That(parameters["show_password"], Is.EqualTo("Y"));
        });
    }

    [Test]
    public void ToParameters_AllSimpleFields_Map()
    {
        var update = new UpdateAccountSettings
        {
            Password = "pw",
            Statistic = AccountStatistic.De,
            LogAge = 30,
            Comment = "c",
            ContactMail = "a@b.de",
        };

        var parameters = update.ToParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters["account_password"], Is.EqualTo("pw"));
            Assert.That(parameters["statistic"], Is.EqualTo("de"));
            Assert.That(parameters["logage"], Is.EqualTo("30"));
            Assert.That(parameters["account_comment"], Is.EqualTo("c"));
            Assert.That(parameters["account_contact_mail"], Is.EqualTo("a@b.de"));
        });
    }
}
