namespace ReviewAssistant.Core;

/// <summary>The three headings of the review file.</summary>
public enum Section { Summary, Suggestion, Risk }

/// <summary>How much it matters. Summary items are always <see cref="Info"/>.</summary>
public enum Severity { Info, Low, Medium, High }

/// <summary>How sure the provider is. A reviewer reads a low-confidence item as a question.</summary>
public enum Confidence { Low, Medium, High }

/// <summary>One line of the review. <see cref="File"/> and <see cref="Line"/> point at the new version of the file.</summary>
public sealed record ReviewItem(
    Section Section,
    string Message,
    Severity Severity,
    Confidence Confidence,
    string Category,
    string? File = null,
    int? Line = null);

/// <summary>The parsed review: what every provider's reply is turned into before it is rendered.</summary>
public sealed record Review(IReadOnlyList<ReviewItem> Items)
{
    /// <summary>The items under one heading, most severe first.</summary>
    public IEnumerable<ReviewItem> In(Section section) =>
        Items.Where(item => item.Section == section)
             .OrderByDescending(item => item.Severity);
}
