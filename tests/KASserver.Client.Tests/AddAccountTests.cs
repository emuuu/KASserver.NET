using KASserver;

namespace KASserver.Client.Tests;

public class AddAccountTests
{
    private static AddAccount Valid() => new()
    {
        KasPassword = "kas-pw",
        FtpPassword = "ftp-pw",
        HostnameKind = AccountHostnameKind.Domain,
        HostnamePart1 = "example",
        HostnamePart2 = "com",
    };

    [Test]
    public void ToParameters_RequiredAndDefaultsAreSent()
    {
        var parameters = Valid().ToParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters["account_kas_password"], Is.EqualTo("kas-pw"));
            Assert.That(parameters["account_ftp_password"], Is.EqualTo("ftp-pw"));
            Assert.That(parameters["hostname_art"], Is.EqualTo("domain"));
            Assert.That(parameters["hostname_part1"], Is.EqualTo("example"));
            Assert.That(parameters["hostname_part2"], Is.EqualTo("com"));
            Assert.That(parameters["inst_htaccess"], Is.EqualTo("Y"));
            Assert.That(parameters["inst_software"], Is.EqualTo("Y"));
            Assert.That(parameters["kas_access_forbidden"], Is.EqualTo("N"));
            Assert.That(parameters["logging"], Is.EqualTo("keine"));
            Assert.That(parameters["logage"], Is.EqualTo("190"));
            Assert.That(parameters["statistic"], Is.EqualTo("0"));
            Assert.That(parameters["dns_settings"], Is.EqualTo("N"));
            Assert.That(parameters["show_password"], Is.EqualTo("N"));
        });
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankKasPassword_Throws(string password)
    {
        var account = Valid();
        account.KasPassword = password;
        Assert.Throws<ArgumentException>(() => account.ToParameters());
    }

    [Test]
    public void ToParameters_BlankFtpPassword_Throws()
    {
        var account = Valid();
        account.FtpPassword = " ";
        Assert.Throws<ArgumentException>(() => account.ToParameters());
    }

    [Test]
    [TestCase(AccountHostnameKind.Domain, "domain")]
    [TestCase(AccountHostnameKind.Subdomain, "subdomain")]
    [TestCase(AccountHostnameKind.None, "")]
    public void ToParameters_HostnameKind_MapsToKasValue(AccountHostnameKind kind, string expected)
    {
        var account = Valid();
        account.HostnameKind = kind;
        Assert.That(account.ToParameters()["hostname_art"], Is.EqualTo(expected));
    }

    [Test]
    [TestCase(AccountLogging.Full, "voll")]
    [TestCase(AccountLogging.Short, "kurz")]
    [TestCase(AccountLogging.WithoutIp, "ohneip")]
    [TestCase(AccountLogging.None, "keine")]
    public void ToParameters_Logging_MapsToKasValue(AccountLogging logging, string expected)
    {
        var account = Valid();
        account.Logging = logging;
        Assert.That(account.ToParameters()["logging"], Is.EqualTo(expected));
    }

    [Test]
    [TestCase(AccountStatistic.None, "0")]
    [TestCase(AccountStatistic.De, "de")]
    [TestCase(AccountStatistic.Ne, "ne")]
    public void ToParameters_Statistic_MapsToKasValue(AccountStatistic statistic, string expected)
    {
        var account = Valid();
        account.Statistic = statistic;
        Assert.That(account.ToParameters()["statistic"], Is.EqualTo(expected));
    }

    [Test]
    [TestCase(0)]
    [TestCase(1000)]
    public void ToParameters_LogAgeOutOfRange_Throws(int logAge)
    {
        var account = Valid();
        account.LogAge = logAge;
        Assert.Throws<ArgumentOutOfRangeException>(() => account.ToParameters());
    }

    [Test]
    public void ToParameters_Quota_IsMerged()
    {
        var account = Valid();
        account.Quota = new AccountQuota { MaxDomain = 3 };
        Assert.That(account.ToParameters()["max_domain"], Is.EqualTo("3"));
    }

    [Test]
    public void ToParameters_OptionalStrings_OnlySentWhenSet()
    {
        var withoutOptionals = Valid().ToParameters();
        Assert.That(withoutOptionals.ContainsKey("account_comment"), Is.False);

        var account = Valid();
        account.Comment = "primary";
        account.ContactMail = "admin@example.com";
        account.HostnamePath = "/example/";
        var parameters = account.ToParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters["account_comment"], Is.EqualTo("primary"));
            Assert.That(parameters["account_contact_mail"], Is.EqualTo("admin@example.com"));
            Assert.That(parameters["hostname_path"], Is.EqualTo("/example/"));
        });
    }
}
