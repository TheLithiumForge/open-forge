using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Application;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryRecoveryLeaseGuardIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Explicit link recovery rejects released leases foreign workspaces and entries outside the verified preparation")]
    [InlineData("released"), InlineData("foreign-workspace"), InlineData("foreign-entry")]
    public static async Task RejectsUnheldOrMismatchedRecoveryAuthority(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("library-recovery-lease");
        using var foreign = TemporaryWorkspace.Create("library-recovery-foreign");
        using var locks = WorkspaceLockTestStore.Create("library-recovery-lease-lock");
        temporary.CreateDirectory(".agents");
        var source = temporary.CreateFile("source.md", "source");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var otherWorkspace = new CliWorkspace(foreign.Path, foreign.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var link = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../source.md");
        var effect = RelativeFileLinkEffect.Delete(CanonicalRelativePath.Create(".agents/a.md"), link);
        var input = RecoveryBundleInput.Create(workspace, command: "library detach",
            attribution: RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Detach, workspace),
            operationId: Guid.NewGuid(), targets:
            [RecoveryBundleTarget.Create(effect, NoFollowLeafObservation.CreateRelativeFileLink(temporary.Combine(".agents/a.md"), link))]);
        var leaseWorkspace = scenario == "foreign-workspace" ? otherWorkspace : workspace;
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new WorkspaceLockRequest(leaseWorkspace, "explicit recovery", Guid.NewGuid()), TestContext.Current.CancellationToken)).Lease);
        RecoveryBundlePreparation? preparation = null;
        try
        {
            preparation = Assert.IsType<RecoveryBundlePreparation>((await RecoveryBundleStore.PrepareAsync(input, TestContext.Current.CancellationToken)).Preparation);
            var entry = Assert.Single(preparation.Entries);
            if (scenario == "released")
            {
                await lease.DisposeAsync();
            }
            if (scenario == "foreign-entry")
            {
                entry = RecoveryEntry.Create(ordinal: 0, logicalPath: CanonicalRelativePath.Create(".agents/other.md"),
                    kind: RecoveryEntryKind.RelativeFileLinkDelete, prior: RecoveryEntryState.RelativeLink(link), intended: RecoveryEntryState.Missing);
            }

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await RelativeFileLinkRecoveryApplier.ApplyAsync(new PhysicalPathResolver(), lease, preparation, entry, TestContext.Current.CancellationToken);
            });

            Assert.Empty(Directory.EnumerateFileSystemEntries(temporary.Combine(".agents")));
            Assert.Equal("source", File.ReadAllText(source));
            Assert.True(File.Exists(preparation.BundlePath));
        }
        finally
        {
            if (preparation is not null)
            {
                File.Delete(preparation.BundlePath);
            }
        }
    }
}
