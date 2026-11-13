namespace MERRICK.DatabaseContext.Entities.Telemetry;

[Index(nameof(UserID), IsUnique = false)]
public class HardwareSnapshot
{
    [Key]
    [MaxLength(64)]
    public required byte[] Hash { get; set; }

    public required byte[] SnapshotJSONCompressed { get; set; }

    public DateTimeOffset TimestampFirstObserved { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(64)]
    public string? HardwareID { get; set; } = null;

    [MaxLength(45)]
    public string? IPAddress { get; set; } = null;

    [MaxLength(36)]
    public string? UserID { get; set; } = null;
}

