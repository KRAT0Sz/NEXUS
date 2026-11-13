namespace MERRICK.DatabaseContext.Entities.Economy;

public class StatsReset
{
    [Key]
    public int ID { get; set; }

    public int AccountID { get; set; }

    public required string ResetType { get; set; }

    public DateTimeOffset TimestampReset { get; set; } = DateTimeOffset.UtcNow;

    public required string StatisticsSnapshotJSON { get; set; }
}

