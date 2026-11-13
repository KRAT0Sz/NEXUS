namespace MERRICK.DatabaseContext.Entities.Administration;

[Flags]
public enum SuspensionBy
{
    AccountName = 1,
    IPAddress = 2,
    HardwareID = 4,
    MACAddress = 8,
    SystemInformation = 16,
    UniquenessScore = 32
}

public class Suspension
{
    [Key]
    public int ID { get; set; }

    public Account? OffenderAccount { get; set; } = null;

    public int OffenderAccountID { get; set; }

    public Account? IssuerAccount { get; set; } = null;

    public int? IssuerAccountID { get; set; } = null;

    public int SuspendByMask { get; set; }

    public DateTimeOffset TimestampStart { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset TimestampBaseEnd { get; set; }

    public DateTimeOffset TimestampEnd { get; set; }

    public float Multiplier { get; set; } = 1.0f;

    public required string Reason { get; set; }

    public string InternalDetails { get; set; } = string.Empty;

    public bool Override { get; set; } = false;
}

