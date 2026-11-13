using System.Text.Json;
using KONGOR.MasterServer.Extensions.Cache;
using KONGOR.MasterServer.Models.ServerManagement;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Services;

/// <summary>
///     Verifies the escalating cooldown calculations, lifecycle state, and serialisation of matchmaking leaver penalties.
/// </summary>
public sealed class LeaverPenaltyServiceTests
{
    [Test]
    public async Task CalculateCooldownDuration_Escalates_Accurately()
    {
        TimeSpan firstOffence = DistributedCacheExtensions.CalculateCooldownDuration(1);
        TimeSpan secondOffence = DistributedCacheExtensions.CalculateCooldownDuration(2);
        TimeSpan thirdOffence = DistributedCacheExtensions.CalculateCooldownDuration(3);
        TimeSpan fourthOffence = DistributedCacheExtensions.CalculateCooldownDuration(4);
        TimeSpan repeatedOffence = DistributedCacheExtensions.CalculateCooldownDuration(10);

        using (Assert.Multiple())
        {
            await Assert.That(firstOffence).IsEqualTo(TimeSpan.FromMinutes(15));
            await Assert.That(secondOffence).IsEqualTo(TimeSpan.FromMinutes(30));
            await Assert.That(thirdOffence).IsEqualTo(TimeSpan.FromMinutes(60));
            await Assert.That(fourthOffence).IsEqualTo(TimeSpan.FromMinutes(120));
            await Assert.That(repeatedOffence).IsEqualTo(TimeSpan.FromMinutes(120));
        }
    }

    [Test]
    public async Task MatchmakingCooldown_Active_When_Expiry_Is_In_Future()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset expires = now.AddMinutes(30);

        MatchmakingCooldown cooldown = new ()
        {
            UserID = 101,
            AccountID = 202,
            AccountName = "LeaverPlayer",
            TimestampAssessed = now,
            TimestampExpires = expires,
            OffenceLevel = 2,
            Reason = "Abandoned Match"
        };

        using (Assert.Multiple())
        {
            await Assert.That(cooldown.IsActive).IsTrue();
            await Assert.That(cooldown.RemainingTime).IsGreaterThan(TimeSpan.Zero);
            await Assert.That(cooldown.RemainingTime.TotalMinutes).IsLessThanOrEqualTo(30);
        }
    }

    [Test]
    public async Task MatchmakingCooldown_Inactive_When_Expiry_Is_In_Past()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset expires = now.AddMinutes(-5);

        MatchmakingCooldown cooldown = new ()
        {
            UserID = 101,
            AccountID = 202,
            AccountName = "ReformedPlayer",
            TimestampAssessed = now.AddMinutes(-35),
            TimestampExpires = expires,
            OffenceLevel = 1,
            Reason = "Disconnected"
        };

        using (Assert.Multiple())
        {
            await Assert.That(cooldown.IsActive).IsFalse();
            await Assert.That(cooldown.RemainingTime).IsEqualTo(TimeSpan.Zero);
        }
    }

    [Test]
    public async Task MatchmakingCooldown_Serialises_And_Deserialises_Accurately()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset expires = now.AddMinutes(15);

        MatchmakingCooldown original = new ()
        {
            UserID = 303,
            AccountID = 404,
            AccountName = "SerialisePlayer",
            TimestampAssessed = now,
            TimestampExpires = expires,
            OffenceLevel = 1,
            Reason = "Intentional disconnect"
        };

        string json = JsonSerializer.Serialize(original);
        MatchmakingCooldown? deserialised = JsonSerializer.Deserialize<MatchmakingCooldown>(json);

        using (Assert.Multiple())
        {
            await Assert.That(deserialised).IsNotNull();
            await Assert.That(deserialised!.UserID).IsEqualTo(original.UserID);
            await Assert.That(deserialised.AccountID).IsEqualTo(original.AccountID);
            await Assert.That(deserialised.AccountName).IsEqualTo(original.AccountName);
            await Assert.That(deserialised.OffenceLevel).IsEqualTo(original.OffenceLevel);
            await Assert.That(deserialised.Reason).IsEqualTo(original.Reason);
            await Assert.That(deserialised.IsActive).IsTrue();
        }
    }
}

