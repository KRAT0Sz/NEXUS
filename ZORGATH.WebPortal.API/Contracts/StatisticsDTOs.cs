namespace ZORGATH.WebPortal.API.Contracts;

public sealed record ServerStatsDTO
(
    int TotalPlayers,
    int TotalUsers,
    int TotalClans
);

public sealed record LocationPointDTO
(
    float Latitude,
    float Longitude,
    string CountryName,
    int PlayerCount
);

public sealed record LeaderboardEntryDTO
(
    int Rank,
    string AccountName,
    string ClanTag,
    double Rating,
    int Wins,
    int Losses
);

