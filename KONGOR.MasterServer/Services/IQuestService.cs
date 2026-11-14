namespace KONGOR.MasterServer.Services;

using Quest = MERRICK.DatabaseContext.Entities.Economy.Quest;

/// <summary>
///     Provides operations for managing the quest and War Effort progression system, including quest assignment, reward bag progression, and response building.
/// </summary>
public interface IQuestService
{
    /// <summary>
    ///     Ensures the default quest catalogue and reward bag tiers are seeded in the database if they are not yet present.
    /// </summary>
    Task EnsureQuestSeedDataAsync();

    /// <summary>
    ///     Retrieves or initialises the War Effort cycle record for the specified account, refreshing daily resets if a new day has started.
    /// </summary>
    /// <param name="accountID">The identifier of the account.</param>
    /// <returns>The account's active War Effort cycle record.</returns>
    Task<AccountWarEffort> GetOrCreateAccountWarEffortAsync(int accountID);

    /// <summary>
    ///     Ensures that the specified account has active quests assigned to both active quest slots (0 and 1), replacing completed expired quests as necessary.
    /// </summary>
    /// <param name="accountID">The identifier of the account.</param>
    /// <returns>The list of active quest progress records for the account.</returns>
    Task<List<AccountQuestProgress>> EnsureAccountActiveQuestsAsync(int accountID);

    /// <summary>
    ///     Builds the complete quest payload dictionary for the specified account, suitable for inclusion in login responses or client quest requests.
    /// </summary>
    /// <param name="accountID">The identifier of the account.</param>
    /// <returns>A dictionary containing quest types, active player quests, all cycle bags, active bag progression, and completed bags.</returns>
    Task<Dictionary<string, object>> BuildCurrentQuestsDictionaryAsync(int accountID);

    /// <summary>
    ///     Retrieves the top player rankings for the quest progression system ordered by total reputation points.
    /// </summary>
    /// <param name="take">The maximum number of rankings to retrieve.</param>
    /// <returns>A list of leaderboard entries ordered by rank.</returns>
    Task<List<QuestLeaderboardEntry>> GetQuestLeaderboardAsync(int take = 50);
}

/// <summary>
///     Represents an entry in the quest progression leaderboard.
/// </summary>
public sealed record QuestLeaderboardEntry
(
    int Rank,
    int AccountID,
    string AccountName,
    string ClanTag,
    int TotalReputation,
    int CurrentBagNumber,
    int CurrentCycle,
    int CompletedBagsCount
);

