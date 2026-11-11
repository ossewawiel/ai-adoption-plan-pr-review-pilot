using ReviewAssistant.Core;

namespace ReviewAssistant.Tests;

public class PatternRedactorTests
{
    private static RedactedChangeSet Redact(string line) =>
        new PatternRedactor().Redact(new ChangeSet("repo", "branch", "title", [new ChangedFile("File.cs", line)]));

    [Theory]
    [InlineData("Server=db01;Password=Winter2026!;", "Winter2026!", "secret")]
    [InlineData("var apiKey = \"sk_live_12345\";", "sk_live_12345", "secret")]
    [InlineData("\"clientSecret\": \"hunter2\"", "hunter2", "secret")]
    [InlineData("// customer 8001015009087 phoned", "8001015009087", "id-number")]
    [InlineData("// mail someone@example.co.za", "someone@example.co.za", "email")]
    [InlineData("// call 0825550142 back", "0825550142", "phone")]
    [InlineData("// last seen at -26.2041, 28.0473", "-26.2041, 28.0473", "location")]
    public void Sensitive_values_are_replaced_and_counted(string line, string sensitiveValue, string kind)
    {
        var result = Redact(line);

        var cleaned = result.ChangeSet.Files[0].Diff;
        Assert.DoesNotContain(sensitiveValue, cleaned);
        Assert.Contains($"[REDACTED:{kind}]", cleaned);
        Assert.Equal(1, result.Redactions[kind]);
    }

    [Theory]
    [InlineData("var total = price * 1.15;")]
    [InlineData("public async Task<Trip> FindAsync(int tripId, CancellationToken token = default)")]
    [InlineData("_logger.LogInformation(\"Trip {TripId} ended\", trip.Id);")]
    public void Ordinary_code_is_left_alone(string line)
    {
        var result = Redact(line);

        Assert.Equal(line, result.ChangeSet.Files[0].Diff);
        Assert.Empty(result.Redactions);
    }

    [Fact]
    public void The_key_name_is_kept_so_the_reviewer_still_sees_what_was_there()
    {
        var result = Redact("Server=db01;Password=Winter2026!;");

        Assert.Equal("Server=db01;Password=[REDACTED:secret];", result.ChangeSet.Files[0].Diff);
    }
}
