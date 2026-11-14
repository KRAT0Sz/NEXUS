namespace MERRICK.DatabaseContext.Entities.Economy;

public enum QuestCategory
{
    Daily = 1,
    Weekly = 2,
    Milestone = 3
}

public enum QuestTargetType
{
    MatchesPlayed = 1,
    MatchWin = 2,
    Kills = 3,
    Assists = 4,
    WardsPlaced = 5,
    CreepScore = 6,
    LoginStreak = 7
}

/// <summary>
///     Represents an in-game or web quest for The War Effort progression system.
/// </summary>
public class Quest
{
    [Key]
    public int ID { get; set; }

    public required string QuestKey { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public QuestCategory Category { get; set; } = QuestCategory.Daily;

    public QuestTargetType TargetType { get; set; } = QuestTargetType.MatchesPlayed;

    /// <summary>
    ///     The K2 / HoN engine quest trigger name (e.g. "achievement_value_games_won", "achievement_value_caldavar_won").
    /// </summary>
    public string TriggerName { get; set; } = "achievement_value_games_won";

    public int TargetCount { get; set; } = 1;

    /// <summary>
    ///     Reputation points awarded toward the active War Effort reward bag upon completing this quest.
    /// </summary>
    public int ReputationReward { get; set; } = 25;

    /// <summary>
    ///     The type identifier mapped for the game client quest types dictionary.
    /// </summary>
    public int TypeID { get; set; } = 1;

    /// <summary>
    ///     The quest type key string expected by the game client (e.g. "games_won", "win_games").
    /// </summary>
    public string TypeKey { get; set; } = "games_won";

    public int RewardSilver { get; set; } = 0;

    public int RewardGold { get; set; } = 0;

    public int RewardTickets { get; set; } = 0;

    public string? RewardUpgradeCode { get; set; } = null;

    /// <summary>
    ///     Optional specific hero identifier product (e.g. "Hero_Hammerstorm") if this is a hero-specific quest.
    /// </summary>
    public string? HeroProduct { get; set; } = null;

    /// <summary>
    ///     Comma-separated hero roles (e.g. "carry,ganker", "support") if this is a role-specific quest.
    /// </summary>
    public string? HeroRoleString { get; set; } = null;

    /// <summary>
    ///     Attribute code: 0 = Strength, 1 = Agility, 2 = Intelligence.
    /// </summary>
    public int? AttributeCode { get; set; } = null;

    /// <summary>
    ///     Attack type: "melee" or "ranged".
    /// </summary>
    public string? AttackType { get; set; } = null;

    /// <summary>
    ///     Team ID: 1 = Legion, 2 = Hellbourne.
    /// </summary>
    public int? TeamID { get; set; } = null;

    /// <summary>
    ///     Client-side texture or icon path (e.g. "/ui/fe2/quests/quest_icons/games_won.tga").
    /// </summary>
    public string? ProductIconPath { get; set; } = null;

    public bool IsActive { get; set; } = true;
}
