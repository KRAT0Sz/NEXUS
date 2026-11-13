namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Relaxed)]
[AllowAnonymous]
public sealed class StatisticsController(MerrickContext databaseContext, ILogger<StatisticsController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;

    [HttpGet("Portal", Name = "Get Server Overview Statistics")]
    [ProducesResponseType(typeof(ServerStatsDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServerStats()
    {
        int totalAccounts = await MerrickContext.Accounts.CountAsync();
        int totalUsers = await MerrickContext.Users.CountAsync();
        int totalClans = await MerrickContext.Clans.CountAsync();

        ServerStatsDTO response = new (totalAccounts, totalUsers, totalClans);
        return Ok(response);
    }

    [HttpGet("Locations/Live", Name = "Get Live Active Locations")]
    [ProducesResponseType(typeof(List<LocationPointDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLiveActiveLocations()
    {
        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddMinutes(-30);

        List<LocationPointDTO> locations = await MerrickContext.GeoInformations
            .Include(info => info.Country)
            .GroupBy(info => new { info.Latitude, info.Longitude, Country = info.Country != null ? info.Country.Name : "Unknown" })
            .Select(group => new LocationPointDTO
            (
                group.Key.Latitude,
                group.Key.Longitude,
                group.Key.Country,
                group.Count()
            ))
            .ToListAsync();

        return Ok(locations);
    }

    [HttpGet("Locations/Last30Days", Name = "Get Last 30 Days Active Locations")]
    [ProducesResponseType(typeof(List<LocationPointDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLast30DaysActiveLocations()
    {
        List<LocationPointDTO> locations = await MerrickContext.GeoInformations
            .Include(info => info.Country)
            .GroupBy(info => new { info.Latitude, info.Longitude, Country = info.Country != null ? info.Country.Name : "Unknown" })
            .Select(group => new LocationPointDTO
            (
                group.Key.Latitude,
                group.Key.Longitude,
                group.Key.Country,
                group.Count()
            ))
            .ToListAsync();

        return Ok(locations);
    }

    [HttpGet("Leaderboard", Name = "Get Leaderboard Rankings")]
    [ProducesResponseType(typeof(List<LeaderboardEntryDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaderboardStats([FromQuery] int take = 50)
    {
        int limit = Math.Clamp(take, 1, 100);

        List<AccountStatistics> topStats = await MerrickContext.AccountStatistics
            .Include(stat => stat.Account)
                .ThenInclude(account => account.Clan)
            .OrderByDescending(stat => stat.SkillRating)
            .Take(limit)
            .ToListAsync();

        List<LeaderboardEntryDTO> response = [];
        int rank = 1;

        foreach (AccountStatistics stat in topStats)
        {
            response.Add(new LeaderboardEntryDTO
            (
                rank++,
                stat.Account.Name,
                stat.Account.Clan?.Tag ?? string.Empty,
                stat.SkillRating,
                stat.MatchesWon,
                stat.MatchesLost
            ));
        }

        return Ok(response);
    }
}
