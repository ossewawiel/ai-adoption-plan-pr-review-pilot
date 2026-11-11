using ReviewAssistant.Core;
using ReviewAssistant.Providers;

const string Usage = """
    Usage: review <change-set.json> [--out <file.md>] [--provider <name>] [--model <name>] [--show-prompt]

      --out          where to write the review (default: review.md)
      --provider     simulated    pattern rules, no network (default)
                     claude-code  runs Claude Code headless; sends the redacted change set to Claude
      --model        model for the claude-code provider (default: Claude Code's own default)
      --show-prompt  also print the exact text a provider is sent
    """;

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    Console.WriteLine(Usage);
    return 2;
}

var changeSetPath = args[0];
var outputPath = "review.md";
var providerName = "simulated";
string? model = null;
var showPrompt = false;

for (var i = 1; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--out" when i + 1 < args.Length:
            outputPath = args[++i];
            break;
        case "--provider" when i + 1 < args.Length:
            providerName = args[++i];
            break;
        case "--model" when i + 1 < args.Length:
            model = args[++i];
            break;
        case "--show-prompt":
            showPrompt = true;
            break;
        default:
            Console.Error.WriteLine($"Unknown or incomplete option: {args[i]}");
            Console.Error.WriteLine(Usage);
            return 2;
    }
}

// The one place a provider is chosen. A new provider is one new line here.
IReviewProvider? provider = providerName switch
{
    "simulated" => new SimulatedReviewProvider(),
    "claude-code" => new ClaudeCodeReviewProvider(model),
    _ => null,
};

if (provider is null)
{
    Console.Error.WriteLine($"Unknown provider: {providerName}");
    return 2;
}

if (provider is ClaudeCodeReviewProvider)
{
    Console.WriteLine("Note: the claude-code provider sends the redacted change set to Claude through your Claude Code login.");
}

try
{
    var pipeline = ReviewPipeline.WithDefaults(provider);
    var result = await pipeline.RunAsync(changeSetPath, DateTimeOffset.UtcNow);

    File.WriteAllText(outputPath, result.Markdown);

    if (showPrompt)
    {
        Console.WriteLine(result.Prompt);
    }

    Console.WriteLine($"Review written to {Path.GetFullPath(outputPath)}");
    Console.WriteLine($"Provider: {provider.Name}");
    Console.WriteLine($"Items: {result.Review.In(Section.Suggestion).Count()} suggestions, {result.Review.In(Section.Risk).Count()} risks");
    return 0;
}
catch (Exception error) when (error is InvalidChangeSetException or ReviewParseException or ReviewProviderException)
{
    Console.Error.WriteLine(error.Message);
    return 1;
}
