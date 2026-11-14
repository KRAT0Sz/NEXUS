namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Quest;

using global::KONGOR.MasterServer.Configuration;
using global::KONGOR.MasterServer.Configuration.Economy;
using MERRICK.DatabaseContext.Entities.Economy;
using QuestEntity = MERRICK.DatabaseContext.Entities.Economy.Quest;

/// <summary>
///     Pure-logic unit tests for The War Effort quest system, verifying quest queue packaging,
///     active slot assignment constraints, reroll candidate filtering, daily reset deduction, and reward bag requirements.
/// </summary>
public sealed class QuestLogicTests_Unit
{
    private static QuestEntity BuildQuest(int id, string questKey, string triggerName, int targetCount, int reputationReward, int typeID)
    {
        return new QuestEntity
        {
            ID = id,
            QuestKey = questKey,
            Title = questKey,
            Description = $"Complete {questKey}",
            Category = QuestCategory.Daily,
            TargetType = QuestTargetType.MatchWin,
            TriggerName = triggerName,
            TargetCount = targetCount,
            ReputationReward = reputationReward,
            TypeID = typeID,
            TypeKey = questKey,
            RewardSilver = 50,
            RewardTickets = 1,
            ProductIconPath = $"/ui/fe2/quests/quest_icons/{questKey}.tga",
            IsActive = true
        };
    }

    private static AccountQuestProgress BuildProgress(int accountID, int questID, int slotIndex, int currentCount, bool isCompleted, DateTimeOffset resetAt, QuestEntity quest)
    {
        return new AccountQuestProgress
        {
            AccountID = accountID,
            QuestID = questID,
            SlotIndex = slotIndex,
            CurrentCount = currentCount,
            IsCompleted = isCompleted,
            IsClaimed = false,
            ResetAt = resetAt,
            Quest = quest
        };
    }

    [Test]
    public async Task Player_Quest_Queue_Packages_Both_Quests_Under_Single_Outer_Slot_To_Prevent_Engine_Wipe()
    {
        // The game engine's ProcessQuestsResponse calls ClearAchievements(-1) at the start of each outer loop iteration.
        // If quests are placed under separate outer keys ("0" and "1"), the second iteration wipes the first quest.
        // Both active quests must therefore be packaged sequentially inside player_quest_queue["0"].
        QuestEntity quest0 = BuildQuest(1, "games_won", "achievement_value_games_won", 1, 25, 1);
        QuestEntity quest1 = BuildQuest(2, "caldavar_won", "achievement_value_caldavar_won", 1, 30, 2);

        Dictionary<string, object> quest0Data = new ()
        {
            { "questid", quest0.ID.ToString() },
            { "questtypekey", quest0.TypeKey },
            { "questname", quest0.Title },
            { "triggername", quest0.TriggerName },
            { "achievement", quest0.TargetCount.ToString() },
            { "achievementstatus", "0" },
            { "questreward", quest0.ReputationReward.ToString() }
        };

        Dictionary<string, object> quest1Data = new ()
        {
            { "questid", quest1.ID.ToString() },
            { "questtypekey", quest1.TypeKey },
            { "questname", quest1.Title },
            { "triggername", quest1.TriggerName },
            { "achievement", quest1.TargetCount.ToString() },
            { "achievementstatus", "0" },
            { "questreward", quest1.ReputationReward.ToString() }
        };

        Dictionary<string, object> innerQueue = new ()
        {
            { "0", quest0Data },
            { "1", quest1Data }
        };

        Dictionary<string, object> playerQuestQueue = new ()
        {
            { "0", innerQueue }
        };

        string serialised = PhpSerialization.Serialize(playerQuestQueue);

        using (Assert.Multiple())
        {
            await Assert.That(playerQuestQueue.ContainsKey("0")).IsTrue();
            await Assert.That(playerQuestQueue.ContainsKey("1")).IsFalse();

            Dictionary<string, object> outerSlot0 = (Dictionary<string, object>) playerQuestQueue["0"];
            await Assert.That(outerSlot0.ContainsKey("0")).IsTrue();
            await Assert.That(outerSlot0.ContainsKey("1")).IsTrue();

            // Verify PHP serialisation preserves the nested structure
            await Assert.That(serialised.Contains(@"s:1:""0"";a:2:{s:1:""0"";a:7:{")).IsTrue();
            await Assert.That(serialised.Contains(@"s:1:""1"";a:7:{")).IsTrue();
        }
    }

    [Test]
    public async Task Active_Quest_Assignment_Requires_Distinct_Quest_IDs_And_Triggers()
    {
        // When assigning quests to active slots 0 and 1, they must never have the same QuestID or TriggerName.
        QuestEntity quest1 = BuildQuest(1, "games_won_standard", "achievement_value_games_won", 1, 25, 1);
        QuestEntity quest2WithSameTrigger = BuildQuest(2, "games_won_duplicate", "achievement_value_games_won", 1, 25, 1);
        QuestEntity quest3WithDifferentTrigger = BuildQuest(3, "caldavar_won", "achievement_value_caldavar_won", 1, 30, 2);

        // Same trigger collision check
        bool isInvalidPairSameTrigger = quest1.ID == quest2WithSameTrigger.ID || quest1.TriggerName == quest2WithSameTrigger.TriggerName;
        await Assert.That(isInvalidPairSameTrigger).IsTrue();

        // Valid pair check
        bool isValidPair = quest1.ID != quest3WithDifferentTrigger.ID && quest1.TriggerName != quest3WithDifferentTrigger.TriggerName;
        await Assert.That(isValidPair).IsTrue();
    }

