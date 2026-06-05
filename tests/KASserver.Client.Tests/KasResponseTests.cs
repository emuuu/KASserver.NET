using KASserver;

namespace KASserver.Client.Tests;

public class KasResponseTests
{
    [Test]
    public void AsList_NullReturnInfo_ReturnsEmpty()
    {
        var response = new KasResponse { ReturnInfo = null };
        Assert.That(response.AsList(), Is.Empty);
    }

    [Test]
    public void AsList_EmptyList_ReturnsEmpty()
    {
        var response = new KasResponse { ReturnInfo = new List<object?>() };
        Assert.That(response.AsList(), Is.Empty);
    }

    [Test]
    public void AsList_AllMaps_ReturnsThem()
    {
        var response = new KasResponse
        {
            ReturnInfo = new List<object?>
            {
                new Dictionary<string, object?> { ["a"] = "1" },
                new Dictionary<string, object?> { ["b"] = "2" },
            },
        };

        Assert.That(response.AsList(), Has.Count.EqualTo(2));
    }

    [Test]
    public void AsList_NonMapElement_ThrowsInsteadOfDropping()
    {
        var response = new KasResponse
        {
            ReturnInfo = new List<object?> { new Dictionary<string, object?>(), "unexpected" },
        };

        Assert.Throws<KasApiException>(() => response.AsList());
    }

    [Test]
    public void AsMap_NonMapReturnInfo_Throws()
    {
        var response = new KasResponse { ReturnInfo = "scalar" };
        Assert.Throws<KasApiException>(() => response.AsMap());
    }
}
