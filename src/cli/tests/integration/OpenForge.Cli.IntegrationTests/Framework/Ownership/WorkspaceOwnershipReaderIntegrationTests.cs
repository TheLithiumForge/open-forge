using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Ownership;

/// <summary>
/// The lock is never a gate. These prove the property that makes that true: every
/// way of failing to read it still yields a document, and that document records
/// no ownership, so a caller proceeds and nothing is deleted on a bad read.
/// </summary>
[Trait("Feature", "workspace-ownership"), Trait("Evidence", "Integration")]
public sealed class WorkspaceOwnershipReaderIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A workspace with no lock reads as absent and owns nothing")]
    public async Task AbsentLockOwnsNothing()
    {
        using var workspace = TemporaryWorkspace.Create("ownership-absent");

        var read = await ReadAsync(workspace);

        Assert.Equal(WorkspaceOwnershipReadState.Absent, read.State);
        Assert.Null(read.Cause);
        Assert.False(read.IsTrustworthy);
        // A missing lock is still a safe basis: it is created, not replaced.
        Assert.Equal(FileExpectationKind.Missing, Assert.IsType<FileStateSnapshot>(read.Snapshot).Kind);
        AssertOwnsNothing(read);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A recorded lock reads back the ownership it carries")]
    public async Task RecordedLockReadsBack()
    {
        using var workspace = TemporaryWorkspace.Create("ownership-complete");
        workspace.WriteText(
            WorkspaceOwnershipDefinitions.RelativePath,
            """
            {
              "schemaVersion": 1,
              "framework": {
                "source": { "id": "open-forge", "version": "0.1.0" },
                "paths": [".agents/loader.md"],
                "regions": [{ "path": ".agents/directives/_directives.md", "region": "entries" }]
              },
              "extensions": [{ "id": "planning", "paths": [".agents/templates/planning/plan.md"] }]
            }
            """);

        var read = await ReadAsync(workspace);

        Assert.Equal(WorkspaceOwnershipReadState.Complete, read.State);
        Assert.True(read.IsTrustworthy);
        Assert.Equal("open-forge", read.Document.Framework?.Source.Id);
        Assert.Equal([".agents/loader.md"], read.Document.Framework?.Paths);
        Assert.Equal(["planning"], read.Document.OwnersOf(".agents/templates/planning/plan.md"));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "An unintelligible lock is reported without claiming any ownership")]
    public async Task UnintelligibleLockOwnsNothing()
    {
        using var workspace = TemporaryWorkspace.Create("ownership-invalid");
        workspace.WriteText(WorkspaceOwnershipDefinitions.RelativePath, "{ this is not json");

        var read = await ReadAsync(workspace);

        Assert.Equal(WorkspaceOwnershipReadState.Invalid, read.State);
        Assert.NotNull(read.Cause);
        Assert.False(read.IsTrustworthy);
        // Unintelligible but readable: the bytes are known, so a writer can
        // safely replace them with a document rebuilt from empty.
        var snapshot = Assert.IsType<FileStateSnapshot>(read.Snapshot);
        Assert.Equal(FileExpectationKind.File, snapshot.Kind);
        Assert.True(snapshot.HasBytes);
        AssertOwnsNothing(read);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A lock that is a directory rather than a file owns nothing")]
    public async Task DirectoryInPlaceOfLockOwnsNothing()
    {
        using var workspace = TemporaryWorkspace.Create("ownership-directory");
        workspace.CreateDirectory(WorkspaceOwnershipDefinitions.RelativePath);

        var read = await ReadAsync(workspace);

        Assert.Equal(WorkspaceOwnershipReadState.Unavailable, read.State);
        Assert.NotNull(read.Cause);
        // No bytes were read, so there is no safe basis for replacement and a
        // writer must skip the lock rather than overwrite what it cannot describe.
        Assert.Null(read.Snapshot);
        AssertOwnsNothing(read);
    }

    /// <summary>
    /// The lock is read under the same no-follow leaf boundary as every other
    /// workspace file, so a symlinked lock cannot make the read leave the
    /// workspace.
    /// </summary>
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A symlinked lock is refused rather than followed out of the workspace")]
    public async Task SymlinkedLockIsNotFollowed()
    {
        using var outside = TemporaryWorkspace.Create("ownership-outside");
        var target = outside.CreateFile(
            "planted.json",
            """{"extensions":[{"id":"planted","paths":["victim.md"]}]}""");

        using var workspace = TemporaryWorkspace.Create("ownership-symlink");
        workspace.CreateDirectory(WorkspaceOwnershipDefinitions.DirectoryName);
        if (!workspace.TryCreateFileSymbolicLink(WorkspaceOwnershipDefinitions.RelativePath, target, out _))
        {
            Assert.Skip("Creating a file symbolic link requires privileges this host does not grant.");
        }

        var read = await ReadAsync(workspace);

        Assert.Equal(WorkspaceOwnershipReadState.Unavailable, read.State);
        AssertOwnsNothing(read);
        Assert.Empty(read.Document.OwnersOf("victim.md"));
    }

    private static async ValueTask<WorkspaceOwnershipRead> ReadAsync(TemporaryWorkspace workspace)
        => await WorkspaceOwnershipReader.ReadAsync(
            new PhysicalPathResolver(),
            new CliWorkspace(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace),
            CancellationToken.None);

    private static void AssertOwnsNothing(WorkspaceOwnershipRead read)
    {
        Assert.Null(read.Document.Framework);
        Assert.Empty(read.Document.Extensions);
        Assert.Empty(read.Document.Libraries);
    }
}
