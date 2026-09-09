using OpenForge.Cli.Core.Commands.Library.Shared.Application;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Application;

[Trait("Feature", "library-mapping"), Trait("Evidence", "Integration")]
public sealed class LibraryLinkPreflightIntegrationTests
{
    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task MissingParentRequiresItsExplicitCreationInTheReviewedPlan(bool planned)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var locks = WorkspaceLockTestStore.Create("library-link-preflight");
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new(workspace.Workspace, "library attach", Guid.NewGuid()), TestContext.Current.CancellationToken)).Lease);
        PlannedDirectoryCreation[] directories = planned
            ? [PlannedDirectoryCreation.Create(FileExpectation.Missing(workspace.Absolute("docs")))]
            : [];
        var preflight = new LibraryLinkPreflight(lease, directories);
        var effect = RelativeFileLinkEffect.Create(CanonicalRelativePath.Create("docs/a.md"),
            RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/team-knowledge/a.md"));
        var before = workspace.Snapshot();

        var result = await preflight.ValidateAsync(effect, TestContext.Current.CancellationToken);

        Assert.Equal(planned ? RelativeFileLinkValidationState.Matched : RelativeFileLinkValidationState.Blocked, result.State);
        Assert.Equal(before, workspace.Snapshot());
        Assert.False(Directory.Exists(workspace.Absolute("docs")));
    }

    [Fact]
    public async Task RedirectedParentCannotSatisfyAPlannedDirectoryCreation()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var locks = WorkspaceLockTestStore.Create("library-link-preflight");
        workspace.Directory("redirected");
        workspace.DirectoryLink(path: "docs", target: "redirected");
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new(workspace.Workspace, "library attach", Guid.NewGuid()), TestContext.Current.CancellationToken)).Lease);
        var preflight = new LibraryLinkPreflight(lease,
            [PlannedDirectoryCreation.Create(FileExpectation.Missing(workspace.Absolute("docs")))]);
        var effect = RelativeFileLinkEffect.Create(CanonicalRelativePath.Create("docs/a.md"),
            RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/team-knowledge/a.md"));
        var before = workspace.Snapshot();

        var result = await preflight.ValidateAsync(effect, TestContext.Current.CancellationToken);

        Assert.Equal(RelativeFileLinkValidationState.Blocked, result.State);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Empty(Directory.EnumerateFileSystemEntries(workspace.Absolute("redirected")));
    }
}
