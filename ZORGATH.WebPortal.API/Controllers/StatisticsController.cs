namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Relaxed)]
[AllowAnonymous]
public sealed class StatisticsController(MerrickContext databaseContext, ILogger<StatisticsController> logger, IServiceProvider serviceProvider) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;
    private IDatabase? DistributedCacheStore { get; } = serviceProvider.GetService<IDatabase>();

    [HttpGet("Portal", Name = "Get Server Overview Statistics")]
    [ProducesResponseType(typeof(ServerStatsDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServerStats()
    {
        int totalAccounts = await MerrickContext.Accounts.CountAsync();
        int totalUsers = await MerrickContext.Users.CountAsync();
        int totalClans = await MerrickContext.Clans.CountAsync();

        int onlinePlayers = 0;
        int matchesInProgress = 0;

        if (DistributedCacheStore is not null)
        {
            try
            {
                RedisValue onlinePlayersValue = await DistributedCacheStore.StringGetAsync(CacheConstants.OnlinePlayersCountKey);
                if (onlinePlayersValue.HasValue && int.TryParse(onlinePlayersValue.ToString(), out int parsedOnline))
                {
                    onlinePlayers = parsedOnline;
                }

                RedisValue matchesValue = await DistributedCacheStore.StringGetAsync(CacheConstants.MatchesInProgressCountKey);
                if (matchesValue.HasValue && int.TryParse(matchesValue.ToString(), out int parsedMatches))
                {
                    matchesInProgress = parsedMatches;
                }
                else
                {
                    List<MatchServer> matchServers = await DistributedCacheStore.GetMatchServers();
                    matchesInProgress = matchServers.Count(server => server.Status is ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE);
                }
            }
            catch (Exception exception)
            {
                Logger.LogWarning(exception, "Failed to read live server statistics from distributed cache.");
            }
        }

        ServerStatsDTO response = new
        (
            TotalPlayers: totalAccounts,
            TotalAccounts: totalAccounts,
            TotalUsers: totalUsers,
            TotalClans: totalClans,
            OnlinePlayers: onlinePlayers,
            MatchesInProgress: matchesInProgress
        );

        return Ok(response);
    }

    [HttpGet("Active", Name = "Get Active Players And Matches")]
    [ProducesResponseType(typeof(ActiveStatsDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveStats()
    {
        int onlinePlayers = 0;
        int matchesInProgress = 0;

        if (DistributedCacheStore is not null)
        {
            try
            {
                RedisValue onlinePlayersValue = await DistributedCacheStore.StringGetAsync(CacheConstants.OnlinePlayersCountKey);
                if (onlinePlayersValue.HasValue && int.TryParse(onlinePlayersValue.ToString(), out int parsedOnline))
                {
                    onlinePlayers = parsedOnline;
                }

                RedisValue matchesValue = await DistributedCacheStore.StringGetAsync(CacheConstants.MatchesInProgressCountKey);
                if (matchesValue.HasValue && int.TryParse(matchesValue.ToString(), out int parsedMatches))
                {
                    matchesInProgress = parsedMatches;
                }
                else
                {
                    List<MatchServer> matchServers = await DistributedCacheStore.GetMatchServers();
                    matchesInProgress = matchServers.Count(server => server.Status is ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE);
                }
            }
            catch (Exception exception)
            {
                Logger.LogWarning(exception, "Failed to read active server statistics from distributed cache.");
            }
        }

        return Ok(new ActiveStatsDTO(onlinePlayers, matchesInProgress));
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
