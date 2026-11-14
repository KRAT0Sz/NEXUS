namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
[Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
public sealed class StaffController(MerrickContext databaseContext, ILogger<StaffController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;

    [HttpPost("Promote", Name = "Promote User Role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PromoteUserRole([FromBody] PromoteDemoteUserDTO payload)
    {
        User? user = await MerrickContext.Users
            .Include(target => target.Role)
            .SingleOrDefaultAsync(candidate => candidate.EmailAddress.Equals(payload.UserName));

        if (user is null)
        {
            return NotFound($@"User ""{payload.UserName}"" Was Not Found");
        }

        Role? targetRole = await MerrickContext.Roles
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(payload.Role));

        if (targetRole is null)
        {
            return BadRequest($@"Target Role ""{payload.Role}"" Does Not Exist");
        }

        string previousRole = user.Role.Name;
        user.Role = targetRole;

        StaffMemberAction action = new ()
        {
            SourceID = Guid.Empty,
            TargetID = user.EmailAddress,
            Action = $"Promote Role To {payload.Role}",
            Details = $"Changed role from {previousRole} to {payload.Role}",
            Timestamp = DateTimeOffset.UtcNow
        };

        await MerrickContext.StaffMemberActions.AddAsync(action);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Promoted User ""{EmailAddress}"" To Role ""{Role}""", user.EmailAddress, payload.Role);

        return Ok(new { Message = $"User {user.EmailAddress} promoted to {payload.Role}" });
    }

    [HttpPost("Currency/Issue", Name = "Issue Currency To User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IssueCurrency([FromBody] IssueCurrencyDTO payload, [FromQuery] string currencyType)
    {
        Account? account = await MerrickContext.Accounts
            .Include(target => target.User)
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(payload.AccountName));

        if (account is null)
        {
            return NotFound($@"Account ""{payload.AccountName}"" Was Not Found");
        }

        User user = account.User;

        switch (currencyType.ToLowerInvariant())
        {
            case "gold":
                user.GoldCoins += payload.Amount;
                break;
            case "silver":
                user.SilverCoins += payload.Amount;
                break;
            case "tickets":
                user.PlinkoTickets += payload.Amount;
                break;
            default:
                return BadRequest($@"Unsupported Currency Type ""{currencyType}""; Use ""gold"", ""silver"", Or ""tickets""");
        }

        StaffMemberAction action = new ()
        {
            SourceID = Guid.Empty,
            TargetID = account.Name,
            Action = $"Issue {currencyType}",
            Details = $"Granted {payload.Amount} {currencyType}",
            Timestamp = DateTimeOffset.UtcNow
        };

        await MerrickContext.StaffMemberActions.AddAsync(action);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Granted {Amount} {Currency} To Account ""{AccountName}""", payload.Amount, currencyType, payload.AccountName);

        return Ok(new { Message = $"Granted {payload.Amount} {currencyType} to account {payload.AccountName}" });
    }

    [HttpGet("Actions", Name = "Get Staff Member Actions")]
    [ProducesResponseType(typeof(List<StaffActionLogDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStaffMemberActions([FromQuery] int count = 50)
    {
        List<StaffMemberAction> actions = await MerrickContext.StaffMemberActions
            .OrderByDescending(action => action.Timestamp)
            .Take(count)
            .ToListAsync();

        List<StaffActionLogDTO> response = actions.Select(action => new StaffActionLogDTO
        (
            action.ID,
            action.SourceID,
            action.TargetID,
            action.Action,
            action.Details,
            action.Timestamp
        )).ToList();

        return Ok(response);
    }

    [HttpGet("Account/{accountName}/Type", Name = "Get Account Type")]
    [ProducesResponseType(typeof(AccountTypeResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountType(string accountName)
    {
        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(accountName));

        if (account is null)
        {
            return NotFound($@"Account ""{accountName}"" Was Not Found");
        }

        return Ok(new AccountTypeResponseDTO(account.Name, account.Type.ToString(), (int)account.Type));
    }

    [HttpPost("Account/Enable", Name = "Enable Account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableAccount([FromBody] AccountActionDTO payload)
    {
        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(payload.AccountName));

        if (account is null)
        {
            return NotFound($@"Account ""{payload.AccountName}"" Was Not Found");
        }

        account.Type = AccountType.Legacy;

        StaffMemberAction action = new ()
        {
            SourceID = Guid.Empty,
            TargetID = account.Name,
            Action = StaffMemberActionType.EnableAccount,
            Details = "Enabled account (restored to Legacy)",
            Timestamp = DateTimeOffset.UtcNow
        };

        await MerrickContext.StaffMemberActions.AddAsync(action);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Account ""{AccountName}"" Enabled", account.Name);

        return Ok(new { Message = $"Account {account.Name} enabled" });
    }

    [HttpPost("Account/Disable", Name = "Disable Account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableAccount([FromBody] AccountActionDTO payload)
    {
        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(payload.AccountName));

        if (account is null)
        {
            return NotFound($@"Account ""{payload.AccountName}"" Was Not Found");
        }

        account.Type = AccountType.Disabled;

        StaffMemberAction action = new ()
        {
            SourceID = Guid.Empty,
            TargetID = account.Name,
            Action = StaffMemberActionType.DisableAccount,
            Details = "Disabled account",
            Timestamp = DateTimeOffset.UtcNow
        };

        await MerrickContext.StaffMemberActions.AddAsync(action);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Account ""{AccountName}"" Disabled", account.Name);

        return Ok(new { Message = $"Account {account.Name} disabled" });
    }

    [HttpPost("Account/PromoteToHost", Name = "Promote Account To Host")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PromoteAccountToHost([FromBody] AccountActionDTO payload)
    {
        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(payload.AccountName));

        if (account is null)
        {
            return NotFound($@"Account ""{payload.AccountName}"" Was Not Found");
        }

        account.Type = AccountType.ServerHost;

        StaffMemberAction action = new ()
        {
            SourceID = Guid.Empty,
            TargetID = account.Name,
            Action = "Promote To TMM Host",
            Details = "Promoted account to ServerHost",
            Timestamp = DateTimeOffset.UtcNow
        };

        await MerrickContext.StaffMemberActions.AddAsync(action);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Account ""{AccountName}"" Promoted To TMM Host", account.Name);

        return Ok(new { Message = $"Account {account.Name} promoted to ServerHost" });
    }

    [HttpPost("Lookup", Name = "Lookup Accounts And Users")]
    [ProducesResponseType(typeof(List<ConnectedAccountUserResultDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LookupAccountsAndUsers([FromBody] LookupQueryDTO payload)
    {
        string query = payload.Query.Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Lookup query cannot be empty");
        }

        List<Account> matchedAccounts = await MerrickContext.Accounts
            .Include(candidate => candidate.User)
                .ThenInclude(u => u.Role)
            .Include(candidate => candidate.Clan)
            .Where(candidate => EF.Functions.Like(candidate.Name, $"%{query}%"))
            .Take(25)
            .ToListAsync();

        List<User> matchedUsers = await MerrickContext.Users
            .Include(candidate => candidate.Accounts)
            .Include(candidate => candidate.Role)
            .Where(candidate => EF.Functions.Like(candidate.EmailAddress, $"%{query}%"))
            .Take(25)
            .ToListAsync();

        List<ConnectedAccountUserResultDTO> results = [];

        foreach (Account acc in matchedAccounts)
        {
            results.Add(new ConnectedAccountUserResultDTO
            (
                "account",
                acc.Name,
                acc.IsMain,
                acc.Type.ToString(),
                acc.NameWithClanTag,
                MaskEmail(acc.User.EmailAddress),
                acc.User.Role?.Name ?? "USER"
            ));
        }

        foreach (User user in matchedUsers)
        {
            foreach (Account acc in user.Accounts)
            {
                if (results.Any(existing => existing.Name == acc.Name))
                {
                    continue;
                }

                results.Add(new ConnectedAccountUserResultDTO
                (
                    "user",
                    acc.Name,
                    acc.IsMain,
                    acc.Type.ToString(),
                    acc.NameWithClanTag,
                    MaskEmail(user.EmailAddress),
                    user.Role?.Name ?? "USER"
                ));
            }
        }

        return Ok(results);
    }

    [HttpGet("Roster", Name = "Get Staff Roster")]
    [ProducesResponseType(typeof(List<StaffRosterMemberDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStaffRoster()
    {
        List<User> staffUsers = await MerrickContext.Users
            .Include(user => user.Role)
            .Include(user => user.Accounts)
            .Where(user => user.Role.Name != UserRoles.User)
            .ToListAsync();

        List<StaffRosterMemberDTO> roster = staffUsers.Select(user => new StaffRosterMemberDTO
        (
            user.ID,
            user.Accounts.FirstOrDefault(acc => acc.IsMain)?.Name ?? user.EmailAddress,
            user.EmailAddress,
            user.Role.Name
        )).ToList();

        return Ok(roster);
    }

    private static string MaskEmail(string email)
    {
        int atIndex = email.IndexOf('@');

        if (atIndex <= 1)
        {
            return "***" + (atIndex >= 0 ? email[atIndex..] : string.Empty);
        }

        return email[0] + "***" + email[atIndex..];
    }
}

