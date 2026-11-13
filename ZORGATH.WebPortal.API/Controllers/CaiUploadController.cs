using System.Buffers.Binary;

namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
[Authorize]
public sealed class CaiUploadController(MerrickContext databaseContext, ILogger<CaiUploadController> logger, IWebHostEnvironment environment) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;
    private IWebHostEnvironment Environment { get; } = environment;

    private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    [HttpPost(Name = "Upload Custom Account Icon")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CaiUpload(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No Image File Provided In The Request");
        }

        if (file.Length > 1024 * 1024) // 1 MB Limit
        {
            return BadRequest("Icon Image File Size Exceeds The Maximum Limit Of 1 MB");
        }

        string? userEmail = User.Claims.FirstOrDefault(claim => claim.Type.Equals(ClaimTypes.Email) || claim.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

        if (string.IsNullOrEmpty(userEmail))
        {
            return NotFound("User Context Not Found");
        }

        User? user = await MerrickContext.Users
            .Include(target => target.Accounts)
            .SingleOrDefaultAsync(candidate => candidate.EmailAddress.Equals(userEmail));

        if (user is null)
        {
            return NotFound($@"User ""{userEmail}"" Was Not Found");
        }

        int customIconsUnlocked = user.OwnedStoreItems.Count(item => item.Contains("ai.custom_icon"));

        if (customIconsUnlocked == 0)
        {
            return BadRequest("No Custom Account Icon Has Been Purchased For This User");
        }

        byte[] fileBytes;
        using (MemoryStream memoryStream = new ())
        {
            await file.CopyToAsync(memoryStream);
            fileBytes = memoryStream.ToArray();
        }

        if (!IsValid128X128Png(fileBytes, out string validationError))
        {
            return BadRequest(validationError);
        }

        string iconsRootPath = Path.Combine(Environment.ContentRootPath, "Storage", "Icons");
        Directory.CreateDirectory(iconsRootPath);

        foreach (Account account in user.Accounts)
        {
            string accountDirectory = Path.Combine
            (
                iconsRootPath,
                (account.ID / 1_000_000).ToString(),
                (account.ID / 1_000).ToString(),
                account.ID.ToString()
            );

            Directory.CreateDirectory(accountDirectory);

            string destinationFilePath = Path.Combine(accountDirectory, "1.cai");
            await System.IO.File.WriteAllBytesAsync(destinationFilePath, fileBytes);
        }

        Logger.LogInformation(@"Successfully Uploaded Custom Account Icon For User ""{UserEmail}""", user.EmailAddress);

        return Ok(new { Message = "Custom Account Icon uploaded successfully" });
    }

    internal static bool IsValid128X128Png(byte[] bytes, out string errorMessage)
    {
        if (bytes.Length < 24)
        {
            errorMessage = "The Uploaded File Is Not A Valid PNG Image";
            return false;
        }

        for (int index = 0; index < PngHeader.Length; index++)
        {
            if (bytes[index] != PngHeader[index])
            {
                errorMessage = "The Uploaded File Must Be In PNG Format";
                return false;
            }
        }

        int width = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4));
        int height = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4));

        if (width != 128 || height != 128)
        {
            errorMessage = $"Image Dimensions Must Be 128x128 Pixels (Received: {width}x{height})";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}

