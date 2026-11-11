using ReviewAssistant.Core;

namespace ReviewAssistant.Tests;

public class DiffLinesTests
{
    [Fact]
    public void Added_lines_get_their_line_number_in_the_new_file()
    {
        var diff = string.Join('\n',
            "--- a/Calc.cs",
            "+++ b/Calc.cs",
            "@@ -10,4 +10,5 @@",
            " unchanged line 10",
            "-removed line",
            "+added line 11",
            "+added line 12",
            " unchanged line 13");

        var lines = DiffLines.Parse(diff);

        Assert.Equal(3, lines.Count);
        Assert.Equal(new DiffLine(DiffLineKind.Removed, 11, "removed line"), lines[0]);
        Assert.Equal(new DiffLine(DiffLineKind.Added, 11, "added line 11"), lines[1]);
        Assert.Equal(new DiffLine(DiffLineKind.Added, 12, "added line 12"), lines[2]);
    }

    [Fact]
    public void File_headers_are_not_counted_as_changes()
    {
        var lines = DiffLines.Parse("--- a/Calc.cs\n+++ b/Calc.cs");

        Assert.Empty(lines);
    }
}
