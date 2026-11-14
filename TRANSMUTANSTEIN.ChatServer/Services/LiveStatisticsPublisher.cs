namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Periodically publishes live server statistics (connected client count and active matches in progress)
///     to the distributed cache so consumer services such as the web portal API can read real-time metrics.
/// </summary>
public sealed class LiveStatisticsPublisher(IDatabase distributedCacheStore, ILogger<LiveStatisticsPublisher> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new (Interval);

        while (stoppingToken.IsCancellationRequested is false)
        {
            try
            {
                int onlinePlayers = Context.ClientChatSessions.Count;
                int matchesInProgress = Context.MatchServerChatSessions.Values.Count(session =>
                    session.Metadata.Status is ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE
                    || (session.Metadata.MatchID > 0 && session.Metadata.Status is not ChatProtocol.ServerStatus.SERVER_STATUS_IDLE));

                await distributedCacheStore.StringSetAsync(CacheConstants.OnlinePlayersCountKey, onlinePlayers, CacheExpiry);
                await distributedCacheStore.StringSetAsync(CacheConstants.MatchesInProgressCountKey, matchesInProgress, CacheExpiry);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Failed to publish live server statistics to distributed cache.");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        try
        {
            await distributedCacheStore.StringSetAsync(CacheConstants.OnlinePlayersCountKey, 0);
            await distributedCacheStore.StringSetAsync(CacheConstants.MatchesInProgressCountKey, 0);
        }
        catch
        {
            // Suppress exception during host shutdown
        }
    }
}

