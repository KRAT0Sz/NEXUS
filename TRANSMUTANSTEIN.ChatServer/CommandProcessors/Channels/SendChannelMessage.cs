namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG)]
public class SendChannelMessage(
    FloodPreventionService floodPreventionService,
    KongorProtectService kongorProtectService,
    StreamerModeService streamerModeService) : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        SendChannelMessageRequestData requestData = new (buffer);

        // Silently Drop Empty Messages
        if (string.IsNullOrEmpty(requestData.Message))
            return;

        // If The Flood Prevention Service Returns False, The Session Has Been Notified That The Client Has Exceeded The Flood Threshold; In This Case, The Command Is Aborted
        if (floodPreventionService.CheckAndHandleFloodPrevention(session) is false)
            return;

        ChatChannel? channel = Context.ChatChannels.Values
            .SingleOrDefault(chatChannel => chatChannel.ID == requestData.ChannelID);

        // An Unknown Channel ID Implies The Server Lost A Channel It Previously Announced Or The Client Sent A Malformed ID
        if (channel is null)
        {
            Log.Error(@"[BUG] Account ""{AccountName}"" Sent A Channel Message For Unknown Channel ID {ChannelID}", session.Account.Name, requestData.ChannelID);

            return;
        }

        // Silently Drop Messages For Channels The Sender Is Not A Member Of
        if (channel.Members.ContainsKey(session.Account.Name) is false)
            return;

        // Check If The Sender Is Silenced In This Channel
        if (channel.IsSilenced(session))
        {
            ChatBuffer response = new ();

            response.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCED);
            response.WriteInt32(requestData.ChannelID); // Channel ID

            session.Send(response);

            return;
        }

        string messageContent = requestData.Message;

        // Intercept Streamer Mode commands before broadcasting
        if (messageContent.StartsWith("/streamer", StringComparison.OrdinalIgnoreCase))
        {
            HandleStreamerCommand(session, messageContent, streamerModeService);

            return;
        }

        // Intercept Avoid commands
        if (messageContent.StartsWith("/avoid", StringComparison.OrdinalIgnoreCase))
        {
            HandleAvoidCommand(session, messageContent);

            return;
        }

        // Intercept Staff Force Matchup command
        if (messageContent.StartsWith("/forcegroupmatchup", StringComparison.OrdinalIgnoreCase))
        {
            HandleForceGroupMatchupCommand(session, messageContent);

            return;
        }

        // Enforce Message Content Length Limit
        // Staff Accounts Are Exempt From Message Length Restrictions, For Moderation And Administration Purposes
        if (session.Account.Type is not AccountType.Staff && messageContent.Length > ChatProtocol.CHAT_MESSAGE_MAX_LENGTH)
        {
            messageContent = messageContent[.. ChatProtocol.CHAT_MESSAGE_MAX_LENGTH];

            // TODO: Notify The Sender That Their Message Was Truncated
        }

        // KONGOR PROTECT: Inspect and moderate message content for non-staff accounts
        if (session.Account.Type is not AccountType.Staff)
        {
            KongorProtectResult protectResult = kongorProtectService.Inspect(messageContent, session.Account.ID);

            if (protectResult.IsBlocked)
            {
                // Message dropped due to severe profanity; inform sender with a warning whisper
                ChatBuffer warningBuffer = new ();
                warningBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER);
                warningBuffer.WriteString("KONGOR PROTECT");
                warningBuffer.WriteString("Your message was blocked due to prohibited language.");
                session.Send(warningBuffer);

                Log.Warning(@"[KONGOR PROTECT] Blocked message from Account ID {AccountID} in Channel ID {ChannelID}",
                    session.Account.ID, requestData.ChannelID);

                return;
            }

            messageContent = protectResult.SanitisedMessage;

            if (kongorProtectService.HasExceededStrikeLimit(session.Account.ID))
            {
                // Automatically silence repeated offenders for 5 minutes
                channel.SilencedAccounts[session.Account.ID] = DateTime.UtcNow.AddMinutes(5);

                ChatBuffer silenceBuffer = new ();
                silenceBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCED);
                silenceBuffer.WriteInt32(requestData.ChannelID);
                session.Send(silenceBuffer);

                Log.Warning(@"[KONGOR PROTECT] Account ID {AccountID} exceeded strike limit and was silenced in Channel ID {ChannelID}",
                    session.Account.ID, requestData.ChannelID);
            }
        }

        // Broadcast The Message To All Channel Members Except The Sender
        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG);
        broadcast.WriteInt32(session.Account.ID);    // Sender Account ID
        broadcast.WriteInt32(requestData.ChannelID); // Channel ID
        broadcast.WriteString(messageContent);       // Message Content (Potentially Truncated)

        channel.BroadcastMessage(broadcast, excludeAccountID: session.Account.ID);
    }

    private static void HandleStreamerCommand(ClientChatSession session, string messageContent, StreamerModeService streamerModeService)
    {
        string[] parts = messageContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string subCommand = parts.Length > 1 ? parts[1].ToLowerInvariant() : "status";

        _ = Task.Run(async () =>
        {
            switch (subCommand)
            {
                case "on":
                {
                    string? requestedAlias = parts.Length > 2 ? parts[2] : null;
                    StreamerModeConfiguration configuration = await streamerModeService.EnableStreamerMode(session.Account.ID, session.Account.Name, requestedAlias);
                    session.StreamerConfiguration = configuration;

                    SendStreamerNotification(session, $"Streamer Mode ENABLED. Your anonymous display name is '{configuration.MaskedName}'. MMR, Stats, and Match Server IP are shielded.");
                    break;
                }

                case "off":
                {
                    await streamerModeService.DisableStreamerMode(session.Account.ID, session.Account.Name);
                    session.StreamerConfiguration = null;

                    SendStreamerNotification(session, "Streamer Mode DISABLED. Your real account name and statistics have been restored.");
                    break;
                }

                case "reroll":
                {
                    string newAlias = await streamerModeService.RerollGuestName(session.Account.ID, session.Account.Name);

                    if (session.StreamerConfiguration is not null)
                        session.StreamerConfiguration.MaskedName = newAlias;

                    SendStreamerNotification(session, $"New anonymous alias generated: '{newAlias}'.");
                    break;
                }

                case "status":
                {
                    StreamerModeConfiguration? configuration = session.StreamerConfiguration ?? await streamerModeService.GetStreamerMode(session.Account.Name);
                    bool isActive = configuration is not null && configuration.Enabled;

                    string statusMessage = isActive
                        ? $"Streamer Mode is ACTIVE. Display Name: '{configuration?.MaskedName}'. MMR, Stats, and Server IP are shielded."
                        : "Streamer Mode is INACTIVE. Type '/streamer on' to enable anonymous protection.";

                    SendNotificationWhisper(session, "KONGOR PROTECT", statusMessage);
                    break;
                }

                default:
                {
                    SendNotificationWhisper(session, "KONGOR PROTECT", "Usage: /streamer on [optional_name] | /streamer off | /streamer reroll | /streamer status");
                    break;
                }
            }
        });
    }

    private static void HandleAvoidCommand(ClientChatSession session, string messageContent)
    {
        string[] parts = messageContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string subCommand = parts.Length > 1 ? parts[1].ToLowerInvariant() : "list";

        switch (subCommand)
        {
            case "clear":
            {
                session.AvoidedAccountIDs.Clear();
                SendNotificationWhisper(session, "MATCHMAKING", "Avoided teammates list cleared.");
                break;
            }

            case "list":
            {
                if (session.AvoidedAccountIDs.Count == 0)
                {
                    SendNotificationWhisper(session, "MATCHMAKING", "You are not currently avoiding any teammates.");
                    return;
                }

                List<string> names = Context.ClientChatSessions.Values
                    .Where(s => session.AvoidedAccountIDs.Contains(s.Account.ID))
                    .Select(s => s.Account.Name)
                    .ToList();

                string listText = names.Count > 0 ? string.Join(", ", names) : $"{session.AvoidedAccountIDs.Count} player(s)";
                SendNotificationWhisper(session, "MATCHMAKING", $"Avoided teammates ({session.AvoidedAccountIDs.Count}): {listText}");
                break;
            }

            case "add":
            {
                if (parts.Length < 3)
                {
                    SendNotificationWhisper(session, "MATCHMAKING", "Usage: /avoid add <accountName>");
                    return;
                }

                string targetName = parts[2];
                ClientChatSession? targetSession = Context.ClientChatSessions.Values
                    .FirstOrDefault(s => string.Equals(s.Account.Name, targetName, StringComparison.OrdinalIgnoreCase));

                if (targetSession is null)
                {
                    SendNotificationWhisper(session, "MATCHMAKING", $"Could not find online player '{targetName}'.");
                    return;
                }

                if (targetSession.Account.ID == session.Account.ID)
                {
                    SendNotificationWhisper(session, "MATCHMAKING", "You cannot avoid yourself.");
                    return;
                }

                if (session.AvoidedAccountIDs.Count >= 10)
                {
                    SendNotificationWhisper(session, "MATCHMAKING", "You have reached the maximum limit of 10 avoided players. Use '/avoid clear' to reset.");
                    return;
                }

                session.AvoidedAccountIDs.Add(targetSession.Account.ID);
                SendNotificationWhisper(session, "MATCHMAKING", $"Player '{targetSession.Account.Name}' added to your avoided teammates list.");
                break;
            }

            case "remove":
            {
                if (parts.Length < 3)
                {
                    SendNotificationWhisper(session, "MATCHMAKING", "Usage: /avoid remove <accountName>");
                    return;
                }

                string targetName = parts[2];
                ClientChatSession? targetSession = Context.ClientChatSessions.Values
                    .FirstOrDefault(s => string.Equals(s.Account.Name, targetName, StringComparison.OrdinalIgnoreCase));

                int targetID = targetSession?.Account.ID ?? -1;
                if (targetID != -1 && session.AvoidedAccountIDs.Remove(targetID))
                {
                    SendNotificationWhisper(session, "MATCHMAKING", $"Player '{targetName}' removed from your avoided teammates list.");
                }
                else
                {
                    SendNotificationWhisper(session, "MATCHMAKING", $"Player '{targetName}' was not in your avoided list.");
                }
                break;
            }

            default:
            {
                string targetName = parts[1];
                ClientChatSession? targetSession = Context.ClientChatSessions.Values
                    .FirstOrDefault(s => string.Equals(s.Account.Name, targetName, StringComparison.OrdinalIgnoreCase));

                if (targetSession is not null && targetSession.Account.ID != session.Account.ID)
                {
                    if (session.AvoidedAccountIDs.Count >= 10)
                    {
                        SendNotificationWhisper(session, "MATCHMAKING", "You have reached the maximum limit of 10 avoided players. Use '/avoid clear' to reset.");
                        return;
                    }

                    session.AvoidedAccountIDs.Add(targetSession.Account.ID);
                    SendNotificationWhisper(session, "MATCHMAKING", $"Player '{targetSession.Account.Name}' added to your avoided teammates list.");
                }
                else
                {
                    SendNotificationWhisper(session, "MATCHMAKING", "Usage: /avoid <accountName> | /avoid add <accountName> | /avoid remove <accountName> | /avoid list | /avoid clear");
                }
                break;
            }
        }
    }

    private static void HandleForceGroupMatchupCommand(ClientChatSession session, string messageContent)
    {
        if (session.Account.Type is not AccountType.Staff)
        {
            SendNotificationWhisper(session, "STAFF", "You do not have permission to use this command.");
            return;
        }

        string[] parts = messageContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string? targetName = parts.Length > 1 ? parts[1] : null;

        MatchmakingGroup? targetGroup = null;

        if (targetName is not null)
        {
            ClientChatSession? targetSession = Context.ClientChatSessions.Values
                .FirstOrDefault(s => string.Equals(s.Account.Name, targetName, StringComparison.OrdinalIgnoreCase));

            if (targetSession is not null)
            {
                targetGroup = MatchmakingService.Groups.Values
                    .FirstOrDefault(g => g.Members.Any(m => m.Session.Account.ID == targetSession.Account.ID));
            }
        }
        else
        {
            targetGroup = MatchmakingService.Groups.Values
                .FirstOrDefault(g => g.Members.Any(m => m.Session.Account.ID == session.Account.ID));
        }

        if (targetGroup is null)
        {
            SendNotificationWhisper(session, "STAFF", "No active matchmaking group found for the specified player.");
            return;
        }

        targetGroup.ForceMatchup = true;
        SendNotificationWhisper(session, "STAFF", $"Force matchup enabled for group {targetGroup.GUID} ({targetGroup.Members.Count} players).");
    }

    private static void SendStreamerNotification(ClientChatSession session, string message)
    {
        SendNotificationWhisper(session, "KONGOR PROTECT", message);
    }

    private static void SendNotificationWhisper(ClientChatSession session, string sender, string message)
    {
        ChatBuffer notification = new ();
        notification.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER);
        notification.WriteString(sender);
        notification.WriteString(message);

        session.Send(notification);
    }
}

file class SendChannelMessageRequestData
{
    public byte[] CommandBytes { get; init; }

    public string Message { get; init; }

    public int ChannelID { get; init; }

    public SendChannelMessageRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Message = buffer.ReadString();
        ChannelID = buffer.ReadInt32();
    }
}
