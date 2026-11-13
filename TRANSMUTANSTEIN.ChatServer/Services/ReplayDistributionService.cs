namespace TRANSMUTANSTEIN.ChatServer.Services;

public class ReplayDistributionService(IServiceScopeFactory scopeFactory, ILogger<ReplayDistributionService> logger)
{
    private IServiceScopeFactory ScopeFactory { get; } = scopeFactory;
    private readonly ILogger<ReplayDistributionService> _logger = logger;

    private readonly ConcurrentDictionary<int, List<ClientChatSession>> _pendingSubscribers = new ();

    public async Task HandleClientUploadRequest(ClientChatSession session, int matchID, byte fileType)
    {
        _logger.LogInformation(@"Account ""{AccountName}"" (ID: {AccountID}) Requested Replay Upload For Match ID {MatchID} (FileType: {FileType})",
            session.Account.Name, session.Account.ID, matchID, fileType);

        string expectedReplayURL = $"https://replays.projectkongor.com/replays/M{matchID}.honreplay";

        MatchStatistics? matchStatistics = null;

        using (IServiceScope scope = ScopeFactory.CreateScope())
        {
            MerrickContext context = scope.ServiceProvider.GetRequiredService<MerrickContext>();
            matchStatistics = await context.MatchStatistics
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.MatchID == matchID);
        }

        if (matchStatistics is null)
        {
            SendUploadStatus(session, matchID, fileType, (byte) ChatProtocol.UploadUpdateType.EUUT_FILE_DOES_NOT_EXIST, expectedReplayURL);
            return;
        }

        // Register client to receive status updates
        _pendingSubscribers.AddOrUpdate(
            matchID,
            [session],
            (_, list) =>
            {
                lock (list)
                {
                    if (list.Contains(session) is false)
                        list.Add(session);
                }

                return list;
            });

        // Find the match server that hosted the match
        if (Context.MatchServerChatSessions.TryGetValue(matchStatistics.ServerID, out MatchServerChatSession? matchServer))
        {
            ChatBuffer requestBuffer = new ();
            requestBuffer.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_UPLOAD_REQUEST);
            requestBuffer.WriteInt32(matchID);
            requestBuffer.WriteInt8(fileType);
            matchServer.Send(requestBuffer);

            SendUploadStatus(session, matchID, fileType, (byte) ChatProtocol.UploadUpdateType.EUUT_FILE_QUEUED, expectedReplayURL);
        }
        else
        {
            // Server is not currently connected; if file is already on CDN, return complete, otherwise report offline
            SendUploadStatus(session, matchID, fileType, (byte) ChatProtocol.UploadUpdateType.EUUT_FILE_ALREADY_UPLOADED, expectedReplayURL);
        }
    }

    public void HandleServerUploadUpdate(MatchServerChatSession serverSession, int matchID, byte fileType, byte status, string? downloadURL)
    {
        string finalURL = downloadURL ?? $"https://replays.projectkongor.com/replays/M{matchID}.honreplay";

        int serverID = serverSession.Metadata?.ServerID ?? 0;

        _logger.LogInformation(@"Match Server ID {ServerID} Reported Upload Update For Match ID {MatchID}: Status={Status}, URL={URL}",
            serverID, matchID, (ChatProtocol.UploadUpdateType) status, finalURL);

        if (_pendingSubscribers.TryGetValue(matchID, out List<ClientChatSession>? subscribers))
        {
            lock (subscribers)
            {
                foreach (ClientChatSession subscriber in subscribers)
                    SendUploadStatus(subscriber, matchID, fileType, status, finalURL);
            }

            ChatProtocol.UploadUpdateType updateType = (ChatProtocol.UploadUpdateType) status;
            if (updateType is ChatProtocol.UploadUpdateType.EUUT_FILE_UPLOAD_COMPLETE
                or ChatProtocol.UploadUpdateType.EUUT_FILE_ALREADY_UPLOADED
                or ChatProtocol.UploadUpdateType.EUUT_FILE_DOES_NOT_EXIST
                or ChatProtocol.UploadUpdateType.EUUT_GENERAL_FAILURE)
            {
                _pendingSubscribers.TryRemove(matchID, out _);
            }
        }
    }

    private static void SendUploadStatus(ClientChatSession session, int matchID, byte fileType, byte status, string downloadURL)
    {
        ChatBuffer statusBuffer = new ();
        statusBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_UPLOAD_STATUS);
        statusBuffer.WriteInt32(matchID);
        statusBuffer.WriteInt8(fileType);
        statusBuffer.WriteInt8(status);
        statusBuffer.WriteString(downloadURL);

        session.Send(statusBuffer);
    }
}
