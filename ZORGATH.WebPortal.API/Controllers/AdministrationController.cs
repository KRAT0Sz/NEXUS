namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class AdministrationController(MerrickContext databaseContext, ILogger<AdministrationController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;

    private static readonly TimeSpan HostAccountAuthorisationTokenValidity = TimeSpan.FromHours(24);

    [HttpPost("Authorise/Host", Name = "Issue Host Account Authorisation Token")]
    [Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
    [ProducesResponseType(typeof(HostAccountAuthorisationTokenDTO), StatusCodes.Status201Created)]
    public async Task<IActionResult> IssueHostAccountAuthorisationToken()
    {
        Token token = new ()
        {
            Purpose = TokenPurpose.HostAccountAuthorisation,
            EmailAddress = string.Empty,
            Value = Guid.CreateVersion7(),
            Data = AccountType.ServerHost.ToString(),
            Validity = HostAccountAuthorisationTokenValidity
        };

        await MerrickContext.Tokens.AddAsync(token);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Issued Host Account Authorisation Token ""{Token}""", token.Value);

        HostAccountAuthorisationTokenDTO response = new (token.Value, token.TimestampCreated + token.Validity);

        return CreatedAtAction(nameof(IssueHostAccountAuthorisationToken), response);
    }

    /// <summary>
    ///     Broadcasts a system message to every account's in-game inbox.
    ///     This is the retroactive counterpart to the system messages seeded at account creation: it reaches accounts that already exist.
    /// </summary>
    [HttpPost("Broadcast", Name = "Broadcast System Message To All Accounts")]
    [Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
    [ProducesResponseType(typeof(BroadcastSystemMessageResultDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BroadcastSystemMessage([FromBody] BroadcastSystemMessageDTO payload)
    {
        int messageSubjectMaximumLength = typeof(Message).GetMaximumLength(nameof(Message.Subject));
        int messageSubtitleMaximumLength = typeof(Message).GetMaximumLength(nameof(Message.Subtitle));
        int messageBodyTitleMaximumLength = typeof(Message).GetMaximumLength(nameof(Message.BodyTitle));
        int messageBodyMaximumLength = typeof(Message).GetMaximumLength(nameof(Message.Body));
        int messageFooterMaximumLength = typeof(Message).GetMaximumLength(nameof(Message.Footer));

        if (string.IsNullOrWhiteSpace(payload.Subject) || payload.Subject.Length > messageSubjectMaximumLength)
            return BadRequest($@"The Subject Is Required And Must Not Exceed {messageSubjectMaximumLength} Characters");

        if (string.IsNullOrWhiteSpace(payload.Body) || payload.Body.Length > messageBodyMaximumLength)
            return BadRequest($@"The Body Is Required And Must Not Exceed {messageBodyMaximumLength} Characters");

        if (payload.Subtitle?.Length > messageSubtitleMaximumLength)
            return BadRequest($@"The Subtitle Must Not Exceed {messageSubtitleMaximumLength} Characters");

        if (payload.BodyTitle?.Length > messageBodyTitleMaximumLength)
            return BadRequest($@"The Body Title Must Not Exceed {messageBodyTitleMaximumLength} Characters");

        if (payload.Footer?.Length > messageFooterMaximumLength)
            return BadRequest($@"The Footer Must Not Exceed {messageFooterMaximumLength} Characters");

        List<int> accountIDs = await MerrickContext.Accounts.Select(account => account.ID).ToListAsync();

        // Messages Are Inserted In Batches So That A Broadcast To Many Accounts Does Not Build One Enormous Transaction Or Change-Tracker Graph.
        const int messageBroadcastBatchSize = 1000;

        for (int index = 0; index < accountIDs.Count; index += messageBroadcastBatchSize)
        {
            IEnumerable<Message> batch = accountIDs
                .Skip(index)
                .Take(messageBroadcastBatchSize)
                .Select(accountID => new Message
                {
                    AccountID = accountID,
                    Subject = payload.Subject,
                    Subtitle = payload.Subtitle ?? string.Empty,
                    BodyTitle = payload.BodyTitle ?? string.Empty,
                    Body = payload.Body,
                    Footer = payload.Footer ?? string.Empty
                });

            await MerrickContext.Messages.AddRangeAsync(batch);
            await MerrickContext.SaveChangesAsync();

            // Detach The Just-Saved Messages So The Change Tracker Does Not Accumulate Across Batches
            MerrickContext.ChangeTracker.Clear();
        }

        Logger.LogInformation(@"Broadcast A System Message (""{Subject}"") To {AccountCount} Account(s)", payload.Subject, accountIDs.Count);

        return Ok(new BroadcastSystemMessageResultDTO(accountIDs.Count));
    }

    [HttpPost("Account/Rename", Name = "Rename Account")]
    [Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RenameAccount([FromBody] RenameAccountDTO payload)
    {
        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(payload.AccountName));

        if (account is null)
        {
            return NotFound($@"Account ""{payload.AccountName}"" Was Not Found");
        }

        string trimmedNewName = payload.NewAccountName.Trim();

        if (string.IsNullOrWhiteSpace(trimmedNewName) || trimmedNewName.Length > 15)
        {
            return BadRequest("New account name must be between 1 and 15 characters");
        }

        bool nameExists = await MerrickContext.Accounts.AnyAsync(candidate => candidate.Name.Equals(trimmedNewName));

        if (nameExists)
        {
            return Conflict($@"Account With Name ""{trimmedNewName}"" Already Exists");
        }

        string oldName = account.Name;
        account.Name = trimmedNewName;

        StaffMemberAction action = new ()
        {
            SourceID = Guid.Empty,
            TargetID = trimmedNewName,
            Action = StaffMemberActionType.RenameAccount,
            Details = $"Renamed account from {oldName} to {trimmedNewName}",
            Timestamp = DateTimeOffset.UtcNow
        };

        await MerrickContext.StaffMemberActions.AddAsync(action);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Renamed Account ""{OldName}"" To ""{NewName}""", oldName, trimmedNewName);

        return Ok(new { Message = $"Account successfully renamed from {oldName} to {trimmedNewName}" });
    }

    [HttpGet("Account/{accountName}/Upgrades", Name = "Get Account Upgrades")]
    [Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountUpgrades(string accountName)
    {
        Account? account = await MerrickContext.Accounts
            .Include(candidate => candidate.User)
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(accountName));

        if (account is null)
        {
            return NotFound($@"Account ""{accountName}"" Was Not Found");
        }

        return Ok(account.User.OwnedStoreItems);
    }

    [HttpPost("Upgrade/Grant", Name = "Grant Account Upgrade")]
    [Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GrantUpgrade([FromBody] AccountUpgradeDTO payload)
    {
        Account? account = await MerrickContext.Accounts
            .Include(candidate => candidate.User)
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(payload.AccountName));

        if (account is null)
        {
            return NotFound($@"Account ""{payload.AccountName}"" Was Not Found");
        }

        string code = payload.UpgradeCode.Trim();

        if (account.User.OwnedStoreItems.Contains(code).Equals(false))
        {
            account.User.OwnedStoreItems.Add(code);

            StaffMemberAction action = new ()
            {
                SourceID = Guid.Empty,
                TargetID = account.Name,
                Action = StaffMemberActionType.GrantUpgrade,
                Details = $"Granted upgrade: {code}",
                Timestamp = DateTimeOffset.UtcNow
            };

            await MerrickContext.StaffMemberActions.AddAsync(action);
            await MerrickContext.SaveChangesAsync();
        }

        return Ok(new { Message = $"Upgrade {code} granted to {account.Name}" });
    }

    [HttpPost("Upgrade/Revoke", Name = "Revoke Account Upgrade")]
    [Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeUpgrade([FromBody] AccountUpgradeDTO payload)
    {
        Account? account = await MerrickContext.Accounts
            .Include(candidate => candidate.User)
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(payload.AccountName));

        if (account is null)
        {
            return NotFound($@"Account ""{payload.AccountName}"" Was Not Found");
        }

        string code = payload.UpgradeCode.Trim();
        account.User.OwnedStoreItems.Remove(code);

        StaffMemberAction action = new ()
        {
            SourceID = Guid.Empty,
            TargetID = account.Name,
            Action = StaffMemberActionType.RevokeUpgrade,
            Details = $"Revoked upgrade: {code}",
            Timestamp = DateTimeOffset.UtcNow
        };

        await MerrickContext.StaffMemberActions.AddAsync(action);
        await MerrickContext.SaveChangesAsync();

        return Ok(new { Message = $"Upgrade {code} revoked from {account.Name}" });
    }
}
