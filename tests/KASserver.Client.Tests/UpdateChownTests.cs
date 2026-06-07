using KASserver;

namespace KASserver.Client.Tests;

public class UpdateChownTests
{
    [Test]
    public void ToParameters_MapsAllFields()
    {
        var chown = new UpdateChown { Path = "/www/htdocs/w01abcde/app", User = "w01abcde", Recursive = true };

        var parameters = chown.ToParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters["chown_path"], Is.EqualTo("/www/htdocs/w01abcde/app"));
            Assert.That(parameters["chown_user"], Is.EqualTo("w01abcde"));
            Assert.That(parameters["recursive"], Is.EqualTo("Y"));
        });
    }

    [Test]
    public void ToParameters_RecursiveDefaultsToN()
    {
        var chown = new UpdateChown { Path = "/p", User = "u" };
        Assert.That(chown.ToParameters()["recursive"], Is.EqualTo("N"));
    }

    [Test]
    [TestCase("", "u")]
    [TestCase("  ", "u")]
    [TestCase("/p", "")]
    [TestCase("/p", "  ")]
    public void ToParameters_BlankRequiredField_Throws(string path, string user)
    {
        var chown = new UpdateChown { Path = path, User = user };
        Assert.Throws<ArgumentException>(() => chown.ToParameters());
    }
}
