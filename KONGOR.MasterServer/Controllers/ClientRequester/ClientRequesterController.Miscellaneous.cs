namespace KONGOR.MasterServer.Controllers.ClientRequester;

public partial class ClientRequesterController
{
    /// <summary>
    ///     Handles the "autocompleteNicks" client requester command, returning a list of player nicknames matching a prefix or substring.
    /// </summary>
    private async Task<IActionResult> AutoCompleteNicks()
    {
        string pattern = Request.Form["nickname"].ToString() ?? Request.Query["nickname"].ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(pattern))
        {
            Dictionary<string, object> emptyResponse = new ()
            {
                ["nicks"] = new List<string>(),
                ["vested_threshold"] = 5,
                ["0"] = true
            };

            return Ok(PhpSerialization.Serialize(emptyResponse));
        }

        List<string> matchingNames = await MerrickContext.Accounts
            .AsNoTracking()
            .Where(account => EF.Functions.Like(account.Name, $"%{pattern}%"))
            .Select(account => account.Name)
            .OrderBy(name => name)
            .Take(40)
            .ToListAsync();

        Dictionary<string, object> response = new ()
        {
            ["nicks"] = matchingNames,
            ["vested_threshold"] = 5,
            ["0"] = true
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Handles the "nick2id" client requester command, resolving one or more player nicknames into account identifiers.
    /// </summary>
    private async Task<IActionResult> NicknameToID()
    {
        string nicknameInput = Request.Form["nickname"].ToString() ?? Request.Query["nickname"].ToString() ?? string.Empty;

        string[] nicknames = nicknameInput.Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        Dictionary<string, object> response = new ()
        {
            ["vested_threshold"] = 5,
            ["0"] = true
        };

        if (nicknames.Length == 0)
            return Ok(PhpSerialization.Serialize(response));

        List<Account> accounts = await MerrickContext.Accounts
            .AsNoTracking()
            .Where(account => nicknames.Contains(account.Name))
            .ToListAsync();

        foreach (Account account in accounts)
            response[account.Name] = account.ID.ToString();

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Handles the "new_ignored" client requester command, adding a player to the client's ignored list.
    /// </summary>
    private IActionResult AddIgnored()
    {
        Dictionary<string, object> response = new ()
        {
            ["0"] = true,
            ["vested_threshold"] = 5
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Handles the "remove_ignored" client requester command, removing a player from the client's ignored list.
    /// </summary>
    private IActionResult RemoveIgnored()
    {
        Dictionary<string, object> response = new ()
        {
            ["0"] = true,
            ["vested_threshold"] = 5
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Handles the "clan_nameCheck" client requester command, checking whether a clan name or tag is available.
    /// </summary>
    private async Task<IActionResult> CheckClanName()
    {
        string clanName = Request.Form["name"].ToString() ?? Request.Query["name"].ToString() ?? string.Empty;
        string clanTag = Request.Form["tag"].ToString() ?? Request.Query["tag"].ToString() ?? string.Empty;

        bool nameExists = false;
        bool tagExists = false;

        if (string.IsNullOrWhiteSpace(clanName) is false)
        {
            nameExists = await MerrickContext.Clans
                .AsNoTracking()
                .AnyAsync(clan => clan.Name.Equals(clanName));
        }

        if (string.IsNullOrWhiteSpace(clanTag) is false)
        {
            tagExists = await MerrickContext.Clans
                .AsNoTracking()
                .AnyAsync(clan => clan.Tag.Equals(clanTag));
        }

        if (nameExists || tagExists)
        {
            Dictionary<string, object> takenResponse = new ()
            {
                ["available"] = false,
                ["error"] = "Clan name or tag is already taken.",
                ["0"] = false,
                ["vested_threshold"] = 5
            };

            return Ok(PhpSerialization.Serialize(takenResponse));
        }

        Dictionary<string, object> availableResponse = new ()
        {
            ["available"] = true,
            ["0"] = true,
            ["vested_threshold"] = 5
        };

        return Ok(PhpSerialization.Serialize(availableResponse));
    }

    /// <summary>
    ///     Handles the "clan_list" client requester command, returning a list of active clans.
    /// </summary>
    private async Task<IActionResult> GetClanList()
    {
        List<Clan> clans = await MerrickContext.Clans
            .AsNoTracking()
            .Include(clan => clan.Members)
            .OrderByDescending(clan => clan.Members.Count)
            .Take(50)
            .ToListAsync();

        List<Dictionary<string, object>> clanList = clans.Select(clan => new Dictionary<string, object>
        {
            ["clan_id"] = clan.ID,
            ["name"] = clan.Name,
            ["tag"] = clan.Tag,
            ["member_count"] = clan.Members.Count
        }).ToList();

        Dictionary<string, object> response = new ()
        {
            ["clans"] = clanList,
            ["0"] = true,
            ["vested_threshold"] = 5
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Handles the "change_buddy_group" client requester command, updating a buddy's category in the player's friend list.
    /// </summary>
    private IActionResult ChangeBuddyGroup()
    {
        Dictionary<string, object> response = new ()
        {
            ["0"] = true,
            ["vested_threshold"] = 5
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Handles the "reset_stats" client requester command, resetting the requesting account's matchmaking ratings and statistics.
    /// </summary>
    private async Task<IActionResult> ResetStatistics()
    {
        string? cookie = Request.Form["cookie"].ToString() ?? Request.Query["cookie"].ToString();

        if (string.IsNullOrEmpty(cookie))
            return BadRequest(@"Missing Value For Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid is false || accountName is null)
            return Unauthorized($@"Unrecognised Cookie ""{cookie}""");

        Account? account = await MerrickContext.Accounts
            .Include(queriedAccount => queriedAccount.User)
            .SingleOrDefaultAsync(acc => acc.Name.Equals(accountName));

        if (account is not null)
        {
            if (account.User.OwnedStoreItems.Contains("m.Stat Reset").Equals(false))
            {
                Dictionary<string, object> notOwnedResponse = new ()
                {
                    ["success"] = false,
                    ["errors"]  = "Stat Reset Item Is Not Owned"
                };

                return Ok(PhpSerialization.Serialize(notOwnedResponse));
            }

            List<AccountStatistics> statisticsList = await MerrickContext.AccountStatistics
                .Where(stat => stat.AccountID == account.ID)
                .ToListAsync();

            foreach (AccountStatistics statistics in statisticsList)
            {
                statistics.SkillRating = 1500.0;
                statistics.MatchesPlayed = 0;
                statistics.MatchesWon = 0;
                statistics.MatchesLost = 0;
                statistics.MatchesDisconnected = 0;
                statistics.MatchesConceded = 0;
                statistics.MatchesKicked = 0;
                statistics.HeroKills = 0;
                statistics.HeroAssists = 0;
                statistics.HeroDeaths = 0;
                statistics.WardsPlaced = 0;
                statistics.Smackdowns = 0;
                statistics.PlacementMatchesData = null;
                statistics.HighestMedal = Rank.NO_MEDAL;
            }

            account.User.OwnedStoreItems.Remove("m.Stat Reset");
            await MerrickContext.SaveChangesAsync();
        }

        Dictionary<string, object> response = new ()
        {
            ["0"] = true,
            ["vested_threshold"] = 5
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Handles the "switch_auth" client requester command, switching sub-accounts or re-validating the session.
    /// </summary>
    private async Task<IActionResult> SwitchAuthentication()
    {
        string? cookie = Request.Form["cookie"].ToString() ?? Request.Query["cookie"].ToString();

        if (string.IsNullOrEmpty(cookie))
            return BadRequest(@"Missing Value For Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid is false || accountName is null)
            return Unauthorized($@"Unrecognised Cookie ""{cookie}""");

        Dictionary<string, object> response = new ()
        {
            ["auth_token"] = cookie,
            ["0"] = true,
            ["vested_threshold"] = 5
        };

        return Ok(PhpSerialization.Serialize(response));
    }
}
