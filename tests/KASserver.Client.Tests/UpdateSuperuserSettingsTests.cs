using KASserver;

namespace KASserver.Client.Tests;

public class UpdateSuperuserSettingsTests
{
    [Test]
    public void ToParameters_NoChangeFields_Throws()
    {
        var update = new UpdateSuperuserSettings();
        Assert.Throws<ArgumentException>(() => update.ToParameters("w01abcde"));
    }

    [Test]
    public void ToParameters_BlankAccountLogin_Throws()
    {
        var update = new UpdateSuperuserSettings { SshAccess = true };
        Assert.Throws<ArgumentException>(() => update.ToParameters(" "));
    }

    [Test]
    [TestCase(true, "Y")]
    [TestCase(false, "N")]
    public void ToParameters_SshAccess_MapsToKasValue(bool access, string expected)
    {
        var update = new UpdateSuperuserSettings { SshAccess = access };

        var parameters = update.ToParameters("w01abcde");

        Assert.That(parameters["ssh_access"], Is.EqualTo(expected));
    }

    [Test]
    public void ToParameters_OnlySetFieldsAreSent()
    {
        var update = new UpdateSuperuserSettings { SshKeys = "ssh-ed25519 AAAA..." };

        var parameters = update.ToParameters("w01abcde");

        Assert.Multiple(() =>
        {
            Assert.That(parameters.Keys, Is.EquivalentTo(new[] { "account_login", "ssh_keys" }));
            Assert.That(parameters["ssh_keys"], Is.EqualTo("ssh-ed25519 AAAA..."));
        });
    }
}
