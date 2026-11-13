namespace KONGOR.MasterServer.Controllers.ClientRequester;

[ApiController]
[Route("client_require_identity.php")]
[Consumes("application/x-www-form-urlencoded")]
public class ClientRequireIdentityController(IDatabase distributedCache) : ControllerBase
{
    private IDatabase DistributedCache { get; } = distributedCache;

    [HttpPost(Name = "Client Require Identity")]
    [HttpGet(Name = "Client Require Identity Get")]
    public async Task<IActionResult> CheckIdentityRequirement()
    {
        string? cookie = Request.Form["cookie"].ToString() ?? Request.Query["cookie"].ToString();

        // 0: Identity / 2FA verification not required, 1: required
        Dictionary<string, object> response = new ()
        {
            ["required"] = 0,
            ["0"] = true,
            ["vested_threshold"] = 5
        };

        if (string.IsNullOrEmpty(cookie) is false)
        {
            (bool isValid, _) = await DistributedCache.ValidateAccountSessionCookie(cookie);

            if (isValid)
            {
                response["required"] = 0;
            }
        }

        return Ok(PhpSerialization.Serialize(response));
    }
}

