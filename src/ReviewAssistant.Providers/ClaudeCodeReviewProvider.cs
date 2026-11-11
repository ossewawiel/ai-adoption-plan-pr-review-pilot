using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using ReviewAssistant.Core;

namespace ReviewAssistant.Providers;

/// <summary>
/// The opt-in provider. It runs the Claude Code command-line tool in headless mode and reads its reply.
/// This is the only class in the solution that causes network traffic, and only when the user asks for it.
///
/// Why shell out to Claude Code and not call an API directly:
/// - no API key lives in this tool; Claude Code uses the login the company has already approved;
/// - the company's managed settings (permissions, plugins, audit export) apply to the call.
/// </summary>
public sealed class ClaudeCodeReviewProvider(string? model = null, string executable = "claude") : IReviewProvider
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(3);

    public string Name => model is null ? "claude-code (headless)" : $"claude-code (headless, {model})";

    public async Task<string> GenerateAsync(ReviewRequest request, CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo(executable)
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        // -p: print mode, no interactive session. The review prompt itself arrives on standard input.
        start.ArgumentList.Add("-p");
        start.ArgumentList.Add("Review the change set on standard input. Reply with the JSON object only.");
        start.ArgumentList.Add("--output-format");
        start.ArgumentList.Add("json");

        // No tools: the model can read the prompt and answer. It cannot open files, run commands or browse.
        start.ArgumentList.Add("--tools");
        start.ArgumentList.Add("");
        start.ArgumentList.Add("--max-turns");
        start.ArgumentList.Add("1");

        if (model is not null)
        {
            start.ArgumentList.Add("--model");
            start.ArgumentList.Add(model);
        }

        using var process = new Process { StartInfo = start };
        try
        {
            process.Start();
        }
        catch (Win32Exception)
        {
            throw new ReviewProviderException(
                $"Could not start '{executable}'. Install Claude Code and sign in, or use the simulated provider.");
        }

        // Stop waiting after the timeout, or when the caller cancels.
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(Timeout);

        try
        {
            await process.StandardInput.WriteAsync(request.Prompt);
            process.StandardInput.Close();

            var output = process.StandardOutput.ReadToEndAsync(deadline.Token);
            var errors = process.StandardError.ReadToEndAsync(deadline.Token);
            await process.WaitForExitAsync(deadline.Token);

            if (process.ExitCode != 0)
            {
                throw new ReviewProviderException($"Claude Code exited with code {process.ExitCode}: {(await errors).Trim()}");
            }

            return ExtractReply(await output);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw new ReviewProviderException($"Claude Code did not answer within {Timeout.TotalMinutes} minutes.");
        }
    }

    /// <summary>
    /// With --output-format json, Claude Code prints one JSON object about the run.
    /// The model's answer is the text in its "result" field.
    /// </summary>
    public static string ExtractReply(string output)
    {
        try
        {
            using var document = JsonDocument.Parse(output);
            if (document.RootElement.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.String)
            {
                return result.GetString()!;
            }
        }
        catch (JsonException)
        {
            // fall through to the error below
        }

        throw new ReviewProviderException("Claude Code's output did not hold a \"result\" text.");
    }
}
