namespace KONGOR.MasterServer.Configuration.Economy;

/// <summary>
///     Defines the seasonal quest configuration, containing the cycle duration, bag milestones, and default quest catalogue.
/// </summary>
public class QuestConfiguration
{
    /// <summary>
    ///     Gets or sets the duration of each quest cycle in days.
    /// </summary>
    public int CycleDurationDays { get; set; } = 28;

    /// <summary>
    ///     Gets or sets the default number of quest resets granted each day.
    /// </summary>
    public int DailyResetsAvailable { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the list of reward bag milestones for the quest progression map.
    /// </summary>
    public required List<QuestBagConfiguration> Bags { get; set; }

    /// <summary>
    ///     Gets or sets the catalogue of quests available in the quest rotation pool.
    /// </summary>
    public required List<QuestDefinitionConfiguration> Quests { get; set; }
}

/// <summary>
///     Defines a milestone reward bag/chest in the quest progression map.
/// </summary>
public class QuestBagConfiguration
{
    /// <summary>
    ///     Gets or sets the 1-based bag number along the quest map path (1 to 14).
    /// </summary>
    public required int BagNumber { get; set; }

    /// <summary>
    ///     Gets or sets the group identifier for the bag.
    /// </summary>
    public int BagGroupID { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the display name of the reward bag or chest.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the reputation points required to unlock this reward bag.
    /// </summary>
    public required int RequiredReputation { get; set; }

    /// <summary>
    ///     Gets or sets the reward type: "MMPOINTS" (Silver), "POINTS" (Gold), or "PRODUCT" (Avatar / Item).
    /// </summary>
    public string RewardType { get; set; } = "MMPOINTS";

    /// <summary>
    ///     Gets or sets the quantity of Silver Coins granted upon opening.
    /// </summary>
    public int RewardSilver { get; set; } = 0;

    /// <summary>
    ///     Gets or sets the quantity of Gold Coins granted upon opening.
    /// </summary>
    public int RewardGold { get; set; } = 0;

    /// <summary>
    ///     Gets or sets the quantity of Plinko Tickets granted upon opening.
    /// </summary>
    public int RewardTickets { get; set; } = 0;

    /// <summary>
    ///     Gets or sets the product code (e.g. "aa.Hero_Valkyrie.Alt4") granted when RewardType is "PRODUCT".
    /// </summary>
    public string? RewardProductCode { get; set; }

    /// <summary>
    ///     Gets or sets the human-readable product name shown upon opening.
    /// </summary>
    public string? RewardProductName { get; set; }

    /// <summary>
    ///     Gets or sets the product type (e.g. "Alt Avatar").
    /// </summary>
    public string? RewardProductType { get; set; }

    /// <summary>
    ///     Gets or sets the client icon path for the product reward (e.g. "/heroes/valkyrie/alt4/icon.tga").
    /// </summary>
    public string? RewardIconPath { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this is the final milestone tier of the cycle.
    /// </summary>
    public bool IsFinalTier { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether this reward bag is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
///     Defines a quest template in the quest catalogue rotation pool.
/// </summary>
public class QuestDefinitionConfiguration
{
    /// <summary>
    ///     Gets or sets the unique configuration key for the quest.
    /// </summary>
    public required string QuestKey { get; set; }

    /// <summary>
    ///     Gets or sets the title of the quest.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    ///     Gets or sets the description explaining the quest objective.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    ///     Gets or sets the quest category name ("Daily", "Weekly", "Achievement").
    /// </summary>
    public string Category { get; set; } = "Daily";

    /// <summary>
    ///     Gets or sets the target completion type ("MatchWin", "MatchesPlayed", "Kills", "Assists", "WardsPlaced").
    /// </summary>
    public string TargetType { get; set; } = "MatchWin";

    /// <summary>
    ///     Gets or sets the trigger name monitored by the game client engine (e.g. "achievement_value_games_won").
    /// </summary>
    public required string TriggerName { get; set; }

    /// <summary>
    ///     Gets or sets the target count required to complete the quest.
    /// </summary>
    public int TargetCount { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the reputation points awarded toward the quest progression map.
    /// </summary>
    public int ReputationReward { get; set; } = 25;

    /// <summary>
    ///     Gets or sets the integer type identifier used by client quest categories.
    /// </summary>
    public int TypeID { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the type key string used by the client UI.
    /// </summary>
    public required string TypeKey { get; set; }

    /// <summary>
    ///     Gets or sets the Silver Coins awarded upon quest completion.
    /// </summary>
    public int RewardSilver { get; set; } = 50;

    /// <summary>
    ///     Gets or sets the Gold Coins awarded upon quest completion.
    /// </summary>
    public int RewardGold { get; set; } = 0;

    /// <summary>
    ///     Gets or sets the Plinko Tickets awarded upon quest completion.
    /// </summary>
    public int RewardTickets { get; set; } = 1;

    /// <summary>
    ///     Gets or sets an optional store upgrade code granted upon quest completion.
    /// </summary>
    public string? RewardUpgradeCode { get; set; }

    /// <summary>
    ///     Gets or sets an optional hero product identifier restriction.
    /// </summary>
    public string? HeroProduct { get; set; }

    /// <summary>
    ///     Gets or sets an optional hero role restriction (e.g. "support", "carry,ganker").
    /// </summary>
    public string? HeroRoleString { get; set; }

    /// <summary>
    ///     Gets or sets an optional hero attribute code (0 = Strength, 1 = Agility, 2 = Intelligence).
    /// </summary>
    public int? AttributeCode { get; set; }

    /// <summary>
    ///     Gets or sets an optional attack type restriction ("melee", "ranged").
    /// </summary>
    public string? AttackType { get; set; }

    /// <summary>
    ///     Gets or sets an optional team restriction (1 = Legion, 2 = Hellbourne).
    /// </summary>
    public int? TeamID { get; set; }

    /// <summary>
    ///     Gets or sets the icon texture path displayed on the quest card.
    /// </summary>
    public string? ProductIconPath { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this quest is active in the rotation pool.
    /// </summary>
    public bool IsActive { get; set; } = true;
}

