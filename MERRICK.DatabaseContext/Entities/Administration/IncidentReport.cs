namespace MERRICK.DatabaseContext.Entities.Administration;

/// <summary>
///     IncidentType values map 1-to-1 with the names and indices of the report-a-player options in the in-game UI.
/// </summary>
public enum IncidentType
{
    Invalid = 0,
    ChatAbuse = 1,
    FeedingOrStatManipulation = 2,
    ImpersonatingStaff = 3,
    AbilityOrItemAbuse = 4,
    BadNicknameOrClanName = 5,
    RefusalToParticipateOrAfk = 6
}

/// <summary>
///     IncidentStatus tracks the state machine flow for a report.
/// </summary>
public enum IncidentStatus
{
    New,
    UnderReview,
    Rejected,
    Done
}

public class IncidentReport
{
    [Key]
    public int ID { get; set; }

    public Account? Reporter { get; set; } = null;

    public int? ReporterID { get; set; } = null;

    public Account? Accused { get; set; } = null;

    public int? AccusedID { get; set; } = null;

    public Account? Reviewer { get; set; } = null;

    public int? ReviewerID { get; set; } = null;

    public int MatchID { get; set; }

    public IncidentType IncidentType { get; set; } = IncidentType.Invalid;

    public string? Description { get; set; } = null;

    public int OffenseOccurrenceTime { get; set; } = 0;

    public IncidentStatus Status { get; set; } = IncidentStatus.New;

    public string? ResolutionDetails { get; set; } = null;

    public DateTimeOffset TimestampCreated { get; set; } = DateTimeOffset.UtcNow;
}

