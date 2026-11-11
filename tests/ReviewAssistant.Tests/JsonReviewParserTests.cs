using ReviewAssistant.Core;

namespace ReviewAssistant.Tests;

public class JsonReviewParserTests
{
    private static readonly ChangeSet ChangeSet = new("repo", "branch", "title", [new ChangedFile("src/Calc.cs", "")]);

    private static Review Parse(string reply) => new JsonReviewParser().Parse(reply, ChangeSet);

    [Fact]
    public void A_well_formed_reply_becomes_review_items()
    {
        var review = Parse("""
            {
              "summary": [ { "message": "Adds a zero guard." } ],
              "suggestions": [ { "message": "Add a test.", "severity": "low", "confidence": "high",
                                 "category": "tests", "file": "src/Calc.cs", "line": 12 } ],
              "risks": [ { "message": "Callers may break.", "severity": "high", "confidence": "medium",
                           "category": "compatibility", "file": "src/Calc.cs", "line": 3 } ]
            }
            """);

        Assert.Equal(3, review.Items.Count);

        var suggestion = Assert.Single(review.In(Section.Suggestion));
        Assert.Equal(new ReviewItem(Section.Suggestion, "Add a test.", Severity.Low, Confidence.High, "tests", "src/Calc.cs", 12), suggestion);

        var risk = Assert.Single(review.In(Section.Risk));
        Assert.Equal(Severity.High, risk.Severity);
    }

    [Fact]
    public void Chat_and_code_fences_around_the_json_are_ignored()
    {
        var review = Parse("""
            Sure, here is the review:
            ```json
            { "summary": [ { "message": "Adds a zero guard." } ] }
            ```
            Let me know if you need more.
            """);

        Assert.Equal("Adds a zero guard.", Assert.Single(review.Items).Message);
    }

    [Fact]
    public void A_file_that_is_not_in_the_change_set_is_dropped_but_the_message_is_kept()
    {
        var review = Parse("""
            { "risks": [ { "message": "Check this.", "severity": "high", "confidence": "high",
                           "category": "security", "file": "src/Invented.cs", "line": 9 } ] }
            """);

        var risk = Assert.Single(review.Items);
        Assert.Equal("Check this.", risk.Message);
        Assert.Null(risk.File);
        Assert.Null(risk.Line);
    }

    [Fact]
    public void Unknown_words_fall_back_to_medium_severity_and_low_confidence()
    {
        var review = Parse("""
            { "risks": [ { "message": "Check this.", "severity": "catastrophic", "confidence": "certain" } ] }
            """);

        var risk = Assert.Single(review.Items);
        Assert.Equal(Severity.Medium, risk.Severity);
        Assert.Equal(Confidence.Low, risk.Confidence);
        Assert.Equal("general", risk.Category);
    }

    [Theory]
    [InlineData("I could not review this change.")]
    [InlineData("{ this is not json }")]
    [InlineData("{ \"summary\": [], \"suggestions\": [], \"risks\": [] }")]
    public void A_reply_that_cannot_be_used_is_rejected_with_a_clear_error(string reply)
    {
        Assert.Throws<ReviewParseException>(() => Parse(reply));
    }
}
