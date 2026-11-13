using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Represents the outcome of a chat message inspection by KONGOR PROTECT.
/// </summary>
public sealed record KongorProtectResult
{
    public bool IsClean => Violations.Count == 0;

    public bool IsBlocked { get; init; }

    public required string SanitisedMessage { get; init; }

    public List<string> Violations { get; init; } = [];
}

/// <summary>
///     Core anti-toxicity and profanity protection engine for in-game chat, whispers, and lobby communication.
/// </summary>
public sealed class KongorProtectService
{
    private readonly KongorProtectSettings Settings;
    private readonly ILogger<KongorProtectService> Logger;
    private readonly HashSet<string> BannedWords = new (StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> WhitelistWords = new (StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<int, List<DateTimeOffset>> InfractionHistory = new ();

    private static readonly Dictionary<char, char> LeetspeakSubstitutions = new ()
    {
        ['@'] = 'a',
        ['$'] = 's',
        ['0'] = 'o',
        ['1'] = 'i',
        ['!'] = 'i',
        ['3'] = 'e',
        ['5'] = 's',
        ['7'] = 't',
        ['+'] = 't'
    };

    public KongorProtectService(IOptions<KongorProtectSettings> options, ILogger<KongorProtectService> logger, IWebHostEnvironment environment)
    {
        Settings = options.Value;
        Logger = logger;

        LoadWords(environment);
    }

    private void LoadWords(IWebHostEnvironment environment)
    {
        foreach (string word in Settings.BannedWords)
        {
            BannedWords.Add(word.Trim().ToLowerInvariant());
        }

        foreach (string word in Settings.WhitelistWords)
        {
            WhitelistWords.Add(word.Trim().ToLowerInvariant());
        }

        // If Configuration in appsettings was empty, attempt loading from KongorProtectConfiguration.json
        if (BannedWords.Count == 0)
        {
            try
            {
                string configPath = Path.Combine(environment.ContentRootPath, "Configuration", "KongorProtectConfiguration.json");
                if (File.Exists(configPath))
                {
                    string jsonContent = File.ReadAllText(configPath);
                    using JsonDocument document = JsonDocument.Parse(jsonContent);

                    if (document.RootElement.TryGetProperty("BannedWords", out JsonElement bannedArray))
                    {
                        foreach (JsonElement element in bannedArray.EnumerateArray())
                        {
                            string? word = element.GetString();
                            if (string.IsNullOrWhiteSpace(word) is false)
                            {
                                BannedWords.Add(word.Trim().ToLowerInvariant());
                            }
                        }
                    }

                    if (document.RootElement.TryGetProperty("WhitelistWords", out JsonElement whitelistArray))
                    {
                        foreach (JsonElement element in whitelistArray.EnumerateArray())
                        {
                            string? word = element.GetString();
                            if (string.IsNullOrWhiteSpace(word) is false)
                            {
                                WhitelistWords.Add(word.Trim().ToLowerInvariant());
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogWarning(exception, "Failed to load KongorProtectConfiguration.json");
            }
        }

        Logger.LogInformation("KONGOR PROTECT Initialised With {BannedCount} Banned Words And {WhitelistCount} Whitelisted Words",
            BannedWords.Count, WhitelistWords.Count);
    }

    /// <summary>
    ///     Inspects a message for offensive content and returns the sanitised result.
    /// </summary>
    public KongorProtectResult Inspect(string rawMessage, int accountID = 0)
    {
        if (Settings.Enabled.Equals(false) || string.IsNullOrWhiteSpace(rawMessage))
        {
            return new KongorProtectResult
            {
                SanitisedMessage = rawMessage,
                IsBlocked = false
            };
        }

        List<string> violations = [];
        string sanitised = rawMessage;

        foreach (string bannedWord in BannedWords)
        {
            if (string.IsNullOrWhiteSpace(bannedWord))
                continue;

            // Check for match in raw and de-obfuscated forms
            bool isAscii = bannedWord.All(character => character < 128);

            Regex regex;
            if (isAscii)
            {
                // Word boundary for English words to prevent Scunthorpe false positives
                regex = new Regex($@"\b{Regex.Escape(bannedWord)}\b", RegexOptions.IgnoreCase);
            }
            else
            {
                // Direct sequence for Non-ASCII / Thai scripts
                regex = new Regex(Regex.Escape(bannedWord), RegexOptions.IgnoreCase);
            }

            MatchCollection matches = regex.Matches(sanitised);
            if (matches.Count > 0)
            {
                bool anyViolated = false;
                foreach (Match match in matches)
                {
                    // Check if match falls within a whitelisted word
                    if (IsMatchWhitelisted(sanitised, match.Index, match.Length))
                        continue;

                    anyViolated = true;
                    string replacement = new ('*', match.Length);
                    sanitised = sanitised.Remove(match.Index, match.Length).Insert(match.Index, replacement);
                }

                if (anyViolated)
                {
                    violations.Add(bannedWord);
                }
            }
            else
            {
                // Check de-obfuscated variant (leetspeak, spaced characters)
                string deobfuscated = Deobfuscate(rawMessage);
                if (deobfuscated.Contains(bannedWord, StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add(bannedWord);
                    sanitised = MaskObfuscatedMatch(sanitised, bannedWord);
                }
            }
        }

        bool isBlocked = Settings.Mode.Equals("Block", StringComparison.OrdinalIgnoreCase) && violations.Count > 0;

        if (violations.Count > 0 && accountID > 0)
        {
            RecordInfraction(accountID);
        }

        return new KongorProtectResult
        {
            SanitisedMessage = sanitised,
            Violations = violations,
            IsBlocked = isBlocked
        };
    }

    /// <summary>
    ///     Normalises leetspeak, punctuation separators, and repeated letters to detect evasion attempts.
    /// </summary>
    public static string Deobfuscate(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        StringBuilder builder = new ();

        foreach (char character in input.ToLowerInvariant())
        {
            if (LeetspeakSubstitutions.TryGetValue(character, out char substituted))
            {
                builder.Append(substituted);
            }
            else if (char.IsLetterOrDigit(character) || character >= 0x0E00) // Keep letters, digits, and Thai Unicode block
            {
                builder.Append(character);
            }
        }

        string intermediate = builder.ToString();

        // Collapse repeated characters (e.g. "fuuuuck" -> "fuck")
        StringBuilder collapsed = new ();
        char previousChar = '\0';
        int repeatCount = 0;

        foreach (char character in intermediate)
        {
            if (character == previousChar)
            {
                repeatCount++;
                if (repeatCount < 2) // Allow at most 2 repeats (e.g. "good", "pass")
                {
                    collapsed.Append(character);
                }
            }
            else
            {
                previousChar = character;
                repeatCount = 1;
                collapsed.Append(character);
            }
        }

        return collapsed.ToString();
    }

    private bool IsMatchWhitelisted(string text, int matchIndex, int matchLength)
    {
        foreach (string whitelist in WhitelistWords)
        {
            int foundIndex = text.IndexOf(whitelist, StringComparison.OrdinalIgnoreCase);
            while (foundIndex >= 0)
            {
                int foundEnd = foundIndex + whitelist.Length;
                int matchEnd = matchIndex + matchLength;

                if (matchIndex >= foundIndex && matchEnd <= foundEnd)
                {
                    return true;
                }

                foundIndex = text.IndexOf(whitelist, foundIndex + 1, StringComparison.OrdinalIgnoreCase);
            }
        }

        return false;
    }

    private static string MaskObfuscatedMatch(string text, string bannedWord)
    {
        // Simple mask for obfuscated words
        string pattern = string.Join(@"[\s\W_]*", bannedWord.Select(c => $"[{c}{GetLeetspeakVariantPattern(c)}]"));
        return Regex.Replace(text, pattern, new string('*', bannedWord.Length), RegexOptions.IgnoreCase);
    }

    private static string GetLeetspeakVariantPattern(char character)
    {
        return character switch
        {
            'a' => "@4",
            's' => "$5",
            'o' => "0",
            'i' => "!1|",
            'e' => "3",
            't' => "+7",
            _   => string.Empty
        };
    }

    public void RecordInfraction(int accountID)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        TimeSpan window = TimeSpan.FromMinutes(Settings.StrikeWindowMinutes);

        InfractionHistory.AddOrUpdate
        (
            accountID,
            _ => [now],
            (_, existing) =>
            {
                lock (existing)
                {
                    existing.RemoveAll(timestamp => now - timestamp > window);
                    existing.Add(now);
                    return existing;
                }
            }
        );
    }

    public bool HasExceededStrikeLimit(int accountID)
    {
        if (InfractionHistory.TryGetValue(accountID, out List<DateTimeOffset>? history))
        {
            lock (history)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                TimeSpan window = TimeSpan.FromMinutes(Settings.StrikeWindowMinutes);
                history.RemoveAll(timestamp => now - timestamp > window);
                return history.Count >= Settings.StrikeLimit;
            }
        }

        return false;
    }
}
