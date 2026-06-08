using KASserver;

namespace KASserver.Client.Tests;

public class AccountQuotaTests
{
    [Test]
    public void ApplyTo_OnlySetFieldsAreSent()
    {
        var quota = new AccountQuota { MaxDomain = 5, MaxWebspace = 1024 };
        var parameters = new Dictionary<string, object?>();

        quota.ApplyTo(parameters);

        Assert.Multiple(() =>
        {
            Assert.That(parameters.Keys, Is.EquivalentTo(new[] { "max_domain", "max_webspace" }));
            Assert.That(parameters["max_domain"], Is.EqualTo("5"));
            Assert.That(parameters["max_webspace"], Is.EqualTo("1024"));
        });
    }

    [Test]
    public void ApplyTo_EmptyQuota_AddsNothing()
    {
        var quota = new AccountQuota();
        var parameters = new Dictionary<string, object?>();

        quota.ApplyTo(parameters);

        Assert.That(parameters, Is.Empty);
    }

    [Test]
    public void ApplyTo_Zero_IsSent()
    {
        var quota = new AccountQuota { MaxAccount = 0 };
        var parameters = new Dictionary<string, object?>();

        quota.ApplyTo(parameters);

        Assert.That(parameters["max_account"], Is.EqualTo("0"));
    }

    [Test]
    public void ApplyTo_NegativeValue_Throws()
    {
        var quota = new AccountQuota { MaxDatabase = -1 };
        var parameters = new Dictionary<string, object?>();

        Assert.Throws<ArgumentOutOfRangeException>(() => quota.ApplyTo(parameters));
    }

    [Test]
    public void ApplyTo_AllFields_MapToKasParameterNames()
    {
        var quota = new AccountQuota
        {
            MaxAccount = 1,
            MaxDomain = 2,
            MaxSubdomain = 3,
            MaxWebspace = 4,
            MaxMailAccount = 5,
            MaxMailForward = 6,
            MaxMailinglist = 7,
            MaxDatabase = 8,
            MaxFtpUser = 9,
            MaxSambaUser = 10,
            MaxCronjobs = 11,
            MaxWbk = 12,
        };
        var parameters = new Dictionary<string, object?>();

        quota.ApplyTo(parameters);

        Assert.Multiple(() =>
        {
            Assert.That(parameters["max_account"], Is.EqualTo("1"));
            Assert.That(parameters["max_domain"], Is.EqualTo("2"));
            Assert.That(parameters["max_subdomain"], Is.EqualTo("3"));
            Assert.That(parameters["max_webspace"], Is.EqualTo("4"));
            Assert.That(parameters["max_mail_account"], Is.EqualTo("5"));
            Assert.That(parameters["max_mail_forward"], Is.EqualTo("6"));
            Assert.That(parameters["max_mailinglist"], Is.EqualTo("7"));
            Assert.That(parameters["max_database"], Is.EqualTo("8"));
            Assert.That(parameters["max_ftpuser"], Is.EqualTo("9"));
            Assert.That(parameters["max_sambauser"], Is.EqualTo("10"));
            Assert.That(parameters["max_cronjobs"], Is.EqualTo("11"));
            Assert.That(parameters["max_wbk"], Is.EqualTo("12"));
        });
    }
}
