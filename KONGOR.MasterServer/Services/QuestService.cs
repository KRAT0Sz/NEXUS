namespace KONGOR.MasterServer.Services;

using Quest = MERRICK.DatabaseContext.Entities.Economy.Quest;

/// <summary>
///     Implements management operations for The War Effort progression system, including quest assignment, reward bag progression, and payload generation.
/// </summary>
public class QuestService(MerrickContext databaseContext, ILogger<QuestService> logger) : IQuestService
{
    private MerrickContext MerrickContext { get; } = databaseContext;

    private ILogger Logger { get; } = logger;

    /// <inheritdoc/>
    public async Task EnsureQuestSeedDataAsync()
    {
        QuestConfiguration config = JSONConfiguration.QuestConfiguration;

        List<Quest> existingQuests = await MerrickContext.Quests.ToListAsync();
        Dictionary<string, Quest> existingQuestsByKey = existingQuests.ToDictionary(q => q.QuestKey, StringComparer.OrdinalIgnoreCase);

        foreach (QuestDefinitionConfiguration questConfig in config.Quests)
        {
            if (Enum.TryParse(questConfig.Category, true, out QuestCategory category).Equals(false))
                category = QuestCategory.Daily;

            if (Enum.TryParse(questConfig.TargetType, true, out QuestTargetType targetType).Equals(false))
                targetType = QuestTargetType.MatchWin;

            if (existingQuestsByKey.TryGetValue(questConfig.QuestKey, out Quest? existing))
            {
                existing.Title = questConfig.Title;
                existing.Description = questConfig.Description;
                existing.Category = category;
                existing.TargetType = targetType;
                existing.TriggerName = questConfig.TriggerName;
                existing.TargetCount = questConfig.TargetCount;
                existing.ReputationReward = questConfig.ReputationReward;
                existing.TypeID = questConfig.TypeID;
                existing.TypeKey = questConfig.TypeKey;
                existing.RewardSilver = questConfig.RewardSilver;
                existing.RewardGold = questConfig.RewardGold;
                existing.RewardTickets = questConfig.RewardTickets;
                existing.RewardUpgradeCode = questConfig.RewardUpgradeCode;
                existing.ProductIconPath = questConfig.ProductIconPath;
                existing.HeroProduct = questConfig.HeroProduct;
                existing.HeroRoleString = questConfig.HeroRoleString;
                existing.AttributeCode = questConfig.AttributeCode;
                existing.AttackType = questConfig.AttackType;
                existing.TeamID = questConfig.TeamID;
                existing.IsActive = questConfig.IsActive;
            }
            else
            {
                Quest newQuest = new ()
                {
                    QuestKey = questConfig.QuestKey,
                    Title = questConfig.Title,
                    Description = questConfig.Description,
                    Category = category,
                    TargetType = targetType,
                    TriggerName = questConfig.TriggerName,
                    TargetCount = questConfig.TargetCount,
                    ReputationReward = questConfig.ReputationReward,
                    TypeID = questConfig.TypeID,
                    TypeKey = questConfig.TypeKey,
                    RewardSilver = questConfig.RewardSilver,
                    RewardGold = questConfig.RewardGold,
                    RewardTickets = questConfig.RewardTickets,
                    RewardUpgradeCode = questConfig.RewardUpgradeCode,
                    ProductIconPath = questConfig.ProductIconPath,
                    HeroProduct = questConfig.HeroProduct,
                    HeroRoleString = questConfig.HeroRoleString,
                    AttributeCode = questConfig.AttributeCode,
                    AttackType = questConfig.AttackType,
                    TeamID = questConfig.TeamID,
                    IsActive = questConfig.IsActive
                };

                await MerrickContext.Quests.AddAsync(newQuest);
            }
        }

        List<WarEffortBag> existingBags = await MerrickContext.WarEffortBags.ToListAsync();
        Dictionary<int, WarEffortBag> existingBagsByNumber = existingBags.ToDictionary(b => b.BagNumber);

        foreach (QuestBagConfiguration bagConfig in config.Bags)
        {
            if (existingBagsByNumber.TryGetValue(bagConfig.BagNumber, out WarEffortBag? existingBag))
            {
                existingBag.Name = bagConfig.Name;
                existingBag.BagGroupID = bagConfig.BagGroupID;
                existingBag.RequiredReputation = bagConfig.RequiredReputation;
                existingBag.RewardType = bagConfig.RewardType;
                existingBag.RewardSilver = bagConfig.RewardSilver;
                existingBag.RewardGold = bagConfig.RewardGold;
                existingBag.RewardTickets = bagConfig.RewardTickets;
                existingBag.RewardProductCode = bagConfig.RewardProductCode;
                existingBag.RewardProductName = bagConfig.RewardProductName;
                existingBag.RewardProductType = bagConfig.RewardProductType;
                existingBag.RewardIconPath = bagConfig.RewardIconPath;
                existingBag.IsFinalTier = bagConfig.IsFinalTier;
                existingBag.IsActive = bagConfig.IsActive;
            }
            else
            {
                WarEffortBag newBag = new ()
                {
                    BagNumber = bagConfig.BagNumber,
                    BagGroupID = bagConfig.BagGroupID,
                    Name = bagConfig.Name,
                    RequiredReputation = bagConfig.RequiredReputation,
                    RewardType = bagConfig.RewardType,
                    RewardSilver = bagConfig.RewardSilver,
                    RewardGold = bagConfig.RewardGold,
                    RewardTickets = bagConfig.RewardTickets,
                    RewardProductCode = bagConfig.RewardProductCode,
                    RewardProductName = bagConfig.RewardProductName,
                    RewardProductType = bagConfig.RewardProductType,
                    RewardIconPath = bagConfig.RewardIconPath,
                    IsFinalTier = bagConfig.IsFinalTier,
                    IsActive = bagConfig.IsActive
                };

                await MerrickContext.WarEffortBags.AddAsync(newBag);
            }
        }

        await MerrickContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<AccountWarEffort> GetOrCreateAccountWarEffortAsync(int accountID)
    {
        AccountWarEffort? warEffort = await MerrickContext.AccountWarEfforts
            .SingleOrDefaultAsync(record => record.AccountID == accountID);

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (warEffort is null)
        {
            warEffort = new AccountWarEffort
            {
                AccountID = accountID,
                CurrentCycle = 1,
                CurrentBagNumber = 1,
                CurrentBagReputation = 0,
                TotalCycleReputation = 0,
                DailyResetsRemaining = 1,
                LastResetDate = today,
                CycleStartDate = DateTimeOffset.UtcNow,
                CycleEndDate = DateTimeOffset.UtcNow.AddDays(28)
            };

            await MerrickContext.AccountWarEfforts.AddAsync(warEffort);
            await MerrickContext.SaveChangesAsync();
        }
        else if (warEffort.LastResetDate is null || warEffort.LastResetDate < today)
        {
            warEffort.DailyResetsRemaining = 1;
            warEffort.LastResetDate = today;
            await MerrickContext.SaveChangesAsync();
        }

        return warEffort;
    }

    /// <inheritdoc/>
    public async Task<List<AccountQuestProgress>> EnsureAccountActiveQuestsAsync(int accountID)
    {
        List<AccountQuestProgress> allProgresses = await MerrickContext.AccountQuestProgresses
            .Include(p => p.Quest)
            .Where(p => p.AccountID == accountID)
            .ToListAsync();

        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset nextMidnight = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero).AddDays(1);

        List<Quest> allActiveQuests = await MerrickContext.Quests
            .Where(q => q.IsActive)
            .ToListAsync();

        if (allActiveQuests.Count == 0)
            return [];

        // Identify existing slot 0 and slot 1 assignments
        AccountQuestProgress? slot0 = allProgresses.FirstOrDefault(p => p.SlotIndex == 0);
        AccountQuestProgress? slot1 = allProgresses.FirstOrDefault(p => p.SlotIndex == 1);

        // Invalidate any other rows that may have retained SlotIndex == 0 or 1 (e.g. from migrations)
        foreach (AccountQuestProgress progress in allProgresses)
        {
            if (progress != slot0 && progress != slot1 && (progress.SlotIndex == 0 || progress.SlotIndex == 1))
            {
                progress.SlotIndex = -1;
            }
        }

        // If slot 0 and slot 1 point to the exact same quest or share the same trigger/objective, clear slot 1 for reassignment
        if (slot0 is not null && slot1 is not null && (slot0.QuestID == slot1.QuestID || slot0.Quest?.TriggerName == slot1.Quest?.TriggerName))
        {
            slot1.SlotIndex = -1;
            slot1 = null;
        }

        // Check if slot 0 needs replacement (expired and completed)
        if (slot0 is not null && slot0.ResetAt <= now && slot0.IsCompleted)
        {
            slot0.SlotIndex = -1;
            slot0 = null;
        }

        // Check if slot 1 needs replacement (expired and completed)
        if (slot1 is not null && slot1.ResetAt <= now && slot1.IsCompleted)
        {
            slot1.SlotIndex = -1;
            slot1 = null;
        }

        // Ensure slot 0 has an active quest
        if (slot0 is null)
        {
            List<int> excludedQuestIDs = slot1 is not null ? [slot1.QuestID] : [];
            List<Quest> candidates = allActiveQuests
                .Where(q => excludedQuestIDs.Contains(q.ID).Equals(false) && (slot1?.Quest is null || q.TriggerName != slot1.Quest.TriggerName))
                .ToList();
            Quest picked = candidates.Count > 0 ? candidates[Random.Shared.Next(0, candidates.Count)] : allActiveQuests[0];

            slot0 = allProgresses.FirstOrDefault(p => p.QuestID == picked.ID);

            if (slot0 is null)
            {
                slot0 = new AccountQuestProgress
                {
                    AccountID = accountID,
                    QuestID = picked.ID,
                    SlotIndex = 0,
                    CurrentCount = 0,
                    IsCompleted = false,
                    IsClaimed = false,
                    ResetAt = nextMidnight,
                    Quest = picked
                };

                await MerrickContext.AccountQuestProgresses.AddAsync(slot0);
                allProgresses.Add(slot0);
            }
            else
            {
                slot0.SlotIndex = 0;
                slot0.CurrentCount = 0;
                slot0.IsCompleted = false;
                slot0.IsClaimed = false;
                slot0.ClaimedAt = null;
                slot0.ResetAt = nextMidnight;
                slot0.Quest = picked;
            }
        }

        // Ensure slot 1 has an active quest
        if (slot1 is null)
        {
            List<int> excludedQuestIDs = [slot0.QuestID];
            List<Quest> candidates = allActiveQuests
                .Where(q => excludedQuestIDs.Contains(q.ID).Equals(false) && (slot0.Quest is null || q.TriggerName != slot0.Quest.TriggerName))
                .ToList();
            Quest picked = candidates.Count > 0 ? candidates[Random.Shared.Next(0, candidates.Count)] : allActiveQuests[0];

            slot1 = allProgresses.FirstOrDefault(p => p.QuestID == picked.ID);

            if (slot1 is null)
            {
                slot1 = new AccountQuestProgress
                {
                    AccountID = accountID,
                    QuestID = picked.ID,
                    SlotIndex = 1,
                    CurrentCount = 0,
                    IsCompleted = false,
                    IsClaimed = false,
                    ResetAt = nextMidnight,
                    Quest = picked
                };

                await MerrickContext.AccountQuestProgresses.AddAsync(slot1);
                allProgresses.Add(slot1);
            }
            else
            {
                slot1.SlotIndex = 1;
                slot1.CurrentCount = 0;
                slot1.IsCompleted = false;
                slot1.IsClaimed = false;
                slot1.ClaimedAt = null;
                slot1.ResetAt = nextMidnight;
                slot1.Quest = picked;
            }
        }

        await MerrickContext.SaveChangesAsync();

        return await MerrickContext.AccountQuestProgresses
            .Include(p => p.Quest)
            .Where(p => p.AccountID == accountID && (p.SlotIndex == 0 || p.SlotIndex == 1))
            .OrderBy(p => p.SlotIndex)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, object>> BuildCurrentQuestsDictionaryAsync(int accountID)
    {
        await EnsureQuestSeedDataAsync();

        AccountWarEffort accountWarEffort = await GetOrCreateAccountWarEffortAsync(accountID);
        List<AccountQuestProgress> activeProgresses = await EnsureAccountActiveQuestsAsync(accountID);
        List<WarEffortBag> allBags = await MerrickContext.WarEffortBags
            .Where(bag => bag.IsActive)
            .OrderBy(bag => bag.BagNumber)
            .ToListAsync();
        List<AccountCompletedBag> completedBags = await MerrickContext.AccountCompletedBags
            .Where(bag => bag.AccountID == accountID && bag.Cycle == accountWarEffort.CurrentCycle)
            .OrderBy(bag => bag.BagNumber)
            .ToListAsync();

        string timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        List<Quest> allAvailableQuests = await MerrickContext.Quests
            .Where(quest => quest.IsActive)
            .OrderBy(quest => quest.ID)
            .ToListAsync();

        // Build quest_types dictionary (0-indexed sequential array for client CPHPData::GetArray)
        Dictionary<string, object> questTypes = [];
        List<Quest> distinctQuests = allAvailableQuests
            .DistinctBy(quest => quest.TypeID)
            .ToList();

        for (int i = 0; i < distinctQuests.Count; i++)
        {
            Quest quest = distinctQuests[i];
            string questTypeDef = quest.HeroRoleString is not null && quest.HeroRoleString.Length > 0
                ? "ROLE"
                : (quest.AttributeCode.HasValue
                    ? "PRIMARY_STAT"
                    : (quest.AttackType is not null && quest.AttackType.Length > 0
                        ? "ATTACK_TYPE"
                        : (quest.HeroProduct is not null && quest.HeroProduct.Length > 0 ? "HERO" : string.Empty)));

            questTypes[i.ToString()] = new Dictionary<string, object>
            {
                { "typeid", quest.TypeID.ToString() },
                { "questtypedef", questTypeDef },
                { "questtypekey", quest.TypeKey }
            };
        }

        // Build quest_queue dictionary (0-indexed sequential array for client CPHPData::GetArray)
        Dictionary<string, object> questQueue = [];
        for (int i = 0; i < allAvailableQuests.Count; i++)
        {
            Quest quest = allAvailableQuests[i];
            Dictionary<string, object> queueQuestInfo = new ()
            {
                { "HERO", quest.HeroProduct ?? string.Empty },
                { "ROLE", quest.HeroRoleString ?? string.Empty },
                { "PRIMARY_STAT", quest.AttributeCode?.ToString() ?? string.Empty },
                { "ATTACK_TYPE", quest.AttackType ?? string.Empty },
                { "teamID", quest.TeamID?.ToString() ?? "1" },
                { "localcontent", quest.ProductIconPath ?? string.Empty }
            };

            questQueue[i.ToString()] = new Dictionary<string, object>
            {
                { "questid", quest.ID.ToString() },
                { "typeid", quest.TypeID.ToString() },
                { "questname", quest.Title },
                { "triggername", quest.TriggerName },
                { "achievement", quest.TargetCount.ToString() },
                { "questreward", quest.ReputationReward.ToString() },
                { "info", queueQuestInfo }
            };
        }

        // Build player_quest_queue
        // The game client's ProcessQuestsResponse invokes ClearAchievements(-1) at the start of each
        // outer slot iteration, which clears the internal achievement vector for slot -1.
        // Therefore, all active quests must reside as sequential 0-indexed inner entries within a single
        // outer slot ("0") so that ClearAchievements(-1) is executed only once and all quests are pushed
        // into the active vector, allowing both quest cards to display correctly.
        Dictionary<string, object> activeSlotQuests = [];

        for (int i = 0; i < activeProgresses.Count; i++)
        {
            AccountQuestProgress progress = activeProgresses[i];
            Quest? quest = progress.Quest;

            if (quest is null)
                continue;

            Dictionary<string, object> questInfo = new ()
            {
                { "HERO", quest.HeroProduct ?? string.Empty },
                { "ROLE", quest.HeroRoleString ?? string.Empty },
                { "PRIMARY_STAT", quest.AttributeCode?.ToString() ?? string.Empty },
                { "ATTACK_TYPE", quest.AttackType ?? string.Empty },
                { "teamID", quest.TeamID?.ToString() ?? "1" },
                { "localcontent", quest.ProductIconPath ?? string.Empty }
            };

            Dictionary<string, object> questEntry = new ()
            {
                { "questid", quest.ID.ToString() },
                { "typeid", quest.TypeID.ToString() },
                { "questname", quest.Title },
                { "triggername", quest.TriggerName },
                { "achievement", quest.TargetCount.ToString() },
                { "achievementstatus", progress.CurrentCount.ToString() },
                { "questreward", quest.ReputationReward.ToString() },
                { "info", questInfo }
            };

            Dictionary<string, object> slotItem = new ()
            {
                { "achievementstatus", progress.CurrentCount.ToString() },
                { "quest", questEntry }
            };

            activeSlotQuests[activeSlotQuests.Count.ToString()] = slotItem;
        }

        Dictionary<string, object> playerQuestQueue = new ()
        {
            { "0", activeSlotQuests }
        };

        // Build bag_queue (all 14 cycle bags)
        Dictionary<string, object> bagQueue = [];

        foreach (WarEffortBag bag in allBags)
        {
            Dictionary<string, object> bagInfo = [];

            if (bag.RewardType.Equals("MMPOINTS", StringComparison.OrdinalIgnoreCase))
            {
                bagInfo["MMPOINTS"] = bag.RewardSilver.ToString();
            }
            else if (bag.RewardType.Equals("POINTS", StringComparison.OrdinalIgnoreCase))
            {
                bagInfo["POINTS"] = bag.RewardGold.ToString();
            }
            else if (bag.RewardType.Equals("PRODUCT", StringComparison.OrdinalIgnoreCase))
            {
                bagInfo["PRODUCT_ID"] = new Dictionary<string, object>
                {
                    { "name", bag.RewardProductName ?? "Reward Avatar" },
                    { "type", bag.RewardProductType ?? "Alt Avatar" },
                    { "productid", bag.RewardProductCode ?? "1" },
                    { "localcontent", bag.RewardIconPath ?? "/ui/fe2/quests/quest_icons/quest_sample_01.tga" }
                };
            }

            bagQueue[bag.BagNumber.ToString()] = new Dictionary<string, object>
            {
                { "bagid", bag.BagNumber.ToString() },
                { "active", "1" },
                { "baggroupid", bag.BagGroupID.ToString() },
                { "priority", "NORMAL" },
                { "achievement", bag.RequiredReputation.ToString() },
                { "reward", bag.RewardSilver > 0 ? bag.RewardSilver.ToString() : (bag.RewardGold > 0 ? bag.RewardGold.ToString() : "1") },
                { "rewardtype", bag.RewardType },
                { "addtimestamp", timestamp },
                { "modifytimestamp", timestamp },
                { "info", bagInfo }
            };
        }

        // Build player_bag_queue (active progressing bag)
        WarEffortBag? activeBag = allBags.FirstOrDefault(bag => bag.BagNumber == accountWarEffort.CurrentBagNumber) ?? allBags.FirstOrDefault();
        Dictionary<string, object> playerBagQueue = [];

        if (activeBag is not null)
        {
            Dictionary<string, object> activeBagInner = new ()
            {
                { "bagid", activeBag.BagNumber.ToString() },
                { "active", "1" },
                { "rewardtype", activeBag.RewardType },
                { "achievement", activeBag.RequiredReputation.ToString() },
                { "achievementstatus", accountWarEffort.CurrentBagReputation.ToString() },
                { "reward", activeBag.RewardSilver > 0 ? activeBag.RewardSilver.ToString() : (activeBag.RewardGold > 0 ? activeBag.RewardGold.ToString() : "1") },
                { "baggroupid", activeBag.BagGroupID.ToString() },
                { "priority", "NORMAL" },
                { "addtimestamp", timestamp },
                { "modifytimestamp", timestamp }
            };

            Dictionary<string, object> slotItem = new ()
            {
                { "achievementstatus", accountWarEffort.CurrentBagReputation.ToString() },
                { "bag", activeBagInner }
            };

            playerBagQueue["0"] = new Dictionary<string, object>
            {
                { "0", slotItem }
            };
        }

        // Build player_bag_complete
        Dictionary<string, object> playerBagComplete = [];

        foreach (AccountCompletedBag completedBag in completedBags)
        {
            WarEffortBag? bagDef = allBags.FirstOrDefault(bag => bag.BagNumber == completedBag.BagNumber);
            Dictionary<string, object> info = [];

            if (bagDef is not null)
            {
                if (bagDef.RewardType.Equals("MMPOINTS", StringComparison.OrdinalIgnoreCase))
                {
                    info["MMPOINTS"] = bagDef.RewardSilver.ToString();
                }
                else if (bagDef.RewardType.Equals("POINTS", StringComparison.OrdinalIgnoreCase))
                {
                    info["POINTS"] = bagDef.RewardGold.ToString();
                }
                else if (bagDef.RewardType.Equals("PRODUCT", StringComparison.OrdinalIgnoreCase))
                {
                    info["PRODUCT_ID"] = new Dictionary<string, object>
                    {
                        { "name", bagDef.RewardProductName ?? "Reward Avatar" },
                        { "type", bagDef.RewardProductType ?? "Alt Avatar" },
                        { "productid", bagDef.RewardProductCode ?? "1" },
                        { "localcontent", bagDef.RewardIconPath ?? "/ui/fe2/quests/quest_icons/quest_sample_01.tga" }
                    };
                }
            }

            playerBagComplete[completedBag.BagNumber.ToString()] = new Dictionary<string, object>
            {
                { "bagid", completedBag.BagNumber.ToString() },
                { "info", info }
            };
        }

        // Assemble top-level dictionary
        return new Dictionary<string, object>
        {
            { "success", true },
            { "quest_resets_available", accountWarEffort.DailyResetsRemaining.ToString() },
            { "quest_types", questTypes },
            { "quest_queue", questQueue },
            { "player_quest_queue", playerQuestQueue },
            { "bag_queue", bagQueue },
            { "player_bag_queue", playerBagQueue },
            { "player_bag_complete", playerBagComplete }
        };
    }

    /// <inheritdoc/>
    public async Task<List<QuestLeaderboardEntry>> GetQuestLeaderboardAsync(int take = 50)
    {
        int limit = Math.Clamp(take, 1, 100);

        List<AccountWarEffort> topRecords = await MerrickContext.AccountWarEfforts
            .AsNoTracking()
            .Include(we => we.Account)
                .ThenInclude(account => account!.Clan)
            .OrderByDescending(we => we.TotalCycleReputation)
            .ThenByDescending(we => we.CurrentBagNumber)
            .Take(limit)
            .ToListAsync();

        List<int> accountIDs = topRecords.Select(we => we.AccountID).ToList();

        Dictionary<int, int> completedBagsCountByAccount = await MerrickContext.AccountCompletedBags
            .AsNoTracking()
            .Where(b => accountIDs.Contains(b.AccountID))
            .GroupBy(b => b.AccountID)
            .Select(g => new { AccountID = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AccountID, x => x.Count);

        List<QuestLeaderboardEntry> leaderboard = [];
        int currentRank = 1;

        foreach (AccountWarEffort we in topRecords)
        {
            int completedBags = completedBagsCountByAccount.TryGetValue(we.AccountID, out int count) ? count : 0;
            string accountName = we.Account?.Name ?? $"Account_{we.AccountID}";
            string clanTag = we.Account?.Clan?.Tag ?? string.Empty;

            leaderboard.Add(new QuestLeaderboardEntry
            (
                Rank: currentRank++,
                AccountID: we.AccountID,
                AccountName: accountName,
                ClanTag: clanTag,
                TotalReputation: we.TotalCycleReputation,
                CurrentBagNumber: we.CurrentBagNumber,
                CurrentCycle: we.CurrentCycle,
                CompletedBagsCount: completedBags
            ));
        }

        return leaderboard;
    }
}

