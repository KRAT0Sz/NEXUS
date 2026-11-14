namespace KONGOR.MasterServer.Controllers.Quest;

using System.Net;
using System.Text;
using Quest = MERRICK.DatabaseContext.Entities.Economy.Quest;

[ApiController]
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

        await QuestService.EnsureQuestSeedDataAsync();

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

    /// <summary>
    ///     Returns the quest leaderboard ranking players by total cycle reputation points in PHP serialised format.
    /// </summary>
    [HttpPost("master/quest/getleaderboard", Name = "Get Quest Leaderboard"), HttpGet("master/quest/getleaderboard", Name = "Get Quest Leaderboard Get")]
    public async Task<IActionResult> GetQuestLeaderboard([FromQuery] int take = 50)
    {
        List<QuestLeaderboardEntry> leaderboard = await QuestService.GetQuestLeaderboardAsync(take);

        Dictionary<string, object> response = [];
        for (int i = 0; i < leaderboard.Count; i++)
        {
            QuestLeaderboardEntry entry = leaderboard[i];
            response[i.ToString()] = new Dictionary<string, object>
            {
                { "rank", entry.Rank.ToString() },
                { "account_id", entry.AccountID.ToString() },
                { "nickname", entry.AccountName },
                { "clan_tag", entry.ClanTag },
                { "reputation", entry.TotalReputation.ToString() },
                { "bag_number", entry.CurrentBagNumber.ToString() },
                { "completed_bags", entry.CompletedBagsCount.ToString() },
                { "cycle", entry.CurrentCycle.ToString() }
            };
        }

        string payload = PhpSerialization.Serialize(response);
        return Content(payload, "text/plain");
    }

    /// <summary>
    ///     Serves an HTML view of the quest leaderboard formatted specifically for the in-game browser panel.
    /// </summary>
    [HttpGet("master/quest/ladder", Name = "Get Quest Ladder Page")]
    [HttpGet("web/Quest-Ladder/index.html", Name = "Get Quest Ladder HTML")]
    public async Task<IActionResult> GetQuestLadderPage([FromQuery] int take = 50)
    {
        List<QuestLeaderboardEntry> leaderboard = await QuestService.GetQuestLeaderboardAsync(take);

        StringBuilder html = new ();
        html.Append("<!DOCTYPE html><html><head><meta charset='utf-8'/><style>");
        html.Append("body { background: #0c0e12; color: #c4c9d4; font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 20px; font-size: 13px; }");
        html.Append(".header { display: flex; align-items: center; justify-content: space-between; border-bottom: 2px solid #b8860b; padding-bottom: 12px; margin-bottom: 16px; }");
        html.Append(".title { font-size: 20px; font-weight: bold; color: #e6c875; text-transform: uppercase; letter-spacing: 1px; }");
        html.Append(".subtitle { font-size: 12px; color: #8e95a5; }");
        html.Append("table { width: 100%; border-collapse: collapse; }");
        html.Append("th { background: #161b24; color: #deb887; text-align: left; padding: 10px 14px; font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px; border-bottom: 1px solid #2d3748; }");
        html.Append("td { padding: 9px 14px; border-bottom: 1px solid #1a2230; }");
        html.Append("tr:nth-child(even) { background: #10141c; }");
        html.Append("tr:hover { background: #1c2333; }");
        html.Append(".rank-1 { color: #ffd700; font-weight: bold; }");
        html.Append(".rank-2 { color: #c0c0c0; font-weight: bold; }");
        html.Append(".rank-3 { color: #cd7f32; font-weight: bold; }");
        html.Append(".clan-tag { color: #7f9cf5; font-size: 11px; margin-right: 4px; }");
        html.Append(".player-name { font-weight: 600; color: #f0f4f8; }");
        html.Append(".rep-badge { display: inline-block; background: rgba(184, 134, 11, 0.2); border: 1px solid rgba(184, 134, 11, 0.4); color: #ffd700; padding: 2px 8px; border-radius: 4px; font-weight: bold; }");
        html.Append(".bag-badge { display: inline-block; background: rgba(56, 178, 172, 0.15); border: 1px solid rgba(56, 178, 172, 0.35); color: #4fd1c5; padding: 2px 8px; border-radius: 4px; }");
        html.Append("</style></head><body>");
        html.Append("<div class='header'><div><div class='title'>The War Effort — Quest Leaderboard</div>");
        html.Append("<div class='subtitle'>Season Rankings • Total Reputation Points Accumulated</div></div></div>");
        html.Append("<table><thead><tr><th>Rank</th><th>Player</th><th style='text-align:right;'>Reputation</th><th style='text-align:right;'>Milestone Bag</th><th style='text-align:right;'>Bags Unlocked</th></tr></thead><tbody>");

        if (leaderboard.Count == 0)
        {
            html.Append("<tr><td colspan='5' style='text-align:center; padding: 30px; color: #6b7280;'>No quest ranking data recorded for the current cycle yet.</td></tr>");
        }
        else
        {
            foreach (QuestLeaderboardEntry entry in leaderboard)
            {
                string rankClass = entry.Rank == 1 ? "rank-1" : (entry.Rank == 2 ? "rank-2" : (entry.Rank == 3 ? "rank-3" : ""));
                string clanHtml = string.IsNullOrEmpty(entry.ClanTag) ? "" : $"<span class='clan-tag'>[{entry.ClanTag}]</span>";
                html.Append($"<tr><td class='{rankClass}'>#{entry.Rank}</td>");
                html.Append($"<td>{clanHtml}<span class='player-name'>{WebUtility.HtmlEncode(entry.AccountName)}</span></td>");
                html.Append($"<td style='text-align:right;'><span class='rep-badge'>{entry.TotalReputation} RP</span></td>");
                html.Append($"<td style='text-align:right;'><span class='bag-badge'>Bag #{entry.CurrentBagNumber}</span></td>");
                html.Append($"<td style='text-align:right;'>{entry.CompletedBagsCount}</td></tr>");
            }
        }

        html.Append("</tbody></table></body></html>");
        return Content(html.ToString(), "text/html");
    }
}
