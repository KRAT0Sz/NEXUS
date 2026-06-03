using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace OBSERVER.DiscordBot;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, DiscordSocketClient client, InteractionService interactions, IServiceProvider services, IConfiguration configuration)
    {
        _logger = logger;
        _client = client;
        _interactions = interactions;
        _services = services;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += LogAsync;
        _interactions.Log += LogAsync;

        _client.Ready += ReadyAsync;

        // Add modules to InteractionService
        await _interactions.AddModulesAsync(typeof(Worker).Assembly, _services);

        _client.InteractionCreated += async interaction =>
        {
            _logger.LogInformation("Received interaction: {Id} of type {Type}", interaction.Id, interaction.Type);
            _ = Task.Run(async () => 
            {
                try
                {
                    _logger.LogInformation("Creating scope for interaction {Id}", interaction.Id);
                    using var scope = _services.CreateScope();
                    var ctx = new SocketInteractionContext(_client, interaction);
                    
                    _logger.LogInformation("Executing command for interaction {Id}", interaction.Id);
                    await _interactions.ExecuteCommandAsync(ctx, scope.ServiceProvider);
                    _logger.LogInformation("Finished Executing command for interaction {Id}", interaction.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatal error executing interaction {Id}", interaction.Id);
                    if (!interaction.HasResponded)
                        await interaction.RespondAsync("❌ Fatal error executing command.", ephemeral: true);
                }
            });
        };

        _interactions.InteractionExecuted += async (command, context, result) =>
        {
            if (!result.IsSuccess)
            {
                _logger.LogError("Command {CommandName} failed: {ErrorReason}", command?.Name ?? "Unknown", result.ErrorReason);
                
                if (!context.Interaction.HasResponded)
                {
                    await context.Interaction.RespondAsync($"❌ Command failed: {result.ErrorReason}", ephemeral: true);
                }
                else
                {
                    await context.Interaction.FollowupAsync($"❌ Command failed: {result.ErrorReason}", ephemeral: true);
                }
            }
        };

        string? token = _configuration["DiscordBotToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("DiscordBotToken is missing from configuration. Bot will not start.");
            return;
        }

        try
        {
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            
            await Task.Delay(-1, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while starting the Discord bot.");
        }
    }

    private async Task ReadyAsync()
    {
        _logger.LogInformation("Bot is connected and ready.");
        
        string? guildIdStr = _configuration["DiscordTestGuildId"];
        if (ulong.TryParse(guildIdStr, out ulong guildId))
        {
            await _interactions.RegisterCommandsToGuildAsync(guildId);
            _logger.LogInformation("Registered commands to guild {GuildId}.", guildId);
        }
        else
        {
            await _interactions.RegisterCommandsGloballyAsync();
            _logger.LogInformation("Registered commands globally.");
        }
    }

    private Task LogAsync(LogMessage msg)
    {
        var logLevel = msg.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, msg.Exception, "{Source}: {Message}", msg.Source, msg.Message);
        return Task.CompletedTask;
    }
}
