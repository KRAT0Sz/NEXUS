using ASPIRE.Common;
using ASPIRE.Common.Extensions.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MERRICK.DatabaseContext;
using MERRICK.DatabaseContext.Entities.Statistics;
using MERRICK.DatabaseContext.Extensions;
using MERRICK.DatabaseContext.Persistence;
using Microsoft.EntityFrameworkCore;
using OBSERVER.DiscordBot;

var builder = Host.CreateApplicationBuilder(args);

// Add Aspire Service Defaults
builder.AddServiceDefaults();

// Add Serilog Logging (if used in ASPIRE.Common, otherwise standard logging is fine)
// builder.AddSerilogLogging(); // I will omit this if it's not defined, but it might be in ASPIRE.Common

// Add The Database Context
builder.AddSqlServerDbContext<MerrickContext>("MERRICK", configureSettings: null, configureDbContextOptions: options =>
{
    // Specify Migrations History Table And Schema
    options.UseSqlServer(sqlServerOptionsAction: sqlServerOptions => sqlServerOptions.MigrationsHistoryTable("MigrationsHistory", MerrickContext.MetadataSchema));

    // Enable Detailed Error Messages In Development Environment
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());

    // Enable Thread Safety Checks For Entity Framework
    options.EnableThreadSafetyChecks();

    // Add Interceptors
    options.AddMerrickInterceptors();
});

// Configure Discord Client
builder.Services.AddSingleton(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
});
builder.Services.AddSingleton<DiscordSocketClient>();

// Configure Interaction Service
builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));

// Add HttpClient for Health Checks
builder.Services.AddHttpClient();

// Add Discord Bot Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
