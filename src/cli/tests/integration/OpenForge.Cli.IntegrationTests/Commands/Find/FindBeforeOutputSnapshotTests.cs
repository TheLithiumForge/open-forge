using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.IntegrationTests.Commands.Find;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class FindBeforeOutputSnapshotTests
{
    [Theory, InlineData(false), InlineData(true), Trait("Boundary", "Output")]
    public async Task Outside(bool exact)
    {
        using var workspace = new FindOutsideWorkspace(FindOutsideWorkspace.CustomRoot);
        workspace.Register();
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        string[] selection = exact ? [FindDefinitions.Include.Name, SourceWorkspaceRelativePath.LiteralReferencePrefix + FindOutsideWorkspace.Leaf,
            FindDefinitions.For.Name, "src/App.cs", FindDefinitions.Tag.Name, FindOutsideWorkspace.RowTag] : [];
        await new ReadCommandOutputCapture(workspace.Files.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = exact ? "outside-conditioned" : "outside-inventory",
            Arguments = [FindDefinitions.CommandIdentity, .. selection],
            ExitCode = 0,
        }, testName: $"{nameof(Outside)}_{exact}");
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Find output preserves named queries and leaves source bytes unchanged")]
    [InlineData("bare-inventory", 0)]
    [InlineData("one-tag", 0)]
    [InlineData("two-tags-all", 0)]
    [InlineData("heading", 0)]
    [InlineData("no-matches", 0)]
    [InlineData("with-content-headings", 0)]
    [InlineData("include-selector", 0)]
    [InlineData("section-missing", 2)]
    [InlineData("invalid-selector", 4)]
    [InlineData("invalid-require", 4)]
    [InlineData("unreadable-source", 3)]
    [InlineData("ambiguous-selector", 5)]
    public async Task Selection(string situation, int exitCode)
    {
        using var workspace = new RoutedOutputWorkspace();
        string[] options = situation switch
        {
            "bare-inventory" => [],
            "one-tag" => ["--tag", "Guide"],
            "two-tags-all" => ["--tag", "Docs", "--tag", "Guide", "--require", "all"],
            "heading" => ["--heading", "Rules"],
            "no-matches" => ["--tag", "AbsentTag"],
            "with-content-headings" => ["--tag", "Guide", "--content", "headings"],
            "include-selector" => ["--include", "docs/guide"],
            "section-missing" => ["--include", "docs/guide", "--content", "section:Absent"],
            "invalid-selector" => ["--include", "missing/source"],
            "invalid-require" => ["--tag", "Guide", "--require", "invalid"],
            "unreadable-source" => [],
            "ambiguous-selector" => ["--include", "docs/guide"],
            _ => throw new ArgumentOutOfRangeException(nameof(situation), situation, "The Find snapshot situation is not defined."),
        };
        if (situation == "ambiguous-selector") workspace.CollidingGuideId();
        if (situation == "unreadable-source")
        {
            workspace.ReplaceBytes(".agents/docs/guide.md", [0xff, 0xfe, 0xfd]);
        }

        var before = workspace.Snapshot();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = situation,
            Arguments = ["find", .. options],
            ExitCode = exitCode,
        }, testName: $"{nameof(Selection)}_{situation}");
        Assert.Equal(before, workspace.Snapshot());
    }
}
