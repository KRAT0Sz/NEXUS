namespace ZORGATH.WebPortal.API.Contracts;

public sealed record PromoteDemoteUserDTO
(
    string UserName,
    string Role
);

public sealed record IssueCurrencyDTO
(
    string AccountName,
    int Amount
);

public sealed record AccountCollisionDTO
(
    string AccountName,
    List<string> MatchingAccountNames,
    string CollisionReason
);

public sealed record SuspensionIssueDTO
(
    string OffenderAccountName,
    string Duration,
    string Reason,
    string InternalDetails,
    int SuspendByMask = 11
);

public sealed record SuspensionDTO
(
    int ID,
    string OffenderAccountName,
    string IssuerAccountName,
    DateTimeOffset TimestampStart,
    DateTimeOffset TimestampEnd,
    string Reason,
    float Multiplier,
    bool Override
);

public sealed record StaffActionLogDTO
(
    int ID,
    Guid SourceID,
    string TargetID,
    string Action,
    string Details,
    DateTimeOffset Timestamp
);

public sealed record IssueRedeemableCodesDTO
(
    int? SilverCoins,
    int? GoldCoins,
    int? PlinkoTickets,
    int? ProductID,
    int Amount
);

