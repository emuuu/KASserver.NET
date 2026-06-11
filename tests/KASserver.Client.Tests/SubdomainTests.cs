using KASserver;

namespace KASserver.Client.Tests;

public class SubdomainTests
{
    // Field shape verified live against the KAS API (get_subdomains on famgmbh.de): the redirect status
    // is returned as subdomain_redirect_status (the request uses redirect_status), subdomain_name is the
    // full host name, plus is_active / in_progress / ssl_* / php_deprecated / subdomain_account /
    // subdomain_server.
    [Test]
    public void FromMap_ExposesConvenienceProperties()
    {
        var subdomain = Subdomain.FromMap(new Dictionary<string, object?>
        {
            ["subdomain_name"] = "shop.example.com",
            ["subdomain_path"] = "/shop/",
            ["subdomain_redirect_status"] = "301",
            ["php_version"] = "8.5",
            ["php_deprecated"] = "N",
            ["statistic_version"] = "7",
            ["statistic_language"] = "en",
            ["subdomain_account"] = "w021a9f9",
            ["subdomain_server"] = "dd17708",
            ["is_active"] = "Y",
            ["in_progress"] = "FALSE",
            ["fpse_active"] = "N",
            ["ssl_proxy"] = "N",
            ["ssl_certificate_ip"] = "N",
            ["ssl_certificate_sni"] = "Y",
        });

        Assert.Multiple(() =>
        {
            Assert.That(subdomain.SubdomainName, Is.EqualTo("shop.example.com"));
            Assert.That(subdomain.SubdomainPath, Is.EqualTo("/shop/"));
            Assert.That(subdomain.RawRedirectStatus, Is.EqualTo("301"));
            Assert.That(subdomain.Redirect, Is.EqualTo(RedirectStatus.MovedPermanently));
            Assert.That(subdomain.PhpVersion, Is.EqualTo("8.5"));
            Assert.That(subdomain.PhpDeprecated, Is.False);
            Assert.That(subdomain.StatisticVersion, Is.EqualTo(WebalizerVersion.Version7));
            Assert.That(subdomain.StatisticLanguage, Is.EqualTo(WebalizerLanguage.English));
            Assert.That(subdomain.Account, Is.EqualTo("w021a9f9"));
            Assert.That(subdomain.Server, Is.EqualTo("dd17708"));
            Assert.That(subdomain.IsActive, Is.True);
            Assert.That(subdomain.InProgress, Is.False);
            Assert.That(subdomain.FpseActive, Is.False);
            Assert.That(subdomain.SslProxy, Is.False);
            Assert.That(subdomain.SslCertificateIp, Is.False);
            Assert.That(subdomain.SslCertificateSni, Is.True);
        });
    }

    // A freshly created subdomain reports in_progress = TRUE while KAS provisions it (verified live).
    [Test]
    public void FromMap_FreshlyCreated_IsInProgress()
    {
        var subdomain = Subdomain.FromMap(new Dictionary<string, object?>
        {
            ["subdomain_name"] = "new.example.com",
            ["in_progress"] = "TRUE",
        });

        Assert.That(subdomain.InProgress, Is.True);
    }

    [Test]
    public void Redirect_UnknownValue_IsNullButRawKept()
    {
        var subdomain = Subdomain.FromMap(new Dictionary<string, object?> { ["subdomain_redirect_status"] = "308" });

        Assert.Multiple(() =>
        {
            Assert.That(subdomain.Redirect, Is.Null);
            Assert.That(subdomain.RawRedirectStatus, Is.EqualTo("308"));
        });
    }

    [Test]
    public void StatisticVersion_UnknownValue_IsNullButRawKept()
    {
        var subdomain = Subdomain.FromMap(new Dictionary<string, object?> { ["statistic_version"] = "9" });

        Assert.Multiple(() =>
        {
            Assert.That(subdomain.StatisticVersion, Is.Null);
            Assert.That(subdomain.RawStatisticVersion, Is.EqualTo("9"));
        });
    }

    [Test]
    public void StatisticLanguage_UnknownValue_IsNullButRawKept()
    {
        var subdomain = Subdomain.FromMap(new Dictionary<string, object?> { ["statistic_language"] = "fr" });

        Assert.Multiple(() =>
        {
            Assert.That(subdomain.StatisticLanguage, Is.Null);
            Assert.That(subdomain.RawStatisticLanguage, Is.EqualTo("fr"));
        });
    }

    [Test]
    public void MissingFields_ReadAsNullOrFalse()
    {
        var subdomain = Subdomain.FromMap(new Dictionary<string, object?>());

        Assert.Multiple(() =>
        {
            Assert.That(subdomain.SubdomainName, Is.Null);
            Assert.That(subdomain.SubdomainPath, Is.Null);
            Assert.That(subdomain.Redirect, Is.Null);
            Assert.That(subdomain.StatisticVersion, Is.Null);
            Assert.That(subdomain.StatisticLanguage, Is.Null);
            Assert.That(subdomain.PhpVersion, Is.Null);
            Assert.That(subdomain.IsActive, Is.False);
            Assert.That(subdomain.InProgress, Is.False);
        });
    }

    [Test]
    public void Raw_IsPreservedVerbatim()
    {
        var map = new Dictionary<string, object?> { ["some_future_field"] = "value" };
        var subdomain = Subdomain.FromMap(map);

        Assert.That(subdomain.Raw["some_future_field"], Is.EqualTo("value"));
    }

    [Test]
    public void ExtractSubdomainName_ScalarReturnInfo_ReturnsName()
    {
        var response = new KasResponse { ReturnInfo = "shop.example.com" };
        Assert.That(KasClient.ExtractSubdomainName(response), Is.EqualTo("shop.example.com"));
    }

    [Test]
    public void ExtractSubdomainName_MapWithSubdomainName_ReturnsName()
    {
        var response = new KasResponse
        {
            ReturnInfo = new Dictionary<string, object?> { ["subdomain_name"] = "shop.example.com" },
        };
        Assert.That(KasClient.ExtractSubdomainName(response), Is.EqualTo("shop.example.com"));
    }

    [Test]
    public void ExtractSubdomainName_NullReturnInfo_Throws()
    {
        var response = new KasResponse { ReturnInfo = null };
        Assert.Throws<KasApiException>(() => KasClient.ExtractSubdomainName(response));
    }

    [Test]
    public void ExtractSubdomainName_WhitespaceScalar_Throws()
    {
        var response = new KasResponse { ReturnInfo = "   " };
        Assert.Throws<KasApiException>(() => KasClient.ExtractSubdomainName(response));
    }

    [Test]
    public void ExtractSubdomainName_MapWithoutSubdomainName_Throws()
    {
        var response = new KasResponse
        {
            ReturnInfo = new Dictionary<string, object?> { ["something_else"] = "x" },
        };
        Assert.Throws<KasApiException>(() => KasClient.ExtractSubdomainName(response));
    }

    [Test]
    public void ExtractSubdomainName_MapWithBlankSubdomainName_Throws()
    {
        var response = new KasResponse
        {
            ReturnInfo = new Dictionary<string, object?> { ["subdomain_name"] = "   " },
        };
        Assert.Throws<KasApiException>(() => KasClient.ExtractSubdomainName(response));
    }
}
