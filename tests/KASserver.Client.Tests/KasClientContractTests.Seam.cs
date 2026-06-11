using KASserver.Client.Tests.Fakes;
using KASserver.Soap;
using Microsoft.Extensions.DependencyInjection;

namespace KASserver.Client.Tests;

/// <summary>
/// Verifies the transport seam itself: the <see cref="IKasClient.RequestAsync"/> passthrough and the
/// DI invariant that <see cref="IKasTransport"/> resolves to the very same shared
/// <c>KasSoapTransport</c> singleton (so the session token and flood window stay process-unique).
/// </summary>
public class KasClientContractSeamTests
{
    [Test]
    public async Task RequestAsync_ForwardsActionParametersTokenAndResponseUnchanged()
    {
        var parameters = new Dictionary<string, object?> { ["foo"] = "bar" };
        var expected = new KasResponse { ReturnString = "TRUE", ReturnInfo = "payload" };
        var fake = new RecordingKasTransport().Enqueue(expected);
        var client = new KasClient(fake);
        using var cts = new CancellationTokenSource();

        var response = await client.RequestAsync("custom_action", parameters, cts.Token);

        Assert.Multiple(() =>
        {
            Assert.That(fake.LastAction, Is.EqualTo("custom_action"));
            // The exact parameter instance is forwarded — no copy, no mutation.
            Assert.That(fake.LastParameters, Is.SameAs(parameters));
            Assert.That(fake.LastCancellationToken, Is.EqualTo(cts.Token));
            Assert.That(response, Is.SameAs(expected));
        });
    }

    [Test]
    public async Task RequestAsync_NullParameters_AreForwardedAsNull()
    {
        var fake = new RecordingKasTransport().EnqueueSuccess();
        var client = new KasClient(fake);

        await client.RequestAsync("custom_action");

        Assert.That(fake.LastParameters, Is.Null);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void RequestAsync_BlankAction_Throws(string? action)
    {
        var client = new KasClient(new RecordingKasTransport());

        // RequestAsync validates synchronously before returning the task; the void-bodied lambda makes
        // NUnit treat it as a synchronous throw. null maps to ArgumentNullException (a derived type).
        Assert.That(() => { _ = client.RequestAsync(action!); }, Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public void AddKasServer_AliasesIKasTransportToTheSameKasSoapTransportSingleton()
    {
        using var provider = BuildProvider();

        var transport = provider.GetRequiredService<KasSoapTransport>();
        var aliased = provider.GetRequiredService<IKasTransport>();

        // The alias must be the identical instance, not a second transport — otherwise the session
        // token and flood window would be split across two objects.
        Assert.That(aliased, Is.SameAs(transport));
    }

    [Test]
    public void AddKasServer_IsIdempotent_StillOneSharedTransport()
    {
        var services = new ServiceCollection();
        services.AddKasServer(Configure);
        services.AddKasServer(Configure);
        using var provider = services.BuildServiceProvider();

        var transport = provider.GetRequiredService<KasSoapTransport>();
        var aliased = provider.GetRequiredService<IKasTransport>();
        var resolvedAgain = provider.GetRequiredService<IKasTransport>();

        Assert.Multiple(() =>
        {
            Assert.That(aliased, Is.SameAs(transport));
            Assert.That(resolvedAgain, Is.SameAs(transport));
        });
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddKasServer(Configure);
        return services.BuildServiceProvider();
    }

    private static void Configure(KasServerOptions options)
    {
        // KasSoapTransport validates these in its constructor, so they must be non-blank.
        options.Login = "w00test";
        options.Password = "secret";
    }
}
