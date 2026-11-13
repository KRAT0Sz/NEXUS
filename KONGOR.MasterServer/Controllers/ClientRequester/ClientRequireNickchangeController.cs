namespace KONGOR.MasterServer.Controllers.ClientRequester;

[ApiController]
[Route("client_require_nickchange.php")]
[Consumes("application/x-www-form-urlencoded")]
public class ClientRequireNickchangeController(MerrickContext databaseContext, IDatabase distributedCache) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;

    [HttpPost(Name = "Client Require Nickname Change")]
    [HttpGet(Name = "Client Require Nickname Change Get")]
    public async Task<IActionResult> CheckNicknameChangeRequirement()
    {
        string? cookie = Request.Form["cookie"].ToString() ?? Request.Query["cookie"].ToString();

        int required = 0;

        if (string.IsNullOrEmpty(cookie) is false)
        {
            (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

            if (isValid && accountName is not null)
            {
                Account? account = await MerrickContext.Accounts
                    .AsNoTracking()
                    .SingleOrDefaultAsync(acc => acc.Name.Equals(accountName));

                if (account is not null)
                {
                    // If account has forced nickname change record that is still matching current name
                    bool needsChange = await MerrickContext.NicknameChanges
                        .AsNoTracking()
                        .AnyAsync(nc => nc.AccountID == account.ID && nc.CurrentUserName.Equals(account.Name));

                    if (needsChange)
                        required = 1;
                }
            }
        }

        Dictionary<string, object> response = new ()
        {
            ["required"] = required,
            ["0"] = true,
            ["vested_threshold"] = 5
        };

        return Ok(PhpSerialization.Serialize(response));
    }
}