    [Test]
    public async Task Reroll_Candidate_Pool_Excludes_Both_Active_Quests_And_Shared_Triggers()
    {
        // Setup a catalogue of 5 quests
        QuestEntity quest1 = BuildQuest(1, "games_won", "achievement_value_games_won", 1, 25, 1);
        QuestEntity quest2 = BuildQuest(2, "caldavar_won", "achievement_value_caldavar_won", 1, 30, 2);
        QuestEntity quest3 = BuildQuest(3, "mid_wars_won", "achievement_value_mid_wars_won", 1, 30, 3);
        QuestEntity quest4 = BuildQuest(4, "strength_won", "achievement_value_strength_won", 1, 40, 4);
        QuestEntity quest5DuplicateTrigger = BuildQuest(5, "strength_alternate", "achievement_value_strength_won", 1, 40, 4);

        List<QuestEntity> allQuests = [quest1, quest2, quest3, quest4, quest5DuplicateTrigger];

        // Currently, slot 0 has quest 1 and slot 1 has quest 4
        List<int> currentActiveQuestIDs = [quest1.ID, quest4.ID];
        List<string> remainingActiveTriggers = [quest4.TriggerName]; // slot 1 trigger

        // Filter replacement candidates
        List<QuestEntity> candidates = allQuests
            .Where(q => q.IsActive
                && currentActiveQuestIDs.Contains(q.ID).Equals(false)
                && remainingActiveTriggers.Contains(q.TriggerName).Equals(false))
            .ToList();

        using (Assert.Multiple())
        {
            // Quest 1 is excluded because it is being rerolled
            await Assert.That(candidates.Any(q => q.ID == 1)).IsFalse();

            // Quest 4 is excluded because it is already active in slot 1
            await Assert.That(candidates.Any(q => q.ID == 4)).IsFalse();

            // Quest 5 is excluded because it shares the same TriggerName as slot 1
            await Assert.That(candidates.Any(q => q.ID == 5)).IsFalse();

            // Quests 2 and 3 are valid replacements
            await Assert.That(candidates.Count).IsEqualTo(2);
            await Assert.That(candidates.Any(q => q.ID == 2)).IsTrue();
            await Assert.That(candidates.Any(q => q.ID == 3)).IsTrue();
        }
    }

    [Test]
    public async Task Reroll_Consumes_Daily_Reset_And_Guards_Against_Depletion()
    {
        AccountWarEffort accountWarEffort = new ()
        {
            AccountID = 10,
            CurrentCycle = 1,
            TotalCycleReputation = 0,
            DailyResetsRemaining = 1,
            LastResetDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // First reroll succeeds and decrements reset count
        bool canRerollFirstTime = accountWarEffort.DailyResetsRemaining > 0;
        await Assert.That(canRerollFirstTime).IsTrue();

        accountWarEffort.DailyResetsRemaining--;
        await Assert.That(accountWarEffort.DailyResetsRemaining).IsEqualTo(0);

        // Second reroll fails because resets are exhausted
        bool canRerollSecondTime = accountWarEffort.DailyResetsRemaining > 0;
        await Assert.That(canRerollSecondTime).IsFalse();
    }

    [Test]
    public async Task Completed_Expired_Quests_Are_Identified_For_Replacement()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset past = now.AddHours(-2);
        DateTimeOffset future = now.AddHours(12);

        QuestEntity quest = BuildQuest(1, "games_won", "achievement_value_games_won", 1, 25, 1);

        // Case 1: Completed and expired -> Needs replacement
        AccountQuestProgress completedExpired = BuildProgress(1, 1, 0, 1, true, past, quest);
        bool needsReplacement1 = completedExpired.ResetAt <= now && completedExpired.IsCompleted;
        await Assert.That(needsReplacement1).IsTrue();

        // Case 2: Incomplete and past midnight -> Retained (player can continue completing it)
        AccountQuestProgress incompleteExpired = BuildProgress(1, 1, 0, 0, false, past, quest);
        bool needsReplacement2 = incompleteExpired.ResetAt <= now && incompleteExpired.IsCompleted;
        await Assert.That(needsReplacement2).IsFalse();

        // Case 3: Completed but not yet past reset timestamp -> Retained until reset window
        AccountQuestProgress completedUnexpired = BuildProgress(1, 1, 0, 1, true, future, quest);
        bool needsReplacement3 = completedUnexpired.ResetAt <= now && completedUnexpired.IsCompleted;
        await Assert.That(needsReplacement3).IsFalse();
    }

