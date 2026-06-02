using Discord.Interactions;

namespace VINDICATOR.DiscordBot.Modules;

public class BasicModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Pings the bot to check its responsiveness.")]
    public async Task PingAsync()
    {
        // Respond to the user
        await RespondAsync("Pong! The Project KONGOR bot is online and ready.", ephemeral: true);
    }
}
