namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles a staff member's request to observe or join an active match.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_STAFF_JOIN_MATCH_REQUEST)]
public class StaffJoinMatch : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        int matchID = buffer.ReadInt32();

        if (session.Account.Type is not AccountType.Staff)
        {
            Log.Warning(@"Non-Staff Account ""{AccountName}"" Attempted Staff Join For Match {MatchID}",
                session.Account.Name, matchID);

            ChatBuffer failureResponse = new ();
            failureResponse.WriteCommand(ChatProtocol.Command.CHAT_CMD_STAFF_JOIN_MATCH_RESPONSE);
            failureResponse.WriteInt8(0); // Failure Status
            session.Send(failureResponse);
            return;
        }

        Log.Information(@"Staff Account ""{AccountName}"" Authorised To Join Match {MatchID}",
            session.Account.Name, matchID);

        ChatBuffer response = new ();
        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_STAFF_JOIN_MATCH_RESPONSE);
        response.WriteInt8(1); // Success Status
        response.WriteInt32(matchID);

        session.Send(response);
    }
}
