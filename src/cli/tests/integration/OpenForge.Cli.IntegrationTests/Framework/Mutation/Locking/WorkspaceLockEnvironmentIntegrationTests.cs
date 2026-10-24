using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking;

[CollectionDefinition("Workspace lock environment", DisableParallelization = true)]
public sealed class WorkspaceLockEnvironmentCollection;

[Collection("Workspace lock environment")]
[Trait("Boundary", "OS"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
public sealed class WorkspaceLockEnvironmentIntegrationTests
{
    [Fact(DisplayName = "Unavailable relative legacy data home permits local ownership and has no recovery location")]
    public async Task RelativeDataHomeDoesNotBlockLocalOwnership()
    {
        using var temporary = TemporaryWorkspace.Create("lock-relative-home");
        using var store = WorkspaceLockTestStore.Create("lock-relative-cleanup");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _ = store.Track(workspace);
        var prior = Environment.GetEnvironmentVariable(CurrentUserDataHome.OverrideVariable);
        try
        {
            Environment.SetEnvironmentVariable(CurrentUserDataHome.OverrideVariable, "relative-data-home");
            var result = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(
                new(workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
            await using var lease = Assert.IsType<WorkspaceLockLease>(result.Lease);
            Assert.Equal(LegacyWorkspaceLockState.Unavailable, lease.LegacyState);
            Assert.Null(lease.LegacyStoreRoot);
            Assert.Null(RecoveryStoreLocator.Legacy(workspace));
            Assert.True(File.Exists(WorkspaceRuntimePaths.IgnorePath(workspace)));
        }
        finally
        {
            Environment.SetEnvironmentVariable(CurrentUserDataHome.OverrideVariable, prior);
        }
    }
}
