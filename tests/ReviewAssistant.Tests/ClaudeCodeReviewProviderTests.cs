using ReviewAssistant.Core;
using ReviewAssistant.Providers;

namespace ReviewAssistant.Tests;

/// <summary>These tests never start Claude Code and never touch the network.</summary>
public class ClaudeCodeReviewProviderTests
{
    [Fact]
    public void The_reply_is_read_from_the_result_field_of_claude_codes_output()
    {
        var output = """{ "type": "result", "result": "{ \"summary\": [] }", "total_cost_usd": 0.01 }""";

        Assert.Equal("{ \"summary\": [] }", ClaudeCodeReviewProvider.ExtractReply(output));
    }

    [Theory]
    [InlineData("not json at all")]
    [InlineData("{ \"type\": \"result\" }")]
    public void Output_without_a_result_is_rejected_with_a_clear_error(string output)
    {
        Assert.Throws<ReviewProviderException>(() => ClaudeCodeReviewProvider.ExtractReply(output));
    }

    [Fact]
    public async Task A_missing_claude_code_install_is_reported_clearly()
    {
        var provider = new ClaudeCodeReviewProvider(executable: "claude-code-is-not-installed-here");
        var input = new RedactedChangeSet(new ChangeSet("repo", "branch", "title", []), new Dictionary<string, int>());

        var error = await Assert.ThrowsAsync<ReviewProviderException>(
            () => provider.GenerateAsync(new ReviewRequest("prompt", input), CancellationToken.None));

        Assert.Contains("use the simulated provider", error.Message);
    }
}
