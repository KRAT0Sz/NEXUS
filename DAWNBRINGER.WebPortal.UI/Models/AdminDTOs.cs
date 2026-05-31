namespace DAWNBRINGER.WebPortal.UI.Models;

public class AdminDashboardStatsResponse
{
    public int TotalUsers { get; set; }
    public int TotalAccounts { get; set; }
    public int TotalClans { get; set; }
    public int ActiveUsersToday { get; set; }
    public int TotalStaffAccounts { get; set; }
    public int TotalServerHostAccounts { get; set; }
}

public class AdminUserSummaryResponse
{
    public int ID { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int AccountCount { get; set; }
    public int GoldCoins { get; set; }
    public int SilverCoins { get; set; }
    public int PlinkoTickets { get; set; }
    public DateTimeOffset TimestampCreated { get; set; }
    public string? DiscordId { get; set; }
}

public class AdminAccountSummaryResponse
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string? ClanName { get; set; }
    public string? ClanTag { get; set; }
    public string ClanTier { get; set; } = string.Empty;
    public int AscensionLevel { get; set; }
    public DateTimeOffset TimestampCreated { get; set; }
    public DateTimeOffset TimestampLastActive { get; set; }
}

public class AdminClanSummaryResponse
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public string LeaderName { get; set; } = string.Empty;
    public DateTimeOffset TimestampCreated { get; set; }
}

public class AdminUserDetailResponse
{
    public int ID { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int GoldCoins { get; set; }
    public int SilverCoins { get; set; }
    public int PlinkoTickets { get; set; }
    public int TotalLevel { get; set; }
    public int TotalExperience { get; set; }
    public List<string> OwnedStoreItems { get; set; } = [];
    public string? DiscordId { get; set; }
    public DateTimeOffset? DiscordLinkedAt { get; set; }
    public DateTimeOffset TimestampCreated { get; set; }
    public DateTimeOffset TimestampLastActive { get; set; }
    public List<AdminAccountDetailResponse> Accounts { get; set; } = [];
}

public class AdminAccountDetailResponse
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public string? ClanName { get; set; }
    public string? ClanTag { get; set; }
    public string ClanTier { get; set; } = string.Empty;
    public int AscensionLevel { get; set; }
    public List<string> SelectedStoreItems { get; set; } = [];
    public DateTimeOffset TimestampCreated { get; set; }
    public DateTimeOffset TimestampLastActive { get; set; }
}

public class AdminClanDetailResponse
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public DateTimeOffset TimestampCreated { get; set; }
    public List<AdminClanMemberResponse> Members { get; set; } = [];
}

public class AdminClanMemberResponse
{
    public int AccountID { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string ClanTier { get; set; } = string.Empty;
    public int AscensionLevel { get; set; }
    public DateTimeOffset TimestampJoined { get; set; }
}

public class AdminPaginatedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
