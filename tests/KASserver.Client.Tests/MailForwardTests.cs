using KASserver;

namespace KASserver.Client.Tests;

public class MailForwardTests
{
    // Field shape verified live against the KAS API (get_mailforwards).
    private static MailForward Build() => new()
    {
        Raw = new Dictionary<string, object?>
        {
            ["mail_forward_adress"] = "info@example.com",
            ["mail_forward_address"] = "info@example.com",
            ["mail_forward_comment"] = null,
            ["mail_forward_targets"] = "ziel1@example.com,ziel2@example.com",
            ["mail_forward_spamfilter"] = "kaspdw",
            ["in_progress"] = "TRUE",
        },
    };

    [Test]
    public void Address_PrefersCorrectlySpelledKey()
    {
        Assert.That(Build().Address, Is.EqualTo("info@example.com"));
    }

    [Test]
    public void Address_FallsBackToMisspelledKey()
    {
        var forward = new MailForward
        {
            Raw = new Dictionary<string, object?> { ["mail_forward_adress"] = "x@example.com" },
        };
        Assert.That(forward.Address, Is.EqualTo("x@example.com"));
    }

    [Test]
    public void TargetAddresses_SplitsCommaSeparatedTargets()
    {
        Assert.That(Build().TargetAddresses, Is.EqualTo(new[] { "ziel1@example.com", "ziel2@example.com" }));
    }

    [Test]
    public void SpamFilter_IsExposed()
    {
        Assert.That(Build().SpamFilter, Is.EqualTo("kaspdw"));
    }

    [Test]
    public void TargetAddresses_EmptyWhenNoRawTargets()
    {
        var forward = new MailForward { Raw = new Dictionary<string, object?>() };
        Assert.That(forward.TargetAddresses, Is.Empty);
    }
}
