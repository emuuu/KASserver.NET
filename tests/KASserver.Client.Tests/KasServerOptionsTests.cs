using KASserver;

namespace KASserver.Client.Tests;

public class KasServerOptionsTests
{
    [Test]
    public void Validate_ValidOptions_DoesNotThrow()
    {
        var options = new KasServerOptions { Login = "w0test99", Password = "secret" };
        Assert.DoesNotThrow(() => options.Validate());
    }

    [Test]
    public void Validate_MissingLogin_Throws()
    {
        var options = new KasServerOptions { Password = "secret" };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Test]
    public void Validate_MissingPassword_Throws()
    {
        var options = new KasServerOptions { Login = "w0test99" };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Test]
    public void Validate_NonPositiveSessionLifetime_Throws()
    {
        var options = new KasServerOptions { Login = "w0test99", Password = "secret", SessionLifetime = 0 };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Test]
    public void Validate_NonPositiveTimeout_Throws()
    {
        var options = new KasServerOptions { Login = "w0test99", Password = "secret", Timeout = TimeSpan.Zero };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Test]
    public void Validate_InfiniteTimeout_DoesNotThrow()
    {
        var options = new KasServerOptions
        {
            Login = "w0test99",
            Password = "secret",
            Timeout = System.Threading.Timeout.InfiniteTimeSpan,
        };
        Assert.DoesNotThrow(() => options.Validate());
    }

    [Test]
    public void Defaults_AreSensible()
    {
        var options = new KasServerOptions();
        Assert.Multiple(() =>
        {
            Assert.That(options.SessionLifetime, Is.EqualTo(1800));
            Assert.That(options.UpdateSessionLifetime, Is.True);
            Assert.That(options.Timeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
        });
    }
}
