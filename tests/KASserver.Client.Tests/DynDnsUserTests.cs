using KASserver;

namespace KASserver.Client.Tests;

public class DynDnsUserTests
{
    // Field shape verified live against the KAS API (get_ddnsusers on verqo.de): dyndns_login,
    // dyndns_comment, dyndns_label, dyndns_zone, dyndns_dual_stack (Y/N), dyndns_target_ip plus
    // the split dyndns_target_ipv4 / dyndns_target_ipv6.
    [Test]
    public void FromMap_ExposesConvenienceProperties()
    {
        var user = DynDnsUser.FromMap(new Dictionary<string, object?>
        {
            ["dyndns_login"] = "dyn001f04c",
            ["dyndns_zone"] = "verqo.de",
            ["dyndns_label"] = "home",
            ["dyndns_comment"] = "home router",
            ["dyndns_target_ip"] = "192.0.2.1",
            ["dyndns_target_ipv4"] = "192.0.2.1",
            ["dyndns_target_ipv6"] = "",
            ["dyndns_dual_stack"] = "Y",
        });

        Assert.Multiple(() =>
        {
            Assert.That(user.Login, Is.EqualTo("dyn001f04c"));
            Assert.That(user.Zone, Is.EqualTo("verqo.de"));
            Assert.That(user.Label, Is.EqualTo("home"));
            Assert.That(user.Comment, Is.EqualTo("home router"));
            Assert.That(user.TargetIp, Is.EqualTo("192.0.2.1"));
            Assert.That(user.TargetIpv4, Is.EqualTo("192.0.2.1"));
            Assert.That(user.TargetIpv6, Is.EqualTo(string.Empty));
            Assert.That(user.DualStack, Is.True);
        });
    }

    [Test]
    public void FromMap_DualStackN_IsFalse()
    {
        var user = DynDnsUser.FromMap(new Dictionary<string, object?> { ["dyndns_dual_stack"] = "N" });
        Assert.That(user.DualStack, Is.False);
    }

    [Test]
    public void FromMap_MissingFields_ReadAsNull()
    {
        var user = DynDnsUser.FromMap(new Dictionary<string, object?>());

        Assert.Multiple(() =>
        {
            Assert.That(user.Login, Is.Null);
            Assert.That(user.TargetIp, Is.Null);
            Assert.That(user.DualStack, Is.False);
        });
    }

    [Test]
    public void ExtractDynDnsLogin_ScalarReturnInfo_ReturnsLogin()
    {
        var response = new KasResponse { ReturnInfo = "dyn0a1b2c" };
        Assert.That(KasClient.ExtractDynDnsLogin(response), Is.EqualTo("dyn0a1b2c"));
    }

    [Test]
    public void ExtractDynDnsLogin_MapWithLogin_ReturnsLogin()
    {
        var response = new KasResponse
        {
            ReturnInfo = new Dictionary<string, object?> { ["dyndns_login"] = "dyn0a1b2c" },
        };
        Assert.That(KasClient.ExtractDynDnsLogin(response), Is.EqualTo("dyn0a1b2c"));
    }

    [Test]
    public void ExtractDynDnsLogin_MapWithDdnsLoginFallback_ReturnsLogin()
    {
        // get_ddnsusers filters on ddns_login; the defensive fallback accepts that key too.
        var response = new KasResponse
        {
            ReturnInfo = new Dictionary<string, object?> { ["ddns_login"] = "dyn0a1b2c" },
        };
        Assert.That(KasClient.ExtractDynDnsLogin(response), Is.EqualTo("dyn0a1b2c"));
    }

    [Test]
    public void ExtractDynDnsLogin_NullReturnInfo_Throws()
    {
        var response = new KasResponse { ReturnInfo = null };
        Assert.Throws<KasApiException>(() => KasClient.ExtractDynDnsLogin(response));
    }

    [Test]
    public void ExtractDynDnsLogin_WhitespaceScalar_Throws()
    {
        var response = new KasResponse { ReturnInfo = "   " };
        Assert.Throws<KasApiException>(() => KasClient.ExtractDynDnsLogin(response));
    }

    [Test]
    public void ExtractDynDnsLogin_MapWithoutLogin_Throws()
    {
        var response = new KasResponse
        {
            ReturnInfo = new Dictionary<string, object?> { ["something_else"] = "x" },
        };
        Assert.Throws<KasApiException>(() => KasClient.ExtractDynDnsLogin(response));
    }
}
