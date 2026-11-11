namespace ReviewAssistant.Core;

// The six steps of a review, one interface each. ReviewPipeline runs them in this order.
// Each interface has one small implementation in this project, except IReviewProvider:
// its implementations live in ReviewAssistant.Providers, and that is the extension point.

/// <summary>Step 1. Reads a change set from disk.</summary>
public interface IChangeSetReader
{
    ChangeSet Read(string path);
}

/// <summary>Step 2. Replaces secrets and personal information with markers.</summary>
public interface IRedactor
{
    RedactedChangeSet Redact(ChangeSet changeSet);
}

/// <summary>Step 3. Turns a redacted change set into the text a provider receives.</summary>
public interface IPromptBuilder
{
    string Build(RedactedChangeSet input);
}

/// <summary>
/// Step 4, the extension point. A provider takes the prompt and returns a reply as text.
/// The reply must be the JSON described in <see cref="ReviewPromptBuilder"/>; the parser checks it.
/// </summary>
public interface IReviewProvider
{
    /// <summary>Shown in the review file so the reader knows what produced it.</summary>
    string Name { get; }

    Task<string> GenerateAsync(ReviewRequest request, CancellationToken cancellationToken);
}

/// <summary>What a provider is given. It only ever holds redacted input.</summary>
public sealed record ReviewRequest(string Prompt, RedactedChangeSet Input);

/// <summary>Step 5. Turns a provider's reply into a <see cref="Review"/>, and distrusts it while doing so.</summary>
public interface IReviewParser
{
    Review Parse(string reply, ChangeSet changeSet);
}

/// <summary>Step 6. Writes the review as text for a person to read.</summary>
public interface IReviewRenderer
{
    string Render(Review review, ReviewContext context);
}

/// <summary>The facts the renderer prints around the review itself.</summary>
public sealed record ReviewContext(
    RedactedChangeSet Input,
    string ProviderName,
    DateTimeOffset GeneratedAt);
