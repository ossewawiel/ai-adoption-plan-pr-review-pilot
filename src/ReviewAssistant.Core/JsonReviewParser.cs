using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReviewAssistant.Core;

/// <summary>
/// Step 5. Turns a provider's reply into a <see cref="Review"/>. It does not trust the reply:
/// a model can wrap the JSON in prose, use a word we do not know, or point at a file that is not there.
/// </summary>
public sealed class JsonReviewParser : IReviewParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString, // accept "line": "12"
    };

    // The shape of the reply. Everything is nullable because a reply may leave things out.
    private sealed record Reply(List<ReplyItem>? Summary, List<ReplyItem>? Suggestions, List<ReplyItem>? Risks);
    private sealed record ReplyItem(string? Message, string? Severity, string? Confidence, string? Category, string? File, int? Line);

    public Review Parse(string reply, ChangeSet changeSet)
    {
        // Keep only the outermost { ... }. This drops code fences and any chat around the JSON.
        var start = reply.IndexOf('{');
        var end = reply.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            throw new ReviewParseException("The provider's reply holds no JSON object.");
        }

        Reply? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<Reply>(reply[start..(end + 1)], Options);
        }
        catch (JsonException error)
        {
            throw new ReviewParseException($"The provider's reply is not the agreed JSON: {error.Message}");
        }

        var knownFiles = changeSet.Files.Select(file => file.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var items = new List<ReviewItem>();

        AddItems(items, Section.Summary, parsed?.Summary, knownFiles);
        AddItems(items, Section.Suggestion, parsed?.Suggestions, knownFiles);
        AddItems(items, Section.Risk, parsed?.Risks, knownFiles);

        if (items.Count == 0)
        {
            throw new ReviewParseException("The provider's reply holds no review items.");
        }

        return new Review(items);
    }

    private static void AddItems(List<ReviewItem> items, Section section, List<ReplyItem>? source, HashSet<string> knownFiles)
    {
        foreach (var item in source ?? [])
        {
            if (string.IsNullOrWhiteSpace(item.Message))
            {
                continue; // an item with nothing to say
            }

            // A file that is not in the change set is a made-up reference. Keep the message, drop the pointer.
            var fileIsKnown = item.File is not null && knownFiles.Contains(item.File);

            items.Add(new ReviewItem(
                section,
                item.Message.Trim(),
                section == Section.Summary ? Severity.Info : ReadOrDefault(item.Severity, Severity.Medium),
                ReadOrDefault(item.Confidence, Confidence.Low), // unknown confidence is treated as low
                string.IsNullOrWhiteSpace(item.Category) ? "general" : item.Category.Trim().ToLowerInvariant(),
                fileIsKnown ? item.File : null,
                fileIsKnown && item.Line > 0 ? item.Line : null));
        }
    }

    /// <summary>Reads "high" as Severity.High and so on. An unknown word gives the fallback.</summary>
    private static T ReadOrDefault<T>(string? text, T fallback) where T : struct, Enum =>
        Enum.TryParse<T>(text, ignoreCase: true, out var value) && Enum.IsDefined(value) ? value : fallback;
}

/// <summary>The provider's reply could not be used. The message is safe to show the user.</summary>
public sealed class ReviewParseException(string message) : Exception(message);
