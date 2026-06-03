using Discord;
using Discord.Interactions;
using MERRICK.DatabaseContext.Entities.Statistics;
using MERRICK.DatabaseContext.Persistence;
using Microsoft.EntityFrameworkCore;

namespace OBSERVER.DiscordBot.Modules;

public class PlayerCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlayerCommands> _logger;

    public PlayerCommands(IServiceProvider serviceProvider, ILogger<PlayerCommands> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [SlashCommand("stats", "Check the statistics of a player.")]
    public async Task StatsAsync([Summary("username", "The name of the player to check")] string username)
    {
        await DeferAsync();

        try
        {
            var dbContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<MerrickContext>(_serviceProvider);
            
            var account = await dbContext.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Name.ToLower() == username.ToLower());

            if (account is null)
            {
                await FollowupAsync($"❌ Could not find a player named `{username}`.");
                return;
            }

            var stats = await dbContext.AccountStatistics
                .AsNoTracking()
                .Where(s => s.AccountID == account.ID)
                .ToListAsync();

            if (!stats.Any())
            {
                await FollowupAsync($"No statistics available for **{account.NameWithClanTag}** yet.");
                return;
            }

            // We'll show stats for Matchmaking mode by default, or aggregate them.
            // For simplicity, let's grab Matchmaking or the first available.
            var mmStats = stats.FirstOrDefault(s => s.Type == AccountStatisticsType.Matchmaking) ?? stats.First();

            var embed = new EmbedBuilder()
                .WithTitle($"📊 Stats for {account.NameWithClanTag}")
                .WithColor(Color.Green)
                .AddField("Skill Rating (MMR)", $"{mmStats.SkillRating:F1}", true)
                .AddField("Matches (W/L)", $"{mmStats.MatchesWon}W / {mmStats.MatchesLost}L", true)
                .AddField("K/D/A", $"{mmStats.HeroKills} / {mmStats.HeroDeaths} / {mmStats.HeroAssists}", true)
                .AddField("Smackdowns", mmStats.Smackdowns.ToString(), true)
                .AddField("Wards Placed", mmStats.WardsPlaced.ToString(), true)
                .AddField("Mode", mmStats.Type.ToString(), true)
                .WithCurrentTimestamp()
                .Build();

            await FollowupAsync(embed: embed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve stats for player {Username}", username);
            await FollowupAsync("⚠️ An error occurred while retrieving player statistics. Please try again later.");
        }
    }
}
