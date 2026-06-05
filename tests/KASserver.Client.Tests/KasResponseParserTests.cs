using KASserver;
using KASserver.Soap;

namespace KASserver.Client.Tests;

public class KasResponseParserTests
{
    private const string SoapPrefix =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
        "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
        "xmlns:ns1=\"https://kasapi.kasserver.com/soap/KasApi.php\" " +
        "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
        "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
        "xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" " +
        "xmlns:ns2=\"http://xml.apache.org/xml-soap\">";

    private const string SoapSuffix = "</SOAP-ENV:Envelope>";

    [Test]
    public void Parse_ScalarReturnInfo_ExtractsValueAndFloodDelay()
    {
        // add_mailaccount-style response: ReturnInfo is the generated mail_login scalar.
        var xml = Envelope(Map(
            Kv("KasFloodDelay", "0.5", "xsd:float") +
            Kv("ReturnString", "TRUE", "xsd:string") +
            Kv("ReturnInfo", "m07f821c", "xsd:string")));

        var response = KasResponseParser.Parse(xml);

        Assert.Multiple(() =>
        {
            Assert.That(response.ReturnString, Is.EqualTo("TRUE"));
            Assert.That(response.ReturnInfo, Is.EqualTo("m07f821c"));
            Assert.That(response.FloodDelay, Is.EqualTo(0.5));
        });
    }

    [Test]
    public void Parse_NestedMapReturnInfo_ProducesNestedDictionaries()
    {
        var xml = Envelope(Map(
            Kv("ReturnString", "TRUE", "xsd:string") +
            ItemMap("ReturnInfo", Map(
                ItemMap("settings", Map(
                    Kv("account_login", "w0test99", "xsd:string")))))));

        var response = KasResponseParser.Parse(xml);
        var settings = response.AsMap()["settings"] as IReadOnlyDictionary<string, object?>;

        Assert.That(settings, Is.Not.Null);
        Assert.That(settings!["account_login"], Is.EqualTo("w0test99"));
    }

    [Test]
    public void Parse_ArrayReturnInfo_ProducesListOfMaps()
    {
        // get_mailaccounts-style response: ReturnInfo is an array of maps.
        var xml = Envelope(Map(
            Kv("ReturnString", "TRUE", "xsd:string") +
            ItemMap("ReturnInfo", Array(
                MapItem(Kv("mail_login", "m07f821c", "xsd:string")) +
                MapItem(Kv("mail_login", "m0abcdef", "xsd:string"))))));

        var response = KasResponseParser.Parse(xml);
        var list = response.AsList();

        Assert.That(list, Has.Count.EqualTo(2));
        Assert.That(list[0]["mail_login"], Is.EqualTo("m07f821c"));
        Assert.That(list[1]["mail_login"], Is.EqualTo("m0abcdef"));
    }

    [Test]
    public void Parse_EmptyArrayReturnInfo_ProducesEmptyList()
    {
        // Empty account: get_mailaccounts returns an empty array — a legitimate "no results".
        var xml = Envelope(Map(
            Kv("ReturnString", "TRUE", "xsd:string") +
            ItemMap("ReturnInfo", Array(string.Empty))));

        var response = KasResponseParser.Parse(xml);

        Assert.That(response.AsList(), Is.Empty);
    }

    [Test]
    public void Parse_EmptyStringField_StaysEmptyNotNull()
    {
        var xml = Envelope(Map(
            Kv("ReturnString", "TRUE", "xsd:string") +
            ItemMap("ReturnInfo", Map(
                Kv("ssh_keys", string.Empty, "xsd:string")))));

        var info = KasResponseParser.Parse(xml).AsMap();

        Assert.That(info["ssh_keys"], Is.EqualTo(string.Empty));
    }

    [Test]
    public void Parse_TypedNilMap_BecomesNull()
    {
        var xml = Envelope(Map(
            Kv("ReturnString", "TRUE", "xsd:string") +
            ItemMap("ReturnInfo", Map(
                ItemMap("optional_block", "<value xsi:type=\"ns2:Map\" xsi:nil=\"true\"/>")))));

        var info = KasResponseParser.Parse(xml).AsMap();

        Assert.That(info["optional_block"], Is.Null);
    }

    [Test]
    public void Parse_MixedKeyedAndUnkeyedItems_Throws()
    {
        var mixed = "<value>" +
            "<item><key xsi:type=\"xsd:string\">a</key><value xsi:type=\"xsd:string\">b</value></item>" +
            "<item xsi:type=\"xsd:string\">c</item>" +
            "</value>";
        var xml = Envelope(Map(
            Kv("ReturnString", "TRUE", "xsd:string") +
            ItemMap("ReturnInfo", mixed)));

        Assert.Throws<KasApiException>(() => KasResponseParser.Parse(xml));
    }

