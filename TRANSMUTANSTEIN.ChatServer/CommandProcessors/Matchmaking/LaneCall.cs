namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

/// <summary>
///     Handles hero pick / lobby lane calls (Top, Mid, Bot, Jungle, Short Carry, Support, Off-lane) for matchmaking teams.
/// </summary>
public static class LaneCallHandler
{
    public static string NormalisePosition(string command)
    {
        string rawPosition = command.Trim();

        if (rawPosition.StartsWith("/lane ", StringComparison.OrdinalIgnoreCase))
        {
            rawPosition = rawPosition[6..].Trim();
        }
        else if (rawPosition.StartsWith("/l ", StringComparison.OrdinalIgnoreCase))
        {
            rawPosition = rawPosition[3..].Trim();
        }

        string lower = rawPosition.ToLowerInvariant();

        return lower switch
        {
            "short" or "farm" or "short farm" or "carry"     => "Short Carry",
            "support" or "sup" or "short sup" or "sup short" => "Short Support",
            "mid" or "middle"                                => "Middle",
            "long" or "sui" or "offlane" or "off-lane"       => "Off-Lane",
            "jungle" or "jg"                                 => "Jungle",
            _                                                => rawPosition
        };
    }
}

