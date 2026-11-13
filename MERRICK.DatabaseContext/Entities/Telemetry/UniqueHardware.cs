namespace MERRICK.DatabaseContext.Entities.Telemetry;

public class UniqueHardware
{
    [Key]
    [MaxLength(64)]
    public required byte[] FirstSnapshotHash { get; set; }

    [MaxLength(36)]
    public required string UserID { get; set; }

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public required string SerialisedData { get; set; }
}