    [Test]
    public void Parse_DeclaredMapWithKeylessItem_Throws()
    {
        var badMap = "<value xsi:type=\"ns2:Map\"><item><value xsi:type=\"xsd:string\">x</value></item></value>";
        var xml = Envelope(Map(
            Kv("ReturnString", "TRUE", "xsd:string") +
            ItemMap("ReturnInfo", badMap)));

        Assert.Throws<KasApiException>(() => KasResponseParser.Parse(xml));
    }

    [Test]
    public void Parse_HugeFloodDelay_IsClampedToMaximum()
    {
        var xml = Envelope(Map(
            Kv("KasFloodDelay", "1e30", "xsd:float") +
            Kv("ReturnString", "TRUE", "xsd:string")));

        var response = KasResponseParser.Parse(xml);

        Assert.That(response.FloodDelay, Is.EqualTo(60d));
    }

    [Test]
    public void Parse_NonFiniteFloodDelay_BecomesZero()
    {
        var xml = Envelope(Map(
            Kv("KasFloodDelay", "Infinity", "xsd:float") +
            Kv("ReturnString", "TRUE", "xsd:string")));

        var response = KasResponseParser.Parse(xml);

        Assert.That(response.FloodDelay, Is.EqualTo(0d));
    }

    [Test]
    public void Parse_ScalarReturn_ThrowsInsteadOfMaskingAsEmpty()
    {
        // e.g. a maintenance page rendered as a SOAP scalar — must not look like an empty success.
        var xml = SoapPrefix +
            "<SOAP-ENV:Body><ns1:KasApiResponse>" +
            "<return xsi:type=\"xsd:string\">maintenance</return>" +
            "</ns1:KasApiResponse></SOAP-ENV:Body>" + SoapSuffix;

        Assert.Throws<KasApiException>(() => KasResponseParser.Parse(xml, "get_mailaccounts"));
    }

    [Test]
    public void Parse_NonXmlBody_ThrowsKasApiException()
    {
        Assert.Throws<KasApiException>(() => KasResponseParser.Parse("<html>502 Bad Gateway", "get_domains"));
    }

    [Test]
    public void Parse_Fault_ThrowsKasApiExceptionWithFaultCode()
    {
        var xml = SoapPrefix +
            "<SOAP-ENV:Body><SOAP-ENV:Fault>" +
            "<faultcode>SOAP-ENV:Server</faultcode>" +
            "<faultstring>email_already_exists</faultstring>" +
            "</SOAP-ENV:Fault></SOAP-ENV:Body>" + SoapSuffix;

        var ex = Assert.Throws<KasApiException>(() => KasResponseParser.Parse(xml, "add_mailaccount"));
        Assert.Multiple(() =>
        {
            Assert.That(ex!.FaultCode, Is.EqualTo("email_already_exists"));
            Assert.That(ex.Action, Is.EqualTo("add_mailaccount"));
        });
    }

    [Test]
    public void ExtractReturnText_AuthResponse_ReturnsToken()
    {
        var xml = SoapPrefix +
            "<SOAP-ENV:Body><ns1:KasAuthResponse>" +
            "<return xsi:type=\"xsd:string\">7a5e7608503c7ff7d0718e59150953d3eb2d7e49</return>" +
            "</ns1:KasAuthResponse></SOAP-ENV:Body>" + SoapSuffix;

        var token = KasResponseParser.ExtractReturnText(xml);

        Assert.That(token, Is.EqualTo("7a5e7608503c7ff7d0718e59150953d3eb2d7e49"));
    }

    // --- helpers to build the Apache xml-soap Map/Array structure ---

    private static string Envelope(string returnMapValue) =>
        SoapPrefix +
        "<SOAP-ENV:Body><ns1:KasApiResponse><return xsi:type=\"ns2:Map\">" +
        Item("Response", returnMapValue) +
        "</return></ns1:KasApiResponse></SOAP-ENV:Body>" + SoapSuffix;

    private static string Map(string items) =>
        $"<value xsi:type=\"ns2:Map\">{items}</value>";

    private static string Array(string items) =>
        $"<value xsi:type=\"SOAP-ENC:Array\" SOAP-ENC:arrayType=\"ns2:Map[0]\">{items}</value>";

    private static string MapItem(string mapEntries) =>
        $"<item xsi:type=\"ns2:Map\">{mapEntries}</item>";

    private static string Kv(string key, string value, string type) =>
        "<item>" +
        $"<key xsi:type=\"xsd:string\">{key}</key>" +
        $"<value xsi:type=\"{type}\">{value}</value>" +
        "</item>";

    private static string ItemMap(string key, string mapValue) =>
        "<item>" +
        $"<key xsi:type=\"xsd:string\">{key}</key>" +
        mapValue +
        "</item>";

    private static string Item(string key, string mapValue) => ItemMap(key, mapValue);
}
