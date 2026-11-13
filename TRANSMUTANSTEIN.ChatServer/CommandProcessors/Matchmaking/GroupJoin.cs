namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN)]
public class GroupJoin(MerrickContext merrick, MatchLockService matchLockService, LeaverPenaltyService leaverPenaltyService) : IAsynchronousCommandProcessor<ClientChatSession>
{
    public async Task Process(ClientChatSession session, ChatBuffer buffer)
    {
        GroupJoinRequestData requestData = new (buffer);

        if (await matchLockService.Enforce(session, matchmaking: true))
            return;

        MatchmakingCooldown? cooldown = await leaverPenaltyService.GetActiveCooldown(session.Account.User.ID);

        if (cooldown is not null && cooldown.IsActive)
        {
            TimeSpan remaining = cooldown.RemainingTime;
            session.SendMatchmakingBan((uint) remaining.TotalSeconds);
            session.SendNotice("Matchmaking", $@"You cannot join a matchmaking group while on cooldown. Time remaining: {Math.Ceiling(remaining.TotalMinutes)} minutes.");

            return;
        }

        await MatchmakingGroup
            .GetByMemberAccountName(requestData.InviteIssuerName)
            .Join(session, merrick);
    }
}

file class GroupJoinRequestData
{
    public byte[] CommandBytes { get; init; }

    public string ClientVersion { get; init; }

    public string InviteIssuerName { get; init; }

    public GroupJoinRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ClientVersion = buffer.ReadString();
        InviteIssuerName = buffer.ReadString();
    }
}
