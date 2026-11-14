namespace KONGOR.MasterServer.Controllers.Quest;

using Quest = MERRICK.DatabaseContext.Entities.Economy.Quest;

[ApiController]
[Consumes("application/x-www-form-urlencoded")]
public class QuestController(MerrickContext databaseContext, IDatabase distributedCache, IQuestService questService, ILogger<QuestController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;

    private IDatabase DistributedCache { get; } = distributedCache;

    private IQuestService QuestService { get; } = questService;

    private ILogger Logger { get; } = logger;

    /// <summary>
    ///     Returns the quests and reward bags currently assigned to the requesting client's account for The War Effort.
    /// </summary>
    [HttpPost("master/quest/getcurrentquests", Name = "Get Current Quests")]
    public async Task<IActionResult> GetCurrentQuests()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Get Current Quests Request With Invalid Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        Account account = await MerrickContext.Accounts
            .Include(queriedAccount => queriedAccount.User)
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        Dictionary<string, object> questDictionary = await QuestService.BuildCurrentQuestsDictionaryAsync(account.ID);
        string payload = PhpSerialization.Serialize(questDictionary);

        return Content(payload, "text/plain");
    }

    /// <summary>
    ///     Resets one of the requesting client's quests and returns the replacement assigned in its place.
    /// </summary>
    [HttpPost("master/quest/resetquest", Name = "Reset Quest")]
    public async Task<IActionResult> ResetQuest()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Reset Quest Request With Invalid Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        string? questIDValue = Request.Form["quest_id"];

        if (questIDValue is null || int.TryParse(questIDValue, out int questID).Equals(false))
            return BadRequest(@"Missing Or Invalid Value For Form Parameter ""quest_id""");

        Account account = await MerrickContext.Accounts
            .Include(queriedAccount => queriedAccount.User)
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        AccountWarEffort accountWarEffort = await QuestService.GetOrCreateAccountWarEffortAsync(account.ID);

        if (accountWarEffort.DailyResetsRemaining <= 0)
        {
            Dictionary<string, object> errorResponse = new ()
            {
                { "success", false },
                { "error", "no_resets_available" }
            };

            return Content(PhpSerialization.Serialize(errorResponse), "text/plain");
        }

        AccountQuestProgress? targetProgress = await MerrickContext.AccountQuestProgresses
            .Include(progress => progress.Quest)
            .SingleOrDefaultAsync(progress => progress.AccountID == account.ID && progress.QuestID == questID);

        if (targetProgress is null || (targetProgress.SlotIndex != 0 && targetProgress.SlotIndex != 1))
        {
            Dictionary<string, object> errorResponse = new ()
            {
                { "success", false },
                { "error", "quest_not_found" }
            };

            return Content(PhpSerialization.Serialize(errorResponse), "text/plain");
        }

        int slotIndex = targetProgress.SlotIndex;

        List<int> currentActiveQuestIDs = await MerrickContext.AccountQuestProgresses
            .Where(progress => progress.AccountID == account.ID && (progress.SlotIndex == 0 || progress.SlotIndex == 1))
            .Select(progress => progress.QuestID)
            .ToListAsync();

        List<string> otherActiveTriggers = await MerrickContext.AccountQuestProgresses
            .Include(progress => progress.Quest)
            .Where(progress => progress.AccountID == account.ID && progress.QuestID != questID && (progress.SlotIndex == 0 || progress.SlotIndex == 1) && progress.Quest != null)
            .Select(progress => progress.Quest!.TriggerName)
            .ToListAsync();

        List<Quest> availableReplacementQuests = await MerrickContext.Quests
            .Where(quest => quest.IsActive
                && currentActiveQuestIDs.Contains(quest.ID).Equals(false)
                && otherActiveTriggers.Contains(quest.TriggerName).Equals(false))
            .ToListAsync();

        if (availableReplacementQuests.Count == 0)
        {
            availableReplacementQuests = await MerrickContext.Quests
                .Where(quest => quest.IsActive && currentActiveQuestIDs.Contains(quest.ID).Equals(false))
                .ToListAsync();
        }

        if (availableReplacementQuests.Count > 0)
        {
            int randomIndex = Random.Shared.Next(0, availableReplacementQuests.Count);
            Quest replacement = availableReplacementQuests[randomIndex];

            targetProgress.SlotIndex = -1;

            AccountQuestProgress? replacementProgress = await MerrickContext.AccountQuestProgresses
                .SingleOrDefaultAsync(progress => progress.AccountID == account.ID && progress.QuestID == replacement.ID);

            DateTimeOffset nextMidnight = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, TimeSpan.Zero).AddDays(1);

            if (replacementProgress is null)
            {
                replacementProgress = new AccountQuestProgress
                {
                    AccountID = account.ID,
                    QuestID = replacement.ID,
                    SlotIndex = slotIndex,
                    CurrentCount = 0,
                    IsCompleted = false,
                    IsClaimed = false,
                    ResetAt = nextMidnight
                };

                await MerrickContext.AccountQuestProgresses.AddAsync(replacementProgress);
            }
            else
            {
                replacementProgress.SlotIndex = slotIndex;
                replacementProgress.CurrentCount = 0;
                replacementProgress.IsCompleted = false;
                replacementProgress.IsClaimed = false;
                replacementProgress.ClaimedAt = null;
                replacementProgress.ResetAt = nextMidnight;
            }
        }

        accountWarEffort.DailyResetsRemaining--;
        await MerrickContext.SaveChangesAsync();

        Dictionary<string, object> questDictionary = await QuestService.BuildCurrentQuestsDictionaryAsync(account.ID);
        string payload = PhpSerialization.Serialize(questDictionary);

        return Content(payload, "text/plain");
    }

    /// <summary>
    ///     Opens a completed reward bag for the requesting client's account and grants its reward.
    /// </summary>
    [HttpPost("master/quest/openbag", Name = "Open Bag")]
    public async Task<IActionResult> OpenBag()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Open Bag Request With Invalid Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        string? bagIDValue = Request.Form["bag_id"];

        if (bagIDValue is null || int.TryParse(bagIDValue, out int bagNumber).Equals(false))
            return BadRequest(@"Missing Or Invalid Value For Form Parameter ""bag_id""");

        Account account = await MerrickContext.Accounts
            .Include(queriedAccount => queriedAccount.User)
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        AccountWarEffort accountWarEffort = await QuestService.GetOrCreateAccountWarEffortAsync(account.ID);

        AccountCompletedBag? completedBag = await MerrickContext.AccountCompletedBags
            .SingleOrDefaultAsync(bag => bag.AccountID == account.ID
                && bag.BagNumber == bagNumber
                && bag.Cycle == accountWarEffort.CurrentCycle
                && bag.IsOpened.Equals(false));

        WarEffortBag? bagDefinition = await MerrickContext.WarEffortBags
            .SingleOrDefaultAsync(bag => bag.BagNumber == bagNumber && bag.IsActive);

        if (completedBag is null || bagDefinition is null)
        {
            Dictionary<string, object> errorResponse = new ()
            {
                { "success", false },
                { "error", "open_reward_request_fail" }
            };

            return Content(PhpSerialization.Serialize(errorResponse), "text/plain");
        }

        completedBag.IsOpened = true;
        completedBag.OpenedAt = DateTimeOffset.UtcNow;
        completedBag.GrantedSilver = bagDefinition.RewardSilver;
        completedBag.GrantedGold = bagDefinition.RewardGold;
        completedBag.GrantedTickets = bagDefinition.RewardTickets;
        completedBag.GrantedProductCode = bagDefinition.RewardProductCode;

        account.User.SilverCoins += bagDefinition.RewardSilver;
        account.User.GoldCoins += bagDefinition.RewardGold;
        account.User.PlinkoTickets += bagDefinition.RewardTickets;

        if (string.IsNullOrWhiteSpace(bagDefinition.RewardProductCode) is false
            && account.User.OwnedStoreItems.Contains(bagDefinition.RewardProductCode).Equals(false))
        {
            account.User.OwnedStoreItems.Add(bagDefinition.RewardProductCode);
        }

        await MerrickContext.SaveChangesAsync();

        string timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        Dictionary<string, object> bagDictionary = new ()
        {
            { "bagid", bagDefinition.BagNumber.ToString() },
            { "active", "1" },
            { "baggroupid", bagDefinition.BagGroupID.ToString() },
            { "priority", "NORMAL" },
            { "achievement", bagDefinition.RequiredReputation.ToString() },
            { "reward", bagDefinition.RewardSilver > 0 ? bagDefinition.RewardSilver.ToString() : (bagDefinition.RewardGold > 0 ? bagDefinition.RewardGold.ToString() : "1") },
            { "rewardtype", bagDefinition.RewardType },
            { "addtimestamp", timestamp },
            { "modifytimestamp", timestamp }
        };

        Dictionary<string, object> infoDictionary = [];

        if (bagDefinition.RewardType.Equals("MMPOINTS", StringComparison.OrdinalIgnoreCase))
        {
            infoDictionary["MMPOINTS"] = bagDefinition.RewardSilver.ToString();
        }
        else if (bagDefinition.RewardType.Equals("POINTS", StringComparison.OrdinalIgnoreCase))
        {
            infoDictionary["POINTS"] = bagDefinition.RewardGold.ToString();
        }
        else if (bagDefinition.RewardType.Equals("PRODUCT", StringComparison.OrdinalIgnoreCase))
        {
            infoDictionary["PRODUCT_ID"] = new Dictionary<string, object>
            {
                { "name", bagDefinition.RewardProductName ?? "Reward Avatar" },
                { "type", bagDefinition.RewardProductType ?? "Alt Avatar" },
                { "productid", bagDefinition.RewardProductCode ?? "1" },
                { "localcontent", bagDefinition.RewardIconPath ?? "/ui/fe2/quests/quest_icons/quest_sample_01.tga" }
            };
        }
        else
        {
            infoDictionary["MMPOINTS"] = bagDefinition.RewardSilver.ToString();
        }

        Dictionary<string, object> openResponse = new ()
        {
            { "success", true },
            { "mmpoints", bagDefinition.RewardSilver },
            { "points", bagDefinition.RewardGold },
            { "product_ids", string.IsNullOrWhiteSpace(bagDefinition.RewardProductCode) ? 0 : 1 },
            { "bag", bagDictionary },
            { "info", infoDictionary }
        };

        Logger.LogInformation(
            @"Account ""{AccountName}"" Opened War Effort Bag #{BagNumber} (+{Silver} Silver, +{Gold} Gold, +{Tickets} Tickets)",
            account.Name, bagNumber, bagDefinition.RewardSilver, bagDefinition.RewardGold, bagDefinition.RewardTickets
        );

        return Content(PhpSerialization.Serialize(openResponse), "text/plain");
    }

    /// <summary>
    ///     Returns the quests currently assigned to the accounts named in the request, on behalf of the requesting game server.
    /// </summary>
    [HttpPost("master/questserver/getplayerquests", Name = "Get Player Quests")]
    public async Task<IActionResult> GetPlayerQuests()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(cookie);

        if (matchServer is null)
        {
            Logger.LogWarning(@"Get Player Quests Request With Invalid Server Session Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        List<int> accountIDs = [];

        foreach (string? accountIDValue in Request.Form["account_ids[]"])
        {
            if (int.TryParse(accountIDValue, out int accountID))
                accountIDs.Add(accountID);
        }

        Dictionary<string, object> response = [];

        foreach (int accountID in accountIDs)
        {
            List<AccountQuestProgress> activeProgresses = await QuestService.EnsureAccountActiveQuestsAsync(accountID);
            Dictionary<string, object> playerQuests = [];

            for (int i = 0; i < activeProgresses.Count; i++)
            {
                AccountQuestProgress progress = activeProgresses[i];
                Quest? quest = progress.Quest;

                if (quest is null)
                    continue;

                playerQuests[i.ToString()] = new Dictionary<string, object>
                {
                    { "questid", quest.ID.ToString() },
                    { "triggername", quest.TriggerName },
                    { "achievement", quest.TargetCount.ToString() },
                    { "achievementstatus", progress.CurrentCount.ToString() }
                };
            }

            response[accountID.ToString()] = playerQuests;
        }

        return Content(PhpSerialization.Serialize(response), "text/plain");
    }

    /// <summary>
    ///     Returns the quest type and quest queue definitions, along with the bag cycle information, on behalf of the requesting game server.
    /// </summary>
    [HttpPost("master/questserver/getsysteminfo", Name = "Get System Information")]
    public async Task<IActionResult> GetSystemInformation()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(cookie);

        if (matchServer is null)
        {
            Logger.LogWarning(@"Get System Information Request With Invalid Server Session Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        await QuestService.EnsureWarEffortSeedDataAsync();

        List<WarEffortBag> bags = await MerrickContext.WarEffortBags
            .Where(bag => bag.IsActive)
            .OrderBy(bag => bag.BagNumber)
            .ToListAsync();

        Dictionary<string, object> systemInfo = new ()
        {
            { "cycle_duration_days", 28 },
            { "total_bags", bags.Count }
        };

        return Content(PhpSerialization.Serialize(systemInfo), "text/plain");
    }
}
