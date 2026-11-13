namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_WHISPER)]
public class SendWhisper(FloodPreventionService floodPreventionService, KongorProtectService kongorProtectService) : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        SendWhisperRequestData requestData = new (buffer);

        // If The Flood Prevention Service Returns False, The Session Has Been Notified That The Client Has Exceeded The Flood Threshold; In This Case, The Command Is Aborted
        if (floodPreventionService.CheckAndHandleFloodPrevention(session) is false)
            return;

        string message = requestData.Message;

        // KONGOR PROTECT: Inspect whispers for non-staff players
        if (session.Account.Type is not AccountType.Staff)
        {
            KongorProtectResult protectResult = kongorProtectService.Inspect(message, session.Account.ID);

            if (protectResult.IsBlocked)
            {
                ChatBuffer warningBuffer = new ();
                warningBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER);
                warningBuffer.WriteString("KONGOR PROTECT");
                warningBuffer.WriteString("Your whisper was blocked due to prohibited language.");
                session.Send(warningBuffer);

                Log.Warning(@"[KONGOR PROTECT] Blocked whisper from Account ID {AccountID} to ""{Target}""",
                    session.Account.ID, requestData.TargetName);

                return;
            }

            message = protectResult.SanitisedMessage;
        }

        Whisper
            .Create(message)
            .Send(session, requestData.TargetName);
    }
}

file class SendWhisperRequestData
{
    public byte[] CommandBytes { get; init; }

    public string TargetName { get; init; }

    public string Message { get; init; }

    public SendWhisperRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetName = buffer.ReadString();
        Message = buffer.ReadString();
    }
}
