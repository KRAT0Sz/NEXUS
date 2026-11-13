namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Statistics;

/// <summary>
///     Handles a client's request for matchmaking campaign season statistics.
/// </summary>
[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CAMPAIGN_STATS)]
public class MatchmakingCampaignStats : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        Log.Debug(@"Sending Campaign Statistics For Account ""{AccountName}""", session.Account.Name);

        ChatBuffer response = new ();
        response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CAMPAIGN_STATS);

        response.WriteInt32(1500); // Standard Rating
        response.WriteInt32(0);    // Rank
        response.WriteInt32(0);    // Wins
        response.WriteInt32(0);    // Losses
        response.WriteInt32(0);    // Win Streak
        response.WriteInt32(0);    // Games Played

        session.Send(response);
    }
}
