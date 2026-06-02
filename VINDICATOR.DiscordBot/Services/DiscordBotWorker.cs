using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VINDICATOR.DiscordBot.Services;

public class DiscordBotWorker : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionHandler _interactionHandler;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiscordBotWorker> _logger;

    public DiscordBotWorker(
        DiscordSocketClient client,
        InteractionHandler interactionHandler,
        IConfiguration configuration,
        ILogger<DiscordBotWorker> logger)
    {
        _client = client;
        _interactionHandler = interactionHandler;
        _configuration = configuration;
        _logger = logger;

        _client.Log += LogAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string? token = _configuration["Discord:BotToken"];
        
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Discord:BotToken is not set. The bot cannot start.");
            return;
        }

        await _interactionHandler.InitializeAsync();

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        
        await _client.SetGameAsync("Project KONGOR Re:BIRTH");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
        await _client.LogoutAsync();
    }

    private Task LogAsync(LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => LogLevel.Information
        };

        _logger.Log(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
        
        return Task.CompletedTask;
    }
}
