namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     Represents a temporary matchmaking queue cooldown assessed against a user for leaving or abandoning a match.
/// </summary>
public class MatchmakingCooldown
{
    public required int UserID { get; init; }

    public required int AccountID { get; init; }

    public required string AccountName { get; init; }

    public required DateTimeOffset TimestampAssessed { get; init; }

    public required DateTimeOffset TimestampExpires { get; init; }

    public required int OffenceLevel { get; init; }

    public required string Reason { get; init; }

    [System.Text.Json.Serialization.JsonIgnore]
    public TimeSpan RemainingTime => TimestampExpires > DateTimeOffset.UtcNow
        ? TimestampExpires - DateTimeOffset.UtcNow
        : TimeSpan.Zero;

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsActive => RemainingTime > TimeSpan.Zero;
}

