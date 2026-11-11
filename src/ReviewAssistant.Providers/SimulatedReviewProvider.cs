using System.Text.Json;
using System.Text.RegularExpressions;
using ReviewAssistant.Core;

namespace ReviewAssistant.Providers;

/// <summary>
/// The default provider. It calls no model and no network: it applies a short list of
/// pattern rules to the added lines and replies in the same JSON a real model is asked for.
/// That keeps the rest of the pipeline (parse, render) identical for every provider.
/// </summary>
public sealed class SimulatedReviewProvider : IReviewProvider
{
    public string Name => "simulated (pattern rules, no model was called)";

    // One rule: when Pattern matches an added line, report Message under Section.
    private sealed record Rule(string Pattern, string Section, string Severity, string Confidence, string Category, string Message);

    // What the provider reports. The property names are the JSON field names of the reply.
    private sealed record Item(string Message, string? Severity, string? Confidence, string? Category, string? File, int? Line);

    private const string Suggestion = "suggestion";
    private const string Risk = "risk";

    private static readonly Rule[] Rules =
    [
        new(@"\[REDACTED:secret\]", Risk, "high", "high", "security",
            "A secret is hard-coded in this change. Move it to the secret store and rotate the value."),
        new(@"\[REDACTED:(id-number|email|phone|location)\]", Risk, "high", "medium", "privacy",
            "Personal information is written into the source. Remove it and check how it got here."),
        new(@"Log\w*\(.*\{(IdNumber|Email|Phone|Latitude|Longitude)", Risk, "high", "medium", "privacy",
            "This log line writes identity or location data. Logs are kept and shared more widely than the database."),
        new(@"""\s*(SELECT|INSERT|UPDATE|DELETE)\b[^""]*""\s*\+", Risk, "high", "medium", "security",
            "SQL is built by joining strings. Use a parameterised query."),
        new(@"\.Result\b|\.Wait\(\)", Risk, "medium", "medium", "correctness",
            "Blocking on an async call can deadlock and ties up a thread. Make the method async and await it."),
        new(@"^[^""/]*[\w)\]]\s*/\s*[A-Za-z_(]", Suggestion, "medium", "low", "correctness",
            "Division by a value that may be zero. Guard the zero case and add a test for it."),
        new(@"DateTime\.Now\b", Suggestion, "low", "high", "correctness",
            "DateTime.Now depends on the server's time zone. Use DateTime.UtcNow or an injected TimeProvider."),
        new(@"\b(TODO|HACK|FIXME)\b", Suggestion, "low", "high", "maintainability",
            "A TODO is being merged. Finish it or log a ticket and reference it here."),
    ];

    // "public TripSummary GetSummary(" gives the name GetSummary.
    private static readonly Regex PublicMember = new(@"\bpublic\b[^=(;]*?\b(\w+)\s*\(");

    public Task<string> GenerateAsync(ReviewRequest request, CancellationToken cancellationToken)
    {
        var changeSet = request.Input.ChangeSet;
        var summary = new List<Item>();
        var suggestions = new List<Item>();
        var risks = new List<Item>();

        if (changeSet.Title.Length > 0)
        {
            summary.Add(new Item($"Stated intent: {changeSet.Title}", null, null, null, null, null));
        }

        foreach (var file in changeSet.Files)
        {
            var lines = DiffLines.Parse(file.Diff);
            var added = lines.Where(line => line.Kind == DiffLineKind.Added).ToList();
            var removed = lines.Where(line => line.Kind == DiffLineKind.Removed).ToList();

            DescribePublicMembers(file, added, removed, summary, risks);

            // Each rule reports once per file, at the first added line it matches.
            foreach (var rule in Rules)
            {
                var hit = added.FirstOrDefault(line => Regex.IsMatch(line.Text, rule.Pattern));
                if (hit is not null)
                {
                    var target = rule.Section == Risk ? risks : suggestions;
                    target.Add(new Item(rule.Message, rule.Severity, rule.Confidence, rule.Category, file.Path, hit.Number));
                }
            }
        }

        if (!changeSet.Files.Any(file => file.Path.Contains("Test", StringComparison.OrdinalIgnoreCase)))
        {
            suggestions.Add(new Item(
                "No test file is part of this change. Add tests for the behaviour that changed.",
                "medium", "high", "tests", null, null));
        }

        var reply = new { summary, suggestions, risks };
        var json = JsonSerializer.Serialize(reply, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return Task.FromResult(json);
    }

    /// <summary>
    /// Compares the public members on removed lines with those on added lines.
    /// New names go in the summary. Changed or removed names are a risk for existing callers.
    /// </summary>
    private static void DescribePublicMembers(
        ChangedFile file, List<DiffLine> added, List<DiffLine> removed, List<Item> summary, List<Item> risks)
    {
        var addedMembers = PublicMembersIn(added);
        var removedMembers = PublicMembersIn(removed);

        var newNames = addedMembers.Keys.Except(removedMembers.Keys).ToList();
        if (newNames.Count > 0)
        {
            var first = addedMembers[newNames[0]];
            summary.Add(new Item($"Adds {Quote(newNames)}.", null, null, null, file.Path, first));
        }

        foreach (var name in removedMembers.Keys)
        {
            var stillThere = addedMembers.TryGetValue(name, out var line);
            summary.Add(new Item(
                stillThere ? $"Changes the public signature of `{name}`." : $"Removes the public member `{name}`.",
                null, null, null, file.Path, stillThere ? line : removedMembers[name]));

            risks.Add(new Item(
                $"Existing callers of `{name}` may rely on the previous signature or behaviour. Check the callers and tell the teams that use it.",
                "medium", "medium", "compatibility", file.Path, stillThere ? line : removedMembers[name]));
        }

        if (newNames.Count == 0 && removedMembers.Count == 0)
        {
            summary.Add(new Item($"Changes {added.Count} lines and removes {removed.Count}.", null, null, null, file.Path, null));
        }
    }

    /// <summary>The public member names on these lines, each with its first line number.</summary>
    private static Dictionary<string, int> PublicMembersIn(List<DiffLine> lines)
    {
        var members = new Dictionary<string, int>();
        foreach (var line in lines)
        {
            var match = PublicMember.Match(line.Text);
            if (match.Success)
            {
                members.TryAdd(match.Groups[1].Value, line.Number);
            }
        }

        return members;
    }

    private static string Quote(IEnumerable<string> names) => string.Join(", ", names.Select(name => $"`{name}`"));
}
