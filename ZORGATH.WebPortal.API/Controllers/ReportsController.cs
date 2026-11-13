namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
[Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
public sealed class ReportsController(MerrickContext databaseContext, ILogger<ReportsController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;

    [HttpGet("Unprocessed", Name = "Get Unprocessed Reports")]
    [ProducesResponseType(typeof(List<ReportDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnprocessedReports()
    {
        List<IncidentReport> reports = await MerrickContext.IncidentReports
            .Where(report => report.Status != IncidentStatus.Done && report.Status != IncidentStatus.Rejected)
            .OrderBy(report => report.TimestampCreated)
            .ToListAsync();

        List<ReportDTO> response = reports.Select(report => new ReportDTO
        (
            report.ID,
            report.ReporterID,
            report.AccusedID,
            report.ReviewerID,
            report.MatchID,
            report.IncidentType,
            report.Description ?? string.Empty,
            report.OffenseOccurrenceTime,
            report.Status,
            report.ResolutionDetails ?? string.Empty,
            report.TimestampCreated
        )).ToList();

        return Ok(response);
    }

    [HttpGet("UnderReview", Name = "Get Reports Under Review")]
    [ProducesResponseType(typeof(List<ReportDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportsUnderReview()
    {
        List<IncidentReport> reports = await MerrickContext.IncidentReports
            .Where(report => report.Status == IncidentStatus.UnderReview)
            .OrderBy(report => report.TimestampCreated)
            .ToListAsync();

        List<ReportDTO> response = reports.Select(report => new ReportDTO
        (
            report.ID,
            report.ReporterID,
            report.AccusedID,
            report.ReviewerID,
            report.MatchID,
            report.IncidentType,
            report.Description ?? string.Empty,
            report.OffenseOccurrenceTime,
            report.Status,
            report.ResolutionDetails ?? string.Empty,
            report.TimestampCreated
        )).ToList();

        return Ok(response);
    }

    [HttpGet("Reporter/{id:int}", Name = "Get Reports By Reporter ID")]
    [ProducesResponseType(typeof(List<ReportDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportsByReporterID(int id)
    {
        List<IncidentReport> reports = await MerrickContext.IncidentReports
            .Where(report => report.ReporterID == id)
            .OrderBy(report => report.TimestampCreated)
            .ToListAsync();

        List<ReportDTO> response = reports.Select(report => new ReportDTO
        (
            report.ID,
            report.ReporterID,
            report.AccusedID,
            report.ReviewerID,
            report.MatchID,
            report.IncidentType,
            report.Description ?? string.Empty,
            report.OffenseOccurrenceTime,
            report.Status,
            report.ResolutionDetails ?? string.Empty,
            report.TimestampCreated
        )).ToList();

        return Ok(response);
    }

    [HttpGet("Accused/{id:int}", Name = "Get Reports By Accused ID")]
    [ProducesResponseType(typeof(List<ReportDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportsByAccusedID(int id)
    {
        List<IncidentReport> reports = await MerrickContext.IncidentReports
            .Where(report => report.AccusedID == id)
            .OrderBy(report => report.TimestampCreated)
            .ToListAsync();

        List<ReportDTO> response = reports.Select(report => new ReportDTO
        (
            report.ID,
            report.ReporterID,
            report.AccusedID,
            report.ReviewerID,
            report.MatchID,
            report.IncidentType,
            report.Description ?? string.Empty,
            report.OffenseOccurrenceTime,
            report.Status,
            report.ResolutionDetails ?? string.Empty,
            report.TimestampCreated
        )).ToList();

        return Ok(response);
    }

    [HttpPut("{id:int}/Status", Name = "Update Report Status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReportStatus(int id, [FromBody] UpdateReportStatusDTO payload)
    {
        IncidentReport? report = await MerrickContext.IncidentReports
            .SingleOrDefaultAsync(candidate => candidate.ID == id);

        if (report is null)
        {
            return NotFound();
        }

        report.Status = payload.Status;
        report.ResolutionDetails = payload.ResolutionDetails;

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Report {ReportID} Status Updated To {Status}", id, payload.Status);

        return NoContent();
    }
}

