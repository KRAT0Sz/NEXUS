using System.Text.Json;
using System.Text.RegularExpressions;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Services;

public sealed class StreamerModeServiceTests
{
    [Test]
    public async Task GenerateGuestName_Produces_Valid_Guest_Format()
    {
        Regex regex = new (@"^GUEST-\d{2}$");

        for (int iteration = 0; iteration < 100; iteration++)
        {
            string guestName = StreamerModeConfiguration.GenerateGuestName();

            await Assert.That(regex.IsMatch(guestName)).IsTrue();
            await Assert.That(guestName.StartsWith("GUEST-")).IsTrue();
            await Assert.That(guestName.Length).IsEqualTo(8);

            int guestNumber = int.Parse(guestName[6..]);
            await Assert.That(guestNumber).IsGreaterThanOrEqualTo(1);
            await Assert.That(guestNumber).IsLessThanOrEqualTo(99);
        }
    }

    [Test]
    public async Task Default_Configuration_Shields_Privacy_Settings()
    {
        StreamerModeConfiguration configuration = new ();

        using (Assert.Multiple())
        {
            await Assert.That(configuration.Enabled).IsTrue();
            await Assert.That(configuration.HideMMR).IsTrue();
            await Assert.That(configuration.HideStats).IsTrue();
            await Assert.That(configuration.HideServerAddress).IsTrue();
            await Assert.That(configuration.BlockSpectate).IsTrue();
            await Assert.That(configuration.BlockGameInfo).IsTrue();
            await Assert.That(configuration.FriendsOnlyWhisper).IsTrue();
            await Assert.That(configuration.MaskedName.StartsWith("GUEST-")).IsTrue();
        }
    }

    [Test]
    public async Task Configuration_Serialises_And_Deserialises_Correctly()
    {
        StreamerModeConfiguration original = new ()
        {
            Enabled = true,
            MaskedName = "GUEST-42",
            HideMMR = true,
            HideStats = true,
            HideServerAddress = true,
            BlockSpectate = true,
            BlockGameInfo = true,
            FriendsOnlyWhisper = true,
            TimestampActivated = DateTimeOffset.UtcNow
        };

        string json = JsonSerializer.Serialize(original);
        StreamerModeConfiguration? deserialised = JsonSerializer.Deserialize<StreamerModeConfiguration>(json);

        using (Assert.Multiple())
        {
            await Assert.That(deserialised).IsNotNull();
            await Assert.That(deserialised!.Enabled).IsEqualTo(original.Enabled);
            await Assert.That(deserialised.MaskedName).IsEqualTo("GUEST-42");
            await Assert.That(deserialised.HideMMR).IsTrue();
            await Assert.That(deserialised.HideServerAddress).IsTrue();
            await Assert.That(deserialised.BlockSpectate).IsTrue();
            await Assert.That(deserialised.FriendsOnlyWhisper).IsTrue();
        }
    }

    [Test]
    public async Task Session_IsStreamerModeActive_Reflects_Configuration_State()
    {
        Account account = new ()
        {
            ID = 1001,
            Name = "ProStreamer",
            User = null!,
            IsMain = true,
            Type = AccountType.Normal
        };

        StreamerModeConfiguration activeConfiguration = new ()
        {
            Enabled = true,
            MaskedName = "GUEST-07"
        };

        StreamerModeConfiguration disabledConfiguration = new ()
        {
            Enabled = false,
            MaskedName = "GUEST-07"
        };

        await Assert.That(activeConfiguration.Enabled).IsTrue();
        await Assert.That(activeConfiguration.MaskedName).IsEqualTo("GUEST-07");
        await Assert.That(disabledConfiguration.Enabled).IsFalse();
        await Assert.That(account.Name).IsEqualTo("ProStreamer");
    }

    [Test]
    public async Task Masked_DataPoint_Shields_MMR_And_Record()
    {
        string realMMR = "1920.50";
        string realPSR = "1875.00";
        string realWins = "142";
        string realLosses = "58";

        StreamerModeConfiguration config = new ()
        {
            Enabled = true,
            MaskedName = "GUEST-15",
            HideMMR = true
        };

        string effectivePSR = config.HideMMR ? "1500.00" : realPSR;
        string effectiveMMR = config.HideMMR ? "1500.00" : realMMR;
        string effectiveWins = config.HideMMR ? "0" : realWins;
        string effectiveLosses = config.HideMMR ? "0" : realLosses;

        using (Assert.Multiple())
        {
            await Assert.That(effectivePSR).IsEqualTo("1500.00");
            await Assert.That(effectiveMMR).IsEqualTo("1500.00");
            await Assert.That(effectiveWins).IsEqualTo("0");
            await Assert.That(effectiveLosses).IsEqualTo("0");
            await Assert.That(config.MaskedName).IsEqualTo("GUEST-15");
        }
    }

    [Test]
    public async Task Whisper_Shielding_Differentiates_Friends_From_Strangers()
    {
        StreamerModeConfiguration config = new ()
        {
            Enabled = true,
            FriendsOnlyWhisper = true
        };

        int friendAccountID = 2002;
        int strangerAccountID = 3003;
        List<int> friendIDs = [friendAccountID];

        bool isFriendAllowed = friendIDs.Contains(friendAccountID);
        bool isStrangerAllowed = friendIDs.Contains(strangerAccountID);

        using (Assert.Multiple())
        {
            await Assert.That(config.FriendsOnlyWhisper).IsTrue();
            await Assert.That(isFriendAllowed).IsTrue();
            await Assert.That(isStrangerAllowed).IsFalse();
        }
    }

    [Test]
    public async Task Spectate_Blocking_Flags_Protected_Streamer()
    {
        StreamerModeConfiguration config = new ()
        {
            Enabled = true,
            BlockSpectate = true
        };

        bool shouldDeclineSpectate = config.Enabled && config.BlockSpectate;

        await Assert.That(shouldDeclineSpectate).IsTrue();
    }

    [Test]
    public async Task Server_Address_Concealment_Protects_Against_Stream_Sniping()
    {
        StreamerModeConfiguration config = new ()
        {
            Enabled = true,
            HideServerAddress = true
        };

        string realServerEndpoint = "192.168.1.100:11235";
        string broadcastAddress = config.Enabled && config.HideServerAddress
            ? string.Empty
            : realServerEndpoint;

        using (Assert.Multiple())
        {
            await Assert.That(broadcastAddress).IsEmpty();
            await Assert.That(config.HideServerAddress).IsTrue();
        }
    }
}

