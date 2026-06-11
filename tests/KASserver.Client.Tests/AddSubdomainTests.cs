using KASserver;

namespace KASserver.Client.Tests;

public class AddSubdomainTests
{
    private static AddSubdomain Valid() => new()
    {
        SubdomainName = "shop",
        DomainName = "example.com",
    };

    [Test]
    public void ToParameters_RequiredFieldsAreSent()
    {
        var parameters = Valid().ToParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters["subdomain_name"], Is.EqualTo("shop"));
            Assert.That(parameters["domain_name"], Is.EqualTo("example.com"));
        });
    }

    [Test]
    public void ToParameters_OptionalFields_AreOmittedWhenUnset()
    {
        var parameters = Valid().ToParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters.ContainsKey("subdomain_path"), Is.False);
            Assert.That(parameters.ContainsKey("redirect_status"), Is.False);
            Assert.That(parameters.ContainsKey("statistic_version"), Is.False);
            Assert.That(parameters.ContainsKey("statistic_language"), Is.False);
            Assert.That(parameters.ContainsKey("php_version"), Is.False);
        });
    }

    [Test]
    public void ToParameters_OptionalFields_AreSentWhenSet()
    {
        var subdomain = Valid();
        subdomain.SubdomainPath = "/shop/";
        subdomain.Redirect = RedirectStatus.MovedPermanently;
        subdomain.StatisticVersion = WebalizerVersion.Version7;
        subdomain.StatisticLanguage = WebalizerLanguage.English;
        subdomain.PhpVersion = "8.1";

        var parameters = subdomain.ToParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters["subdomain_path"], Is.EqualTo("/shop/"));
            Assert.That(parameters["redirect_status"], Is.EqualTo("301"));
            Assert.That(parameters["statistic_version"], Is.EqualTo("7"));
            Assert.That(parameters["statistic_language"], Is.EqualTo("en"));
            Assert.That(parameters["php_version"], Is.EqualTo("8.1"));
        });
    }

    [Test]
    [TestCase(RedirectStatus.None, "0")]
    [TestCase(RedirectStatus.MovedPermanently, "301")]
    [TestCase(RedirectStatus.Found, "302")]
    [TestCase(RedirectStatus.TemporaryRedirect, "307")]
    public void ToParameters_Redirect_MapsToKasValue(RedirectStatus status, string expected)
    {
        var subdomain = Valid();
        subdomain.Redirect = status;
        Assert.That(subdomain.ToParameters()["redirect_status"], Is.EqualTo(expected));
    }

    [Test]
    [TestCase(WebalizerVersion.None, "0")]
    [TestCase(WebalizerVersion.Version4, "4")]
    [TestCase(WebalizerVersion.Version5, "5")]
    [TestCase(WebalizerVersion.Version7, "7")]
    public void ToParameters_StatisticVersion_MapsToKasValue(WebalizerVersion version, string expected)
    {
        var subdomain = Valid();
        subdomain.StatisticVersion = version;
        Assert.That(subdomain.ToParameters()["statistic_version"], Is.EqualTo(expected));
    }

    [Test]
    [TestCase(WebalizerLanguage.German, "de")]
    [TestCase(WebalizerLanguage.English, "en")]
    public void ToParameters_StatisticLanguage_MapsToKasValue(WebalizerLanguage language, string expected)
    {
        var subdomain = Valid();
        subdomain.StatisticLanguage = language;
        Assert.That(subdomain.ToParameters()["statistic_language"], Is.EqualTo(expected));
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankSubdomainName_Throws(string name)
    {
        var subdomain = Valid();
        subdomain.SubdomainName = name;
        Assert.Throws<ArgumentException>(() => subdomain.ToParameters());
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankDomainName_Throws(string domain)
    {
        var subdomain = Valid();
        subdomain.DomainName = domain;
        Assert.Throws<ArgumentException>(() => subdomain.ToParameters());
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankSubdomainPath_Throws(string path)
    {
        var subdomain = Valid();
        subdomain.SubdomainPath = path;
        Assert.Throws<ArgumentException>(() => subdomain.ToParameters());
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankPhpVersion_Throws(string version)
    {
        var subdomain = Valid();
        subdomain.PhpVersion = version;
        Assert.Throws<ArgumentException>(() => subdomain.ToParameters());
    }
}