    [Test]
    [Arguments(50, 100, false)]
    [Arguments(99, 100, false)]
    [Arguments(100, 100, true)]
    [Arguments(250, 100, true)]
    public async Task War_Effort_Bag_Unlocks_Only_When_Required_Reputation_Is_Met(int earnedReputation, int requiredReputation, bool expectedUnlocked)
    {
        bool isUnlocked = earnedReputation >= requiredReputation;
        await Assert.That(isUnlocked).IsEqualTo(expectedUnlocked);
    }

    [Test]
    public async Task Reward_Bag_Information_Dictionary_Is_Constructed_Appropriately_For_Each_Reward_Type()
    {
        // 1. Silver reward type ("MMPOINTS")
        WarEffortBag silverBag = new ()
        {
            BagNumber = 1,
            BagGroupID = 1,
            Name = "Copper Spoils",
            RequiredReputation = 50,
            RewardType = "MMPOINTS",
            RewardSilver = 100,
            RewardGold = 0,
            RewardTickets = 1,
            IsActive = true
        };

        Dictionary<string, object> silverInfo = [];
        if (silverBag.RewardType.Equals("MMPOINTS", StringComparison.OrdinalIgnoreCase))
        {
            silverInfo["MMPOINTS"] = silverBag.RewardSilver.ToString();
        }

        await Assert.That(silverInfo.ContainsKey("MMPOINTS")).IsTrue();
        await Assert.That(silverInfo["MMPOINTS"]).IsEqualTo("100");

        // 2. Gold reward type ("POINTS")
        WarEffortBag goldBag = new ()
        {
            BagNumber = 4,
            BagGroupID = 1,
            Name = "Golden Trove",
            RequiredReputation = 300,
            RewardType = "POINTS",
            RewardSilver = 0,
            RewardGold = 50,
            RewardTickets = 2,
            IsActive = true
        };

        Dictionary<string, object> goldInfo = [];
        if (goldBag.RewardType.Equals("POINTS", StringComparison.OrdinalIgnoreCase))
        {
            goldInfo["POINTS"] = goldBag.RewardGold.ToString();
        }

        await Assert.That(goldInfo.ContainsKey("POINTS")).IsTrue();
        await Assert.That(goldInfo["POINTS"]).IsEqualTo("50");

        // 3. Avatar Product reward type ("PRODUCT")
        WarEffortBag productBag = new ()
        {
            BagNumber = 6,
            BagGroupID = 1,
            Name = "Grand Hero Cache",
            RequiredReputation = 600,
            RewardType = "PRODUCT",
            RewardProductName = "Valkyrie General",
            RewardProductType = "Alt Avatar",
            RewardProductCode = "aa.Valkyrie.Alt4",
            RewardIconPath = "/ui/fe2/quests/quest_icons/quest_sample_01.tga",
            IsActive = true
        };

        Dictionary<string, object> productInfo = [];
        if (productBag.RewardType.Equals("PRODUCT", StringComparison.OrdinalIgnoreCase))
        {
            productInfo["PRODUCT_ID"] = new Dictionary<string, object>
            {
                { "name", productBag.RewardProductName ?? "Reward Avatar" },
                { "type", productBag.RewardProductType ?? "Alt Avatar" },
                { "productid", productBag.RewardProductCode ?? "1" },
                { "localcontent", productBag.RewardIconPath ?? "/ui/fe2/quests/quest_icons/quest_sample_01.tga" }
            };
        }

        await Assert.That(productInfo.ContainsKey("PRODUCT_ID")).IsTrue();
        Dictionary<string, object> productDetails = (Dictionary<string, object>) productInfo["PRODUCT_ID"];
        await Assert.That(productDetails["name"]).IsEqualTo("Valkyrie General");
        await Assert.That(productDetails["productid"]).IsEqualTo("aa.Valkyrie.Alt4");
    }

    [Test]
    public async Task Quest_Configuration_Loads_All_14_Bags_And_Quests_Correctly()
    {
        QuestConfiguration configuration = JSONConfiguration.QuestConfiguration;

        using (Assert.Multiple())
        {
            await Assert.That(configuration.CycleDurationDays).IsEqualTo(28);
            await Assert.That(configuration.DailyResetsAvailable).IsEqualTo(1);
            await Assert.That(configuration.Bags.Count).IsEqualTo(14);
            await Assert.That(configuration.Quests.Count).IsGreaterThanOrEqualTo(8);

            // Final tier bag (Bag 14) awards the seasonal Avatar
            QuestBagConfiguration? finalBag = configuration.Bags.FirstOrDefault(b => b.BagNumber == 14);
            await Assert.That(finalBag).IsNotNull();
            await Assert.That(finalBag!.IsFinalTier).IsTrue();
            await Assert.That(finalBag.RewardType).IsEqualTo("PRODUCT");
            await Assert.That(finalBag.RewardProductCode).IsNotNull();
            await Assert.That(finalBag.RewardProductName).IsNotNull();
        }
    }
}
