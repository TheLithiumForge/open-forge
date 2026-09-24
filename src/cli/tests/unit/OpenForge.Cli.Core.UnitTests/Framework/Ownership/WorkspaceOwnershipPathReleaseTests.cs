using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Ownership;

[Trait("Feature", "workspace-ownership"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class WorkspaceOwnershipPathReleaseTests
{
    [Fact]
    public void ExplicitLibraryPathReleaseRetainsRegistrationAndOtherSources()
    {
        var document = WorkspaceOwnershipDocument.Empty with
        {
            Libraries = [new("library", "source", "destination", ["one.md", "two.md"])],
        };
        var current = Observe(document);
        var store = new WorkspaceOwnershipStore();
        Assert.Equal(OwnershipWritePlanState.Unchanged,
            store.PlanContentPathRelease(current, ["destination/one.md"]).State);
        var result = store.PlanContentPathRelease(current, ["destination/one.md"], libraryPaths: [new("library", "one.md")]);
        var change = Assert.IsType<PlannedFileChange>(result.Change);
        var written = Assert.IsType<WorkspaceOwnershipDocument>(WorkspaceOwnershipCodec.Read(change.IntendedBytes.ToArray()).Document);
        var library = Assert.Single(written.Libraries);
        Assert.Equal("library", library.Id);
        Assert.Equal("source", library.SourceRoot);
        Assert.Equal(["two.md"], library.Paths);
        Assert.Throws<ArgumentException>(() => store.PlanContentPathRelease(current,
            ["destination/one.md"], libraryPaths: [new("library", "unrecorded.md")]));
    }

    [Fact]
    public void ReleasesEveryContentOwnerButPreservesRegistrationsAndUnselectedClaims()
    {
        var document = new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            new FrameworkOwnership(new OwnedSource("open-forge", "1.0.0"), ["one.md", "two.md"], [new("one.md", "entries")]),
            [new("first", "1.0.0", "bundled", [], ["one.md", "three.md"], []),
                new("second", "1.0.0", "bundled", ["first"], ["one.md"], [new("one.md", "entries")])],
            [new("library", "source", "destination", ["one.md"])]);
        var current = Observe(document);

        var result = new WorkspaceOwnershipStore().PlanContentPathRelease(current, ["one.md"]);

        Assert.Equal(OwnershipWritePlanState.Planned, result.State);
        var change = Assert.IsType<PlannedFileChange>(result.Change);
        Assert.Equal(current.Snapshot?.Expectation, change.Expectation);
        var written = Assert.IsType<WorkspaceOwnershipDocument>(WorkspaceOwnershipCodec.Read(change.IntendedBytes.ToArray()).Document);
        Assert.Equal(["two.md"], written.Framework?.Paths);
        Assert.Empty(Assert.IsType<FrameworkOwnership>(written.Framework).Regions);
        Assert.Equal(2, written.Extensions.Length);
        Assert.Equal(["three.md"], written.Extensions[0].Paths);
        Assert.Empty(written.Extensions[1].Paths);
        Assert.Empty(written.Extensions[1].Regions);
        Assert.Equal(["first"], written.Extensions[1].Dependencies);
        var library = Assert.Single(written.Libraries);
        Assert.Equal("library", library.Id);
        Assert.Equal("source", library.SourceRoot);
        Assert.Equal("destination", library.DestinationRoot);
        Assert.Equal(["one.md"], library.Paths);
    }

    [Fact]
    public void NoClaimDoesNotNormalizeOrCreateLock()
    {
        var absent = WorkspaceOwnershipRead.Absent(LockPath());
        var store = new WorkspaceOwnershipStore();
        Assert.Equal(OwnershipWritePlanState.Unchanged, store.PlanContentPathRelease(absent, ["one.md"]).State);
        Assert.Equal(OwnershipWritePlanState.Unchanged, store.PlanContentPathRelease(Observe(WorkspaceOwnershipDocument.Empty), ["one.md"]).State);
    }

    [Theory]
    [InlineData((int)WorkspaceOwnershipReadState.Invalid)]
    [InlineData((int)WorkspaceOwnershipReadState.Unavailable)]
    public void UnknownOwnershipCannotReleaseClaims(int state)
    {
        var observed = Observe(WorkspaceOwnershipDocument.Empty) with { State = (WorkspaceOwnershipReadState)state, Cause = "Unreadable ownership." };
        var result = new WorkspaceOwnershipStore().PlanContentPathRelease(observed, ["one.md"]);
        Assert.Equal(OwnershipWritePlanState.Skipped, result.State);
        Assert.Null(result.Change);
    }

    [Theory]
    [InlineData("../outside.md")]
    [InlineData("./one.md")]
    public void NoncanonicalPathsAreRejected(string path)
        => Assert.Throws<ArgumentException>(() => new WorkspaceOwnershipStore().PlanContentPathRelease(
            WorkspaceOwnershipRead.Absent(LockPath()), [path]));

    private static WorkspaceOwnershipRead Observe(WorkspaceOwnershipDocument document)
    {
        var path = LockPath();
        return new(WorkspaceOwnershipReadState.Complete, document, path,
            FileStateSnapshot.File(path, path, WorkspaceOwnershipCodec.Write(document)), Cause: null);
    }

    private static string LockPath() => Path.Combine(Path.GetTempPath(), "ownership-path-release", ".agents", "open-forge.lock.json");
}
