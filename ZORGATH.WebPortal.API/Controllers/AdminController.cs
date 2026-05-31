namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Authorize(UserRoles.Administrator)]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class AdminController : ControllerBase
{
    private MerrickContext MerrickContext { get; }

    public AdminController(MerrickContext databaseContext)
    {
        MerrickContext = databaseContext;
    }

    // MARK: - Dashboard Statistics

    [HttpGet("Dashboard/Stats", Name = "Get Dashboard Statistics")]
    [ProducesResponseType(typeof(GetDashboardStatsDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStatistics()
    {
        int totalUsers = await MerrickContext.Users.CountAsync();
        int totalAccounts = await MerrickContext.Accounts.CountAsync();
        int totalClans = await MerrickContext.Clans.CountAsync();
        int totalStaffAccounts = await MerrickContext.Accounts.CountAsync(account => account.Type == AccountType.Staff);
        int totalServerHostAccounts = await MerrickContext.Accounts.CountAsync(account => account.Type == AccountType.ServerHost);

        DateTimeOffset startOfDay = DateTimeOffset.UtcNow.Date;
        int activeUsersToday = await MerrickContext.Users.CountAsync(user => user.TimestampLastActive >= startOfDay);

        GetDashboardStatsDTO response = new (
            TotalUsers: totalUsers,
            TotalAccounts: totalAccounts,
            TotalClans: totalClans,
            ActiveUsersToday: activeUsersToday,
            TotalStaffAccounts: totalStaffAccounts,
            TotalServerHostAccounts: totalServerHostAccounts);

        return Ok(response);
    }

    // MARK: - User Management

    [HttpGet("Users", Name = "List Users")]
    [ProducesResponseType(typeof(PaginatedResultDTO<GetUserSummaryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListUsers([FromQuery] PaginationQueryDTO pagination, [FromQuery] string? search = null, [FromQuery] string? role = null)
    {
        if (pagination.Page < 1 || pagination.PageSize < 1 || pagination.PageSize > 100)
            return BadRequest("Invalid pagination parameters");

        IQueryable<User> query = MerrickContext.Users.Include(user => user.Role);

        if (string.IsNullOrWhiteSpace(search) is false)
        {
            string searchLower = search.ToLower();
            query = query.Where(user => user.EmailAddress.ToLower().Contains(searchLower));
        }

        if (string.IsNullOrWhiteSpace(role) is false)
        {
            query = query.Where(user => user.Role.Name == role);
        }

        int totalCount = await query.CountAsync();

        List<GetUserSummaryDTO> users = await query
            .OrderByDescending(user => user.ID)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(user => new GetUserSummaryDTO(
                ID: user.ID,
                EmailAddress: user.EmailAddress,
                Role: user.Role.Name,
                AccountCount: user.Accounts.Count,
                GoldCoins: user.GoldCoins,
                SilverCoins: user.SilverCoins,
                PlinkoTickets: user.PlinkoTickets,
                TimestampCreated: user.TimestampCreated,
                DiscordId: user.DiscordId))
            .ToListAsync();

        return Ok(new PaginatedResultDTO<GetUserSummaryDTO>(users, totalCount, pagination.Page, pagination.PageSize));
    }

    [HttpGet("Users/{id}", Name = "Get User Detail")]
    [ProducesResponseType(typeof(GetUserDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetail(int id)
    {
        User? user = await MerrickContext.Users
            .Include(record => record.Role)
            .Include(record => record.Accounts).ThenInclude(record => record.Clan)
            .SingleOrDefaultAsync(record => record.ID == id);

        if (user is null)
            return NotFound($@"User With ID ""{id}"" Was Not Found");

        GetUserDetailDTO response = new (
            ID: user.ID,
            EmailAddress: user.EmailAddress,
            Role: user.Role.Name,
            GoldCoins: user.GoldCoins,
            SilverCoins: user.SilverCoins,
            PlinkoTickets: user.PlinkoTickets,
            TotalLevel: user.TotalLevel,
            TotalExperience: user.TotalExperience,
            OwnedStoreItems: user.OwnedStoreItems,
            DiscordId: user.DiscordId,
            DiscordLinkedAt: user.DiscordLinkedAt,
            TimestampCreated: user.TimestampCreated,
            TimestampLastActive: user.TimestampLastActive,
            Accounts: user.Accounts
                .OrderByDescending(account => account.IsMain)
                .ThenBy(account => account.Name)
                .Select(account => new GetUserAccountDetailDTO(
                    ID: account.ID,
                    Name: account.Name,
                    IsMain: account.IsMain,
                    AccountType: account.Type.ToString(),
                    ClanName: account.Clan != null ? account.Clan.Name : null,
                    ClanTag: account.Clan != null ? account.Clan.Tag : null,
                    ClanTier: account.ClanTier,
                    AscensionLevel: account.AscensionLevel,
                    TimestampCreated: account.TimestampCreated))
                .ToList());

        return Ok(response);
    }

    [HttpPatch("Users/{id}/Status", Name = "Update User Status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDTO payload)
    {
        User? user = await MerrickContext.Users
            .Include(record => record.Accounts)
            .SingleOrDefaultAsync(record => record.ID == id);

        if (user is null)
            return NotFound($@"User With ID ""{id}"" Was Not Found");

        switch (payload.Action.ToUpperInvariant())
        {
            case "SUSPEND":
                foreach (Account account in user.Accounts)
                    account.Type = AccountType.Disabled;
                break;

            case "UNSUSPEND":
                foreach (Account account in user.Accounts)
                    if (account.Type == AccountType.Disabled)
                        account.Type = AccountType.Legacy;
                break;

            default:
                return BadRequest($@"Invalid action ""{payload.Action}""");
        }

        await MerrickContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("Users/{id}/Role", Name = "Update User Role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDTO payload)
    {
        User? user = await MerrickContext.Users
            .Include(record => record.Role)
            .SingleOrDefaultAsync(record => record.ID == id);

        if (user is null)
            return NotFound($@"User With ID ""{id}"" Was Not Found");

        Role? targetRole = await MerrickContext.Roles.SingleOrDefaultAsync(role => role.Name == payload.Role);

        if (targetRole is null)
            return BadRequest($@"Invalid role ""{payload.Role}""");

        if (targetRole.Name != UserRoles.Administrator && targetRole.Name != UserRoles.User)
            return BadRequest($@"Role ""{payload.Role}"" is not a valid user role");

        user.Role = targetRole;

        await MerrickContext.SaveChangesAsync();

        return NoContent();
    }

    // MARK: - Account Management

    [HttpGet("Accounts", Name = "List Accounts")]
    [ProducesResponseType(typeof(PaginatedResultDTO<GetAccountSummaryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListAccounts([FromQuery] PaginationQueryDTO pagination, [FromQuery] string? search = null, [FromQuery] string? type = null)
    {
        if (pagination.Page < 1 || pagination.PageSize < 1 || pagination.PageSize > 100)
            return BadRequest("Invalid pagination parameters");

        IQueryable<Account> query = MerrickContext.Accounts.Include(account => account.User).Include(account => account.Clan);

        if (string.IsNullOrWhiteSpace(search) is false)
        {
            string searchLower = search.ToLower();
            query = query.Where(account => account.Name.ToLower().Contains(searchLower) || account.User.EmailAddress.ToLower().Contains(searchLower));
        }

        if (string.IsNullOrWhiteSpace(type) is false)
        {
            if (Enum.TryParse<AccountType>(type, ignoreCase: true, out AccountType accountType))
                query = query.Where(account => account.Type == accountType);
        }

        int totalCount = await query.CountAsync();

        List<GetAccountSummaryDTO> accounts = await query
            .OrderByDescending(account => account.ID)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(account => new GetAccountSummaryDTO(
                ID: account.ID,
                Name: account.Name,
                AccountType: account.Type.ToString(),
                EmailAddress: account.User.EmailAddress,
                ClanName: account.Clan != null ? account.Clan.Name : null,
                ClanTag: account.Clan != null ? account.Clan.Tag : null,
                ClanTier: account.ClanTier.ToString(),
                AscensionLevel: account.AscensionLevel,
                TimestampCreated: account.TimestampCreated,
                TimestampLastActive: account.TimestampLastActive))
            .ToListAsync();

        return Ok(new PaginatedResultDTO<GetAccountSummaryDTO>(accounts, totalCount, pagination.Page, pagination.PageSize));
    }

    [HttpGet("Accounts/{id}", Name = "Get Account Detail")]
    [ProducesResponseType(typeof(GetAccountDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountDetail(int id)
    {
        Account? account = await MerrickContext.Accounts
            .Include(record => record.User)
            .Include(record => record.Clan)
            .SingleOrDefaultAsync(record => record.ID == id);

        if (account is null)
            return NotFound($@"Account With ID ""{id}"" Was Not Found");

        GetAccountDetailDTO response = new (
            ID: account.ID,
            Name: account.Name,
            AccountType: account.Type.ToString(),
            EmailAddress: account.User.EmailAddress,
            IsMain: account.IsMain,
            ClanName: account.Clan != null ? account.Clan.Name : null,
            ClanTag: account.Clan != null ? account.Clan.Tag : null,
            ClanTier: account.ClanTier,
            AscensionLevel: account.AscensionLevel,
            SelectedStoreItems: account.SelectedStoreItems,
            TimestampCreated: account.TimestampCreated,
            TimestampLastActive: account.TimestampLastActive);

        return Ok(response);
    }

    [HttpPatch("Accounts/{id}/Type", Name = "Update Account Type")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAccountType(int id, [FromBody] UpdateAccountTypeDTO payload)
    {
        Account? account = await MerrickContext.Accounts.SingleOrDefaultAsync(record => record.ID == id);

        if (account is null)
            return NotFound($@"Account With ID ""{id}"" Was Not Found");

        if (Enum.TryParse<AccountType>(payload.AccountType, ignoreCase: true, out AccountType newType) is false)
            return BadRequest($@"Invalid account type ""{payload.AccountType}""");

        account.Type = newType;

        await MerrickContext.SaveChangesAsync();

        return NoContent();
    }

    // MARK: - Clan Management

    [HttpGet("Clans", Name = "List Clans")]
    [ProducesResponseType(typeof(PaginatedResultDTO<GetClanSummaryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListClans([FromQuery] PaginationQueryDTO pagination, [FromQuery] string? search = null)
    {
        if (pagination.Page < 1 || pagination.PageSize < 1 || pagination.PageSize > 100)
            return BadRequest("Invalid pagination parameters");

        IQueryable<Clan> query = MerrickContext.Clans.Include(clan => clan.Members);

        if (string.IsNullOrWhiteSpace(search) is false)
        {
            string searchLower = search.ToLower();
            query = query.Where(clan => clan.Name.ToLower().Contains(searchLower) || clan.Tag.ToLower().Contains(searchLower));
        }

        int totalCount = await query.CountAsync();

        List<GetClanSummaryDTO> clans = await query
            .OrderByDescending(clan => clan.ID)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(clan => new GetClanSummaryDTO(
                ID: clan.ID,
                Name: clan.Name,
                Tag: clan.Tag,
                MemberCount: clan.Members.Count,
                LeaderName: clan.Members
                    .Where(member => member.ClanTier == ClanTier.Leader)
                    .Select(member => member.Name)
                    .SingleOrDefault() ?? string.Empty,
                TimestampCreated: clan.TimestampCreated))
            .ToListAsync();

        return Ok(new PaginatedResultDTO<GetClanSummaryDTO>(clans, totalCount, pagination.Page, pagination.PageSize));
    }

    [HttpGet("Clans/{id}", Name = "Get Clan Detail")]
    [ProducesResponseType(typeof(GetClanDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClanDetail(int id)
    {
        Clan? clan = await MerrickContext.Clans
            .Include(record => record.Members)
            .ThenInclude(member => member.User)
            .SingleOrDefaultAsync(record => record.ID == id);

        if (clan is null)
            return NotFound($@"Clan With ID ""{id}"" Was Not Found");

        GetClanDetailDTO response = new (
            ID: clan.ID,
            Name: clan.Name,
            Tag: clan.Tag,
            TimestampCreated: clan.TimestampCreated,
            Members: clan.Members
                .OrderByDescending(member => member.ClanTier)
                .ThenBy(member => member.Name)
                .Select(member => new GetClanMemberDetailDTO(
                    AccountID: member.ID,
                    AccountName: member.Name,
                    AccountType: member.Type.ToString(),
                    ClanTier: member.ClanTier,
                    AscensionLevel: member.AscensionLevel,
                    TimestampJoined: member.TimestampJoinedClan ?? member.TimestampCreated))
                .ToList());

        return Ok(response);
    }

    [HttpPost("Clans", Name = "Create Clan")]
    [ProducesResponseType(typeof(GetClanDetailDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateClan([FromBody] CreateClanDTO payload)
    {
        if (string.IsNullOrWhiteSpace(payload.Name) || payload.Name.Length > 30)
            return BadRequest("Clan name must be between 1 and 30 characters");

        if (string.IsNullOrWhiteSpace(payload.Tag) || payload.Tag.Length > 4)
            return BadRequest("Clan tag must be between 1 and 4 characters");

        string tagUpper = payload.Tag.ToUpperInvariant();

        if (await MerrickContext.Clans.AnyAsync(clan => clan.Name == payload.Name || (clan.Name == payload.Name && clan.Tag == tagUpper)))
            return Conflict($@"Clan With Name ""{payload.Name}"" ({tagUpper}) Already Exists");

        Clan clan = new ()
        {
            Name = payload.Name,
            Tag = tagUpper,
            TimestampCreated = DateTimeOffset.UtcNow
        };

        await MerrickContext.Clans.AddAsync(clan);
        await MerrickContext.SaveChangesAsync();

        GetClanDetailDTO response = new (
            ID: clan.ID,
            Name: clan.Name,
            Tag: clan.Tag,
            TimestampCreated: clan.TimestampCreated,
            Members: []);

        return CreatedAtAction(nameof(GetClanDetail), new { id = clan.ID }, response);
    }

    [HttpPatch("Clans/{clanId}/Members/{accountId}/Rank", Name = "Update Clan Member Rank")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClanMemberRank(int clanId, int accountId, [FromBody] UpdateClanMemberRankDTO payload)
    {
        Account? account = await MerrickContext.Accounts
            .Include(record => record.Clan)
            .SingleOrDefaultAsync(record => record.ID == accountId && record.Clan != null && record.Clan.ID == clanId);

        if (account is null)
            return NotFound($@"Account With ID ""{accountId}"" Was Not Found In Clan With ID ""{clanId}""");

        if (Enum.TryParse<ClanTier>(payload.ClanTier, ignoreCase: true, out ClanTier newTier) is false)
            return BadRequest($@"Invalid clan tier ""{payload.ClanTier}""");

        account.ClanTier = newTier;

        await MerrickContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("Clans/{clanId}/Members/{accountId}", Name = "Remove Account From Clan")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAccountFromClan(int clanId, int accountId)
    {
        Account? account = await MerrickContext.Accounts
            .Include(record => record.Clan)
            .SingleOrDefaultAsync(record => record.ID == accountId && record.Clan != null && record.Clan.ID == clanId);

        if (account is null)
            return NotFound($@"Account With ID ""{accountId}"" Was Not Found In Clan With ID ""{clanId}""");

        account.Clan = null;
        account.ClanTier = ClanTier.None;
        account.TimestampJoinedClan = null;

        await MerrickContext.SaveChangesAsync();

        return NoContent();
    }
}
