using ReviewAssistant.Core;

namespace ReviewAssistant.Tests;

public class MarkdownReviewRendererTests
{
    private static readonly ReviewContext Context = new(
        new RedactedChangeSet(
            new ChangeSet("Sample.App", "feature/x", "A title", [new ChangedFile("src/Calc.cs", "@@ -1,1 +1,2 @@\n line\n+added")]),
            new Dictionary<string, int> { ["secret"] = 2 }),
        "test provider",
        new DateTimeOffset(2026, 11, 12, 8, 30, 0, TimeSpan.Zero));

    private static string Render(params ReviewItem[] items) =>
        new MarkdownReviewRenderer().Render(new Review(items), Context);

    [Fact]
    public void The_file_has_the_title_and_the_three_headings_in_order()
    {
        var markdown = Render(new ReviewItem(Section.Summary, "Adds a guard.", Severity.Info, Confidence.High, "general"));

        Assert.StartsWith("# AI Review: Sample.App (feature/x)", markdown);

        var summary = markdown.IndexOf("## Summary", StringComparison.Ordinal);
        var suggestions = markdown.IndexOf("## Suggestions", StringComparison.Ordinal);
        var risks = markdown.IndexOf("## Potential Risks", StringComparison.Ordinal);
        Assert.True(0 < summary && summary < suggestions && suggestions < risks);
    }

    [Fact]
    public void The_header_says_what_produced_the_review_and_what_was_redacted()
    {
        var markdown = Render(new ReviewItem(Section.Summary, "Adds a guard.", Severity.Info, Confidence.High, "general"));

        Assert.Contains("**Provider:** test provider", markdown);
        Assert.Contains("1 files, +1 / -0 lines", markdown);
        Assert.Contains("2 × secret", markdown);
        Assert.Contains("2026-11-12 08:30 UTC", markdown);
    }

    [Fact]
    public void A_risk_shows_severity_place_category_and_confidence()
    {
        var markdown = Render(new ReviewItem(Section.Risk, "Callers may break.", Severity.High, Confidence.Medium, "compatibility", "src/Calc.cs", 3));

        Assert.Contains("- **[HIGH]** Callers may break. (`src/Calc.cs:3`)", markdown);
        Assert.Contains("_compatibility · confidence: medium_", markdown);
    }

    [Fact]
    public void The_most_severe_item_comes_first()
    {
        var markdown = Render(
            new ReviewItem(Section.Risk, "Minor thing.", Severity.Low, Confidence.High, "maintainability"),
            new ReviewItem(Section.Risk, "Serious thing.", Severity.High, Confidence.High, "security"));

        Assert.True(markdown.IndexOf("Serious thing.", StringComparison.Ordinal) < markdown.IndexOf("Minor thing.", StringComparison.Ordinal));
    }

    [Fact]
    public void An_empty_section_says_so()
    {
        var markdown = Render(new ReviewItem(Section.Summary, "Adds a guard.", Severity.Info, Confidence.High, "general"));

        Assert.Contains("## Potential Risks\r\n\r\n- Nothing to report.".ReplaceLineEndings(), markdown);
    }
}
