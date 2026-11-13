namespace TRANSMUTANSTEIN.ChatServer.Configuration;

/// <summary>
///     Configuration options for the KONGOR PROTECT anti-toxicity and profanity filtering engine.
/// </summary>
public sealed class KongorProtectSettings
{
    public const string SectionName = "KongorProtect";

    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Action mode: "Censor" (masks offensive words with asterisks) or "Block" (drops the message entirely).
    /// </summary>
    public string Mode { get; set; } = "Censor";

    /// <summary>
    ///     The number of profanity infractions within the strike window before disciplinary actions (e.g. temporary silence) trigger.
    /// </summary>
    public int StrikeLimit { get; set; } = 3;

    /// <summary>
    ///     The duration in minutes for which strikes are retained before rolling off.
    /// </summary>
    public int StrikeWindowMinutes { get; set; } = 5;

    public List<string> BannedWords { get; set; } = [];

    public List<string> WhitelistWords { get; set; } = [];
}

