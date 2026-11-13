namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_REPORT_LEAVER)]
public class ReportLeaver(MerrickContext merrick, LeaverPenaltyService leaverPenaltyService) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        ReportLeaverRequestData requestData = new (buffer);

        Log.Information(@"Match Server ID ""{MatchServerID}"" Reported Leaver Account ID ""{AccountID}""",
            session.Metadata.ServerID, requestData.AccountID);

        Account? account = await merrick.Accounts
            .Include(a => a.User)
            .SingleOrDefaultAsync(a => a.ID == requestData.AccountID);

        if (account is null)
        {
            Log.Warning(@"Reported Leaver Account ID ""{AccountID}"" Not Found In Database", requestData.AccountID);
            return;
        }

        await leaverPenaltyService.AssessPenalty(
            account.User.ID,
            account.ID,
            account.Name,
            matchID: session.Metadata.MatchID >= 0 ? session.Metadata.MatchID : null,
            reason: $@"In-Match Abandonment On Server {session.Metadata.ServerID}"
        );
    }
}

file class ReportLeaverRequestData
{
    public byte[] CommandBytes { get; init; }

    public int AccountID { get; init; }

    public ReportLeaverRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        AccountID = buffer.ReadInt32();
    }
}

