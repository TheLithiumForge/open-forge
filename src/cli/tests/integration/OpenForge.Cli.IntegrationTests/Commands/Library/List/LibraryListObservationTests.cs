using OpenForge.Cli.Core.Commands.Library.List.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.List;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.List;

public sealed class LibraryListObservationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library List distinguishes an absent record from a strict empty record and never starts inventory"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task KnownEmptyRecord(bool present)
    {
        using var fixture = new LibraryReadWorkspace();
        if (present)
        {
            fixture.Files.WriteText(LibraryReadWorkspace.RecordPath, "{\"schemaVersion\":1,\"libraries\":[]}");
        }

        var before = fixture.Snapshot();
        fixture.AssertNoPersistentState();
        var result = await fixture.ListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(present ? LibraryRecordViewState.Complete : LibraryRecordViewState.Missing, result.Result.Record.State);
        Assert.Equal(present ? 0 : (int?)null, result.Result.Record.LibraryCount);
        Assert.Empty(result.Result.Libraries);
        if (present)
        {
            Assert.Empty(result.Result.Findings);
        }
        else
        {
            Assert.Equal(LibraryListFindingCode.OwnershipObservation, Assert.Single(result.Result.Findings).Code);
        }
        Assert.Equal(LibraryListInventoryState.NotRequested, result.Result.Inventory);
        Assert.Equal(LibraryCoverage.Complete, result.Result.Coverage);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library List observes only registered links and cannot infer unregistered source additions"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task BoundedFactsAndDeterminism()
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        fixture.SourceFile();
        fixture.SourceFile(".agents/directives/unregistered.md");
        fixture.Record(LibraryReadWorkspace.ReviewPath);
        fixture.CurrentLink();
        var before = fixture.Snapshot();

        var first = await fixture.ListAsync(TestContext.Current.CancellationToken);
        var second = await fixture.ListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        var library = Assert.Single(first.Result.Libraries);
        Assert.Equal("team-knowledge", library.Id);
        Assert.Equal("shared/team", library.SourceRoot);
        Assert.Equal(LibrarySourceRootViewState.Available, library.SourceRootState);
        var path = Assert.Single(library.Paths);
        Assert.Equal(".agents/directives/review.md", path.SourcePath);
        Assert.Equal(path.SourcePath, path.DestinationPath);
        Assert.Equal("../../shared/team/.agents/directives/review.md", path.ExpectedRelativeLink);
        Assert.Equal(path.ExpectedRelativeLink, path.ObservedRelativeLink);
        Assert.Equal("directives/review", path.SourceId);
        Assert.Equal(LibraryLinkViewState.Current, path.State);
        Assert.Equal(LibraryListInventoryState.NotRequested, first.Result.Inventory);
        Assert.Equal(LibraryCoverage.Complete, first.Result.Coverage);
        Assert.Empty(first.Result.Findings);
        Assert.Equal(Render(first), Render(second));
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library List reports safe missing or changed registered occupants as attention without adopting them"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData("missing", (int)LibraryLinkViewState.Missing, (int)LibraryListFindingCode.LinkMissing)]
    [InlineData("ordinary", (int)LibraryLinkViewState.Changed, (int)LibraryListFindingCode.LinkChanged)]
    [InlineData("different-target", (int)LibraryLinkViewState.Changed, (int)LibraryListFindingCode.LinkChanged)]
    public static async Task SafeDrift(string occupant, int state, int code)
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        fixture.SourceFile();
        fixture.Record(LibraryReadWorkspace.ReviewPath);
        if (occupant == "ordinary")
        {
            fixture.Files.WriteText(LibraryReadWorkspace.ReviewPath, "local occupant\n");
        }
        else if (occupant == "different-target")
        {
            fixture.SourceFile(".agents/directives/other.md");
            fixture.Files.CreateFileSymbolicLink(LibraryReadWorkspace.ReviewPath, "../../shared/team/.agents/directives/other.md");
        }

        var before = fixture.Snapshot();
        var result = await fixture.ListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal((LibraryLinkViewState)state, Assert.Single(Assert.Single(result.Result.Libraries).Paths).State);
        var finding = Assert.Single(result.Result.Findings);
        Assert.Equal((LibraryListFindingCode)code, finding.Code);
        Assert.Equal(LibraryReadWorkspace.ReviewPath, finding.Path);
        Assert.Equal("team-knowledge", finding.LibraryId);
        Assert.Equal(LibraryCoverage.Complete, result.Result.Coverage);
        Assert.Equal(LibraryListInventoryState.NotRequested, result.Result.Inventory);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library List accepts an exact dangling registered link without resolving target existence"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task ExactDanglingLinkIsCurrent()
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        fixture.Record(LibraryReadWorkspace.ReviewPath);
        fixture.CurrentLink();
        var before = fixture.Snapshot();
        var result = await fixture.ListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var path = Assert.Single(Assert.Single(result.Result.Libraries).Paths);
        Assert.Equal(LibraryLinkViewState.Current, path.State);
        Assert.Equal(LibraryReadWorkspace.ReviewTarget, path.ObservedRelativeLink);
        Assert.Equal(before, fixture.Snapshot());
    }

    private static string Render(LibraryListResult result)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryListResult>(result, new(CliFormat.Json, CliDetail.Standard, null)),
            LibraryListPresentation.Rendering).PrimaryContent;
}
