namespace ZORGATH.WebPortal.API.Contracts;

public sealed record QuestDTO
(
    int ID,
    string QuestKey,
    string Title,
    string Description,
    string Category,
    string TargetType,
    int TargetCount,
    int CurrentCount,
    int RewardSilver,
    int RewardGold,
    int RewardTickets,
    string? RewardUpgradeCode,
    bool IsCompleted,
    bool IsClaimed,
    DateTimeOffset? ClaimedAt,
    DateTimeOffset ResetAt,
    int ReputationReward = 0,
    string? TriggerName = null
);

public sealed record ClaimQuestRewardResponseDTO
(
    bool Success,
    string Message,
    int QuestID,
    int GrantedSilver,
    int GrantedGold,
    int GrantedTickets,
    string? GrantedUpgrade,
    int NewSilverBalance,
    int NewGoldBalance,
    int NewTicketBalance
);

public sealed record DailyCheckInStatusDTO
(
    int CurrentStreak,
    bool HasCheckedInToday,
    DateOnly? LastCheckInDate,
    IReadOnlyList<DailyCheckInRewardTierDTO> StreakRewards
);

public sealed record DailyCheckInRewardTierDTO
(
    int Day,
    int RewardSilver,
    int RewardGold,
    int RewardTickets,
    bool IsToday,
    bool IsClaimed
);

public sealed record ClaimCheckInResponseDTO
(
    bool Success,
    string Message,
    int DayClaimed,
    int NewStreak,
    int GrantedSilver,
    int GrantedGold,
    int GrantedTickets,
    int NewSilverBalance,
    int NewGoldBalance,
    int NewTicketBalance
);

public sealed record LuckySpinStatusDTO
(
    bool CanFreeSpin,
    int TicketCostPerSpin,
    int AvailableTickets,
    int TotalSpinsPlayed,
    IReadOnlyList<LuckySpinWheelSliceDTO> WheelSlices,
    DateTimeOffset? NextFreeSpinAvailableAt,
    int CooldownRemainingSeconds
);

public sealed record LuckySpinWheelSliceDTO
(
    int SliceIndex,
    string Label,
    string RewardType,
    int Amount,
    string? UpgradeCode,
    string ColorHex
);

public sealed record LuckySpinResultDTO
(
    bool Success,
    int WinningSliceIndex,
    string RewardLabel,
    string RewardType,
    int Amount,
    string? UpgradeCode,
    int RemainingTickets,
    int NewSilverBalance,
    int NewGoldBalance,
    int NewTicketBalance,
    string Message,
    DateTimeOffset? NextFreeSpinAvailableAt,
    int CooldownRemainingSeconds
);

public sealed record SimulateQuestProgressDTO
(
    string TargetType,
    int Increment = 1
);

public sealed record WarEffortStatusDTO
(
    int CurrentCycle,
    int CurrentBagNumber,
    int CurrentBagReputation,
    int TotalCycleReputation,
    int DailyResetsRemaining,
    DateTimeOffset CycleEndDate,
    IReadOnlyList<WarEffortBagDTO> Bags,
    IReadOnlyList<AccountCompletedBagDTO> CompletedBags
);

public sealed record WarEffortBagDTO
(
    int BagNumber,
    string Name,
    int RequiredReputation,
    string RewardType,
    int RewardSilver,
    int RewardGold,
    int RewardTickets,
    string? RewardProductName,
    bool IsFinalTier
);

public sealed record AccountCompletedBagDTO
(
    int BagNumber,
    bool IsOpened,
    DateTimeOffset CompletedAt,
    DateTimeOffset? OpenedAt,
    int GrantedSilver,
    int GrantedGold,
    int GrantedTickets,
    string? GrantedProductCode
);

public sealed record OpenWarEffortBagResponseDTO
(
    bool Success,
    string Message,
    int BagNumber,
    int GrantedSilver,
    int GrantedGold,
    int GrantedTickets,
    string? GrantedUpgrade,
    int NewSilverBalance,
    int NewGoldBalance,
    int NewTicketBalance
);
