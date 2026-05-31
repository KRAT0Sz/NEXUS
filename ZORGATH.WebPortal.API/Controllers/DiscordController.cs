namespace ZORGATH.WebPortal.API.Controllers;

using System.Collections.Concurrent;
using System.Net.Http.Headers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class DiscordController : ControllerBase
{
    private const string DiscordBaseURL = "https://discord.com/api/v10";

    private MerrickContext MerrickContext { get; }

    private OperationalConfiguration Configuration { get; }

    private IHttpClientFactory HTTPClientFactory { get; }

    private ILogger<DiscordController> Logger { get; }

    private IWebHostEnvironment HostEnvironment { get; }

    private static readonly ConcurrentDictionary<string, int> PendingStates = new();

    public DiscordController(MerrickContext databaseContext, IOptions<OperationalConfiguration> configuration, IHttpClientFactory httpClientFactory, ILogger<DiscordController> logger, IWebHostEnvironment hostEnvironment)
    {
        MerrickContext = databaseContext;
        Configuration = configuration.Value;
        HTTPClientFactory = httpClientFactory;
        Logger = logger;
        HostEnvironment = hostEnvironment;
    }

    [HttpGet("Authentication", Name = "Initiate Discord Authentication")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status302Found)]
    public IActionResult InitiateDiscordAuthentication()
    {
        string state = $"register:{Guid.CreateVersion7()}";

        PendingStates[state] = 0;

        Logger.LogDebug("OAuth state created: [{State}], PendingStates count: {Count}", state, PendingStates.Count);

        string scopes = Uri.EscapeDataString("identify email");

        string authorizationURL = $"{DiscordBaseURL}/oauth2/authorize?client_id={Configuration.Discord.ClientID}&redirect_uri={Uri.EscapeDataString(Configuration.Discord.RedirectURI)}&response_type=code&scope={scopes}&state={Uri.EscapeDataString(state)}";

        Logger.LogDebug("Initiating Discord OAuth. Redirect URI: {RedirectURI}, Auth URL: {AuthURL}", Configuration.Discord.RedirectURI, authorizationURL);

        return Redirect(authorizationURL);
    }

    [HttpGet("Link", Name = "Initiate Discord OAuth Link")]
    [Authorize(Policy = UserRoles.AllRoles)]
    [ProducesResponseType(typeof(string), StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public IActionResult InitiateDiscordOAuthLink(bool update = false)
    {
        int userID = int.Parse(User.Claims.Single(claim => claim.Type.Equals(Claims.UserID)).Value);

        string state = update
            ? $"update:{userID}:{Guid.CreateVersion7()}"
            : $"link:{userID}:{Guid.CreateVersion7()}";

        PendingStates[state] = userID;

        string scopes = Uri.EscapeDataString("identify email");

        string authorizationURL = $"{DiscordBaseURL}/oauth2/authorize?client_id={Configuration.Discord.ClientID}&redirect_uri={Uri.EscapeDataString(Configuration.Discord.RedirectURI)}&response_type=code&scope={scopes}&state={Uri.EscapeDataString(state)}";

        return Redirect(authorizationURL);
    }

    [HttpGet("PasswordReset", Name = "Initiate Discord Password Reset")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status302Found)]
    public IActionResult InitiateDiscordPasswordReset()
    {
        string state = $"passwordreset:{Guid.CreateVersion7()}";

        PendingStates[state] = 0;

        string scopes = Uri.EscapeDataString("identify email");

        string authorizationURL = $"{DiscordBaseURL}/oauth2/authorize?client_id={Configuration.Discord.ClientID}&redirect_uri={Uri.EscapeDataString(Configuration.Discord.RedirectURI)}&response_type=code&scope={scopes}&state={Uri.EscapeDataString(state)}";

        Logger.LogDebug("Initiating Discord OAuth for password reset. Redirect URI: {RedirectURI}", Configuration.Discord.RedirectURI);

        return Redirect(authorizationURL);
    }

    [HttpPost("Register", Name = "Register User via Discord")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetBasicUserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterUserViaDiscord([FromBody] RegisterUserViaDiscordDTO payload)
    {
        Token? token = await MerrickContext.Tokens
            .SingleOrDefaultAsync(t => t.Value.ToString().Equals(payload.Token) && t.Purpose.Equals(TokenPurpose.DiscordRegistration));

        if (token is null)
        {
            return NotFound($@"Discord Registration Token ""{payload.Token}"" Was Not Found");
        }

        if (token.TimestampConsumed is not null)
        {
            return Conflict($@"This registration link has already been used. If you have already created your account, please log in.");
        }

        if (await MerrickContext.Users.AnyAsync(user => user.DiscordId != null && user.DiscordId.Equals(token.Data)))
        {
            return Conflict($@"A user with this Discord account already exists. Please log in with your account.");
        }

        ValidationResult accountNameValidationResult = await new AccountNameValidator(HostEnvironment.IsDevelopment()).ValidateAsync(payload.Name);

        if (accountNameValidationResult.IsValid is false)
            return BadRequest(accountNameValidationResult.Errors.Select(error => error.ErrorMessage));

        if (payload.Password.Equals(payload.ConfirmPassword).Equals(false))
            return BadRequest($@"Password ""{payload.ConfirmPassword}"" Does Not Match ""{payload.Password}"" (These Values Are Only Visible To You)");

        if (HostEnvironment.IsDevelopment() is false)
        {
            ValidationResult passwordValidationResult = await new PasswordValidator().ValidateAsync(payload.Password);

            if (passwordValidationResult.IsValid is false)
                return BadRequest(passwordValidationResult.Errors.Select(error => error.ErrorMessage));
        }

        if (await MerrickContext.Accounts.AnyAsync(account => account.Name.Equals(payload.Name)))
        {
            return Conflict($@"Account With Name ""{payload.Name}"" Already Exists");
        }

        Role? role = await MerrickContext.Roles.SingleOrDefaultAsync(role => role.Name.Equals(UserRoles.User));

        if (role is null)
        {
            return NotFound($@"User Role ""{UserRoles.User}"" Was Not Found");
        }

        string sanitizedDiscordID = token.Data;
        string srpSalt = SRPRegistrationHandlers.GenerateSRPPasswordSalt();

        User user = new ()
        {
            EmailAddress = sanitizedDiscordID,
            Role = role,
            SRPPasswordSalt = srpSalt,
            SRPPasswordHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(payload.Password, srpSalt),
            DiscordId = sanitizedDiscordID,
            DiscordLinkedAt = DateTimeOffset.UtcNow,
            GoldCoins = 0,
            SilverCoins = 0,
            PlinkoTickets = 0
        };

        user.PBKDF2PasswordHash = new PasswordHasher<User>().HashPassword(user, payload.Password);

        await MerrickContext.Users.AddAsync(user);

        Account account = new ()
        {
            Name = payload.Name,
            User = user,
            IsMain = true
        };

        user.Accounts.Add(account);

        token.TimestampConsumed = DateTimeOffset.UtcNow;

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation("Created new user {UserID} via Discord registration for Discord user {DiscordUserID}", user.ID, sanitizedDiscordID);

        return Created($"/User/{user.ID}",
            new GetBasicUserDTO(user.ID, user.EmailAddress, [new GetBasicAccountDTO(account.ID, account.Name)]));
    }

    [HttpGet("Callback", Name = "Discord OAuth Callback")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> HandleDiscordOAuthCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string? error)
    {
        string frontendURL = GetFrontendURL();

        if (string.IsNullOrWhiteSpace(error).Equals(false))
        {
            return BadRequest($@"Discord OAuth Error: ""{error}""");
        }

        if (string.IsNullOrWhiteSpace(state) || PendingStates.TryRemove(state, out int authenticatedUserID).Equals(false))
        {
            return BadRequest("Invalid or expired OAuth state. Please try again.");
        }

        string redirectURI = Configuration.Discord.RedirectURI;

        HttpClient tokenClient = HTTPClientFactory.CreateClient();
        Dictionary<string, string> tokenRequestParams = new ()
        {
            { "client_id", Configuration.Discord.ClientID },
            { "client_secret", Configuration.Discord.ClientSecret },
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectURI }
        };

        HttpResponseMessage tokenResponse = await tokenClient.PostAsync($"{DiscordBaseURL}/oauth2/token", new FormUrlEncodedContent(tokenRequestParams));

        if (tokenResponse.IsSuccessStatusCode.Equals(false))
        {
            Logger.LogError("Discord token exchange failed with status {StatusCode}", tokenResponse.StatusCode);

            return Unauthorized("Failed to exchange authorization code for access token.");
        }

        JsonDocument tokenDocument = await JsonDocument.ParseAsync(await tokenResponse.Content.ReadAsStreamAsync());
        JsonElement tokenData = tokenDocument.RootElement;

        string accessToken = tokenData.GetProperty("access_token").GetString()!;

        HttpClient userInfoClient = HTTPClientFactory.CreateClient();
        userInfoClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage userInfoResponse = await userInfoClient.GetAsync($"{DiscordBaseURL}/users/@me");

        if (userInfoResponse.IsSuccessStatusCode.Equals(false))
        {
            Logger.LogError("Discord user info fetch failed with status {StatusCode}", userInfoResponse.StatusCode);

            return Unauthorized("Failed to retrieve Discord user information.");
        }

        JsonDocument userDocument = await JsonDocument.ParseAsync(await userInfoResponse.Content.ReadAsStreamAsync());
        JsonElement userData = userDocument.RootElement;

        string discordUserID = userData.GetProperty("id").GetString()!;
        string discordEmail = userData.GetProperty("email").GetString()!;
        string discordUsername = userData.GetProperty("username").GetString()!;
        string? discordAvatar = userData.TryGetProperty("avatar", out JsonElement avatarElement) ? avatarElement.GetString() : null;

        Logger.LogDebug("Discord callback: using frontend URL {FrontendURL}", frontendURL);

        string statePrefix = state.Split(':')[0];

        if (statePrefix.Equals("register"))
        {
            return await HandleDiscordRegistration(discordUserID, discordUsername, discordAvatar, frontendURL);
        }

        if (statePrefix.Equals("update"))
        {
            return await HandleDiscordAccountUpdate(authenticatedUserID, discordUserID, discordUsername, discordAvatar, frontendURL);
        }

        return await HandleDiscordLinking(authenticatedUserID, discordUserID, discordEmail, discordUsername, discordAvatar, frontendURL);
    }

    [HttpGet("CallbackData", Name = "Discord OAuth Callback Data")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDiscordCallbackData([FromQuery] string code, [FromQuery] string state)
    {
        Logger.LogDebug("GetDiscordCallbackData called. State: [{State}], Code: [{Code}], PendingStates count: {Count}", state, code, PendingStates.Count);
        Logger.LogDebug("PendingStates keys: [{Keys}]", string.Join(", ", PendingStates.Keys));

        if (string.IsNullOrWhiteSpace(state) || PendingStates.TryRemove(state, out int authenticatedUserID).Equals(false))
        {
            Logger.LogWarning("OAuth state validation failed. State: [{State}], PendingStates count: {Count}", state, PendingStates.Count);
            return BadRequest(new { error = "Invalid or expired OAuth state. Please try again." });
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { error = "Authorization code is required." });
        }

        string redirectURI = Configuration.Discord.RedirectURI;

        HttpClient tokenClient = HTTPClientFactory.CreateClient();
        Dictionary<string, string> tokenRequestParams = new ()
        {
            { "client_id", Configuration.Discord.ClientID },
            { "client_secret", Configuration.Discord.ClientSecret },
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectURI }
        };

        HttpResponseMessage tokenResponse = await tokenClient.PostAsync($"{DiscordBaseURL}/oauth2/token", new FormUrlEncodedContent(tokenRequestParams));

        if (tokenResponse.IsSuccessStatusCode.Equals(false))
        {
            Logger.LogError("Discord token exchange failed with status {StatusCode}", tokenResponse.StatusCode);

            return Unauthorized(new { error = "Failed to exchange authorization code for access token." });
        }

        JsonDocument tokenDocument = await JsonDocument.ParseAsync(await tokenResponse.Content.ReadAsStreamAsync());
        JsonElement tokenData = tokenDocument.RootElement;

        string accessToken = tokenData.GetProperty("access_token").GetString()!;

        HttpClient userInfoClient = HTTPClientFactory.CreateClient();
        userInfoClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage userInfoResponse = await userInfoClient.GetAsync($"{DiscordBaseURL}/users/@me");

        if (userInfoResponse.IsSuccessStatusCode.Equals(false))
        {
            Logger.LogError("Discord user info fetch failed with status {StatusCode}", userInfoResponse.StatusCode);

            return Unauthorized(new { error = "Failed to retrieve Discord user information." });
        }

        JsonDocument userDocument = await JsonDocument.ParseAsync(await userInfoResponse.Content.ReadAsStreamAsync());
        JsonElement userData = userDocument.RootElement;

        string discordUserID = userData.GetProperty("id").GetString()!;
        string discordEmail = userData.GetProperty("email").GetString()!;
        string discordUsername = userData.GetProperty("username").GetString()!;
        string? discordAvatar = userData.TryGetProperty("avatar", out JsonElement avatarElement) ? avatarElement.GetString() : null;

        string frontendURL = GetFrontendURL();

        string statePrefix = state.Split(':')[0];

        if (statePrefix.Equals("register"))
        {
            return await GetDiscordRegistrationCallbackData(discordUserID, discordUsername, discordAvatar, frontendURL);
        }

        if (statePrefix.Equals("passwordreset"))
        {
            return await GetDiscordPasswordResetCallbackData(discordUserID, discordEmail, discordUsername, discordAvatar, frontendURL);
        }

        return await GetDiscordLinkingCallbackData(authenticatedUserID, discordUserID, discordEmail, discordUsername, discordAvatar, frontendURL);
    }

    private string GetFrontendURL()
    {
        return Configuration.Discord.FrontendURL;
    }

    private async Task<IActionResult> GetDiscordRegistrationCallbackData(string discordUserID, string discordUsername, string? discordAvatar, string frontendURL)
    {
        User? existingUserByDiscordID = await MerrickContext.Users
            .Include(record => record.Role)
            .Include(record => record.Accounts)
            .SingleOrDefaultAsync(record => record.DiscordId != null && record.DiscordId.Equals(discordUserID));

        if (existingUserByDiscordID is not null)
        {
            Logger.LogInformation("User with Discord ID {DiscordUserID} already exists, returning login token", discordUserID);

            return await GetDiscordLoginCallbackData(existingUserByDiscordID, discordUsername, discordAvatar);
        }

        string registrationToken = Guid.CreateVersion7().ToString();

        Token token = new ()
        {
            Purpose = TokenPurpose.DiscordRegistration,
            EmailAddress = discordUserID,
            Value = Guid.Parse(registrationToken),
            Data = discordUserID,
            Validity = TimeSpan.FromMinutes(15)
        };

        await MerrickContext.Tokens.AddAsync(token);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation("Created Discord registration token for Discord user {DiscordUserID}", discordUserID);

        string registrationURL = $"{frontendURL}/account/register/{registrationToken}?discord=true&username={Uri.EscapeDataString(discordUsername)}";
        string dmContent = $@"You need to verify your Discord before you can create your Heroes Of Newerth account.
Please follow the link below to continue:

{registrationURL}

Regards,
The Project KONGOR ReBIRTH Team";

        bool dmSent = await SendDiscordDM(discordUserID, dmContent);

        if (dmSent is false)
        {
            Logger.LogWarning("Failed to send registration DM to Discord user {DiscordUserID}. Token saved but user was not notified.", discordUserID);
        }

        return Ok(new
        {
            type = "register",
            registrationToken,
            discordUsername,
            discordAvatar
        });
    }

    private async Task<IActionResult> GetDiscordLinkingCallbackData(int authenticatedUserID, string discordUserID, string discordEmail, string discordUsername, string? discordAvatar, string frontendURL)
    {
        User? user = await MerrickContext.Users
            .Include(record => record.Role)
            .SingleOrDefaultAsync(record => record.ID.Equals(authenticatedUserID));

        if (user is null)
        {
            return NotFound(new { error = $@"User With ID ""{authenticatedUserID}"" Was Not Found" });
        }

        if (user.EmailAddress.Equals(discordEmail, StringComparison.OrdinalIgnoreCase).Equals(false))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = $@"Your Discord email address ""{discordEmail}"" does not match the email address on your account ""{user.EmailAddress}""." });
        }

        bool emailVerified = await MerrickContext.Tokens
            .AnyAsync(token => token.Data.Equals(user.EmailAddress, StringComparison.OrdinalIgnoreCase)
                && token.Purpose.Equals(TokenPurpose.EmailAddressVerification)
                && token.TimestampConsumed != null);

        if (emailVerified.Equals(false))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "You must verify your email address before linking a Discord account." });
        }

        user.DiscordId = discordUserID;
        user.DiscordUsername = discordUsername;
        user.DiscordAvatarHash = discordAvatar;
        user.DiscordLinkedAt = DateTimeOffset.UtcNow;

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation("User {UserID} linked Discord account {DiscordUserID} ({DiscordUsername})", user.ID, discordUserID, discordUsername);

        return Ok(new
        {
            type = "link",
            discordUsername,
            discordAvatar
        });
    }

    private async Task<IActionResult> HandleDiscordAccountUpdate(int authenticatedUserID, string discordUserID, string discordUsername, string? discordAvatar, string frontendURL)
    {
        User? user = await MerrickContext.Users
            .Include(record => record.Role)
            .SingleOrDefaultAsync(record => record.ID.Equals(authenticatedUserID));

        if (user is null)
        {
            return NotFound($@"User With ID ""{authenticatedUserID}"" Was Not Found");
        }

        user.DiscordId = discordUserID;
        user.DiscordUsername = discordUsername;
        user.DiscordAvatarHash = discordAvatar;
        user.DiscordLinkedAt = DateTimeOffset.UtcNow;

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation("User {UserID} updated Discord account to {DiscordUserID} ({DiscordUsername})", user.ID, discordUserID, discordUsername);

        return Redirect($"{frontendURL}/account?discordUpdated=true&username={Uri.EscapeDataString(discordUsername)}&avatar={Uri.EscapeDataString(discordAvatar ?? string.Empty)}");
    }

    private async Task<IActionResult> GetDiscordLoginCallbackData(User user, string discordUsername, string? discordAvatar)
    {
        IEnumerable<Claim> userRoleClaims = user.Role.Name is UserRoles.Administrator ? UserRoleClaims.Administrator : UserRoleClaims.User;

        IEnumerable<Claim> openIDClaims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Accounts.Single(account => account.IsMain).Name, ClaimValueTypes.String),
            new (JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new (JwtRegisteredClaimNames.AuthTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new (JwtRegisteredClaimNames.Nonce, Guid.CreateVersion7().ToString(), ClaimValueTypes.String),
            new (JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString(), ClaimValueTypes.String),
            new (JwtRegisteredClaimNames.Email, user.EmailAddress, ClaimValueTypes.Email)
        };

        IEnumerable<Claim> customClaims = new List<Claim>
        {
            new (Claims.UserID, user.ID.ToString(), ClaimValueTypes.String),
            new (Claims.AccountID, user.Accounts.Single(account => account.IsMain).ID.ToString(), ClaimValueTypes.String),
            new (Claims.AccountIsMain, true.ToString(), ClaimValueTypes.Boolean),
            new (Claims.ClanName, string.Empty, ClaimValueTypes.String),
            new (Claims.ClanTag, string.Empty, ClaimValueTypes.String)
        };

        IEnumerable<Claim> allTokenClaims = Enumerable.Empty<Claim>().Union(userRoleClaims).Union(openIDClaims).Union(customClaims).OrderBy(claim => claim.Type);

        JwtSecurityToken jwtToken = new
        (
            issuer: Configuration.JWT.Issuer,
            audience: Configuration.JWT.Audience,
            claims: allTokenClaims,
            expires: DateTimeOffset.UtcNow.AddHours(Configuration.JWT.DurationInHours).DateTime,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.JWT.SigningKey)), SecurityAlgorithms.HmacSha256)
        );

        string jwt = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        return Ok(new
        {
            type = "login",
            token = jwt,
            discordUsername,
            discordAvatar
        });
    }

    private async Task<IActionResult> GetDiscordPasswordResetCallbackData(string discordUserID, string discordEmail, string discordUsername, string? discordAvatar, string frontendURL)
    {
        User? user = await MerrickContext.Users
            .Include(record => record.Role)
            .SingleOrDefaultAsync(record => record.DiscordId != null && record.DiscordId.Equals(discordUserID));

        if (user is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "No account is linked to this Discord account. You must first link your Discord account from the account settings page." });
        }

        AccountPasswordTokenData? existingTokenData = null;
        Token? existingToken = null;

        List<Token> potentialTokens = await MerrickContext.Tokens
            .Where(token =>
                token.Purpose.Equals(TokenPurpose.AccountPasswordReset)
                && token.EmailAddress.Equals(discordUserID)
                && token.TimestampConsumed == null)
            .ToListAsync();

        foreach (Token potentialToken in potentialTokens)
        {
            try
            {
                AccountPasswordTokenData? parsed = JsonSerializer.Deserialize<AccountPasswordTokenData>(potentialToken.Data);
                if (parsed?.DiscordUserID == discordUserID)
                {
                    existingTokenData = parsed;
                    existingToken = potentialToken;
                    break;
                }
            }
            catch (JsonException)
            {
                continue;
            }
        }

        if (existingToken is not null)
        {
            Logger.LogInformation("Returning existing password reset token for Discord user {DiscordUserID}", discordUserID);

            string tokenValue = existingToken.Value.ToString();
            string existingResetURL = $"{frontendURL}/password/recover/{tokenValue}";

            string existingDmContent = $@"You requested a password reset for your Project KONGOR account.
Click the link below to activate your new password:

{existingResetURL}

If you did not request this, please ignore this message.

Regards,
The Project KONGOR Team";

            await SendDiscordDM(discordUserID, existingDmContent);

            return Ok(new
            {
                type = "passwordreset",
                discordUsername,
                discordAvatar,
                message = "A password reset link has been sent to your Discord DMs."
            });
        }

        string newGeneratedPassword = AccountPasswordGenerationHelpers.GenerateRandomPassword();

        string salt = SRPRegistrationHandlers.GenerateSRPPasswordSalt();
        string srpHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(newGeneratedPassword, salt);
        string pbkdf2Hash = new PasswordHasher<User>().HashPassword(null!, newGeneratedPassword);

        AccountPasswordTokenData tokenData = new (discordUserID, salt, srpHash, pbkdf2Hash, discordUserID);

        Token token = new ()
        {
            Purpose = TokenPurpose.AccountPasswordReset,
            EmailAddress = discordUserID,
            Value = Guid.CreateVersion7(),
            Data = JsonSerializer.Serialize(tokenData),
            Validity = TimeSpan.FromMinutes(15)
        };

        await MerrickContext.Tokens.AddAsync(token);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation("Created Discord password reset token for Discord user {DiscordUserID}", discordUserID);

        string newResetURL = $"{frontendURL}/password/recover/{token.Value}";

        string newDmContent = $@"You requested a password reset for your Project KONGOR account.
