namespace KONGOR.MasterServer.Controllers.RAP;

[ApiController]
[Consumes("application/x-www-form-urlencoded")]
public sealed class IncidentReportController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<IncidentReportController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    [HttpPost("rap/report2.php", Name = "Report Incident")]
    public async Task<IActionResult> ReportIncident()
    {
        string cookie = Request.Form["cookie"].ToString();
        string accountIDString = Request.Form["account_id"].ToString();
        string offenderName = Request.Form["offenderName"].ToString();
        string matchIDString = Request.Form["match_id"].ToString();
        string typeString = Request.Form["type"].ToString();
        string body = Request.Form["body"].ToString();
        string timeString = Request.Form["time"].ToString();

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Invalid Cookie ""{Cookie}"" In Incident Report Submission", cookie);
            return NotFound();
        }

        Account? reporterAccount = await MerrickContext.Accounts
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(accountName));

        if (reporterAccount is null)
        {
            return NotFound();
        }

        string reportedAccountName = offenderName.Contains(']') ? offenderName.Split(']').Last() : offenderName;

        Account? reportedAccount = await MerrickContext.Accounts
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(reportedAccountName));

        if (reportedAccount is null)
        {
            return NotFound();
        }

        int matchID = int.TryParse(matchIDString, out int parsedMatchID) ? parsedMatchID : 0;
        int occurrenceTime = int.TryParse(timeString, out int parsedTime) ? parsedTime : 0;
        IncidentType incidentType = Enum.TryParse(typeString, out IncidentType parsedType) ? parsedType : IncidentType.Invalid;

        IncidentReport report = new ()
        {
            Reporter = reporterAccount,
            ReporterID = reporterAccount.ID,
            Accused = reportedAccount,
            AccusedID = reportedAccount.ID,
            MatchID = matchID,
            IncidentType = incidentType,
            Description = body,
            OffenseOccurrenceTime = occurrenceTime,
            Status = IncidentStatus.New,
            TimestampCreated = DateTimeOffset.UtcNow
        };

        await MerrickContext.IncidentReports.AddAsync(report);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Incident Report Filed By ""{Reporter}"" Against ""{Accused}"" In Match {MatchID}",
            reporterAccount.Name, reportedAccount.Name, matchID);

        return Ok(PhpSerialization.Serialize(new Dictionary<string, string>
        {
            ["0"] = "1"
        }));
    }
}

