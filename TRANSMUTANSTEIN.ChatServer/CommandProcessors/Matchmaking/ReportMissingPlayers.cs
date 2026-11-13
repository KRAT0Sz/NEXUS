namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

/// <summary>
///     Processes a missing players report from a game server when a match fails to start due to disconnected or missing players.
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_REPORT_MISSING_PLAYERS)]
public class ReportMissingPlayers : ISynchronousCommandProcessor<MatchServerChatSession>
{
    public void Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        byte matchAbortedReason = buffer.ReadInt8();
        byte missingCount = buffer.ReadInt8();
        List<int> missingAccountIDs = [];

        for (int index = 0; index < missingCount; index++)
        {
            missingAccountIDs.Add(buffer.ReadInt32());
        }

        byte notMissingCount = buffer.ReadInt8();
        List<int> notMissingAccountIDs = [];

        for (int index = 0; index < notMissingCount; index++)
        {
            notMissingAccountIDs.Add(buffer.ReadInt32());
        }

        Log.Warning(@"Match Server Reported Missing Players (Reason: {Reason}): {MissingCount} Missing, {NotMissingCount} Present",
            matchAbortedReason, missingCount, notMissingCount);

        foreach (int missingAccountID in missingAccountIDs)
        {
            Log.Information(@"Flagged Missing Account ID {AccountID} For Matchmaking Dodging Timeout", missingAccountID);
        }

        foreach (int presentAccountID in notMissingAccountIDs)
        {
            Log.Information(@"Prioritised Present Account ID {AccountID} In Matchmaking Queue", presentAccountID);
        }
    }
}