Click the link below to activate your new password:

{newResetURL}

Your temporary password is: {newGeneratedPassword}

If you did not request this, please ignore this message and consider securing your account.

Regards,
The Project KONGOR Team";

        bool dmSent = await SendDiscordDM(discordUserID, newDmContent);

        if (dmSent is false)
        {
            Logger.LogWarning("Failed to send password reset DM to Discord user {DiscordUserID}. Token saved but user was not notified.", discordUserID);
        }

        return Ok(new
        {
            type = "passwordreset",
            discordUsername,
            discordAvatar,
            message = dmSent
                ? "A password reset link has been sent to your Discord DMs."
                : "Failed to send a DM to your Discord account. Please ensure you accept direct messages from server members."
        });
    }

    private async Task<IActionResult> HandleDiscordRegistration(string discordUserID, string discordUsername, string? discordAvatar, string frontendURL)
    {
        User? existingUserByDiscordID = await MerrickContext.Users
            .Include(record => record.Role)
            .SingleOrDefaultAsync(record => record.DiscordId != null && record.DiscordId.Equals(discordUserID));

        if (existingUserByDiscordID is not null)
        {
            Logger.LogInformation("User with Discord ID {DiscordUserID} already exists, logging in", discordUserID);

            return await RedirectToLogin(existingUserByDiscordID, discordUsername, discordAvatar, frontendURL);
        }

        string registrationToken = Guid.CreateVersion7().ToString();

        Token token = new ()
        {
            Purpose = TokenPurpose.DiscordRegistration,
            EmailAddress = discordUserID,
            Value = Guid.Parse(registrationToken),
            Data = discordUserID,
            Validity = TimeSpan.FromMinutes(15)
        };

        await MerrickContext.Tokens.AddAsync(token);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation("Created Discord registration token for Discord user {DiscordUserID}", discordUserID);

        string registrationURL = $"{frontendURL}/account/register/{registrationToken}?discord=true&username={Uri.EscapeDataString(discordUsername)}";

        string dmMessage = $@"You need to verify your Discord before you can create your Heroes Of Newerth account.
Please follow the link below to continue:

{registrationURL}

Regards,
The Project KONGOR ReBIRTH Team";

        bool dmSent = await SendDiscordDM(discordUserID, dmMessage);

        if (dmSent is false)
        {
            Logger.LogWarning("Failed to send registration DM to Discord user {DiscordUserID}. Token saved but user was not notified.", discordUserID);
        }

        string callbackURL = $"{frontendURL}/discord/callback";
        string redirectURL = $"{callbackURL}?discordRegister=true&username={Uri.EscapeDataString(discordUsername)}&avatar={Uri.EscapeDataString(discordAvatar ?? string.Empty)}";
        Logger.LogDebug("Discord registration redirecting to {RedirectURL}", redirectURL);

        return Redirect(redirectURL);
    }

    private async Task<bool> SendDiscordDM(string discordUserID, string message)
    {
        try
        {
            HttpClient dmClient = HTTPClientFactory.CreateClient();
            dmClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", Configuration.Discord.BotToken);

            string createDMEndpoint = $"{DiscordBaseURL}/users/@me/channels";
            string channelPayload = $@"{{""recipient_id"": ""{discordUserID}""}}";

            HttpResponseMessage createDMResponse = await dmClient.PostAsync(createDMEndpoint, new StringContent(channelPayload, Encoding.UTF8, "application/json"));

            if (createDMResponse.IsSuccessStatusCode.Equals(false))
            {
                string errorBody = await createDMResponse.Content.ReadAsStringAsync();
                Logger.LogError("Failed to create DM channel with Discord user {DiscordUserID}. Status: {StatusCode}, Response: {ResponseBody}", discordUserID, createDMResponse.StatusCode, errorBody);
                return false;
            }

            JsonDocument channelDocument = await JsonDocument.ParseAsync(await createDMResponse.Content.ReadAsStreamAsync());
            string channelID = channelDocument.RootElement.GetProperty("id").GetString()!;

            string messageEndpoint = $"{DiscordBaseURL}/channels/{channelID}/messages";
            using StringContent messageContent = new (JsonSerializer.Serialize(new { content = message }), Encoding.UTF8, "application/json");

            HttpResponseMessage sendMessageResponse = await dmClient.PostAsync(messageEndpoint, messageContent);

            if (sendMessageResponse.IsSuccessStatusCode.Equals(false))
            {
                string errorBody = await sendMessageResponse.Content.ReadAsStringAsync();
                Logger.LogError("Failed to send registration DM to channel {ChannelID}. Status: {StatusCode}, Response: {ResponseBody}", channelID, sendMessageResponse.StatusCode, errorBody);
                return false;
            }

            Logger.LogInformation("Sent registration DM to Discord user {DiscordUserID}", discordUserID);
            return true;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Exception while sending DM to Discord user {DiscordUserID}", discordUserID);
            return false;
        }
    }

    private async Task<IActionResult> HandleDiscordLinking(int authenticatedUserID, string discordUserID, string discordEmail, string discordUsername, string? discordAvatar, string callbackURL)
    {
        User? user = await MerrickContext.Users
            .Include(record => record.Role)
            .SingleOrDefaultAsync(record => record.ID.Equals(authenticatedUserID));

        if (user is null)
        {
            return NotFound($@"User With ID ""{authenticatedUserID}"" Was Not Found");
        }

        if (user.EmailAddress.Equals(discordEmail, StringComparison.OrdinalIgnoreCase).Equals(false))
        {
            return StatusCode(StatusCodes.Status403Forbidden, $@"Your Discord email address ""{discordEmail}"" does not match the email address on your account ""{user.EmailAddress}"". Please ensure you are using the correct Discord account.");
        }

        bool emailVerified = await MerrickContext.Tokens
            .AnyAsync(token => token.Data.Equals(user.EmailAddress, StringComparison.OrdinalIgnoreCase)
                && token.Purpose.Equals(TokenPurpose.EmailAddressVerification)
                && token.TimestampConsumed != null);

        if (emailVerified.Equals(false))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "You must verify your email address before linking a Discord account.");
        }

        user.DiscordId = discordUserID;
        user.DiscordUsername = discordUsername;
        user.DiscordAvatarHash = discordAvatar;
        user.DiscordLinkedAt = DateTimeOffset.UtcNow;

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation("User {UserID} linked Discord account {DiscordUserID} ({DiscordUsername})", user.ID, discordUserID, discordUsername);

        return Redirect($"{callbackURL}?discordLinked=true&username={Uri.EscapeDataString(discordUsername)}&avatar={Uri.EscapeDataString(discordAvatar ?? string.Empty)}");
    }

    private async Task<IActionResult> RedirectToLogin(User user, string discordUsername, string? discordAvatar, string callbackURL)
    {
        user.DiscordUsername = discordUsername;
        user.DiscordAvatarHash = discordAvatar;

        IEnumerable<Claim> userRoleClaims = user.Role.Name is UserRoles.Administrator ? UserRoleClaims.Administrator : UserRoleClaims.User;

        IEnumerable<Claim> openIDClaims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Accounts.Single(account => account.IsMain).Name, ClaimValueTypes.String),
            new (JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new (JwtRegisteredClaimNames.AuthTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new (JwtRegisteredClaimNames.Nonce, Guid.CreateVersion7().ToString(), ClaimValueTypes.String),
            new (JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString(), ClaimValueTypes.String),
            new (JwtRegisteredClaimNames.Email, user.EmailAddress, ClaimValueTypes.Email)
        };

        IEnumerable<Claim> customClaims = new List<Claim>
        {
            new (Claims.UserID, user.ID.ToString(), ClaimValueTypes.String),
            new (Claims.AccountID, user.Accounts.Single(account => account.IsMain).ID.ToString(), ClaimValueTypes.String),
            new (Claims.AccountIsMain, true.ToString(), ClaimValueTypes.Boolean),
            new (Claims.ClanName, string.Empty, ClaimValueTypes.String),
            new (Claims.ClanTag, string.Empty, ClaimValueTypes.String)
        };

        IEnumerable<Claim> allTokenClaims = Enumerable.Empty<Claim>().Union(userRoleClaims).Union(openIDClaims).Union(customClaims).OrderBy(claim => claim.Type);

        JwtSecurityToken token = new
        (
            issuer: Configuration.JWT.Issuer,
            audience: Configuration.JWT.Audience,
            claims: allTokenClaims,
            expires: DateTimeOffset.UtcNow.AddHours(Configuration.JWT.DurationInHours).DateTime,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.JWT.SigningKey)), SecurityAlgorithms.HmacSha256)
        );

        string jwt = new JwtSecurityTokenHandler().WriteToken(token);

        await MerrickContext.SaveChangesAsync();

        return Redirect($"{callbackURL}?discordLoggedIn=true&token={Uri.EscapeDataString(jwt)}&username={Uri.EscapeDataString(discordUsername)}&avatar={Uri.EscapeDataString(discordAvatar ?? string.Empty)}");
    }

    [HttpGet("Me", Name = "Get Linked Discord Account")]
    [Authorize(Policy = UserRoles.AllRoles)]
    [ProducesResponseType(typeof(GetDiscordLinkedAccountDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLinkedDiscordAccount()
    {
        int userID = int.Parse(User.Claims.Single(claim => claim.Type.Equals(Claims.UserID)).Value);

        User? user = await MerrickContext.Users
            .Include(record => record.Role)
            .SingleOrDefaultAsync(record => record.ID.Equals(userID));

        if (user is null)
        {
            return NotFound($@"User With ID ""{userID}"" Was Not Found");
        }

        return Ok(new GetDiscordLinkedAccountDTO(user.DiscordId, user.DiscordUsername, user.DiscordAvatarHash, user.DiscordLinkedAt));
    }

    [HttpDelete("Link", Name = "Unlink Discord Account")]
    [Authorize(Policy = UserRoles.AllRoles)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkDiscordAccount()
    {
        int userID = int.Parse(User.Claims.Single(claim => claim.Type.Equals(Claims.UserID)).Value);

        User? user = await MerrickContext.Users
            .Include(record => record.Role)
            .SingleOrDefaultAsync(record => record.ID.Equals(userID));

        if (user is null)
        {
            return NotFound($@"User With ID ""{userID}"" Was Not Found");
        }

        if (string.IsNullOrWhiteSpace(user.DiscordId))
        {
            return NotFound("No Discord account is currently linked to this user.");
        }

        string unlinkedDiscordID = user.DiscordId;

        user.DiscordId = null;
        user.DiscordUsername = null;
        user.DiscordAvatarHash = null;
        user.DiscordLinkedAt = null;

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation("User {UserID} unlinked Discord account {DiscordUserID}", user.ID, unlinkedDiscordID);

        return Ok("Discord account has been unlinked successfully.");
    }
}
