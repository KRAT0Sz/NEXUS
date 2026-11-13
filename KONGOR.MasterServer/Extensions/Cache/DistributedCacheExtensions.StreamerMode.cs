namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    private static string ConstructStreamerModeKey(string accountName) => $@"STREAMER-MODE:[""{accountName}""]";

    private static string ConstructStreamerModeIDKey(int accountID) => $@"STREAMER-MODE-ID:[{accountID}]";

    public static async Task SetStreamerMode(this IDatabase distributedCacheStore, string accountName, int accountID, StreamerModeConfiguration configuration)
    {
        string serializedData = JsonSerializer.Serialize(configuration);

        await distributedCacheStore.StringSetAsync(ConstructStreamerModeKey(accountName), serializedData, TimeSpan.FromDays(7));
        await distributedCacheStore.StringSetAsync(ConstructStreamerModeIDKey(accountID), serializedData, TimeSpan.FromDays(7));
    }

    public static async Task<StreamerModeConfiguration?> GetStreamerMode(this IDatabase distributedCacheStore, string accountName)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructStreamerModeKey(accountName));

        return cachedValue.IsNullOrEmpty ? null : JsonSerializer.Deserialize<StreamerModeConfiguration>(cachedValue.ToString());
    }

    public static async Task<StreamerModeConfiguration?> GetStreamerModeByID(this IDatabase distributedCacheStore, int accountID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructStreamerModeIDKey(accountID));

        return cachedValue.IsNullOrEmpty ? null : JsonSerializer.Deserialize<StreamerModeConfiguration>(cachedValue.ToString());
    }

    public static async Task RemoveStreamerMode(this IDatabase distributedCacheStore, string accountName, int accountID)
    {
        await distributedCacheStore.KeyDeleteAsync(ConstructStreamerModeKey(accountName));
        await distributedCacheStore.KeyDeleteAsync(ConstructStreamerModeIDKey(accountID));
    }
}

