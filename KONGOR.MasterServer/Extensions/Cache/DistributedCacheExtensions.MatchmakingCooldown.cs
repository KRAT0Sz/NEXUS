namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    /// <summary>
    ///     Constructs the cache key for a user's active matchmaking cooldown.
    /// </summary>
    internal static string ConstructMatchmakingCooldownKey(int userID) => $@"TMM-COOLDOWN:[""{userID}""]";

    /// <summary>
    ///     Constructs the cache key for tracking recent leaver infractions within a sliding window.
    /// </summary>
    internal static string ConstructLeaverOffenceCountKey(int userID) => $@"TMM-LEAVER-OFFENCES:[""{userID}""]";

    /// <summary>
    ///     Stores an active matchmaking cooldown in the distributed cache.
    /// </summary>
    public static async Task SetMatchmakingCooldown(this IDatabase distributedCacheStore, MatchmakingCooldown cooldown)
    {
        TimeSpan timeToLive = cooldown.RemainingTime;

        if (timeToLive <= TimeSpan.Zero)
            return;

        string serialised = JsonSerializer.Serialize(cooldown);

        await distributedCacheStore.StringSetAsync(ConstructMatchmakingCooldownKey(cooldown.UserID), serialised, timeToLive);
    }

    /// <summary>
    ///     Retrieves the active matchmaking cooldown for a user, or null if none exists or it has expired.
    /// </summary>
    public static async Task<MatchmakingCooldown?> GetMatchmakingCooldown(this IDatabase distributedCacheStore, int userID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructMatchmakingCooldownKey(userID));

        if (cachedValue.IsNullOrEmpty)
            return null;

        try
        {
            MatchmakingCooldown? cooldown = JsonSerializer.Deserialize<MatchmakingCooldown>(cachedValue.ToString());

            if (cooldown is not null && cooldown.IsActive)
                return cooldown;

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Removes an active matchmaking cooldown for a user (e.g. administrator forgiveness or manual lift).
    /// </summary>
    public static async Task RemoveMatchmakingCooldown(this IDatabase distributedCacheStore, int userID)
        => await distributedCacheStore.KeyDeleteAsync(ConstructMatchmakingCooldownKey(userID));

    /// <summary>
    ///     Increments and returns the recorded leaver infractions count for a user within a 24-hour sliding window.
    /// </summary>
    public static async Task<int> IncrementLeaverOffenceCount(this IDatabase distributedCacheStore, int userID)
    {
        string key = ConstructLeaverOffenceCountKey(userID);

        long count = await distributedCacheStore.StringIncrementAsync(key);

        if (count is 1)
            await distributedCacheStore.KeyExpireAsync(key, TimeSpan.FromHours(24));

        return (int) count;
    }

    /// <summary>
    ///     Calculates the escalating cooldown duration according to the user's infraction level.
    /// </summary>
    public static TimeSpan CalculateCooldownDuration(int offenceLevel)
    {
        return offenceLevel switch
        {
            <= 1 => TimeSpan.FromMinutes(15),
            2    => TimeSpan.FromMinutes(30),
            3    => TimeSpan.FromMinutes(60),
            _    => TimeSpan.FromMinutes(120)
        };
    }
}

