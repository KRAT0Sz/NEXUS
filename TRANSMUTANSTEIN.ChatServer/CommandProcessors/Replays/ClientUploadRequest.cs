namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Replays;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_UPLOAD_REQUEST)]
public class ClientUploadRequest(ReplayDistributionService replayDistributionService) : IAsynchronousCommandProcessor<ClientChatSession>
{
    private ReplayDistributionService ReplayDistributionService { get; } = replayDistributionService;

    public async Task Process(ClientChatSession session, ChatBuffer buffer)
    {
        ClientUploadRequestData requestData = new (buffer);

        await ReplayDistributionService.HandleClientUploadRequest(session, requestData.MatchID, requestData.FileType);
    }
}

file class ClientUploadRequestData
{
    public byte[] CommandBytes { get; init; }

    public int MatchID { get; init; }

    public byte FileType { get; init; }

    public ClientUploadRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchID = buffer.ReadInt32();
        FileType = buffer.ReadInt8();
    }
}

