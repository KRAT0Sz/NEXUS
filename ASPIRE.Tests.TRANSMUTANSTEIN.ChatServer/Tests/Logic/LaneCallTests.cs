namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Logic;

public sealed class LaneCallTests
{
    [Test]
    public async Task NormalisePosition_Recognises_Middle_Lane_Variants()
    {
        using (Assert.Multiple())
        {
            await Assert.That(LaneCallHandler.NormalisePosition("mid")).IsEqualTo("Middle");
            await Assert.That(LaneCallHandler.NormalisePosition("middle")).IsEqualTo("Middle");
            await Assert.That(LaneCallHandler.NormalisePosition("/lane mid")).IsEqualTo("Middle");
            await Assert.That(LaneCallHandler.NormalisePosition("/l mid")).IsEqualTo("Middle");
            await Assert.That(LaneCallHandler.NormalisePosition("  MID  ")).IsEqualTo("Middle");
        }
    }

    [Test]
    public async Task NormalisePosition_Recognises_Short_Carry_Variants()
    {
        using (Assert.Multiple())
        {
            await Assert.That(LaneCallHandler.NormalisePosition("short")).IsEqualTo("Short Carry");
            await Assert.That(LaneCallHandler.NormalisePosition("farm")).IsEqualTo("Short Carry");
            await Assert.That(LaneCallHandler.NormalisePosition("carry")).IsEqualTo("Short Carry");
            await Assert.That(LaneCallHandler.NormalisePosition("short farm")).IsEqualTo("Short Carry");
            await Assert.That(LaneCallHandler.NormalisePosition("/lane short")).IsEqualTo("Short Carry");
        }
    }

    [Test]
    public async Task NormalisePosition_Recognises_Support_Variants()
    {
        using (Assert.Multiple())
        {
            await Assert.That(LaneCallHandler.NormalisePosition("support")).IsEqualTo("Short Support");
            await Assert.That(LaneCallHandler.NormalisePosition("sup")).IsEqualTo("Short Support");
            await Assert.That(LaneCallHandler.NormalisePosition("short sup")).IsEqualTo("Short Support");
            await Assert.That(LaneCallHandler.NormalisePosition("sup short")).IsEqualTo("Short Support");
        }
    }

    [Test]
    public async Task NormalisePosition_Recognises_OffLane_And_Jungle_Variants()
    {
        using (Assert.Multiple())
        {
            await Assert.That(LaneCallHandler.NormalisePosition("long")).IsEqualTo("Off-Lane");
            await Assert.That(LaneCallHandler.NormalisePosition("sui")).IsEqualTo("Off-Lane");
            await Assert.That(LaneCallHandler.NormalisePosition("offlane")).IsEqualTo("Off-Lane");
            await Assert.That(LaneCallHandler.NormalisePosition("off-lane")).IsEqualTo("Off-Lane");
            await Assert.That(LaneCallHandler.NormalisePosition("jungle")).IsEqualTo("Jungle");
            await Assert.That(LaneCallHandler.NormalisePosition("jg")).IsEqualTo("Jungle");
        }
    }

    [Test]
    public async Task NormalisePosition_Unknown_Command_Returns_Raw_Input()
    {
        string custom = "roaming pebbles";
        await Assert.That(LaneCallHandler.NormalisePosition(custom)).IsEqualTo(custom);
    }
}

