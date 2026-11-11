using ReviewAssistant.Core;
using ReviewAssistant.Providers;

namespace ReviewAssistant.Tests;

/// <summary>End-to-end: the real steps, run on the sample change set in samples/changeset.json.</summary>
public class PipelineTests
{
    private static readonly string SamplePath = Path.Combine(AppContext.BaseDirectory, "samples", "changeset.json");
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 8, 30, 0, TimeSpan.Zero);

    // The sensitive values that are in the sample on purpose.
    private static readonly string[] SensitiveValues = ["Winter2026!", "8001015009087", "-26.2041, 28.0473"];

    [Fact]
    public async Task The_sample_change_set_becomes_a_review_file()
    {
        var pipeline = ReviewPipeline.WithDefaults(new SimulatedReviewProvider());

        var result = await pipeline.RunAsync(SamplePath, Now);

        Assert.StartsWith("# AI Review: Sample.FleetApi (feature/trip-average-speed)", result.Markdown);
        Assert.Contains("## Summary", result.Markdown);
        Assert.Contains("## Suggestions", result.Markdown);
        Assert.Contains("## Potential Risks", result.Markdown);

        // The simulated provider finds the problems that were planted in the sample.
        Assert.Contains(result.Review.In(Section.Risk), risk => risk.Category == "security" && risk.Severity == Severity.High);
        Assert.Contains(result.Review.In(Section.Risk), risk => risk.Category == "privacy");
        Assert.Contains(result.Review.In(Section.Risk), risk => risk.Category == "compatibility");
        Assert.Contains(result.Review.In(Section.Suggestion), suggestion => suggestion.Category == "tests");
    }

    [Fact]
    public async Task Sensitive_values_never_reach_the_prompt_or_the_review()
    {
        var pipeline = ReviewPipeline.WithDefaults(new SimulatedReviewProvider());

        var result = await pipeline.RunAsync(SamplePath, Now);

        foreach (var value in SensitiveValues)
        {
            Assert.DoesNotContain(value, result.Prompt);
            Assert.DoesNotContain(value, result.Markdown);
        }

        Assert.Contains("[REDACTED:secret]", result.Prompt);
        Assert.Contains("1 × secret", result.Markdown);
    }

    [Fact]
    public async Task Another_provider_plugs_in_without_changing_any_other_step()
    {
        var provider = new CannedProvider("""
            { "summary": [ { "message": "A reply from a different provider." } ] }
            """);
        var pipeline = ReviewPipeline.WithDefaults(provider);

        var result = await pipeline.RunAsync(SamplePath, Now);

        Assert.Contains("A reply from a different provider.", result.Markdown);
        Assert.Contains("**Provider:** canned", result.Markdown);

        // The provider was handed the prompt and the redacted change set, nothing else.
        Assert.Equal(result.Prompt, provider.LastRequest!.Prompt);
        foreach (var value in SensitiveValues)
        {
            Assert.DoesNotContain(provider.LastRequest.Input.ChangeSet.Files, file => file.Diff.Contains(value));
        }
    }

    [Fact]
    public async Task A_provider_that_replies_with_nonsense_fails_loudly()
    {
        var pipeline = ReviewPipeline.WithDefaults(new CannedProvider("I am sorry, I cannot help with that."));

        await Assert.ThrowsAsync<ReviewParseException>(() => pipeline.RunAsync(SamplePath, Now));
    }

    [Fact]
    public void A_missing_change_set_is_reported_clearly()
    {
        var error = Assert.Throws<InvalidChangeSetException>(() => new JsonChangeSetReader().Read("does-not-exist.json"));

        Assert.Contains("does-not-exist.json", error.Message);
    }

    /// <summary>A stand-in provider: returns a fixed reply and remembers what it was asked.</summary>
    private sealed class CannedProvider(string reply) : IReviewProvider
    {
        public string Name => "canned";

        public ReviewRequest? LastRequest { get; private set; }

        public Task<string> GenerateAsync(ReviewRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(reply);
        }
    }
}
