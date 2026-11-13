namespace KONGOR.MasterServer.Controllers.ServerRequester;

[ApiController]
[Route("upload_file.php")]
public sealed class UploadFileController(MerrickContext databaseContext, ILogger<UploadFileController> logger, IWebHostEnvironment environment) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;
    private IWebHostEnvironment Environment { get; } = environment;

    [HttpPost(Name = "Upload Replay Or Crash Dump File")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No File Provided In The Request");
        }

        string fileName = file.FileName.Trim('"');

        if (string.IsNullOrWhiteSpace(fileName) || fileName[0] != 'M')
        {
            return BadRequest(@"Replay Filename Must Start With ""M"" (e.g. ""M123.honreplay"")");
        }

        int dotIndex = fileName.IndexOf('.');

        if (dotIndex <= 1)
        {
            return BadRequest("Invalid Replay Filename Format");
        }

        string matchIDSubstring = fileName[1..dotIndex];

        if (int.TryParse(matchIDSubstring, out int matchID).Equals(false))
        {
            return BadRequest($@"Invalid Match ID ""{matchIDSubstring}"" In Filename");
        }

        MatchStatistics? matchStatistics = await MerrickContext.MatchStatistics
            .SingleOrDefaultAsync(candidate => candidate.MatchID == matchID);

        if (matchStatistics is null)
        {
            Logger.LogWarning(@"Replay Upload Attempted For Unknown Match ID {MatchID}", matchID);
            return BadRequest($@"Match ID {matchID} Was Not Found");
        }

        string replaysDirectory = Path.Combine(Environment.ContentRootPath, "Storage", "Replays");
        Directory.CreateDirectory(replaysDirectory);

        string destinationPath = Path.Combine(replaysDirectory, fileName);

        using (FileStream fileStream = System.IO.File.Create(destinationPath))
        {
            await file.CopyToAsync(fileStream);
        }

        matchStatistics.FileSize = Convert.ToInt32(file.Length);
        matchStatistics.FileName = fileName;

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Successfully Stored Replay File ""{FileName}"" ({FileSize} Bytes) For Match ID {MatchID}",
            fileName, file.Length, matchID);

        // The HoN Game Server Expects A 204 NoContent Response Code Upon Success
        return NoContent();
    }
}

