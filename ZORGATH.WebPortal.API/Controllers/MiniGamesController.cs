namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
[Authorize(UserRoles.AllRoles)]
public sealed class MiniGamesController(MerrickContext databaseContext, ILogger<MiniGamesController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger<MiniGamesController> Logger { get; } = logger;

    private static readonly TimeSpan FreeSpinCooldown = TimeSpan.FromHours(8);

    private static readonly (int Silver, int Gold, int Tickets)[] StreakRewardsTable =
    [
        (50, 0, 0),     // Day 1
        (75, 0, 1),     // Day 2
        (100, 0, 0),    // Day 3
        (125, 0, 1),    // Day 4
        (150, 0, 0),    // Day 5
        (200, 0, 2),    // Day 6
        (300, 50, 3)    // Day 7 (Jackpot)
    ];

    private static readonly LuckySpinWheelSliceDTO[] WheelSlices =
    [
        new (0, "AVATAR", "Avatar", 1, "avatar_skin", "#f98e54"),
        new (1, "COIN", "Coin", 150, null, "#cca5df"),
        new (2, "WARD", "Ward", 1, "ward_skin", "#7dd8db"),
        new (3, "THANKS!", "Thanks", 25, null, "#fde08b"),
        new (4, "COURIER", "Courier", 1, "courier_skin", "#e86b79"),
        new (5, "ICON", "Icon", 1, "icon_custom", "#7dd8db")
    ];

    [HttpGet("CheckIn/Status", Name = "Get Daily Check-in Status")]
    [ProducesResponseType(typeof(DailyCheckInStatusDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCheckInStatus()
    {
        (User? user, Account? account) = await GetAuthenticatedContextAsync();

        if (user is null || account is null)
        {
            return Unauthorized("The authenticated user or account was not found");
        }

        AccountDailyCheckIn checkIn = await GetOrCreateCheckInRecordAsync(account.ID);
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly yesterday = today.AddDays(-1);

        // If streak broke (more than 1 day missed), current active streak displays as 0 until claimed today
        int effectiveStreak = checkIn.CurrentStreak;

        if (checkIn.LastCheckInDate.HasValue && checkIn.LastCheckInDate.Value != today && checkIn.LastCheckInDate.Value != yesterday)
        {
            effectiveStreak = 0;
        }

        bool hasCheckedInToday = checkIn.LastCheckInDate == today;

        List<DailyCheckInRewardTierDTO> tierDTOs = [];

        for (int i = 0; i < 7; i++)
        {
            int dayNumber = i + 1;
            (int silver, int gold, int tickets) = StreakRewardsTable[i];
            bool isClaimed = hasCheckedInToday && dayNumber <= effectiveStreak;
            bool isToday = (hasCheckedInToday && dayNumber == effectiveStreak) || (hasCheckedInToday.Equals(false) && dayNumber == (effectiveStreak % 7) + 1);

            tierDTOs.Add(new DailyCheckInRewardTierDTO
            (
                dayNumber,
                silver,
                gold,
                tickets,
                isToday,
                isClaimed
            ));
        }

        return Ok(new DailyCheckInStatusDTO
        (
            effectiveStreak,
            hasCheckedInToday,
            checkIn.LastCheckInDate,
            tierDTOs
        ));
    }

    [HttpPost("CheckIn/Claim", Name = "Claim Daily Check-in")]
    [ProducesResponseType(typeof(ClaimCheckInResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ClaimCheckIn()
    {
        (User? user, Account? account) = await GetAuthenticatedContextAsync();

        if (user is null || account is null)
        {
            return Unauthorized("The authenticated user or account was not found");
        }

        AccountDailyCheckIn checkIn = await GetOrCreateCheckInRecordAsync(account.ID);
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly yesterday = today.AddDays(-1);

        if (checkIn.LastCheckInDate == today)
        {
            return Conflict("You have already claimed your daily check-in reward today. Please come back tomorrow!");
        }

        if (checkIn.LastCheckInDate == yesterday)
        {
            checkIn.CurrentStreak = (checkIn.CurrentStreak % 7) + 1;
        }
        else
        {
            checkIn.CurrentStreak = 1;
        }

        checkIn.LastCheckInDate = today;

        int rewardIndex = checkIn.CurrentStreak - 1;
        (int silver, int gold, int tickets) = StreakRewardsTable[rewardIndex];

        user.SilverCoins += silver;
        user.GoldCoins += gold;
        user.PlinkoTickets += tickets;

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(
            @"User ""{UserEmail}"" Checked In Day {Day} (+{Silver} Silver, +{Gold} Gold, +{Tickets} Tickets)",
            user.EmailAddress, checkIn.CurrentStreak, silver, gold, tickets
        );

        ClaimCheckInResponseDTO response = new
        (
            true,
            $@"Daily check-in successful! Claimed Day {checkIn.CurrentStreak} rewards.",
            checkIn.CurrentStreak,
            checkIn.CurrentStreak,
            silver,
            gold,
            tickets,
            user.SilverCoins,
            user.GoldCoins,
            user.PlinkoTickets
        );

        return Ok(response);
    }

    [HttpGet("Spin/Status", Name = "Get Lucky Spin Status")]
    [ProducesResponseType(typeof(LuckySpinStatusDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSpinStatus()
    {
        (User? user, Account? account) = await GetAuthenticatedContextAsync();

        if (user is null || account is null)
        {
            return Unauthorized("The authenticated user or account was not found");
        }

        AccountDailyCheckIn checkIn = await GetOrCreateCheckInRecordAsync(account.ID);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset? nextFreeSpinTime = checkIn.LastFreeSpinTimestamp?.Add(FreeSpinCooldown);
        bool canFreeSpin = nextFreeSpinTime is null || now >= nextFreeSpinTime.Value;
        int remainingSeconds = canFreeSpin || nextFreeSpinTime is null ? 0 : Math.Max(0, (int)(nextFreeSpinTime.Value - now).TotalSeconds);

        return Ok(new LuckySpinStatusDTO
        (
            canFreeSpin,
            1,
            user.PlinkoTickets,
            checkIn.TotalSpinsPlayed,
            WheelSlices,
            canFreeSpin ? null : nextFreeSpinTime,
            remainingSeconds
        ));
    }

    [HttpPost("Spin/Play", Name = "Play Lucky Spin")]
    [ProducesResponseType(typeof(LuckySpinResultDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PlaySpin()
    {
        (User? user, Account? account) = await GetAuthenticatedContextAsync();

        if (user is null || account is null)
        {
            return Unauthorized("The authenticated user or account was not found");
        }

        AccountDailyCheckIn checkIn = await GetOrCreateCheckInRecordAsync(account.ID);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset? nextFreeSpinTime = checkIn.LastFreeSpinTimestamp?.Add(FreeSpinCooldown);
        bool isFreeSpin = nextFreeSpinTime is null || now >= nextFreeSpinTime.Value;

        if (isFreeSpin.Equals(false))
        {
            if (user.PlinkoTickets < 1)
            {
                int waitSeconds = nextFreeSpinTime.HasValue ? Math.Max(0, (int)(nextFreeSpinTime.Value - now).TotalSeconds) : 0;
                TimeSpan remainingWait = TimeSpan.FromSeconds(waitSeconds);
                return BadRequest($@"You do not have enough Plinko Tickets. Next free spin will be available in {remainingWait.Hours}h {remainingWait.Minutes}m {remainingWait.Seconds}s.");
            }

            user.PlinkoTickets -= 1;
        }
        else
        {
            checkIn.LastFreeSpinTimestamp = now;
        }

        checkIn.TotalSpinsPlayed += 1;

        // Weighted random selection for the 6 slices (GAS HoN Wheel)
        int winningIndex = PickWeightedSliceIndex();
        LuckySpinWheelSliceDTO winningSlice = WheelSlices[winningIndex];
        string wonMessage;

        switch (winningSlice.RewardType)
        {
            case "Avatar":
                user.GoldCoins += 100;
                user.SilverCoins += 250;
                user.PlinkoTickets += 3;
                wonMessage = "ยินดีด้วย! คุณได้รับรางวัลใหญ่ Hero Avatar (100 Gold + 250 Silver + 3 Tickets)!";
                break;
            case "Coin":
                user.SilverCoins += winningSlice.Amount;
                wonMessage = $@"ยินดีด้วย! คุณได้รับ {winningSlice.Amount} Silver Coins!";
                break;
            case "Ward":
                user.PlinkoTickets += 1;
                user.SilverCoins += 50;
                wonMessage = "ยินดีด้วย! คุณได้รับ Ward Skin (+1 Ticket + 50 Silver)!";
                break;
            case "Thanks":
                user.SilverCoins += winningSlice.Amount;
                wonMessage = "ขอบคุณที่ร่วมสนุก! คุณได้รับ 25 Silver Coins ปลอบใจ";
                break;
            case "Courier":
                user.PlinkoTickets += 2;
                user.SilverCoins += 100;
                wonMessage = "ยินดีด้วย! คุณได้รับ Custom Courier Skin (+2 Tickets + 100 Silver)!";
                break;
            case "Icon":
                user.SilverCoins += 75;
                user.PlinkoTickets += 1;
                wonMessage = "ยินดีด้วย! คุณได้รับ Special Account Icon (+1 Ticket + 75 Silver)!";
                break;
            default:
                user.SilverCoins += 50;
                wonMessage = $@"ยินดีด้วย! คุณได้รับ {winningSlice.Label}!";
                break;
        }

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(
            @"User ""{UserEmail}"" Spun Lucky Wheel: Won Slice {Index} ({Label})",
            user.EmailAddress, winningIndex, winningSlice.Label
        );

        DateTimeOffset? updatedNextFreeSpin = checkIn.LastFreeSpinTimestamp?.Add(FreeSpinCooldown);
        bool updatedCanFreeSpin = updatedNextFreeSpin is null || now >= updatedNextFreeSpin.Value;
        int updatedRemainingSeconds = updatedCanFreeSpin || updatedNextFreeSpin is null ? 0 : Math.Max(0, (int)(updatedNextFreeSpin.Value - now).TotalSeconds);

        LuckySpinResultDTO response = new
        (
            true,
            winningIndex,
            winningSlice.Label,
            winningSlice.RewardType,
            winningSlice.Amount,
            winningSlice.UpgradeCode,
            user.PlinkoTickets,
            user.SilverCoins,
            user.GoldCoins,
            user.PlinkoTickets,
            wonMessage,
            updatedCanFreeSpin ? null : updatedNextFreeSpin,
            updatedRemainingSeconds
        );

        return Ok(response);
    }

    private static int PickWeightedSliceIndex()
    {
        // Slice probabilities:
        // Slice 0 (AVATAR - Rare prize): 5%
        // Slice 1 (COIN - 150): 25%
        // Slice 2 (WARD): 15%
        // Slice 3 (THANKS! - 25): 30%
        // Slice 4 (COURIER): 10%
        // Slice 5 (ICON): 15%
        int[] weights = [5, 25, 15, 30, 10, 15];
        int randomRoll = RandomNumberGenerator.GetInt32(0, 100);
        int runningTotal = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            runningTotal += weights[i];

            if (randomRoll < runningTotal)
            {
                return i;
            }
        }

        return 0;
    }

    private async Task<AccountDailyCheckIn> GetOrCreateCheckInRecordAsync(int accountId)
    {
        AccountDailyCheckIn? checkIn = await MerrickContext.AccountDailyCheckIns
            .SingleOrDefaultAsync(c => c.AccountID == accountId);

        if (checkIn is null)
        {
            checkIn = new AccountDailyCheckIn
            {
                AccountID = accountId,
                CurrentStreak = 0,
                LastCheckInDate = null,
                LastFreeSpinTimestamp = null,
                TotalSpinsPlayed = 0
            };

            await MerrickContext.AccountDailyCheckIns.AddAsync(checkIn);
            await MerrickContext.SaveChangesAsync();
        }

        return checkIn;
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
}

