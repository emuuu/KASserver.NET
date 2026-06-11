using KASserver;

namespace KASserver.Client.Tests;

public class UpdateSubdomainTests
{
    [Test]
    public void ToParameters_SendsSubdomainNameAndSetFields()
    {
        var parameters = new UpdateSubdomain
        {
            SubdomainPath = "http://target.tld",
            Redirect = RedirectStatus.Found,
            PhpVersion = "8.2",
        }.ToParameters("shop.example.com");

        Assert.Multiple(() =>
        {
            Assert.That(parameters["subdomain_name"], Is.EqualTo("shop.example.com"));
            Assert.That(parameters["subdomain_path"], Is.EqualTo("http://target.tld"));
            Assert.That(parameters["redirect_status"], Is.EqualTo("302"));
            Assert.That(parameters["php_version"], Is.EqualTo("8.2"));
        });
    }

    [Test]
    public void ToParameters_OmitsUnsetFields()
    {
        var parameters = new UpdateSubdomain { PhpVersion = "8.2" }.ToParameters("shop.example.com");

        Assert.Multiple(() =>
        {
            Assert.That(parameters.ContainsKey("subdomain_path"), Is.False);
            Assert.That(parameters.ContainsKey("redirect_status"), Is.False);
            Assert.That(parameters["php_version"], Is.EqualTo("8.2"));
        });
    }

    [Test]
    public void ToParameters_Redirect_MapsToKasValue()
    {
        var parameters = new UpdateSubdomain { Redirect = RedirectStatus.TemporaryRedirect }
            .ToParameters("shop.example.com");

        Assert.That(parameters["redirect_status"], Is.EqualTo("307"));
    }

    // update_subdomain only accepts subdomain_path/redirect_status/php_version per the official phpdoc;
    // the webalizer statistics fields are add-only and must never leak into an update payload.
    [Test]
    public void ToParameters_NeverSendsStatisticFields()
    {
        var parameters = new UpdateSubdomain
        {
            SubdomainPath = "/x/",
            Redirect = RedirectStatus.MovedPermanently,
            PhpVersion = "8.3",
        }.ToParameters("shop.example.com");

        Assert.Multiple(() =>
        {
            Assert.That(parameters.ContainsKey("statistic_version"), Is.False);
            Assert.That(parameters.ContainsKey("statistic_language"), Is.False);
        });
    }

    [Test]
    public void ToParameters_NoFieldsSet_Throws()
    {
        Assert.Throws<ArgumentException>(() => new UpdateSubdomain().ToParameters("shop.example.com"));
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankSubdomainName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => new UpdateSubdomain { PhpVersion = "8.2" }.ToParameters(name));
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankSubdomainPath_Throws(string path)
    {
        Assert.Throws<ArgumentException>(() => new UpdateSubdomain { SubdomainPath = path }.ToParameters("shop.example.com"));
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void ToParameters_BlankPhpVersion_Throws(string version)
    {
        Assert.Throws<ArgumentException>(() => new UpdateSubdomain { PhpVersion = version }.ToParameters("shop.example.com"));
    }
}
