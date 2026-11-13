namespace ZORGATH.WebPortal.API.Contracts;

public sealed record ReportDTO
(
    int ID,
    int? ReporterID,
    int? AccusedID,
    int? ReviewerID,
    int MatchID,
    IncidentType IncidentType,
    string Description,
    int OffenseOccurrenceTime,
    IncidentStatus Status,
    string ResolutionDetails,
    DateTimeOffset TimestampCreated
);

public sealed record UpdateReportStatusDTO
(
    IncidentStatus Status,
    string? ResolutionDetails
);

