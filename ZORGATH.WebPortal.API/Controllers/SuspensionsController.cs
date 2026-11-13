namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
[Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
public sealed class SuspensionsController(MerrickContext databaseContext, ILogger<SuspensionsController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;

    [HttpPost(Name = "Issue Suspension")]
    [ProducesResponseType(typeof(SuspensionDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IssueSuspension([FromBody] SuspensionIssueDTO payload)
    {
        Account? offender = await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(payload.OffenderAccountName));

        if (offender is null)
        {
            return NotFound($@"Offender Account ""{payload.OffenderAccountName}"" Was Not Found");
        }

        TimeSpan duration = ParseDuration(payload.Duration);

        if (duration.Equals(TimeSpan.Zero))
        {
            return BadRequest($@"Invalid Suspension Duration ""{payload.Duration}""; Use Values Like ""1D"", ""3D"", ""1W"", Or ""1M""");
        }

        int previousSuspensionsCount = await MerrickContext.Suspensions
            .CountAsync(suspension => suspension.OffenderAccountID == offender.ID);

        float multiplier = 1.0f + (0.30f * previousSuspensionsCount);
        DateTimeOffset timestampStart = DateTimeOffset.UtcNow;
        DateTimeOffset timestampBaseEnd = timestampStart.Add(duration);
        DateTimeOffset timestampEnd = timestampStart.Add(duration * multiplier);

        Suspension suspension = new ()
        {
            OffenderAccount = offender,
            OffenderAccountID = offender.ID,
            IssuerAccount = null,
            IssuerAccountID = null,
            SuspendByMask = payload.SuspendByMask,
            TimestampStart = timestampStart,
            TimestampBaseEnd = timestampBaseEnd,
            TimestampEnd = timestampEnd,
            Multiplier = multiplier,
            Reason = payload.Reason,
            InternalDetails = payload.InternalDetails,
            Override = false
        };

        await MerrickContext.Suspensions.AddAsync(suspension);

        StaffMemberAction action = new ()
        {
            SourceID = Guid.Empty,
            TargetID = offender.Name,
            Action = "Issue Suspension",
            Details = $"Suspended until {timestampEnd:u} (Multiplier: {multiplier:F2}). Reason: {payload.Reason}",
            Timestamp = DateTimeOffset.UtcNow
        };

        await MerrickContext.StaffMemberActions.AddAsync(action);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Issued Suspension For Account ""{AccountName}"" Until {EndTimestamp}", offender.Name, timestampEnd);

        SuspensionDTO response = new
        (
            suspension.ID,
            offender.Name,
            "SYSTEM/STAFF",
            timestampStart,
            timestampEnd,
            payload.Reason,
            multiplier,
            false
        );

        return CreatedAtAction(nameof(IssueSuspension), response);
    }

    [HttpGet("Account/{accountName}", Name = "Get Suspensions By Account Name")]
    [ProducesResponseType(typeof(List<SuspensionDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuspensionsByAccountName(string accountName)
    {
        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(accountName));

        if (account is null)
        {
            return NotFound($@"Account ""{accountName}"" Was Not Found");
        }

        List<Suspension> suspensions = await MerrickContext.Suspensions
            .Include(suspension => suspension.OffenderAccount)
            .Include(suspension => suspension.IssuerAccount)
            .Where(suspension => suspension.OffenderAccountID == account.ID)
            .OrderByDescending(suspension => suspension.TimestampStart)
            .ToListAsync();

        List<SuspensionDTO> response = suspensions.Select(suspension => new SuspensionDTO
        (
            suspension.ID,
            suspension.OffenderAccount?.Name ?? accountName,
            suspension.IssuerAccount?.Name ?? "SYSTEM",
            suspension.TimestampStart,
            suspension.TimestampEnd,
            suspension.Reason,
            suspension.Multiplier,
            suspension.Override
        )).ToList();

        return Ok(response);
    }

    [HttpPost("{id:int}/Override", Name = "Override Suspension")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OverrideSuspension(int id)
    {
        Suspension? suspension = await MerrickContext.Suspensions
            .SingleOrDefaultAsync(candidate => candidate.ID == id);

        if (suspension is null)
        {
            return NotFound($@"Suspension With ID {id} Was Not Found");
        }

        suspension.Override = true;
        suspension.TimestampEnd = DateTimeOffset.UtcNow;

        StaffMemberAction action = new ()
        {
            SourceID = Guid.Empty,
            TargetID = suspension.OffenderAccountID.ToString(),
            Action = StaffMemberActionType.OverrideSuspension,
            Details = $"Overrode suspension ID {id}",
            Timestamp = DateTimeOffset.UtcNow
        };

        await MerrickContext.StaffMemberActions.AddAsync(action);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Suspension {SuspensionID} Overridden", id);

        return Ok(new { Message = $"Suspension {id} overrode successfully" });
    }

    internal static TimeSpan ParseDuration(string duration)
    {
        if (string.IsNullOrWhiteSpace(duration) || duration.Length < 2)
        {
            return TimeSpan.Zero;
        }

        char unit = char.ToUpperInvariant(duration[^1]);
        string valuePart = duration[..^1];

        if (int.TryParse(valuePart, out int amount).Equals(false) || amount <= 0)
        {
            return TimeSpan.Zero;
        }

        return unit switch
        {
            'H' => TimeSpan.FromHours(amount),
            'D' => TimeSpan.FromDays(amount),
            'W' => TimeSpan.FromDays(amount * 7),
            'M' => TimeSpan.FromDays(amount * 30),
            'Y' => TimeSpan.FromDays(amount * 365),
            _   => TimeSpan.Zero
        };
    }
}

