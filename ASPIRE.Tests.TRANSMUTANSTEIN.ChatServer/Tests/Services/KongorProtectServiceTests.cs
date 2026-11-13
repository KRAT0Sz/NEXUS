using Microsoft.Extensions.FileProviders;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Services;

public sealed class KongorProtectServiceTests
{
    private static KongorProtectService CreateService(string mode = "Censor", int strikeLimit = 3)
    {
        KongorProtectSettings settings = new ()
        {
            Enabled = true,
            Mode = mode,
            StrikeLimit = strikeLimit,
            StrikeWindowMinutes = 5,
            BannedWords = ["fuck", "shit", "bitch", "ควย", "เหี้ย", "สัส"],
            WhitelistWords = ["pass", "classic", "assassin", "assume"]
        };

        IOptions<KongorProtectSettings> options = Options.Create(settings);
        TestHostEnvironment environment = new () { ContentRootPath = AppContext.BaseDirectory };

        return new KongorProtectService(options, NullLogger<KongorProtectService>.Instance, environment);
    }

    [Test]
    public async Task Clean_Message_Passes_Through_Untouched()
    {
        KongorProtectService service = CreateService();
        string cleanMessage = "Good game, well played everyone!";

        KongorProtectResult result = service.Inspect(cleanMessage);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsClean).IsTrue();
            await Assert.That(result.IsBlocked).IsFalse();
            await Assert.That(result.SanitisedMessage).IsEqualTo(cleanMessage);
            await Assert.That(result.Violations.Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Whitelisted_Words_Do_Not_Trigger_False_Positives()
    {
        KongorProtectService service = CreateService();
        string tacticalCall = "Please pass the rune, assassin is missing!";

        KongorProtectResult result = service.Inspect(tacticalCall);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsClean).IsTrue();
            await Assert.That(result.SanitisedMessage).IsEqualTo(tacticalCall);
        }
    }

    [Test]
    public async Task English_Profanity_Is_Detected_And_Censored()
    {
        KongorProtectService service = CreateService();
        string toxicMessage = "What the fuck is this shit";

        KongorProtectResult result = service.Inspect(toxicMessage);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsClean).IsFalse();
            await Assert.That(result.SanitisedMessage).Contains("****");
            await Assert.That(result.SanitisedMessage).DoesNotContain("fuck");
            await Assert.That(result.SanitisedMessage).DoesNotContain("shit");
            await Assert.That(result.Violations).Contains("fuck");
            await Assert.That(result.Violations).Contains("shit");
        }
    }

    [Test]
    public async Task Thai_Profanity_Is_Detected_And_Censored()
    {
        KongorProtectService service = CreateService();
        string thaiToxic = "เล่นเหี้ยอะไรวะ ไอ้สัส";

        KongorProtectResult result = service.Inspect(thaiToxic);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsClean).IsFalse();
            await Assert.That(result.SanitisedMessage).Contains("***");
            await Assert.That(result.SanitisedMessage).DoesNotContain("เหี้ย");
            await Assert.That(result.SanitisedMessage).DoesNotContain("สัส");
            await Assert.That(result.Violations).Contains("เหี้ย");
            await Assert.That(result.Violations).Contains("สัส");
        }
    }

    [Test]
    public async Task Leetspeak_And_Character_Stretching_Are_Detected()
    {
        KongorProtectService service = CreateService();
        string stretched = "fuuuuck you";

        KongorProtectResult result = service.Inspect(stretched);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsClean).IsFalse();
            await Assert.That(result.Violations).Contains("fuck");
        }
    }

    [Test]
    public async Task Block_Mode_Sets_IsBlocked_To_True()
    {
        KongorProtectService service = CreateService(mode: "Block");
        string toxicMessage = "You are a bitch";

        KongorProtectResult result = service.Inspect(toxicMessage);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsClean).IsFalse();
            await Assert.That(result.IsBlocked).IsTrue();
        }
    }

    [Test]
    public async Task Repeated_Infractions_Trigger_Strike_Limit()
    {
        KongorProtectService service = CreateService(strikeLimit: 3);
        const int testAccountID = 9999;

        await Assert.That(service.HasExceededStrikeLimit(testAccountID)).IsFalse();

        service.Inspect("toxic fuck 1", testAccountID);
        service.Inspect("toxic fuck 2", testAccountID);
        await Assert.That(service.HasExceededStrikeLimit(testAccountID)).IsFalse();

        service.Inspect("toxic fuck 3", testAccountID);
        await Assert.That(service.HasExceededStrikeLimit(testAccountID)).IsTrue();
    }

    [Test]
    public async Task Leetspeak_Substitutions_Are_Detected_And_Masked()
    {
        KongorProtectService service = CreateService();

        KongorProtectResult bitchResult = service.Inspect("You b!tch");
        KongorProtectResult shitResult = service.Inspect("Holy sh!t");

        using (Assert.Multiple())
        {
            await Assert.That(bitchResult.IsClean).IsFalse();
            await Assert.That(bitchResult.Violations).Contains("bitch");
            await Assert.That(shitResult.IsClean).IsFalse();
            await Assert.That(shitResult.Violations).Contains("shit");
        }
    }

    [Test]
    public async Task Punctuation_Separators_Are_Detected()
    {
        KongorProtectService service = CreateService();

        KongorProtectResult englishDotted = service.Inspect("f.u.c.k this");
        KongorProtectResult thaiDotted = service.Inspect("ไอ้ ค.ว.ย");

        using (Assert.Multiple())
        {
            await Assert.That(englishDotted.IsClean).IsFalse();
            await Assert.That(englishDotted.Violations).Contains("fuck");
            await Assert.That(thaiDotted.IsClean).IsFalse();
            await Assert.That(thaiDotted.Violations).Contains("ควย");
        }
    }

    [Test]
    public async Task Disabled_Service_Does_Not_Modify_Messages()
    {
        KongorProtectSettings settings = new ()
        {
            Enabled = false,
            BannedWords = ["fuck", "shit"]
        };

        IOptions<KongorProtectSettings> options = Options.Create(settings);
        TestHostEnvironment environment = new () { ContentRootPath = AppContext.BaseDirectory };
        KongorProtectService service = new (options, NullLogger<KongorProtectService>.Instance, environment);

        const string rawToxic = "What the fuck is this";
        KongorProtectResult result = service.Inspect(rawToxic);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsClean).IsTrue();
            await Assert.That(result.SanitisedMessage).IsEqualTo(rawToxic);
        }
    }

    [Test]
    public async Task Mixed_Language_Profanity_In_Sentence_Is_Sanitised()
    {
        KongorProtectService service = CreateService();
        string mixed = "fuck you ไอ้เหี้ย";

        KongorProtectResult result = service.Inspect(mixed);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsClean).IsFalse();
            await Assert.That(result.Violations).Contains("fuck");
            await Assert.That(result.Violations).Contains("เหี้ย");
            await Assert.That(result.SanitisedMessage).DoesNotContain("fuck");
            await Assert.That(result.SanitisedMessage).DoesNotContain("เหี้ย");
        }
    }

    [Test]
    public async Task Strikes_Are_Tracked_Per_Account_Separately()
    {
        KongorProtectService service = CreateService(strikeLimit: 2);
        const int accountA = 1001;
        const int accountB = 2002;

        service.Inspect("toxic fuck 1", accountA);
        service.Inspect("toxic fuck 2", accountA);

        service.Inspect("toxic fuck 1", accountB);

        using (Assert.Multiple())
        {
            await Assert.That(service.HasExceededStrikeLimit(accountA)).IsTrue();
            await Assert.That(service.HasExceededStrikeLimit(accountB)).IsFalse();
        }
    }
}

file sealed class TestHostEnvironment : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "TRANSMUTANSTEIN.ChatServer";
    public string WebRootPath { get; set; } = string.Empty;
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; } = string.Empty;
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
}

