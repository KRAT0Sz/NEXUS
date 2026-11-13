namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Manages the lifecycle, configuration, and real-time state of Streamer Mode for players.
///     Coordinates pseudorandom guest pseudonym generation (GUEST-01 to GUEST-99), rating shielding, and match server concealment across Redis cache.
/// </summary>
public class StreamerModeService(IDatabase distributedCacheStore, ILogger<StreamerModeService> logger)
{
    /// <summary>
    ///     Enables Streamer Mode for the specified account, generating a randomised guest identity if no explicit alias was requested.
    /// </summary>
    public async Task<StreamerModeConfiguration> EnableStreamerMode(int accountID, string accountName, string? requestedAlias = null)
    {
        string alias = string.IsNullOrWhiteSpace(requestedAlias)
            ? StreamerModeConfiguration.GenerateGuestName()
            : requestedAlias.Trim();

        StreamerModeConfiguration configuration = new ()
        {
            Enabled = true,
            MaskedName = alias,
            HideMMR = true,
            HideStats = true,
            HideServerAddress = true,
            BlockSpectate = true,
            BlockGameInfo = true,
            FriendsOnlyWhisper = true,
            TimestampActivated = DateTimeOffset.UtcNow
        };

        await distributedCacheStore.SetStreamerMode(accountName, accountID, configuration);

        logger.LogInformation(@"[KONGOR PROTECT] Account ID {AccountID} ({AccountName}) Activated Streamer Mode With Alias ""{MaskedName}""",
            accountID, accountName, alias);

        return configuration;
    }

    /// <summary>
    ///     Disables Streamer Mode for the specified account, reverting them to their original account name and public statistics.
    /// </summary>
    public async Task DisableStreamerMode(int accountID, string accountName)
    {
        await distributedCacheStore.RemoveStreamerMode(accountName, accountID);

        logger.LogInformation(@"[KONGOR PROTECT] Account ID {AccountID} ({AccountName}) Deactivated Streamer Mode",
            accountID, accountName);
    }

    /// <summary>
    ///     Retrieves the active Streamer Mode configuration for the account, or null if Streamer Mode is not currently active.
    /// </summary>
    public async Task<StreamerModeConfiguration?> GetStreamerMode(string accountName)
    {
        return await distributedCacheStore.GetStreamerMode(accountName);
    }

    /// <summary>
    ///     Retrieves the active Streamer Mode configuration by account ID.
    /// </summary>
    public async Task<StreamerModeConfiguration?> GetStreamerModeByID(int accountID)
    {
        return await distributedCacheStore.GetStreamerModeByID(accountID);
    }

    /// <summary>
    ///     Checks whether the account currently has an active Streamer Mode session.
    /// </summary>
    public async Task<bool> IsStreamerModeActive(string accountName)
    {
        StreamerModeConfiguration? configuration = await distributedCacheStore.GetStreamerMode(accountName);

        return configuration is not null && configuration.Enabled;
    }

    /// <summary>
    ///     Checks whether the account ID currently has an active Streamer Mode session.
    /// </summary>
    public async Task<bool> IsStreamerModeActiveByID(int accountID)
    {
        StreamerModeConfiguration? configuration = await distributedCacheStore.GetStreamerModeByID(accountID);

        return configuration is not null && configuration.Enabled;
    }

    /// <summary>
    ///     Re-rolls a new randomised guest name for an active Streamer Mode session.
    /// </summary>
    public async Task<string> RerollGuestName(int accountID, string accountName)
    {
        StreamerModeConfiguration? existing = await distributedCacheStore.GetStreamerMode(accountName);

        string newAlias = StreamerModeConfiguration.GenerateGuestName();

        StreamerModeConfiguration updated = existing ?? new StreamerModeConfiguration();
        updated.Enabled = true;
        updated.MaskedName = newAlias;
        updated.TimestampActivated = DateTimeOffset.UtcNow;

        await distributedCacheStore.SetStreamerMode(accountName, accountID, updated);

        logger.LogInformation(@"[KONGOR PROTECT] Account ID {AccountID} ({AccountName}) Re-rolled Streamer Alias To ""{NewAlias}""",
            accountID, accountName, newAlias);

        return newAlias;
    }
}

