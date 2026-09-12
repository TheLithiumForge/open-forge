using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Application;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryRelativeFileLinkRecoveryIntegrationTests
{
    private const string Destination = ".agents/a.md";
    private const string RawTarget = "../shared/.agents/a.md";

    [Theory(DisplayName = "Explicit link recovery reverses only exact intended create or delete and can recreate a dangling link")]
    [InlineData(false, false), InlineData(true, false), InlineData(true, true)]
    public static async Task RestoresExactPriorObject(bool originalDelete, bool dangling)
    {
        using var temporary = TemporaryWorkspace.Create("library-link-recovery");
        using var locks = WorkspaceLockTestStore.Create("library-link-recovery-lock");
        temporary.CreateDirectory(".agents");
        var source = temporary.CreateFile("shared/.agents/a.md", "source remains");
        var path = temporary.Combine(Destination);
        if (dangling)
        {
            File.Delete(source);
        }
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var link = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, RawTarget);
        var effect = originalDelete ? RelativeFileLinkEffect.Delete(CanonicalRelativePath.Create(Destination), link)
            : RelativeFileLinkEffect.Create(CanonicalRelativePath.Create(Destination), link);
        var before = originalDelete ? NoFollowLeafObservation.CreateRelativeFileLink(path, link) : NoFollowLeafObservation.Missing(path);
        var input = Input(workspace, RecoveryBundleTarget.Create(effect, before));
        RecoveryBundlePreparation? preparation = null;
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new WorkspaceLockRequest(workspace, "explicit recovery", Guid.NewGuid()), TestContext.Current.CancellationToken)).Lease);
        try
        {
            preparation = Assert.IsType<RecoveryBundlePreparation>((await RecoveryBundleStore.PrepareAsync(input, TestContext.Current.CancellationToken)).Preparation);
            if (!originalDelete)
            {
                File.CreateSymbolicLink(path, RawTarget);
            }
            var entry = Assert.Single(preparation.Entries);

            var result = await RelativeFileLinkRecoveryApplier.ApplyAsync(new PhysicalPathResolver(), lease, preparation, entry, TestContext.Current.CancellationToken);

            Assert.Equal(RelativeFileLinkRecoveryState.Restored, result.State);
            Assert.Equal(originalDelete ? RawTarget : null, new FileInfo(path).LinkTarget);
            Assert.Equal(originalDelete ? NoFollowLeafState.RelativeFileLink : NoFollowLeafState.Missing,
                Assert.IsType<NoFollowLeafObservation>(result.After).State);
            Assert.True(File.Exists(preparation.BundlePath));
            if (dangling)
            {
                Assert.False(File.Exists(source));
            }
            else
            {
                Assert.Equal("source remains", File.ReadAllText(source));
            }
        }
        finally
        {
            if (new FileInfo(path).LinkTarget == RawTarget)
            {
                File.Delete(path);
            }
            if (preparation is not null)
            {
                File.Delete(preparation.BundlePath);
            }
        }
    }

    [Theory(DisplayName = "Explicit link recovery blocks third-state occupants and linked parents while preserving recovery evidence")]
    [InlineData(false, "ordinary"), InlineData(false, "different-link"), InlineData(false, "absolute-link")]
    [InlineData(true, "ordinary"), InlineData(true, "different-link"), InlineData(true, "absolute-link"), InlineData(true, "parent")]
    public static async Task RejectsThirdOrUnsafeObject(bool originalDelete, string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("library-link-recovery-blocked");
        using var locks = WorkspaceLockTestStore.Create("library-link-recovery-blocked-lock");
        var source = temporary.CreateFile("shared/.agents/a.md", "source remains");
        if (scenario == "parent")
        {
            temporary.CreateDirectory("actual");
            temporary.CreateDirectorySymbolicLink(".agents", "actual");
        }
        else
        {
            temporary.CreateDirectory(".agents");
        }
        var path = temporary.Combine(Destination);
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var link = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, RawTarget);
        var effect = originalDelete ? RelativeFileLinkEffect.Delete(CanonicalRelativePath.Create(Destination), link)
            : RelativeFileLinkEffect.Create(CanonicalRelativePath.Create(Destination), link);
        var before = originalDelete ? NoFollowLeafObservation.CreateRelativeFileLink(path, link) : NoFollowLeafObservation.Missing(path);
        var input = Input(workspace, RecoveryBundleTarget.Create(effect, before));
        RecoveryBundlePreparation? preparation = null;
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new WorkspaceLockRequest(workspace, "explicit recovery", Guid.NewGuid()), TestContext.Current.CancellationToken)).Lease);
        try
        {
            preparation = Assert.IsType<RecoveryBundlePreparation>((await RecoveryBundleStore.PrepareAsync(input, TestContext.Current.CancellationToken)).Preparation);
            switch (scenario)
            {
                case "ordinary": temporary.CreateFile(Destination, "local occupant"); break;
                case "different-link": temporary.CreateFileSymbolicLink(Destination, "different.md"); break;
                case "absolute-link": temporary.CreateFileSymbolicLink(Destination, source); break;
            }
            var rawBefore = new FileInfo(path).LinkTarget;

            var result = await RelativeFileLinkRecoveryApplier.ApplyAsync(new PhysicalPathResolver(), lease, preparation,
                Assert.Single(preparation.Entries), TestContext.Current.CancellationToken);

            Assert.Contains(result.State, new[] { RelativeFileLinkRecoveryState.Mismatched, RelativeFileLinkRecoveryState.Blocked });
            Assert.Equal(rawBefore, new FileInfo(path).LinkTarget);
            Assert.True(File.Exists(preparation.BundlePath));
            Assert.Equal("source remains", File.ReadAllText(source));
            if (scenario == "ordinary")
            {
                Assert.Equal("local occupant", File.ReadAllText(path));
            }
            if (scenario == "parent")
            {
                Assert.Empty(Directory.EnumerateFileSystemEntries(temporary.Combine("actual")));
            }
        }
        finally
        {
            if (preparation is not null)
            {
                File.Delete(preparation.BundlePath);
            }
        }
    }

    private static RecoveryBundleInput Input(CliWorkspace workspace, RecoveryBundleTarget target)
        => RecoveryBundleInput.Create(workspace, command: "library sync",
            attribution: RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Sync, workspace),
            operationId: Guid.NewGuid(), targets: [target]);
}
