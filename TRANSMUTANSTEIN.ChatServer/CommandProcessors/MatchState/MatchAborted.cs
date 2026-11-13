namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

/// <summary>
///     Handles match abortion from game servers.
///     This is sent only for arranged matches that were aborted (e.g. player left during loading, connection timeout).
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ABORTED)]
public class MatchAborted(IDatabase distributedCacheStore, LeaverPenaltyService leaverPenaltyService) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchAbortedRequestData requestData = new (buffer);

        // Remove Match Information From Distributed Cache
        await distributedCacheStore.RemoveMatchInformation(requestData.MatchupID);

        Log.Information(@"Arranged Match {MatchupID} Aborted On Server ID ""{MatchServerID}"": Reason={Reason}",
            requestData.MatchupID, session.Metadata.ServerID, requestData.Reason);

        // Check if any player failed to connect or disconnected during loading, causing the abort
        MatchmakingMatch? match = MatchmakingService.GetMatchByServerID(session.Metadata.ServerID);

        if (match is not null)
        {
            foreach (MatchmakingGroupMember member in match.CommittedRoster)
            {
                if (member.Session.Metadata.LastKnownClientState is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_DISCONNECTED
                    || member.LoadingPercent < 100)
                {
                    await leaverPenaltyService.AssessPenalty(
                        member.Account.User.ID,
                        member.Account.ID,
                        member.Account.Name,
                        matchID: session.Metadata.MatchID >= 0 ? session.Metadata.MatchID : null,
                        reason: $@"Aborted Match On Server {session.Metadata.ServerID} (Failed To Connect/Load)"
                    );
                }
            }
        }

        // Make The Aborted Match's Groups Available To Queue Again And Drop The Match From The Active Matches Registry
        // This Is Harmless If The Subsequent MatchAbandoned "Server Reset" Signal Has Already Cleaned The Match Up
        MatchmakingService.CleanUpMatchesForServer(session.Metadata.ServerID);

        // A Match Server Hosts One Match At A Time, So Every Lock On The Server Is For The Aborted Match
        await distributedCacheStore.RemoveMatchLocksForServer(session.Metadata.ServerID);
    }
}

file class MatchAbortedRequestData
{
    public byte[] CommandBytes { get; init; }

    /// <summary>
    ///     The matchup ID assigned by the matchmaking system when the match was arranged.
    ///     This is the matchup/arrangement ID, not the match ID assigned when stats recording starts.
    /// </summary>
    public int MatchupID { get; init; }

    /// <summary>
    ///     The reason the match was aborted.
    /// </summary>
    public ChatProtocol.MatchAbortedReason Reason { get; init; }

    public MatchAbortedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchupID = buffer.ReadInt32();
        Reason = (ChatProtocol.MatchAbortedReason) buffer.ReadInt8();
    }
}
