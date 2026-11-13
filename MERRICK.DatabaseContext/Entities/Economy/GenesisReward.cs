namespace MERRICK.DatabaseContext.Entities.Economy;

[Flags]
public enum GenesisRewards
{
    GenesisExclusiveDevourerUnlocked = 1,
    GenesisExclusivePuppetMasterUnlocked = 2
}

public class GenesisReward
{
    [Key]
    public required string Code { get; set; }

    public required string ClaimedUserID { get; set; }

    public int ClaimedRewardsMask { get; set; }
}

