using Discord;
using Discord.Interactions;

namespace OBSERVER.DiscordBot.Modules;

public class ServerCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ServerCommands> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ServerCommands(IHttpClientFactory httpClientFactory, ILogger<ServerCommands> logger, IServiceProvider serviceProvider)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    [SlashCommand("status", "Check the status of Project KONGOR servers.")]
    public async Task StatusAsync()
    {
        await DeferAsync();

        var client = _httpClientFactory.CreateClient();
        
        string masterStatus = await CheckStatusAsync(client, "http://kongor-masterserver/health");
        string chatStatus = await CheckStatusAsync(client, "http://transmutanstein-chatserver/health");
        string portalStatus = await CheckStatusAsync(client, "http://web-portal-api/health");

        var embed = new EmbedBuilder()
            .WithTitle("Server Status")
            .WithColor(Color.Blue)
            .AddField("Master Server", masterStatus, true)
            .AddField("Chat Server", chatStatus, true)
            .AddField("Web Portal API", portalStatus, true)
            .WithCurrentTimestamp()
            .Build();

        await FollowupAsync(embed: embed);
    }

    private async Task<string> CheckStatusAsync(HttpClient client, string url)
    {
        try
        {
            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode ? "🟢 Online" : "🔴 Offline";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check status for {Url}", url);
            return "🔴 Offline";
        }
    }

    [SlashCommand("host-token", "Generate a new Host Account Authorisation Token")]
    [DefaultMemberPermissions(GuildPermission.Administrator)] // Only allow server administrators to run this command
    public async Task GenerateHostTokenAsync()
    {
        await DeferAsync(ephemeral: true);

        try
        {
            var dbContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<MERRICK.DatabaseContext.Persistence.MerrickContext>(_serviceProvider);
            
            MERRICK.DatabaseContext.Entities.Utility.Token token = new ()
            {
                Purpose = MERRICK.DatabaseContext.Entities.Utility.TokenPurpose.HostAccountAuthorisation,
                EmailAddress = string.Empty,
                Value = Guid.CreateVersion7(),
                Data = MERRICK.DatabaseContext.Enumerations.AccountType.ServerHost.ToString(),
                Validity = TimeSpan.FromHours(24)
            };

            await dbContext.Tokens.AddAsync(token);
            await dbContext.SaveChangesAsync();

            await FollowupAsync($"✅ **Host Account Authorisation Token Created!**\n\n`{token.Value}`\n\n*This token is valid for 24 hours.*", ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate host token");
            await FollowupAsync("❌ An error occurred while generating the token.", ephemeral: true);
        }
    }
}
