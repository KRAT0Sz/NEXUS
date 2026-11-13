namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Replays;

[ChatCommand(ChatProtocol.ServerManagerToChatServer.NET_CHAT_SM_UPLOAD_UPDATE)]
public class ServerUploadUpdate(ReplayDistributionService replayDistributionService) : ISynchronousCommandProcessor<MatchServerChatSession>
{
    private ReplayDistributionService ReplayDistributionService { get; } = replayDistributionService;

    public void Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        ServerUploadUpdateData requestData = new (buffer);

        ReplayDistributionService.HandleServerUploadUpdate(
            session,
            requestData.MatchID,
            requestData.FileType,
            requestData.Status,
            requestData.DownloadURL);
    }
}

file class ServerUploadUpdateData
{
    public byte[] CommandBytes { get; init; }

    public int MatchID { get; init; }

    public byte FileType { get; init; }

    public byte Status { get; init; }

    public string? DownloadURL { get; init; }

    public ServerUploadUpdateData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchID = buffer.ReadInt32();
        FileType = buffer.ReadInt8();
        Status = buffer.ReadInt8();

        DownloadURL = buffer.Size > buffer.Offset ? buffer.ReadString() : null;
    }
}
