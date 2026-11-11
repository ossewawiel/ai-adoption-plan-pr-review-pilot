namespace ReviewAssistant.Core;

/// <summary>
/// The whole tool on one page: read, redact, build the prompt, call the provider, parse, render.
/// Each step is an interface, so any one of them can be swapped without touching the others.
/// </summary>
public sealed class ReviewPipeline(
    IChangeSetReader reader,
    IRedactor redactor,
    IPromptBuilder promptBuilder,
    IReviewProvider provider,
    IReviewParser parser,
    IReviewRenderer renderer)
{
    public async Task<PipelineResult> RunAsync(
        string changeSetPath,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var changeSet = reader.Read(changeSetPath);                       // 1. read
        var redacted = redactor.Redact(changeSet);                        // 2. redact
        var prompt = promptBuilder.Build(redacted);                       // 3. prompt
        var reply = await provider.GenerateAsync(                         // 4. provider
            new ReviewRequest(prompt, redacted), cancellationToken);
        var review = parser.Parse(reply, redacted.ChangeSet);             // 5. parse
        var markdown = renderer.Render(                                   // 6. render
            review, new ReviewContext(redacted, provider.Name, now));

        return new PipelineResult(markdown, review, prompt);
    }

    /// <summary>The pipeline with the standard steps and the provider you hand it.</summary>
    public static ReviewPipeline WithDefaults(IReviewProvider provider) => new(
        new JsonChangeSetReader(),
        new PatternRedactor(),
        new ReviewPromptBuilder(),
        provider,
        new JsonReviewParser(),
        new MarkdownReviewRenderer());
}

/// <summary>What a run produces. The prompt is kept so a person can see exactly what a provider was sent.</summary>
public sealed record PipelineResult(string Markdown, Review Review, string Prompt);
