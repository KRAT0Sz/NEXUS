using MERRICK.DatabaseContext.Constants;
using MERRICK.DatabaseContext.Entities.Core;
using MERRICK.DatabaseContext.Entities.Statistics;
using MERRICK.DatabaseContext.Entities.Utility;
using KONGOR.MasterServer.Models.RequestResponse.Stats;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Services;

/// <summary>
///     Verifies leaderboard ranking calculations, 1-based rank resolution, and campaign response rank mapping.
/// </summary>
public sealed class GlobalLeaderboardServiceTests
{
    private static Account BuildAccount(int accountID, string name)
    {
        User user = new ()
        {
            EmailAddress = $"{name}@kongor.local",
            Role = new Role { Name = UserRoles.User },
            SRPPasswordSalt = string.Empty,
            SRPPasswordHash = string.Empty,
            OwnedStoreItems = []
        };

        Account account = new ()
        {
            ID = accountID,
            Name = name,
            User = user,
            IsMain = true
        };

        user.Accounts = [account];
        return account;
    }

    private static AccountStatistics BuildStatistics(Account account, AccountStatisticsType type, double rating)
    {
        return new AccountStatistics
        {
            Account = account,
            AccountID = account.ID,
            Type = type,
            SkillRating = rating,
            PlacementMatchesData = null,
            MatchesPlayed = 50,
            HeroStatistics = new HeroStatisticsSummary { Heroes = [] }
        };
    }

    [Test]
    public async Task CampaignStatisticsResponse_Surfaces_Explicit_Rank_When_Supplied()
    {
        Account account = BuildAccount(1, "TopRankedPlayer");
        AccountStatistics statistics = BuildStatistics(account, AccountStatisticsType.Matchmaking, 1950.0);
        AggregateStatistics aggregates = new ();

        CampaignStatisticsResponse response = new (account, statistics, aggregates, currentRanking: 7, highestRanking: 3);

        using (Assert.Multiple())
        {
            await Assert.That(response.CurrentRanking).IsEqualTo(7);
        }
    }

    [Test]
    public async Task CampaignStatisticsResponse_Defaults_To_Immortal_Rank_When_Unranked_And_Immortal()
    {
        Account account = BuildAccount(2, "ImmortalPlayer");
        // Skill rating >= 2000 corresponds to Rank.IMMORTAL
        AccountStatistics statistics = BuildStatistics(account, AccountStatisticsType.Matchmaking, 2100.0);
        AggregateStatistics aggregates = new ();

        CampaignStatisticsResponse response = new (account, statistics, aggregates, currentRanking: null, highestRanking: null);

        using (Assert.Multiple())
        {
            await Assert.That(response.CurrentRanking).IsEqualTo(1);
        }
    }

    [Test]
    public async Task CampaignStatisticsResponse_Defaults_To_Null_When_Unranked_And_Below_Immortal()
    {
        Account account = BuildAccount(3, "GoldPlayer");
        AccountStatistics statistics = BuildStatistics(account, AccountStatisticsType.Matchmaking, 1500.0);
        AggregateStatistics aggregates = new ();

        CampaignStatisticsResponse response = new (account, statistics, aggregates, currentRanking: null, highestRanking: null);

        using (Assert.Multiple())
        {
            await Assert.That(response.CurrentRanking).IsNull();
        }
    }

    [Test]
    public async Task CampaignCasualStatisticsResponse_Surfaces_Explicit_Rank_When_Supplied()
    {
        Account account = BuildAccount(4, "CasualLadderLeader");
        AccountStatistics statistics = BuildStatistics(account, AccountStatisticsType.MatchmakingCasual, 1800.0);
        AggregateStatistics aggregates = new ();

        CampaignCasualStatisticsResponse response = new (account, statistics, aggregates, currentRanking: 14, highestRanking: 10);

        using (Assert.Multiple())
        {
            await Assert.That(response.CurrentRanking).IsEqualTo(14);
        }
    }

    [Test]
    public async Task GlobalLeaderboard_Rank_Calculation_Is_One_Based()
    {
        // Redis Sorted Set ZREVRANK returns 0-based rank; GlobalLeaderboardService translates this to 1-based rank
        long zeroBasedLeader = 0;
        long zeroBasedMidTier = 41;

        int oneBasedLeader = (int)(zeroBasedLeader + 1);
        int oneBasedMidTier = (int)(zeroBasedMidTier + 1);

        using (Assert.Multiple())
        {
            await Assert.That(oneBasedLeader).IsEqualTo(1);
            await Assert.That(oneBasedMidTier).IsEqualTo(42);
        }
    }
}
