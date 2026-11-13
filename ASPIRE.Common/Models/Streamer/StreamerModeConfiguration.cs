namespace ASPIRE.Common.Models.Streamer;

public class StreamerModeConfiguration
{
    public bool Enabled { get; set; } = true;

    public string MaskedName { get; set; } = GenerateGuestName();

    public bool HideMMR { get; set; } = true;

    public bool HideStats { get; set; } = true;

    public bool HideServerAddress { get; set; } = true;

    public bool BlockSpectate { get; set; } = true;

    public bool BlockGameInfo { get; set; } = true;

    public bool FriendsOnlyWhisper { get; set; } = true;

    public DateTimeOffset TimestampActivated { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Generates a pseudorandom guest identifier conforming to the standard guest naming scheme (GUEST-01 to GUEST-99).
    /// </summary>
    public static string GenerateGuestName()
    {
        int guestNumber = Random.Shared.Next(1, 100);

        return $"GUEST-{guestNumber:D2}";
    }
}

