using System.Text;

namespace ReviewAssistant.Core;

/// <summary>
/// Step 6. Writes the review as a Markdown file with the three headings the team expects:
/// Summary, Suggestions, Potential Risks.
/// </summary>
public sealed class MarkdownReviewRenderer : IReviewRenderer
{
    public string Render(Review review, ReviewContext context)
    {
        var changeSet = context.Input.ChangeSet;
        var lines = changeSet.Files.SelectMany(file => DiffLines.Parse(file.Diff)).ToList();
        var added = lines.Count(line => line.Kind == DiffLineKind.Added);
        var removed = lines.Count(line => line.Kind == DiffLineKind.Removed);

        var redactions = context.Input.Redactions.Count == 0
            ? "nothing"
            : string.Join(", ", context.Input.Redactions.Select(pair => $"{pair.Value} × {pair.Key}"));

        var page = new StringBuilder();
        page.AppendLine($"# AI Review: {changeSet.Repository} ({changeSet.Branch})");
        page.AppendLine();
        if (changeSet.Title.Length > 0)
        {
            page.AppendLine($"_{changeSet.Title}_");
            page.AppendLine();
        }

        page.AppendLine($"- **Provider:** {context.ProviderName}");
        page.AppendLine($"- **Change:** {changeSet.Files.Count} files, +{added} / -{removed} lines");
        page.AppendLine($"- **Redacted before the prompt was built:** {redactions}");
        page.AppendLine($"- **Generated:** {context.GeneratedAt:yyyy-MM-dd HH:mm} UTC");
        page.AppendLine();

        AppendSection(page, "Summary", review.In(Section.Summary));
        AppendSection(page, "Suggestions", review.In(Section.Suggestion));
        AppendSection(page, "Potential Risks", review.In(Section.Risk));

        page.AppendLine("---");
        page.AppendLine("This review is advice for a human reviewer. It does not approve or block the change.");
        return page.ToString();
    }

    private static void AppendSection(StringBuilder page, string heading, IEnumerable<ReviewItem> items)
    {
        page.AppendLine($"## {heading}");
        page.AppendLine();

        var any = false;
        foreach (var item in items)
        {
            any = true;
            var where = item.File is null ? "" : item.Line is null ? $" (`{item.File}`)" : $" (`{item.File}:{item.Line}`)";

            if (item.Section == Section.Summary)
            {
                page.AppendLine($"- {item.Message}{where}");
            }
            else
            {
                page.AppendLine($"- **[{item.Severity.ToString().ToUpperInvariant()}]** {item.Message}{where}  ");
                page.AppendLine($"  _{item.Category} · confidence: {item.Confidence.ToString().ToLowerInvariant()}_");
            }
        }

        if (!any)
        {
            page.AppendLine("- Nothing to report.");
        }

        page.AppendLine();
    }
}
