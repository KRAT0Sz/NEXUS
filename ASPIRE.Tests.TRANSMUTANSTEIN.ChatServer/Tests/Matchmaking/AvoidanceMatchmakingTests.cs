using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Utilities.Matchmaking;
using MERRICK.DatabaseContext.Entities.Statistics;
using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Verifies player avoidance filtering, staff forced matchup bypass, and tournament mode configuration within matchmaking.
/// </summary>
public sealed class AvoidanceMatchmakingTests
{
    [Test]
    public async Task HasAvoidanceConflict_Returns_False_When_No_Avoidance_Configured()
    {
        MatchmakingGroup groupA = MatchmakingTestBuilder.BuildSoloGroup(1500.0);
        MatchmakingGroup groupB = MatchmakingTestBuilder.BuildSoloGroup(1500.0);

        bool hasConflict = MatchmakingAlgorithm.HasAvoidanceConflict([groupA], groupB);

        await Assert.That(hasConflict).IsFalse();
    }

    [Test]
    public async Task HasAvoidanceConflict_Returns_True_When_Candidate_Is_In_Team_Member_Avoid_List()
    {
        MatchmakingGroup groupA = MatchmakingTestBuilder.BuildSoloGroup(1500.0);
        MatchmakingGroup groupB = MatchmakingTestBuilder.BuildSoloGroup(1500.0);

        int candidateAccountID = groupB.Members[0].Account.ID;
        groupA.Members[0].Session.AvoidedAccountIDs = [candidateAccountID];

        bool hasConflict = MatchmakingAlgorithm.HasAvoidanceConflict([groupA], groupB);

        await Assert.That(hasConflict).IsTrue();
    }

    [Test]
    public async Task HasAvoidanceConflict_Returns_True_When_Team_Member_Is_In_Candidate_Avoid_List()
    {
        MatchmakingGroup groupA = MatchmakingTestBuilder.BuildSoloGroup(1500.0);
        MatchmakingGroup groupB = MatchmakingTestBuilder.BuildSoloGroup(1500.0);

        int teamMemberAccountID = groupA.Members[0].Account.ID;
        groupB.Members[0].Session.AvoidedAccountIDs = [teamMemberAccountID];

        bool hasConflict = MatchmakingAlgorithm.HasAvoidanceConflict([groupA], groupB);

        await Assert.That(hasConflict).IsTrue();
    }

    [Test]
    public async Task HasAvoidanceConflict_Handles_Null_Sessions_And_Accounts_Gracefully()
    {
        MatchmakingGroup groupA = MatchmakingTestBuilder.BuildSoloGroup(1500.0);
        MatchmakingGroup groupB = MatchmakingTestBuilder.BuildSoloGroup(1500.0);

        // Ensure Uninitialised Session With Null AvoidedAccountIDs Does Not Throw
        groupA.Members[0].Session.AvoidedAccountIDs = null!;
        groupB.Members[0].Session.AvoidedAccountIDs = null!;

        bool hasConflict = MatchmakingAlgorithm.HasAvoidanceConflict([groupA], groupB);

        await Assert.That(hasConflict).IsFalse();
    }

    [Test]
    public async Task Avoided_Players_Are_Never_Placed_On_The_Same_Team()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingGroup player1 = MatchmakingTestBuilder.BuildSoloGroup(1500.0, queuedMinutesAgo: 5);
        MatchmakingGroup player2 = MatchmakingTestBuilder.BuildSoloGroup(1500.0, queuedMinutesAgo: 4);

        // Player 1 Avoids Player 2
        player1.Members[0].Session.AvoidedAccountIDs = [player2.Members[0].Account.ID];

        // 8 Other Neutral Players To Form Two 5v5 Teams
        List<MatchmakingGroup> remainingPlayers = [];
        for (int index = 0; index < 8; index++)
        {
            remainingPlayers.Add(MatchmakingTestBuilder.BuildSoloGroup(1500.0, queuedMinutesAgo: 3));
        }

        List<MatchmakingGroup> allGroups = [player1, player2, .. remainingPlayers];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(allGroups, settings);

        await Assert.That(matches.Count).IsEqualTo(1);

        MatchmakingMatch match = matches[0];

        List<int> legionAccountIDs = [.. match.LegionTeam.GetAllMembers().Select(m => m.Account.ID)];
        List<int> hellbourneAccountIDs = [.. match.HellbourneTeam!.GetAllMembers().Select(m => m.Account.ID)];

        int player1AccountID = player1.Members[0].Account.ID;
        int player2AccountID = player2.Members[0].Account.ID;

        // Player 1 And Player 2 Must Not Be On The Same Team
        bool bothOnLegion = legionAccountIDs.Contains(player1AccountID) && legionAccountIDs.Contains(player2AccountID);
        bool bothOnHellbourne = hellbourneAccountIDs.Contains(player1AccountID) && hellbourneAccountIDs.Contains(player2AccountID);

        using (Assert.Multiple())
        {
            await Assert.That(bothOnLegion).IsFalse();
            await Assert.That(bothOnHellbourne).IsFalse();
        }
    }

    [Test]
    public async Task ForceMatchup_Bypasses_TMR_Spread_Constraint()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        // 2000 TMR Spread Would Normally Never Match In Standard Pool
        MatchmakingGroup highTMRGroup = MatchmakingTestBuilder.BuildGroup([3000.0, 3000.0, 3000.0, 3000.0, 3000.0]);
        MatchmakingGroup lowTMRGroup  = MatchmakingTestBuilder.BuildGroup([1000.0, 1000.0, 1000.0, 1000.0, 1000.0]);

        // Without ForceMatchup, They Do Not Match
        IReadOnlyList<MatchmakingMatch> normalMatches = MatchmakingAlgorithm.RunMatchBrokerCycle([highTMRGroup, lowTMRGroup], settings);
        await Assert.That(normalMatches.Count).IsEqualTo(0);

        // With ForceMatchup Set On The Queueing Group, Match Broker Matches Them
        highTMRGroup.ForceMatchup = true;
        IReadOnlyList<MatchmakingMatch> forcedMatches = MatchmakingAlgorithm.RunMatchBrokerCycle([highTMRGroup, lowTMRGroup], settings);

        await Assert.That(forcedMatches.Count).IsEqualTo(1);
    }

    [Test]
    public async Task GrimmsCrossing_Configures_Three_Versus_Three_And_Tournament_Rating()
    {
        MatchmakingGroupInformation information = MatchmakingTestBuilder.Information();
        information.MapName = "grimms_crossing";

        using (Assert.Multiple())
        {
            await Assert.That(information.TeamSize).IsEqualTo((byte) 3);
            await Assert.That(information.StatisticsType).IsEqualTo(AccountStatisticsType.Tournament);
        }
    }
}
