using System.Text.RegularExpressions;

namespace ReviewAssistant.Core;

/// <summary>
/// Step 2. Replaces secrets and personal information with markers such as [REDACTED:secret]
/// before any prompt is built. It is pattern-based: it catches the common shapes, not every shape.
/// </summary>
public sealed class PatternRedactor : IRedactor
{
    // Kind: the word that goes in the marker. Pattern: what to find.
    // Replacement: what to put there; "$1" keeps the first bracketed part of the match (the key name).
    private sealed record Rule(string Kind, Regex Pattern, string Replacement);

    private const RegexOptions IgnoreCase = RegexOptions.IgnoreCase;

    // Order matters: the 13-digit ID rule runs before the 10-digit phone rule.
    private static readonly Rule[] Rules =
    [
        // Password=Winter2026!;  inside a connection string
        new("secret", new Regex(@"\b(password|pwd)=[^;""]+", IgnoreCase), "$1=[REDACTED:secret]"),

        // ApiKey = "abc123"   or   "token": "abc123"
        new("secret", new Regex(@"(\w*(?:password|secret|api_?key|token)\w*""?\s*[=:]\s*)""[^""]+""", IgnoreCase),
            "$1\"[REDACTED:secret]\""),

        // South African ID number: 13 digits
        new("id-number", new Regex(@"\b\d{13}\b"), "[REDACTED:id-number]"),

        new("email", new Regex(@"\b[\w.+-]+@[\w-]+(?:\.[\w-]+)+\b"), "[REDACTED:email]"),

        // 082 555 0142 written without spaces, or +27 82 555 0142 written without spaces
        new("phone", new Regex(@"(?<!\d)(?:\+27|0)\d{9}(?!\d)"), "[REDACTED:phone]"),

        // A latitude, longitude pair with four or more decimals: -26.2041, 28.0473
        new("location", new Regex(@"-?\d{1,2}\.\d{4,}\s*,\s*-?\d{1,3}\.\d{4,}"), "[REDACTED:location]"),
    ];

    public RedactedChangeSet Redact(ChangeSet changeSet)
    {
        var counts = new SortedDictionary<string, int>();

        var files = changeSet.Files
            .Select(file => file with { Diff = Clean(file.Diff, counts) })
            .ToList();

        var cleaned = changeSet with { Title = Clean(changeSet.Title, counts), Files = files };
        return new RedactedChangeSet(cleaned, counts);
    }

    private static string Clean(string text, IDictionary<string, int> counts)
    {
        foreach (var rule in Rules)
        {
            var found = rule.Pattern.Matches(text).Count;
            if (found == 0)
            {
                continue;
            }

            text = rule.Pattern.Replace(text, rule.Replacement);
            counts[rule.Kind] = counts.TryGetValue(rule.Kind, out var soFar) ? soFar + found : found;
        }

        return text;
    }
}
