using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Text;

public sealed class CliTableWidthTests
{
    [Theory(DisplayName = "Plan rows retain complete paths and wrap actions at spaces"), Trait("Boundary", "Output"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData(80)]
    [InlineData(100)]
    public void PlansKeepEveryPathAndAction(int width)
    {
        IReadOnlyList<IReadOnlyList<string>> rows =
        [
            [".agents/skills/use-workflow/references/planning/_planning.md", "would record the removal in workspace settings"],
            [".agents/a folder/another folder/complete source.md", "metadata would move under open-forge:"],
            [".gitignore", "Open Forge Git-ignore rules would be updated"],
        ];
        var rendered = CliTable.RenderRows(rows, width: width);
        Assert.Equal(rows.Count, rendered.Count);
        for (var index = 0; index < rows.Count; index++) Assert.Contains(rows[index][0], rendered[index], StringComparison.Ordinal);
        Assert.All(string.Concat(rendered).Split('\n'), line => Assert.True(line.Length <= width, line));
        CommandOutputSnapshot.MatchSnapshot(string.Concat(rendered), $"plan-{width}");
    }

    [Fact(DisplayName = "A path wider than the host remains intact"), Trait("Boundary", "Output"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void OverlongPathsRemainIntact()
    {
        var path = ".agents/" + new string('p', 110) + "/last.md";
        var text = CliTable.Render([[path, "would delete"]], width: 80);
        Assert.Equal($"  {path}\n    would delete\n", text);
    }

    [Fact(DisplayName = "Stacked descriptions retain their row's hierarchy indentation"), Trait("Boundary", "Output"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void StackedDescriptionsKeepHierarchy()
    {
        var id = "    memory/crystallized/documents/" + new string('i', 40);
        var text = string.Concat(CliTable.RenderRows([[id, "A complete description"]], width: 80, stackOverflow: false));
        Assert.Equal($"  {id}\n        A complete description\n", text);
    }

    [Fact(DisplayName = "Stacked tables wrap prose before a trailing count column"), Trait("Boundary", "Output"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void StackedTablesWrapEveryProseColumn()
    {
        var description = "Record ideas, investigate questions, preserve decisions, and organize tasks, plans, backlogs, and checkpoints";
        var text = CliTable.Render([["planning", "0.4.0", description, "(2 packages)"]], width: 80);
        Assert.All(text.Split('\n'), line => Assert.True(line.Length <= 80, line));
        Assert.Contains(description, string.Join(" ", text.Split('\n').Select(line => line.Trim())), StringComparison.Ordinal);
        Assert.EndsWith("    (2 packages)\n", text, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Find's path column stays intact when its path contains spaces"), Trait("Boundary", "Output"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void PathColumnsWithSpacesStayIntact()
    {
        var path = ".agents/a folder/another folder/" + new string('p', 70) + "/source.md";
        var rows = CliTable.RenderRows([["example", path, "A complete description"]], width: 80, unwrappedColumns: new HashSet<int> { 1 });
        Assert.Contains($"    {path}\n", rows[0], StringComparison.Ordinal);
        Assert.EndsWith("    A complete description\n", rows[0], StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Read descriptions continue under their column without losing rows"), Trait("Boundary", "Output"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData(80)]
    [InlineData(100)]
    public void DescriptionsAlignTheirContinuation(int width)
    {
        IReadOnlyList<IReadOnlyList<string>> rows =
        [
            ["  skills/open-forge-cli", "Check the sample inputs, report the matching items, and keep each remaining detail aligned beneath its column."],
            ["  guidance/example", "A short sample description"],
        ];
        var text = string.Concat(CliTable.RenderRows(rows, width: width, stackOverflow: false));
        Assert.All(text.Split('\n'), line => Assert.True(line.Length <= width, line));
        CommandOutputSnapshot.MatchSnapshot(text, $"descriptions-{width}");
    }
}
