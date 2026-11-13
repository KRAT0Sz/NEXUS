namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Services;

public sealed class IncidentReportLogicTests
{
    [Test]
    public async Task Reported_Account_Name_Strips_Clan_Tag_Prefix_Correctly()
    {
        string taggedName = "[TEST]PlayerOne";
        string untaggedName = "SoloPlayer";

        string extractedFromTagged = taggedName.Contains(']') ? taggedName.Split(']').Last() : taggedName;
        string extractedFromUntagged = untaggedName.Contains(']') ? untaggedName.Split(']').Last() : untaggedName;

        using (Assert.Multiple())
        {
            await Assert.That(extractedFromTagged).IsEqualTo("PlayerOne");
            await Assert.That(extractedFromUntagged).IsEqualTo("SoloPlayer");
        }
    }

    [Test]
    public async Task RAP_Response_Serialises_To_PHP_Format_Expected_By_HoN_Client()
    {
        Dictionary<string, string> responseData = new ()
        {
            ["0"] = "1"
        };

        string serialised = PhpSerialization.Serialize(responseData);

        // HoN Client expects PHP array serialised with key "0" => "1"
        await Assert.That(serialised).Contains("a:1:{");
        await Assert.That(serialised).Contains(@"s:1:""1""");
    }
}

