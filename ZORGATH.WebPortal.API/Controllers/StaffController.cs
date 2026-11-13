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

    [HttpPost("Hardware/ResetHWID", Name = "Reset Account HWID")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAccountHWID([FromBody] string accountName)
    {
        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(accountName));

        if (account is null)
        {
            return NotFound($@"Account ""{accountName}"" Was Not Found");
        }

        account.SystemInformationHashCollection.Clear();
        account.SystemInformationCollection.Clear();

        StaffMemberAction action = new ()
        {
            SourceID = Guid.Empty,
            TargetID = account.Name,
            Action = StaffMemberActionType.RemoveHWID,
            Details = "Cleared system information and hardware hashes",
            Timestamp = DateTimeOffset.UtcNow
        };

        await MerrickContext.StaffMemberActions.AddAsync(action);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Hardware Hashes Reset For Account ""{AccountName}""", accountName);

        return Ok(new { Message = $"Hardware hashes cleared for account {accountName}" });
    }

    [HttpPost("Hardware/ResetNetwork", Name = "Reset Account IP And MAC")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAccountNetwork([FromBody] string accountName)
    {
        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(accountName));

        if (account is null)
        {
            return NotFound($@"Account ""{accountName}"" Was Not Found");
        }

        account.IPAddressCollection.Clear();
        account.MACAddressCollection.Clear();

        StaffMemberAction action = new ()
        {
            SourceID = Guid.Empty,
            TargetID = account.Name,
            Action = StaffMemberActionType.RemoveIP,
            Details = "Cleared IP and MAC address history",
            Timestamp = DateTimeOffset.UtcNow
        };

        await MerrickContext.StaffMemberActions.AddAsync(action);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Network Information Reset For Account ""{AccountName}""", accountName);

        return Ok(new { Message = $"Network history cleared for account {accountName}" });
    }

    [HttpGet("Collisions/{accountName}", Name = "Detect Account Collisions")]
    [ProducesResponseType(typeof(List<AccountCollisionDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DetectAccountCollisions(string accountName)
    {
        Account? targetAccount = await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(accountName));

        if (targetAccount is null)
        {
            return NotFound($@"Account ""{accountName}"" Was Not Found");
        }

        List<Account> allAccounts = await MerrickContext.Accounts
            .Include(account => account.User)
            .Where(account => account.Name != accountName)
            .ToListAsync();

        List<AccountCollisionDTO> collisions = [];

        // Match By IP Address
        List<string> ipMatches = allAccounts
            .Where(other => other.IPAddressCollection.Intersect(targetAccount.IPAddressCollection).Any())
            .Select(other => other.Name)
            .ToList();

        if (ipMatches.Count > 0)
        {
            collisions.Add(new AccountCollisionDTO(accountName, ipMatches, "Shared IP Address"));
        }

        // Match By MAC Address
        List<string> macMatches = allAccounts
            .Where(other => other.MACAddressCollection.Intersect(targetAccount.MACAddressCollection).Any())
            .Select(other => other.Name)
            .ToList();

        if (macMatches.Count > 0)
        {
            collisions.Add(new AccountCollisionDTO(accountName, macMatches, "Shared MAC Address"));
        }

        // Match By Hardware Hash
        List<string> hwMatches = allAccounts
            .Where(other => other.SystemInformationHashCollection.Intersect(targetAccount.SystemInformationHashCollection).Any())
            .Select(other => other.Name)
            .ToList();

        if (hwMatches.Count > 0)
        {
            collisions.Add(new AccountCollisionDTO(accountName, hwMatches, "Shared Hardware Specification Hash"));
        }

        return Ok(collisions);
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
}

