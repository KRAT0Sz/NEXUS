namespace MERRICK.DatabaseContext.Entities.Economy;

/// <summary>
///     Represents a reward bag/chest milestone in The War Effort 28-day cycle.
/// </summary>
public class WarEffortBag
{
    [Key]
    public int ID { get; set; }

    public int BagNumber { get; set; }

    public int BagGroupID { get; set; } = 1;

    public required string Name { get; set; }

    public int RequiredReputation { get; set; } = 100;

    public string RewardType { get; set; } = "MMPOINTS";

    public int RewardSilver { get; set; } = 0;

    public int RewardGold { get; set; } = 0;

    public int RewardTickets { get; set; } = 0;

    public string? RewardProductCode { get; set; } = null;

    public string? RewardProductName { get; set; } = null;

    public string? RewardProductType { get; set; } = null;

    public string? RewardIconPath { get; set; } = null;

    public bool IsFinalTier { get; set; } = false;

    public bool IsActive { get; set; } = true;
}

