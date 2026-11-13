namespace KONGOR.MasterServer.Services;

public class GlobalLeaderboardService(IServiceScopeFactory scopeFactory, IDatabase distributedCache, ILogger<GlobalLeaderboardService> logger) : BackgroundService
{
    private const string NormalLeaderboardKey = "LEADERBOARD:NORMAL";
    private const string CasualLeaderboardKey = "LEADERBOARD:CASUAL";

    private IServiceScopeFactory ScopeFactory { get; } = scopeFactory;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger<GlobalLeaderboardService> Logger { get; } = logger;

    public async Task<int?> GetRank(int accountID, AccountStatisticsType type)
    {
        string key = type is AccountStatisticsType.MatchmakingCasual ? CasualLeaderboardKey : NormalLeaderboardKey;

        long? zeroBasedRank = await DistributedCache.SortedSetRankAsync(key, accountID.ToString(), Order.Descending);

        if (zeroBasedRank is null)
            return null;

        return (int) (zeroBasedRank.Value + 1);
    }

    public async Task UpdateRating(int accountID, AccountStatisticsType type, double rating)
    {
        string key = type is AccountStatisticsType.MatchmakingCasual ? CasualLeaderboardKey : NormalLeaderboardKey;

        await DistributedCache.SortedSetAddAsync(key, accountID.ToString(), rating);
    }

    public async Task<IReadOnlyList<(int AccountID, double SkillRating, int Rank)>> GetTopRankings(AccountStatisticsType type, int count)
    {
        string key = type is AccountStatisticsType.MatchmakingCasual ? CasualLeaderboardKey : NormalLeaderboardKey;

        SortedSetEntry[] entries = await DistributedCache.SortedSetRangeByRankWithScoresAsync(key, 0, count - 1, Order.Descending);

        List<(int AccountID, double SkillRating, int Rank)> rankings = [];

        int currentRank = 1;

        foreach (SortedSetEntry entry in entries)
        {
            if (int.TryParse(entry.Element.ToString(), out int accountID))
            {
                rankings.Add((accountID, entry.Score, currentRank++));
            }
        }

        return rankings;
    }

    public async Task SyncLeaderboardsFromDatabase()
    {
        try
        {
            Logger.LogInformation("Synchronising global leaderboards from database to distributed cache...");

            using IServiceScope scope = ScopeFactory.CreateScope();
            MerrickContext context = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            List<AccountStatistics> normalStats = await context.AccountStatistics
                .AsNoTracking()
                .Where(s => s.Type == AccountStatisticsType.Matchmaking)
                .OrderByDescending(s => s.SkillRating)
                .Take(500)
                .ToListAsync();

            SortedSetEntry[] normalEntries = normalStats
                .Select(s => new SortedSetEntry(s.AccountID.ToString(), s.SkillRating))
                .ToArray();

            if (normalEntries.Length > 0)
                await DistributedCache.SortedSetAddAsync(NormalLeaderboardKey, normalEntries);

            List<AccountStatistics> casualStats = await context.AccountStatistics
                .AsNoTracking()
                .Where(s => s.Type == AccountStatisticsType.MatchmakingCasual)
                .OrderByDescending(s => s.SkillRating)
                .Take(500)
                .ToListAsync();

            SortedSetEntry[] casualEntries = casualStats
                .Select(s => new SortedSetEntry(s.AccountID.ToString(), s.SkillRating))
                .ToArray();

            if (casualEntries.Length > 0)
                await DistributedCache.SortedSetAddAsync(CasualLeaderboardKey, casualEntries);

            Logger.LogInformation("Global leaderboards initialised: {NormalCount} ranked players, {CasualCount} casual players.",
                normalEntries.Length, casualEntries.Length);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to synchronise global leaderboards from database.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial sync on startup
        await SyncLeaderboardsFromDatabase();

        using PeriodicTimer timer = new (TimeSpan.FromHours(1));

        while (stoppingToken.IsCancellationRequested is false && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SyncLeaderboardsFromDatabase();
        }
    }
}

