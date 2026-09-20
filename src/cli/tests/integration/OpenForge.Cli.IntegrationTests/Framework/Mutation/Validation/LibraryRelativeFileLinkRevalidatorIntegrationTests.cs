using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Validation;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryRelativeFileLinkRevalidatorIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Relative link validation independently checks ordinary parents and leaves initially or under lease")]
    [InlineData("ordinary", false), InlineData("linked-parent", false), InlineData("linked-root", false), InlineData("occupied", false)]
    [InlineData("ordinary", true), InlineData("linked-parent", true), InlineData("linked-root", true), InlineData("occupied", true)]
    public static async Task RevalidatesPhysicalParentsAndLeaves(string scenario, bool underLease)
    {
        using var temporary = TemporaryWorkspace.Create("library-link-revalidation");
        using var locks = WorkspaceLockTestStore.Create("library-link-revalidation-lock");
        temporary.CreateDirectory("actual");
        if (scenario == "linked-root")
        {
            temporary.CreateDirectorySymbolicLink(".agents", "actual");
        }
        else if (scenario == "linked-parent")
        {
            temporary.CreateDirectorySymbolicLink(".agents/directives", "../actual");
        }
        else
        {
            temporary.CreateDirectory(".agents/directives");
            if (scenario == "occupied")
            {
                temporary.CreateFile(".agents/directives/a.md", "local");
            }
        }
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var effect = RelativeFileLinkEffect.Create(CanonicalRelativePath.Create(".agents/directives/a.md"),
            RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../../shared/team/.agents/directives/a.md"));
        var resolver = new PhysicalPathResolver();
        RelativeFileLinkValidationResult result;
        if (underLease)
        {
            await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
                new WorkspaceLockRequest(workspace, "library attach", Guid.NewGuid()), TestContext.Current.CancellationToken)).Lease);

            result = await RelativeFileLinkRevalidator.ValidateAsync(resolver, lease, effect, TestContext.Current.CancellationToken);
        }
        else
        {
            result = await RelativeFileLinkRevalidator.ValidateAsync(resolver, workspace, effect, TestContext.Current.CancellationToken);
        }

        if (scenario == "ordinary")
        {
            Assert.Equal(RelativeFileLinkValidationState.Matched, result.State);
        }
        else
        {
            Assert.Contains(result.State, new[] { RelativeFileLinkValidationState.Blocked, RelativeFileLinkValidationState.Mismatched });
        }
        Assert.Empty(Directory.EnumerateFileSystemEntries(temporary.Combine("actual")));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Under-lease link revalidation detects a new leaf after an independently observed missing state")]
    public static async Task DetectsLeafRace()
    {
        using var temporary = TemporaryWorkspace.Create("library-link-race");
        using var locks = WorkspaceLockTestStore.Create("library-link-race-lock");
        temporary.CreateDirectory(".agents");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var effect = RelativeFileLinkEffect.Create(CanonicalRelativePath.Create(".agents/a.md"),
            RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/.agents/a.md"));
        var resolver = new PhysicalPathResolver();
        var initial = NoFollowLeafObservation.Missing(temporary.Combine(".agents/a.md"));
        Assert.False(File.Exists(initial.LogicalPath));
        Assert.False(Directory.Exists(initial.LogicalPath));
        Assert.Null(new FileInfo(initial.LogicalPath).LinkTarget);
        temporary.CreateFile(".agents/a.md", "arrived after preflight");
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new WorkspaceLockRequest(workspace, "library attach", Guid.NewGuid()), TestContext.Current.CancellationToken)).Lease);

        var result = await RelativeFileLinkRevalidator.ValidateAsync(resolver, lease, effect, TestContext.Current.CancellationToken);

        Assert.Equal(RelativeFileLinkValidationState.Mismatched, result.State);
        Assert.Equal("arrived after preflight", File.ReadAllText(temporary.Combine(".agents/a.md")));
    }
}
