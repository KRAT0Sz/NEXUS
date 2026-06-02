using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MERRICK.DatabaseContext.Persistence;

namespace VINDICATOR.DiscordBot.Modules;

public class StatsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public StatsModule(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [SlashCommand("stats", "View your Project KONGOR Re:BIRTH player statistics.")]
    public async Task GetStatsAsync([Summary("user", "Optional: Tag a user to see their stats")] IUser? targetUser = null)
    {
        // Acknowledge the interaction to prevent timeout while querying DB (must be ephemeral as well)
        await DeferAsync(ephemeral: true);

        var userToLookup = targetUser ?? Context.User;
        var discordIdStr = userToLookup.Id.ToString();

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        // Find the user in the database by their DiscordId
        var merrickUser = await db.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.DiscordId == discordIdStr);

        if (merrickUser == null || merrickUser.Accounts.Count == 0)
        {
            await FollowupAsync($"The Discord account {userToLookup.Mention} has not been linked to a Project KONGOR account yet. Please link it via the Web Portal!", ephemeral: true);
            return;
        }

        var mainAccount = merrickUser.Accounts.FirstOrDefault(a => a.IsMain) ?? merrickUser.Accounts.FirstOrDefault();
        if (mainAccount == null)
        {
            await FollowupAsync("No active game account found.", ephemeral: true);
            return;
        }

        var statsList = await db.AccountStatistics.Where(s => s.AccountID == mainAccount.ID).ToListAsync();

        // We will look for Matchmaking stats, or fallback to Public stats
        var stats = statsList.FirstOrDefault(s => s.Type == MERRICK.DatabaseContext.Entities.Statistics.AccountStatisticsType.Matchmaking) 
                    ?? statsList.FirstOrDefault();

        if (stats == null)
        {
            await FollowupAsync($"Account **{mainAccount.Name}** has not played any matches yet.", ephemeral: true);
            return;
        }

        double winRate = stats.MatchesPlayed > 0 
            ? Math.Round((double)stats.MatchesWon / stats.MatchesPlayed * 100, 2) 
            : 0;

        var embed = new EmbedBuilder()
            .WithTitle($"📊 Player Statistics: {mainAccount.Name}")
            .WithColor(Color.Green)
            .WithThumbnailUrl(userToLookup.GetAvatarUrl() ?? userToLookup.GetDefaultAvatarUrl())
            .AddField("Skill Rating (MMR)", $"{Math.Round(stats.SkillRating, 2)}", inline: true)
            .AddField("Matches Played", $"{stats.MatchesPlayed} ({stats.MatchesWon}W / {stats.MatchesLost}L)", inline: true)
            .AddField("Win Rate", $"{winRate}%", inline: true)
            .AddField("Kills / Deaths / Assists", $"{stats.HeroKills} / {stats.HeroDeaths} / {stats.HeroAssists}", inline: true)
            .AddField("K/D Ratio", $"{Math.Round((double)stats.HeroKills / Math.Max(1, stats.HeroDeaths), 2)}", inline: true)
            .AddField("Wards Placed", $"{stats.WardsPlaced}", inline: true)
            .AddField("Smackdowns", $"{stats.Smackdowns}", inline: true)
            .WithFooter("Project KONGOR Re:BIRTH")
            .WithCurrentTimestamp()
            .Build();

        await FollowupAsync(embed: embed, ephemeral: true);
    }
}
