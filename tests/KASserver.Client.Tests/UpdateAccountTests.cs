using KASserver;

namespace KASserver.Client.Tests;

public class UpdateAccountTests
{
    [Test]
    public void ToParameters_NoChangeFields_Throws()
    {
        var update = new UpdateAccount();
        Assert.Throws<ArgumentException>(() => update.ToParameters("w01abcde"));
    }

    [Test]
    public void ToParameters_BlankAccountLogin_Throws()
    {
        var update = new UpdateAccount { Comment = "x" };
        Assert.Throws<ArgumentException>(() => update.ToParameters(" "));
    }

    [Test]
    public void ToParameters_WhitespacePassword_Throws()
    {
        var update = new UpdateAccount { KasPassword = "   " };
        Assert.Throws<ArgumentException>(() => update.ToParameters("w01abcde"));
    }

    [Test]
    [TestCase(AccountAccessState.Allowed, "N")]
    [TestCase(AccountAccessState.Forbidden, "Y")]
    [TestCase(AccountAccessState.ForbiddenExplicit, "forbidden")]
    public void ToParameters_AccessForbidden_MapsToKasValue(AccountAccessState state, string expected)
    {
        var update = new UpdateAccount { AccessForbidden = state };
        Assert.That(update.ToParameters("w01abcde")["kas_access_forbidden"], Is.EqualTo(expected));
    }

    [Test]
    public void ToParameters_OnlySetFieldsAreSent()
    {
        var update = new UpdateAccount { Comment = "renamed", DnsSettings = true };

        var parameters = update.ToParameters("w01abcde");

        Assert.Multiple(() =>
        {
            Assert.That(parameters.Keys, Is.EquivalentTo(new[] { "account_login", "account_comment", "dns_settings" }));
            Assert.That(parameters["account_comment"], Is.EqualTo("renamed"));
            Assert.That(parameters["dns_settings"], Is.EqualTo("Y"));
        });
    }

    [Test]
    public void ToParameters_PartialQuota_IsMerged()
    {
        var update = new UpdateAccount { Quota = new AccountQuota { MaxWebspace = 2048 } };

        var parameters = update.ToParameters("w01abcde");

        Assert.Multiple(() =>
        {
            Assert.That(parameters["max_webspace"], Is.EqualTo("2048"));
            Assert.That(parameters.ContainsKey("max_domain"), Is.False);
        });
    }

    [Test]
    [TestCase(0)]
    [TestCase(1000)]
    public void ToParameters_LogAgeOutOfRange_Throws(int logAge)
    {
        var update = new UpdateAccount { LogAge = logAge };
        Assert.Throws<ArgumentOutOfRangeException>(() => update.ToParameters("w01abcde"));
    }

    [Test]
    public void ToParameters_AllFields_MapToKasParametersAndValues()
    {
        var update = new UpdateAccount
        {
            KasPassword = "kas-pw",
            Quota = new AccountQuota { MaxDomain = 2 },
            InstHtaccess = true,
            InstFpse = false,
            InstSoftware = true,
            AccessForbidden = AccountAccessState.ForbiddenExplicit,
            ShowPassword = false,
            Logging = AccountLogging.WithoutIp,
            LogAge = 90,
            Statistic = AccountStatistic.Ne,
            DnsSettings = true,
            Comment = "renamed",
            ContactMail = "admin@example.com",
        };

        var parameters = update.ToParameters("w01abcde");

        Assert.Multiple(() =>
        {
            Assert.That(parameters["account_login"], Is.EqualTo("w01abcde"));
            Assert.That(parameters["account_kas_password"], Is.EqualTo("kas-pw"));
            Assert.That(parameters["max_domain"], Is.EqualTo("2"));
            Assert.That(parameters["inst_htaccess"], Is.EqualTo("Y"));
            Assert.That(parameters["inst_fpse"], Is.EqualTo("N"));
            Assert.That(parameters["inst_software"], Is.EqualTo("Y"));
            Assert.That(parameters["kas_access_forbidden"], Is.EqualTo("forbidden"));
            Assert.That(parameters["show_password"], Is.EqualTo("N"));
            Assert.That(parameters["logging"], Is.EqualTo("ohneip"));
            Assert.That(parameters["logage"], Is.EqualTo("90"));
            Assert.That(parameters["statistic"], Is.EqualTo("ne"));
            Assert.That(parameters["dns_settings"], Is.EqualTo("Y"));
            Assert.That(parameters["account_comment"], Is.EqualTo("renamed"));
            Assert.That(parameters["account_contact_mail"], Is.EqualTo("admin@example.com"));
        });
    }
}
