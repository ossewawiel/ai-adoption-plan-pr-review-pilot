using System.Text.RegularExpressions;

namespace ReviewAssistant.Core;

public enum DiffLineKind { Added, Removed }

/// <summary>One added or removed line. <see cref="Number"/> is the line number in the new file.</summary>
public sealed record DiffLine(DiffLineKind Kind, int Number, string Text);

/// <summary>Reads the added and removed lines out of a unified diff, with line numbers.</summary>
public static partial class DiffLines
{
    // A hunk header looks like "@@ -12,7 +12,9 @@". The number after '+' is where the new lines start.
    [GeneratedRegex(@"^@@ -\d+(?:,\d+)? \+(\d+)(?:,\d+)? @@")]
    private static partial Regex HunkHeader();

    public static IReadOnlyList<DiffLine> Parse(string diff)
    {
        var lines = new List<DiffLine>();
        var insideHunk = false;
        var newLineNumber = 0;

        foreach (var rawLine in diff.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');

            var header = HunkHeader().Match(line);
            if (header.Success)
            {
                insideHunk = true;
                newLineNumber = int.Parse(header.Groups[1].Value);
                continue;
            }

            if (!insideHunk)
            {
                continue; // file headers such as "--- a/File.cs" and "+++ b/File.cs"
            }

            if (line.StartsWith('+'))
            {
                lines.Add(new DiffLine(DiffLineKind.Added, newLineNumber, line[1..]));
                newLineNumber++;
            }
            else if (line.StartsWith('-'))
            {
                lines.Add(new DiffLine(DiffLineKind.Removed, newLineNumber, line[1..]));
            }
            else
            {
                newLineNumber++; // an unchanged line, shown for context
            }
        }

        return lines;
    }
}
