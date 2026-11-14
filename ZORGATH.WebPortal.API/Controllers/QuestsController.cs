namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
[Authorize(UserRoles.AllRoles)]
public sealed class QuestsController(MerrickContext databaseContext, ILogger<QuestsController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger<QuestsController> Logger { get; } = logger;

    [HttpGet(Name = "Get Portal Quests")]
    [ProducesResponseType(typeof(List<QuestDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuests()
    {
        (User? user, Account? account) = await GetAuthenticatedContextAsync();

        if (user is null || account is null)
        {
            return Unauthorized("The authenticated user or account was not found");
        }

        await EnsureDefaultQuestsExistAsync();

        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset nextMidnight = GetNextMidnightUtc();

        List<Quest> activeQuests = await MerrickContext.Quests
            .Where(quest => quest.IsActive)
            .OrderBy(quest => quest.Category)
            .ThenBy(quest => quest.ID)
            .ToListAsync();

        List<AccountQuestProgress> userProgresses = await MerrickContext.AccountQuestProgresses
            .Where(progress => progress.AccountID == account.ID)
            .ToListAsync();

        bool hasChanges = false;
        List<QuestDTO> results = [];

        foreach (Quest quest in activeQuests)
        {
            AccountQuestProgress? progress = userProgresses.FirstOrDefault(p => p.QuestID == quest.ID);

            if (progress is null)
            {
                progress = new AccountQuestProgress
                {
                    AccountID = account.ID,
                    QuestID = quest.ID,
                    CurrentCount = 0,
                    IsCompleted = false,
                    IsClaimed = false,
                    ResetAt = nextMidnight
                };

                await MerrickContext.AccountQuestProgresses.AddAsync(progress);
                hasChanges = true;
            }
            else if (quest.Category == QuestCategory.Daily && progress.ResetAt <= now)
            {
                progress.CurrentCount = 0;
                progress.IsCompleted = false;
                progress.IsClaimed = false;
                progress.ClaimedAt = null;
                progress.ResetAt = nextMidnight;
                hasChanges = true;
            }

            results.Add(new QuestDTO
            (
                quest.ID,
                quest.QuestKey,
                quest.Title,
                quest.Description,
                quest.Category.ToString(),
                quest.TargetType.ToString(),
                quest.TargetCount,
                progress.CurrentCount,
                quest.RewardSilver,
                quest.RewardGold,
                quest.RewardTickets,
                quest.RewardUpgradeCode,
                progress.IsCompleted,
                progress.IsClaimed,
                progress.ClaimedAt,
                progress.ResetAt,
                quest.ReputationReward,
                quest.TriggerName
            ));
        }

        if (hasChanges)
        {
            await MerrickContext.SaveChangesAsync();
        }

        return Ok(results);
    }

    [HttpGet("WarEffort", Name = "Get The War Effort Status")]
    [ProducesResponseType(typeof(WarEffortStatusDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWarEffortStatus()
    {
        (User? user, Account? account) = await GetAuthenticatedContextAsync();

        if (user is null || account is null)
        {
            return Unauthorized("The authenticated user or account was not found");
        }

        await EnsureDefaultBagsExistAsync();

        AccountWarEffort? warEffort = await MerrickContext.AccountWarEfforts
            .SingleOrDefaultAsync(we => we.AccountID == account.ID);

        if (warEffort is null)
        {
            warEffort = new AccountWarEffort
            {
                AccountID = account.ID,
                CurrentCycle = 1,
                CurrentBagNumber = 1,
                CurrentBagReputation = 0,
                TotalCycleReputation = 0,
                DailyResetsRemaining = 1,
                CycleStartDate = DateTimeOffset.UtcNow,
                CycleEndDate = DateTimeOffset.UtcNow.AddDays(28)
            };

            await MerrickContext.AccountWarEfforts.AddAsync(warEffort);
            await MerrickContext.SaveChangesAsync();
        }

        List<WarEffortBag> allBags = await MerrickContext.WarEffortBags
            .Where(b => b.IsActive)
            .OrderBy(b => b.BagNumber)
            .ToListAsync();

        List<AccountCompletedBag> completedBags = await MerrickContext.AccountCompletedBags
            .Where(b => b.AccountID == account.ID && b.Cycle == warEffort.CurrentCycle)
            .OrderBy(b => b.BagNumber)
            .ToListAsync();

        WarEffortStatusDTO response = new
        (
            warEffort.CurrentCycle,
            warEffort.CurrentBagNumber,
            warEffort.CurrentBagReputation,
            warEffort.TotalCycleReputation,
            warEffort.DailyResetsRemaining,
            warEffort.CycleEndDate,
            allBags.Select(b => new WarEffortBagDTO
            (
                b.BagNumber,
                b.Name,
                b.RequiredReputation,
                b.RewardType,
                b.RewardSilver,
                b.RewardGold,
                b.RewardTickets,
                b.RewardProductName,
                b.IsFinalTier
            )).ToList(),
            completedBags.Select(c => new AccountCompletedBagDTO
            (
                c.BagNumber,
                c.IsOpened,
                c.CompletedAt,
                c.OpenedAt,
                c.GrantedSilver,
                c.GrantedGold,
                c.GrantedTickets,
                c.GrantedProductCode
            )).ToList()
        );

        return Ok(response);
    }

    [HttpPost("Bags/{bagNumber:int}/Open", Name = "Open War Effort Bag")]
    [ProducesResponseType(typeof(OpenWarEffortBagResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OpenBag(int bagNumber)
    {
        (User? user, Account? account) = await GetAuthenticatedContextAsync();

        if (user is null || account is null)
        {
            return Unauthorized("The authenticated user or account was not found");
        }

        AccountWarEffort? warEffort = await MerrickContext.AccountWarEfforts
            .SingleOrDefaultAsync(we => we.AccountID == account.ID);

        int cycle = warEffort?.CurrentCycle ?? 1;

        AccountCompletedBag? completedBag = await MerrickContext.AccountCompletedBags
            .SingleOrDefaultAsync(b => b.AccountID == account.ID && b.BagNumber == bagNumber && b.Cycle == cycle && b.IsOpened.Equals(false));

        WarEffortBag? bagDef = await MerrickContext.WarEffortBags
            .SingleOrDefaultAsync(b => b.BagNumber == bagNumber && b.IsActive);

        if (completedBag is null || bagDef is null)
        {
            return NotFound("Completed unopened bag not found");
        }

        completedBag.IsOpened = true;
        completedBag.OpenedAt = DateTimeOffset.UtcNow;
        completedBag.GrantedSilver = bagDef.RewardSilver;
        completedBag.GrantedGold = bagDef.RewardGold;
        completedBag.GrantedTickets = bagDef.RewardTickets;
        completedBag.GrantedProductCode = bagDef.RewardProductCode;

        user.SilverCoins += bagDef.RewardSilver;
        user.GoldCoins += bagDef.RewardGold;
        user.PlinkoTickets += bagDef.RewardTickets;

        if (string.IsNullOrWhiteSpace(bagDef.RewardProductCode) is false
            && user.OwnedStoreItems.Contains(bagDef.RewardProductCode).Equals(false))
        {
            user.OwnedStoreItems.Add(bagDef.RewardProductCode);
        }

        await MerrickContext.SaveChangesAsync();

        OpenWarEffortBagResponseDTO response = new
        (
            true,
            $@"Successfully opened {bagDef.Name}!",
            bagNumber,
            bagDef.RewardSilver,
            bagDef.RewardGold,
            bagDef.RewardTickets,
            bagDef.RewardProductCode,
            user.SilverCoins,
            user.GoldCoins,
            user.PlinkoTickets
        );

        return Ok(response);
    }

    [HttpPost("{questId:int}/Claim", Name = "Claim Portal Quest Reward")]
    [ProducesResponseType(typeof(ClaimQuestRewardResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ClaimReward(int questId)
    {
        (User? user, Account? account) = await GetAuthenticatedContextAsync();

        if (user is null || account is null)
        {
            return Unauthorized("The authenticated user or account was not found");
        }

        Quest? quest = await MerrickContext.Quests.SingleOrDefaultAsync(q => q.ID == questId && q.IsActive);

        if (quest is null)
        {
            return NotFound($@"Quest With ID {questId} Was Not Found Or Is Inactive");
        }

        AccountQuestProgress? progress = await MerrickContext.AccountQuestProgresses
            .SingleOrDefaultAsync(p => p.AccountID == account.ID && p.QuestID == questId);

        if (progress is null || progress.IsCompleted.Equals(false))
        {
            return BadRequest("Quest objectives have not been completed yet");
        }

        if (progress.IsClaimed)
        {
            return Conflict("The reward for this quest has already been claimed for the current period");
        }

        progress.IsClaimed = true;
        progress.ClaimedAt = DateTimeOffset.UtcNow;

        user.SilverCoins += quest.RewardSilver;
        user.GoldCoins += quest.RewardGold;
        user.PlinkoTickets += quest.RewardTickets;

        string? grantedUpgrade = null;

        if (string.IsNullOrWhiteSpace(quest.RewardUpgradeCode) is false && user.OwnedStoreItems.Contains(quest.RewardUpgradeCode).Equals(false))
        {
            user.OwnedStoreItems.Add(quest.RewardUpgradeCode);
            grantedUpgrade = quest.RewardUpgradeCode;
        }

        // Progress War Effort
        AccountWarEffort? warEffort = await MerrickContext.AccountWarEfforts
            .SingleOrDefaultAsync(we => we.AccountID == account.ID);

        if (warEffort is null)
        {
            warEffort = new AccountWarEffort
            {
                AccountID = account.ID,
                CurrentCycle = 1,
                CurrentBagNumber = 1,
                CurrentBagReputation = 0,
                TotalCycleReputation = 0,
                DailyResetsRemaining = 1,
                CycleStartDate = DateTimeOffset.UtcNow,
                CycleEndDate = DateTimeOffset.UtcNow.AddDays(28)
            };

            await MerrickContext.AccountWarEfforts.AddAsync(warEffort);
        }

        warEffort.CurrentBagReputation += quest.ReputationReward;
        warEffort.TotalCycleReputation += quest.ReputationReward;

        WarEffortBag? currentBag = await MerrickContext.WarEffortBags
            .SingleOrDefaultAsync(bag => bag.BagNumber == warEffort.CurrentBagNumber && bag.IsActive);

        if (currentBag is not null && warEffort.CurrentBagReputation >= currentBag.RequiredReputation)
        {
            AccountCompletedBag completedBag = new ()
            {
                AccountID = account.ID,
                Cycle = warEffort.CurrentCycle,
                BagNumber = currentBag.BagNumber,
                BagID = currentBag.ID,
                IsOpened = false,
                CompletedAt = DateTimeOffset.UtcNow,
                GrantedSilver = currentBag.RewardSilver,
                GrantedGold = currentBag.RewardGold,
                GrantedTickets = currentBag.RewardTickets,
                GrantedProductCode = currentBag.RewardProductCode
            };

            await MerrickContext.AccountCompletedBags.AddAsync(completedBag);

            warEffort.CurrentBagNumber = Math.Min(14, warEffort.CurrentBagNumber + 1);
            warEffort.CurrentBagReputation = 0;
        }

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(
            @"User ""{UserEmail}"" Claimed Reward For Quest ""{QuestTitle}"" (+{Silver} Silver, +{Gold} Gold, +{Tickets} Tickets)",
            user.EmailAddress, quest.Title, quest.RewardSilver, quest.RewardGold, quest.RewardTickets
        );

        ClaimQuestRewardResponseDTO response = new
        (
            true,
            $@"Successfully claimed rewards for quest ""{quest.Title}""!",
            quest.ID,
            quest.RewardSilver,
            quest.RewardGold,
            quest.RewardTickets,
            grantedUpgrade,
            user.SilverCoins,
            user.GoldCoins,
            user.PlinkoTickets
        );

        return Ok(response);
    }

    [HttpPost("SimulateProgress", Name = "Simulate Portal Quest Progress")]
    [ProducesResponseType(typeof(List<QuestDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SimulateProgress([FromBody] SimulateQuestProgressDTO payload)
    {
        (User? user, Account? account) = await GetAuthenticatedContextAsync();

        if (user is null || account is null)
        {
            return Unauthorized("The authenticated user or account was not found");
        }

        if (Enum.TryParse(payload.TargetType, true, out QuestTargetType targetType).Equals(false))
        {
            return BadRequest($@"Unsupported Target Type ""{payload.TargetType}""");
        }

        List<Quest> matchingQuests = await MerrickContext.Quests
            .Where(quest => quest.IsActive && quest.TargetType == targetType)
            .ToListAsync();

        List<AccountQuestProgress> userProgresses = await MerrickContext.AccountQuestProgresses
            .Where(progress => progress.AccountID == account.ID)
            .ToListAsync();

        DateTimeOffset nextMidnight = GetNextMidnightUtc();

        foreach (Quest quest in matchingQuests)
        {
            AccountQuestProgress? progress = userProgresses.FirstOrDefault(p => p.QuestID == quest.ID);

            if (progress is null)
            {
                progress = new AccountQuestProgress
                {
                    AccountID = account.ID,
                    QuestID = quest.ID,
                    CurrentCount = 0,
                    IsCompleted = false,
                    IsClaimed = false,
                    ResetAt = nextMidnight
                };

                await MerrickContext.AccountQuestProgresses.AddAsync(progress);
                userProgresses.Add(progress);
            }

            if (progress.IsClaimed.Equals(false))
            {
                progress.CurrentCount += Math.Max(1, payload.Increment);

                if (progress.CurrentCount >= quest.TargetCount)
                {
                    progress.CurrentCount = quest.TargetCount;
                    progress.IsCompleted = true;
                }
            }
        }

        await MerrickContext.SaveChangesAsync();

        return await GetQuests();
    }

    private async Task<(User? user, Account? account)> GetAuthenticatedContextAsync()
    {
        string? userIdString = User.Claims.SingleOrDefault(claim => claim.Type.Equals(Claims.UserID))?.Value;

        if (int.TryParse(userIdString, out int userId).Equals(false))
        {
            return (null, null);
        }

        User? user = await MerrickContext.Users
            .Include(u => u.Accounts)
            .SingleOrDefaultAsync(u => u.ID == userId);

        if (user is null)
        {
            return (null, null);
        }

        string? accountIdString = User.Claims.SingleOrDefault(claim => claim.Type.Equals(Claims.AccountID))?.Value;
        Account? account = null;

        if (int.TryParse(accountIdString, out int accountId))
        {
            account = user.Accounts.FirstOrDefault(a => a.ID == accountId);
        }

        account ??= user.Accounts.FirstOrDefault(a => a.IsMain) ?? user.Accounts.FirstOrDefault();

        return (user, account);
    }

    private async Task EnsureDefaultBagsExistAsync()
    {
        if (await MerrickContext.WarEffortBags.AnyAsync())
        {
            return;
        }

        List<WarEffortBag> defaultBags =
        [
            new () { BagNumber = 1,  Name = "Bronze Satchel I",        RequiredReputation = 50,  RewardType = "MMPOINTS", RewardSilver = 50,  RewardGold = 0,   RewardTickets = 0,  IsActive = true },
            new () { BagNumber = 2,  Name = "Bronze Satchel II",       RequiredReputation = 75,  RewardType = "MMPOINTS", RewardSilver = 75,  RewardGold = 0,   RewardTickets = 0,  IsActive = true },
            new () { BagNumber = 3,  Name = "Silver Satchel I",        RequiredReputation = 100, RewardType = "MMPOINTS", RewardSilver = 100, RewardGold = 0,   RewardTickets = 1,  IsActive = true },
            new () { BagNumber = 4,  Name = "Silver Satchel II",       RequiredReputation = 125, RewardType = "MMPOINTS", RewardSilver = 125, RewardGold = 0,   RewardTickets = 1,  IsActive = true },
            new () { BagNumber = 5,  Name = "Silver Satchel III",      RequiredReputation = 150, RewardType = "MMPOINTS", RewardSilver = 150, RewardGold = 0,   RewardTickets = 2,  IsActive = true },
            new () { BagNumber = 6,  Name = "Gold Chest I",            RequiredReputation = 175, RewardType = "POINTS",   RewardSilver = 100, RewardGold = 10,  RewardTickets = 1,  IsActive = true },
            new () { BagNumber = 7,  Name = "Gold Chest II",           RequiredReputation = 200, RewardType = "POINTS",   RewardSilver = 150, RewardGold = 20,  RewardTickets = 1,  IsActive = true },
            new () { BagNumber = 8,  Name = "Gold Chest III",          RequiredReputation = 225, RewardType = "POINTS",   RewardSilver = 200, RewardGold = 25,  RewardTickets = 2,  IsActive = true },
            new () { BagNumber = 9,  Name = "Diamond Chest I",         RequiredReputation = 250, RewardType = "POINTS",   RewardSilver = 250, RewardGold = 30,  RewardTickets = 2,  IsActive = true },
            new () { BagNumber = 10, Name = "Diamond Chest II",        RequiredReputation = 275, RewardType = "POINTS",   RewardSilver = 300, RewardGold = 40,  RewardTickets = 3,  IsActive = true },
            new () { BagNumber = 11, Name = "Diamond Chest III",       RequiredReputation = 300, RewardType = "POINTS",   RewardSilver = 350, RewardGold = 50,  RewardTickets = 3,  IsActive = true },
            new () { BagNumber = 12, Name = "Master Chest I",          RequiredReputation = 350, RewardType = "POINTS",   RewardSilver = 400, RewardGold = 75,  RewardTickets = 4,  IsActive = true },
            new () { BagNumber = 13, Name = "Master Chest II",         RequiredReputation = 400, RewardType = "POINTS",   RewardSilver = 500, RewardGold = 100, RewardTickets = 5,  IsActive = true },
            new () { BagNumber = 14, Name = "Treasure of Champions",   RequiredReputation = 500, RewardType = "POINTS",   RewardSilver = 1000, RewardGold = 250, RewardTickets = 10, IsFinalTier = true, IsActive = true }
        ];

        await MerrickContext.WarEffortBags.AddRangeAsync(defaultBags);
        await MerrickContext.SaveChangesAsync();
    }

    private async Task EnsureDefaultQuestsExistAsync()
    {
        if (await MerrickContext.Quests.AnyAsync())
        {
            return;
        }

        List<Quest> starterQuests =
        [
            new ()
            {
                QuestKey = "daily_first_blood",
                Title = "First Blood Hunter",
                Description = "Secure at least 1 kill in any battle",
                Category = QuestCategory.Daily,
                TargetType = QuestTargetType.Kills,
                TriggerName = "achievement_value_prophet_kills",
                TargetCount = 1,
                ReputationReward = 25,
                TypeID = 4,
                TypeKey = "prophet_kills",
                RewardSilver = 60,
                RewardTickets = 1,
                ProductIconPath = "/ui/fe2/quests/quest_icons/prophet_kills.tga",
                IsActive = true
            },
            new ()
            {
                QuestKey = "daily_victory",
                Title = "Daily Triumph",
                Description = "Win 1 Matchmaking or Public match",
                Category = QuestCategory.Daily,
                TargetType = QuestTargetType.MatchWin,
                TriggerName = "achievement_value_games_won",
                TargetCount = 1,
                ReputationReward = 25,
                TypeID = 1,
                TypeKey = "games_won",
                RewardSilver = 100,
                RewardTickets = 1,
                ProductIconPath = "/ui/fe2/quests/quest_icons/games_won.tga",
                IsActive = true
            },
            new ()
            {
                QuestKey = "daily_caldavar_victory",
                Title = "Forests of Caldavar",
                Description = "Win 1 match on Forests of Caldavar",
                Category = QuestCategory.Daily,
                TargetType = QuestTargetType.MatchWin,
                TriggerName = "achievement_value_caldavar_won",
                TargetCount = 1,
                ReputationReward = 30,
                TypeID = 2,
                TypeKey = "caldavar_won",
                RewardSilver = 120,
                RewardTickets = 1,
                ProductIconPath = "/ui/fe2/quests/quest_icons/caldavar_won.tga",
                IsActive = true
            },
            new ()
            {
                QuestKey = "daily_support_hero",
                Title = "Eyes of the Legion",
                Description = "Place 5 wards across your matches",
                Category = QuestCategory.Daily,
                TargetType = QuestTargetType.WardsPlaced,
                TriggerName = "achievement_value_wards_placed",
                TargetCount = 5,
                ReputationReward = 20,
                TypeID = 5,
                TypeKey = "role_support",
                RewardSilver = 80,
                RewardTickets = 0,
                ProductIconPath = "/ui/fe2/quests/quest_icons/role_support.tga",
                IsActive = true
            },
            new ()
            {
                QuestKey = "daily_team_player",
                Title = "Team Catalyst",
                Description = "Accumulate 10 assists in team fights",
                Category = QuestCategory.Daily,
                TargetType = QuestTargetType.Assists,
                TriggerName = "achievement_value_assists",
                TargetCount = 10,
                ReputationReward = 25,
                TypeID = 6,
                TypeKey = "role_carry_ganker",
                RewardSilver = 90,
                RewardTickets = 1,
                ProductIconPath = "/ui/fe2/quests/quest_icons/role_carry_ganker.tga",
                IsActive = true
            }
        ];

        await MerrickContext.Quests.AddRangeAsync(starterQuests);
        await MerrickContext.SaveChangesAsync();
    }

    private static DateTimeOffset GetNextMidnightUtc()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero).AddDays(1);
    }
}
