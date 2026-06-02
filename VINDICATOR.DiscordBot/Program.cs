using ASPIRE.Common.Extensions.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MERRICK.DatabaseContext.Persistence;
using VINDICATOR.DiscordBot;
using VINDICATOR.DiscordBot.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add Aspire Service Defaults
builder.AddServiceDefaults();

// Add Database Context
builder.AddSqlServerDbContext<MerrickContext>("MERRICK", configureDbContextOptions: options =>
{
    options.UseSqlServer(sqlServerOptions => sqlServerOptions.MigrationsHistoryTable("MigrationsHistory", MerrickContext.MetadataSchema));
});

// Add HttpClient for Server Status checks (Bypass SSL for local dev certs)
builder.Services.AddHttpClient("HealthCheckClient")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// Configure Discord Socket Client
builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
    AlwaysDownloadUsers = true,
    MessageCacheSize = 100,
    LogLevel = LogSeverity.Info
}));

// Configure Interaction Service
builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), new InteractionServiceConfig
{
    LogLevel = LogSeverity.Info,
    DefaultRunMode = RunMode.Async
}));

// Register Handlers and Hosted Services
builder.Services.AddSingleton<InteractionHandler>();
builder.Services.AddHostedService<DiscordBotWorker>();

var host = builder.Build();
host.Run();
