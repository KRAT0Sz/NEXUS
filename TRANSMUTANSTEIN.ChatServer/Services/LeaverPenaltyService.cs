namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Manages matchmaking queue cooldowns and penalties for players who abandon or leave matches in progress.
/// </summary>
public class LeaverPenaltyService(IDatabase distributedCacheStore, ILogger<LeaverPenaltyService> logger)
{
    private const string MessageSenderName = "Matchmaking";

    /// <summary>
    ///     Assesses an escalating matchmaking cooldown penalty against a user for leaving or abandoning a match.
    /// </summary>
    public async Task<MatchmakingCooldown> AssessPenalty(int userID, int accountID, string accountName, int? matchID, string reason)
    {
        int offenceLevel = await distributedCacheStore.IncrementLeaverOffenceCount(userID);
        TimeSpan duration = DistributedCacheExtensions.CalculateCooldownDuration(offenceLevel);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset expires = now + duration;

        MatchmakingCooldown cooldown = new ()
        {
            UserID = userID,
            AccountID = accountID,
            AccountName = accountName,
            TimestampAssessed = now,
            TimestampExpires = expires,
            OffenceLevel = offenceLevel,
            Reason = reason
        };

        await distributedCacheStore.SetMatchmakingCooldown(cooldown);

        logger.LogWarning(@"Assessed Matchmaking Cooldown Of {DurationMinutes} Minutes Against Account ""{AccountName}"" (User ID: {UserID}, Infraction Level: {OffenceLevel}) For Reason: {Reason}",
            (int) duration.TotalMinutes, accountName, userID, offenceLevel, reason);

        // Notify Player If An Active Chat Session Exists
        if (Context.ClientChatSessions.TryGetValue(accountName, out ClientChatSession? session))
        {
            session.SendMatchmakingBan((uint) duration.TotalSeconds);
            session.SendNotice(MessageSenderName, $@"You have been placed on a {(int) duration.TotalMinutes}-minute matchmaking queue cooldown for leaving or abandoning a match. Infraction level: {offenceLevel}.");
        }

        return cooldown;
    }

    /// <summary>
    ///     Retrieves the active matchmaking cooldown for a user, or null if none exists.
    /// </summary>
    public async Task<MatchmakingCooldown?> GetActiveCooldown(int userID)
        => await distributedCacheStore.GetMatchmakingCooldown(userID);

    /// <summary>
    ///     Decides whether a user is currently barred from matchmaking queue entry due to an active cooldown.
    /// </summary>
    public async Task<bool> IsUnderCooldown(int userID)
    {
        MatchmakingCooldown? cooldown = await distributedCacheStore.GetMatchmakingCooldown(userID);

        return cooldown is not null && cooldown.IsActive;
    }

    /// <summary>
    ///     Removes an active cooldown for a user.
    /// </summary>
    public async Task LiftCooldown(int userID)
        => await distributedCacheStore.RemoveMatchmakingCooldown(userID);
}

