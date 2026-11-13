namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
[Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
public sealed class CommunityController(MerrickContext databaseContext, ILogger<CommunityController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;

    [HttpPost("Issue/RedeemableCode", Name = "Issue Redeemable Codes")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IssueRedeemableCode([FromBody] IssueRedeemableCodesDTO payload)
    {
        if (payload.Amount < 1 || payload.Amount > 1000)
        {
            return BadRequest("The Amount Of Requested Redeemable Codes Must Be Between 1 And 1000");
        }

        if (payload.SilverCoins is null && payload.GoldCoins is null && payload.PlinkoTickets is null && payload.ProductID is null)
        {
            return BadRequest("No Reward Specified For The Redeemable Code");
        }

        List<RedeemableCode> createdCodes = [];
        List<string> codeStrings = [];

        for (int index = 0; index < payload.Amount; index++)
        {
            string codeString = Guid.CreateVersion7().ToString("N")[..16].ToUpperInvariant();

            RedeemableCode code = new ()
            {
                Code = codeString,
                SilverCoinsReward = payload.SilverCoins ?? 0,
                GoldCoinsReward = payload.GoldCoins ?? 0,
                PlinkoTicketsReward = payload.PlinkoTickets ?? 0,
                ProductID = payload.ProductID,
                TimestampCreated = DateTimeOffset.UtcNow,
                TimestampRedeemed = null
            };

            createdCodes.Add(code);
            codeStrings.Add(codeString);
        }

        await MerrickContext.RedeemableCodes.AddRangeAsync(createdCodes);

        StaffMemberAction action = new ()
        {
            SourceID = Guid.Empty,
            TargetID = "COMMUNITY_REDEEMABLE_CODES",
            Action = StaffMemberActionType.IssueRedeemableCodes,
            Details = $"Generated {payload.Amount} codes (Gold: {payload.GoldCoins ?? 0}, Silver: {payload.SilverCoins ?? 0}, Tickets: {payload.PlinkoTickets ?? 0}, Product: {payload.ProductID?.ToString() ?? "NULL"})",
            Timestamp = DateTimeOffset.UtcNow
        };

        await MerrickContext.StaffMemberActions.AddAsync(action);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Generated {Count} Redeemable Codes", payload.Amount);

        return CreatedAtAction(nameof(IssueRedeemableCode), codeStrings);
    }
}

