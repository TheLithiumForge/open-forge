using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Inspect;

public sealed class LibraryInspectComparisonTests
{
    [Fact(DisplayName = "Library Inspect compares the complete union with all five safe relations"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task CompleteComparisonRelations()
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        foreach (var name in new[] { "missing", "current", "changed", "added" })
        {
            fixture.SourceFile($".agents/directives/{name}.md");
        }

        fixture.Record(".agents/directives/changed.md", ".agents/directives/current.md", ".agents/directives/missing.md", ".agents/directives/retired.md");
        fixture.CurrentLink(".agents/directives/current.md");
        fixture.CurrentLink(".agents/directives/retired.md");
        fixture.Files.WriteText(".agents/directives/changed.md", "local changed occupant\n");
        var before = fixture.Snapshot();
        var first = await fixture.InspectAsync(TestContext.Current.CancellationToken);
        var second = await fixture.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Attention, first.Status);
        Assert.Equal("team-knowledge", first.Result.Record.Id);
        Assert.Equal(LibraryInventoryViewState.Complete, first.Result.Source.State);
        Assert.Equal(LibraryCoverage.Complete, first.Result.Projection.State);
        Assert.Equal(
            [".agents/directives/added.md", ".agents/directives/changed.md", ".agents/directives/current.md", ".agents/directives/missing.md"],
            first.Result.Source.EligiblePaths.Select(path => path.SourcePath));
        var comparisons = first.Result.Projection.Comparisons;
        Assert.Equal(
            [".agents/directives/added.md", ".agents/directives/changed.md", ".agents/directives/current.md", ".agents/directives/missing.md", ".agents/directives/retired.md"],
            comparisons.Select(path => path.DestinationPath));
        Assert.Equal(
            [LibraryComparisonRelation.Added, LibraryComparisonRelation.Changed, LibraryComparisonRelation.Current, LibraryComparisonRelation.Missing, LibraryComparisonRelation.Retired],
            comparisons.Select(path => path.Relation));
        Assert.Null(comparisons[0].Registered);
        Assert.NotNull(comparisons[4].Registered);
        Assert.Equal("../../shared/team/.agents/directives/current.md", comparisons[2].ObservedRelativeLink);
        Assert.Null(comparisons[3].ObservedRelativeLink);
        Assert.Equal("../../shared/team/.agents/directives/retired.md", comparisons[4].ObservedRelativeLink);
        foreach (var comparison in comparisons)
        {
            Assert.Equal(comparison.SourcePath, comparison.DestinationPath);
            Assert.Equal($"directives/{System.IO.Path.GetFileNameWithoutExtension(comparison.DestinationPath)}", comparison.SourceId);
            Assert.NotEqual(first.Result.Record.Id, comparison.SourceId);
        }

        Assert.Equal(
            [LibraryInspectFindingCode.PathAdded, LibraryInspectFindingCode.PathRetired, LibraryInspectFindingCode.LinkMissing, LibraryInspectFindingCode.LinkChanged],
            first.Result.Findings.Select(finding => finding.Code).OrderBy(code => code));
        Assert.Equal(Render(first), Render(second));
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Theory(DisplayName = "Library Inspect proves both empty and exact nonempty inventories complete"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task CompleteExactInventory(bool populated)
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        if (populated)
        {
            fixture.SourceFile();
            fixture.Record(LibraryReadWorkspace.ReviewPath);
            fixture.CurrentLink();
        }
        else
        {
            fixture.Record();
        }

        var before = fixture.Snapshot();
        var result = await fixture.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(LibraryRecordViewState.Complete, result.Result.Record.State);
        Assert.Equal(LibrarySourceRootViewState.Available, result.Result.Source.RootState);
        Assert.Equal(LibraryInventoryViewState.Complete, result.Result.Source.State);
        Assert.Equal(LibraryCoverage.Complete, result.Result.Projection.State);
        Assert.Equal(populated ? 1 : 0, result.Result.Source.EligiblePaths.Length);
        Assert.Equal(populated ? 1 : 0, result.Result.Record.RegisteredPaths.Length);
        Assert.All(result.Result.Projection.Comparisons, comparison => Assert.Equal(LibraryComparisonRelation.Current, comparison.Relation));
        Assert.Empty(result.Result.Findings);
        Assert.Null(result.Next);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Fact(DisplayName = "Library Inspect selects one ID and derives Unicode skill and resource source IDs"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task ExactSelectionAndDestinationIds()
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        string[] paths = [".agents/directives/Équipe Review.md", ".agents/resources/data.json", ".agents/skills/review/SKILL.md"];
        foreach (var path in paths)
        {
            fixture.SourceFile(path);
            fixture.CurrentLink(path);
        }

        fixture.Files.WriteText(LibraryReadWorkspace.RecordPath, """
            {"schemaVersion":1,"libraries":[
              {"id":"other","sourceRoot":"missing/unselected","destinationRoot":".","paths":[]},
              {"id":"team-knowledge","sourceRoot":"shared/team","destinationRoot":".","paths":[
                ".agents/directives/Équipe Review.md",".agents/resources/data.json",".agents/skills/review/SKILL.md"]}]}
            """);
        var before = fixture.Snapshot();
        var result = await fixture.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("team-knowledge", result.Result.Record.Id);
        Assert.Equal(paths, result.Result.Record.RegisteredPaths.Select(path => path.DestinationPath));
        Assert.Equal(["directives/Équipe Review", "resources/data.json", "skills/review"], result.Result.Source.EligiblePaths.Select(path => path.SourceId));
        Assert.All(result.Result.Projection.Comparisons, comparison => Assert.Equal(LibraryComparisonRelation.Current, comparison.Relation));
        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact(DisplayName = "Library Inspect uses complete eligible source facts rather than source manager control files"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task CompleteInventoryExcludesControls()
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        fixture.SourceFile();
        fixture.SourceFile(".agents/loader.md");
        fixture.SourceFile(".agents/directives/_directives.md");
        fixture.SourceFile(".agents/directives/review.overwrite.md");
        fixture.SourceFile(".agents/open-forge.lifecycle.json");
        fixture.SourceFile(".agents/open-forge.libraries.json");
        fixture.Files.CreateFileSymbolicLink("shared/team/.agents/directives/excluded.md", "review.md");
        fixture.Record(LibraryReadWorkspace.ReviewPath);
        fixture.CurrentLink();
        var before = fixture.Snapshot();
        var result = await fixture.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(LibraryReadWorkspace.ReviewPath, Assert.Single(result.Result.Source.EligiblePaths).SourcePath);
        Assert.Equal(LibraryComparisonRelation.Current, Assert.Single(result.Result.Projection.Comparisons).Relation);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    private static string Render(LibraryInspectResult result)
        => LibraryInspectPresentation.RenderJson(new(result, new(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));
}
