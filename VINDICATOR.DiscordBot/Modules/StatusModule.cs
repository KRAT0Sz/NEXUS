using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace VINDICATOR.DiscordBot.Modules;

public class StatusModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<StatusModule> _logger;

    public StatusModule(IHttpClientFactory httpClientFactory, ILogger<StatusModule> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [SlashCommand("serverstatus", "Check the current online status of the game servers.")]
    public async Task GetServerStatusAsync()
    {
        await DeferAsync(ephemeral: true);

        var client = _httpClientFactory.CreateClient("HealthCheckClient");
        client.Timeout = TimeSpan.FromSeconds(5);

        // Ping endpoints
        var masterServerTask = PingEndpointAsync(client, "http://master-server/alive");
        var chatServerTask = PingEndpointAsync(client, "https://chat-server/alive");
        var webPortalTask = PingEndpointAsync(client, "https://web-portal-api/alive");

        await Task.WhenAll(masterServerTask, chatServerTask, webPortalTask);

        bool isMasterOnline = await masterServerTask;
        bool isChatOnline = await chatServerTask;
        bool isWebOnline = await webPortalTask;

        var embed = new EmbedBuilder()
            .WithTitle("🌐 Project KONGOR Re:BIRTH Server Status")
            .WithColor((isMasterOnline && isChatOnline) ? Color.Green : Color.Red)
            .AddField("Master Server", isMasterOnline ? "🟢 Online" : "🔴 Offline", inline: true)
            .AddField("Chat Server", isChatOnline ? "🟢 Online" : "🔴 Offline", inline: true)
            .AddField("Web Portal API", isWebOnline ? "🟢 Online" : "🔴 Offline", inline: true)
            .WithFooter("Project KONGOR Re:BIRTH")
            .WithCurrentTimestamp()
            .Build();

        await FollowupAsync(embed: embed, ephemeral: true);
    }

    private async Task<bool> PingEndpointAsync(HttpClient client, string url)
    {
        try
        {
            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to ping {Url}", url);
            return false;
        }
    }
}
