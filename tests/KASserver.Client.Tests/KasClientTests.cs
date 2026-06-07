using KASserver;

namespace KASserver.Client.Tests;

public class KasClientTests
{
    [Test]
    public void ExtractAccountLogin_ScalarReturnInfo_ReturnsLogin()
    {
        var response = new KasResponse { ReturnInfo = "w01abcde" };
        Assert.That(KasClient.ExtractAccountLogin(response), Is.EqualTo("w01abcde"));
    }

    [Test]
    public void ExtractAccountLogin_MapWithAccountLogin_ReturnsLogin()
    {
        var response = new KasResponse
        {
            ReturnInfo = new Dictionary<string, object?> { ["account_login"] = "w01abcde" },
        };
        Assert.That(KasClient.ExtractAccountLogin(response), Is.EqualTo("w01abcde"));
    }

    [Test]
    public void ExtractAccountLogin_MapWithoutAccountLogin_Throws()
    {
        var response = new KasResponse
        {
            ReturnInfo = new Dictionary<string, object?> { ["something_else"] = "x" },
        };
        Assert.Throws<KasApiException>(() => KasClient.ExtractAccountLogin(response));
    }

    [Test]
    public void ExtractAccountLogin_NullReturnInfo_Throws()
    {
        var response = new KasResponse { ReturnInfo = null };
        Assert.Throws<KasApiException>(() => KasClient.ExtractAccountLogin(response));
    }

    [Test]
    public void ExtractAccountLogin_WhitespaceScalar_Throws()
    {
        var response = new KasResponse { ReturnInfo = "   " };
        Assert.Throws<KasApiException>(() => KasClient.ExtractAccountLogin(response));
    }

    // Mirrors the projection used by GetAccountsAsync/GetAccountAsync
    // (response.AsList().Select(SubAccount.FromMap)[.FirstOrDefault()]).
    [Test]
    public void AsList_ProjectsToSubAccounts()
    {
        var response = new KasResponse
        {
            ReturnInfo = new List<object?>
            {
                new Dictionary<string, object?> { ["account_login"] = "w01aaaaa" },
                new Dictionary<string, object?> { ["account_login"] = "w01bbbbb" },
            },
        };

        var accounts = response.AsList().Select(SubAccount.FromMap).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(accounts, Has.Count.EqualTo(2));
            Assert.That(accounts[0].AccountLogin, Is.EqualTo("w01aaaaa"));
            Assert.That(accounts.FirstOrDefault()?.AccountLogin, Is.EqualTo("w01aaaaa"));
        });
    }

    [Test]
    public void AsList_NoResults_FirstOrDefaultIsNull()
    {
        var response = new KasResponse { ReturnInfo = null };

        var account = response.AsList().Select(SubAccount.FromMap).FirstOrDefault();

        Assert.That(account, Is.Null);
    }
}
