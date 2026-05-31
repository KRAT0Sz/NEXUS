namespace ZORGATH.WebPortal.API.Contracts;

// Dashboard Statistics

public record GetDashboardStatsDTO(int TotalUsers, int TotalAccounts, int TotalClans, int ActiveUsersToday, int TotalStaffAccounts, int TotalServerHostAccounts);

// User Management

public record GetUserSummaryDTO(int ID, string EmailAddress, string Role, int AccountCount, int GoldCoins, int SilverCoins, int PlinkoTickets, DateTimeOffset TimestampCreated, string? DiscordId);

public record GetUserDetailDTO(
    int ID,
    string EmailAddress,
    string Role,
    int GoldCoins,
    int SilverCoins,
    int PlinkoTickets,
    int TotalLevel,
    int TotalExperience,
    List<string> OwnedStoreItems,
    string? DiscordId,
    DateTimeOffset? DiscordLinkedAt,
    DateTimeOffset TimestampCreated,
    DateTimeOffset TimestampLastActive,
    List<GetUserAccountDetailDTO> Accounts);

public record GetUserAccountDetailDTO(int ID, string Name, bool IsMain, string AccountType, string? ClanName, string? ClanTag, ClanTier ClanTier, int AscensionLevel, DateTimeOffset TimestampCreated);

public record UpdateUserStatusDTO(string Action);

public record UpdateUserRoleDTO(string Role);

// Account Management

public record GetAccountSummaryDTO(int ID, string Name, string AccountType, string EmailAddress, string? ClanName, string? ClanTag, string ClanTier, int AscensionLevel, DateTimeOffset TimestampCreated, DateTimeOffset TimestampLastActive);

public record GetAccountDetailDTO(
    int ID,
    string Name,
    string AccountType,
    string EmailAddress,
    bool IsMain,
    string? ClanName,
    string? ClanTag,
    ClanTier ClanTier,
    int AscensionLevel,
    List<string> SelectedStoreItems,
    DateTimeOffset TimestampCreated,
    DateTimeOffset TimestampLastActive);

public record UpdateAccountTypeDTO(string AccountType);

public record UpdateAccountClanTierDTO(string ClanTier);

public record RemoveAccountFromClanDTO(int AccountID);

// Clan Management

public record GetClanSummaryDTO(int ID, string Name, string Tag, int MemberCount, string LeaderName, DateTimeOffset TimestampCreated);

public record GetClanDetailDTO(
    int ID,
    string Name,
    string Tag,
    DateTimeOffset TimestampCreated,
    List<GetClanMemberDetailDTO> Members);

public record GetClanMemberDetailDTO(int AccountID, string AccountName, string AccountType, ClanTier ClanTier, int AscensionLevel, DateTimeOffset TimestampJoined);

public record UpdateClanMemberRankDTO(string ClanTier);

public record CreateClanDTO(string Name, string Tag);

// Pagination

public record PaginatedResultDTO<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public record PaginationQueryDTO(int Page = 1, int PageSize = 20)
{
    public int Skip => (Page - 1) * PageSize;
}
