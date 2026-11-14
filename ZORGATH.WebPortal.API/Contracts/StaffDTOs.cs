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

public sealed record SuspensionIssueDTO
(
    string OffenderAccountName,
    string Duration,
    string Reason,
    string InternalDetails,
    int SuspendByMask = 1
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

public sealed record ChangeSuspensionDurationDTO
(
    string Duration
);

public sealed record AccountActionDTO
(
    string AccountName
);

public sealed record LookupQueryDTO
(
    string Query
);

public sealed record ConnectedAccountUserResultDTO
(
    string Origin,
    string Name,
    bool Main,
    string Type,
    string NameWithClanTag,
    string MaskedEmail,
    string UserRole
);

public sealed record StaffRosterMemberDTO
(
    int ID,
    string Name,
    string Email,
    string Role
);

public sealed record AccountTypeResponseDTO
(
    string AccountName,
    string AccountType,
    int TypeId
);


