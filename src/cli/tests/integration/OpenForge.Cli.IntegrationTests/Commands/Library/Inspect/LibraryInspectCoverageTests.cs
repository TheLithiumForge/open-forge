using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Inspect;

public sealed class LibraryInspectCoverageTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library Inspect cannot infer retirement or attention from an inaccessible inventory suffix"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task IncompleteInventoryPreventsRetirementClaims()
    {
        if (!OperatingSystem.IsLinux())
        {
            Assert.Skip("Required permission evidence targets Linux.");
            return;
        }

        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        fixture.SourceFile(".agents/a-safe/addition.md");
        fixture.SourceFile(".agents/z-unavailable/retained.md");
        fixture.Record(".agents/z-unavailable/retained.md");
        fixture.CurrentLink(".agents/z-unavailable/retained.md");
        var before = fixture.Snapshot();
        var directory = fixture.Files.Combine("shared/team/.agents/z-unavailable");
        var mode = File.GetUnixFileMode(directory);
        try
        {
            File.SetUnixFileMode(directory, UnixFileMode.None);
            var result = await fixture.InspectAsync(TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
            Assert.Equal(LibraryInventoryViewState.Incomplete, result.Result.Source.State);
            Assert.NotEqual(LibraryCoverage.Complete, result.Result.Projection.State);
            Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryInspectFindingCode.InventoryIncomplete);
            Assert.DoesNotContain(result.Result.Projection.Comparisons, comparison => comparison.Relation == LibraryComparisonRelation.Retired);
            Assert.Equal(".agents/z-unavailable/retained.md", Assert.Single(result.Result.Record.RegisteredPaths).DestinationPath);
        }
        finally
        {
            File.SetUnixFileMode(directory, mode);
        }

        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library Inspect unavailable source or destination facts prevent safe drift from becoming attention"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData("source")]
    [InlineData("destination")]
    public static async Task UnavailableFactsDominateDrift(string boundary)
    {
        if (!OperatingSystem.IsLinux())
        {
            Assert.Skip("Required permission evidence targets Linux.");
            return;
        }

        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        fixture.SourceFile(".agents/hidden/review.md");
        fixture.SourceFile(".agents/directives/addition.md");
        fixture.Record(".agents/hidden/review.md");
        fixture.CurrentLink(".agents/hidden/review.md");
        var before = fixture.Snapshot();
        var directory = fixture.Files.Combine(boundary == "source" ? "shared/team" : ".agents/hidden");
        var mode = File.GetUnixFileMode(directory);
        try
        {
            File.SetUnixFileMode(directory, UnixFileMode.None);
            var result = await fixture.InspectAsync(TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
            if (boundary == "source")
            {
                Assert.Equal(LibrarySourceRootViewState.Unavailable, result.Result.Source.RootState);
                Assert.NotEqual(LibraryInventoryViewState.Complete, result.Result.Source.State);
                Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryInspectFindingCode.SourceRootUnavailable);
            }
            else
            {
                Assert.Equal(LibraryCoverage.Incomplete, result.Result.Projection.State);
                Assert.Contains(result.Result.Projection.Comparisons, comparison =>
                    comparison.DestinationPath == ".agents/hidden/review.md" && comparison.Relation == LibraryComparisonRelation.Unavailable);
            }
        }
        finally
        {
            File.SetUnixFileMode(directory, mode);
        }

        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library Inspect blocked destination identity dominates safe missing-link drift"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task BlockedDestinationDominatesDrift()
    {
        using var fixture = new LibraryReadWorkspace();
        LibraryReadBoundary.Arrange(fixture, "destination-parent-link");
        fixture.SourceFile();
        fixture.Files.ReplaceText(LibraryReadWorkspace.RecordPath, """
            {"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":"shared/team","destinationRoot":".",
            "paths":[".agents/directives/review.md",".agents/linked/review.md"]}]}
            """);
        var before = fixture.Snapshot();
        var result = await fixture.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryInspectFindingCode.LinkBlocked);
        Assert.NotEqual(LibraryCoverage.Complete, result.Result.Projection.State);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }
}
