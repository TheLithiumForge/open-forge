using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Application;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryRelativeFileLinkApplierIntegrationTests
{
    private const string Destination = ".agents/a.md";
    private const string RawTarget = "../shared/team/.agents/a.md";

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Typed link application creates and deletes exact real links including dangling links without source effects")]
    [InlineData("create"), InlineData("delete"), InlineData("dangling-delete")]
    public static async Task AppliesExactLinkObject(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("library-link-apply");
        using var locks = WorkspaceLockTestStore.Create("library-link-apply-lock");
        temporary.CreateDirectory(".agents");
        var source = temporary.CreateFile("shared/team/.agents/a.md", "source bytes");
        var path = temporary.Combine(Destination);
        var delete = scenario != "create";
        if (delete)
        {
            temporary.CreateFileSymbolicLink(Destination, RawTarget);
        }
        if (scenario == "dangling-delete")
        {
            File.Delete(source);
        }
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var link = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, RawTarget);
        var effect = delete ? RelativeFileLinkEffect.Delete(CanonicalRelativePath.Create(Destination), link)
            : RelativeFileLinkEffect.Create(CanonicalRelativePath.Create(Destination), link);
        var before = delete ? NoFollowLeafObservation.CreateRelativeFileLink(path, link) : NoFollowLeafObservation.Missing(path);
        var input = Input(workspace, RecoveryBundleTarget.Create(effect, before));
        RecoveryBundlePreparation? preparation = null;
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new WorkspaceLockRequest(workspace, input.Command, input.OperationId), TestContext.Current.CancellationToken)).Lease);
        try
        {
            preparation = Assert.IsType<RecoveryBundlePreparation>((await RecoveryBundleStore.PrepareAsync(input, TestContext.Current.CancellationToken)).Preparation);

            var result = await RelativeFileLinkApplier.ApplyAsync(new PhysicalPathResolver(), lease,
                new RelativeFileLinkApplicationInput { Effect = effect, Expected = before, RecoveryPreparation = preparation }, TestContext.Current.CancellationToken);

            Assert.Equal(FilesystemEffectState.Applied, result.EffectState);
            Assert.Equal(FilesystemVerificationState.Verified, result.VerificationState);
            var after = Assert.IsType<NoFollowLeafObservation>(result.After);
            Assert.Equal(delete ? NoFollowLeafState.Missing : NoFollowLeafState.RelativeFileLink, after.State);
            Assert.Equal(delete ? null : RawTarget, new FileInfo(path).LinkTarget);
            Assert.Equal(delete ? null : link, after.RelativeFileLink);
            if (scenario == "dangling-delete")
            {
                Assert.False(File.Exists(source));
            }
            else
            {
                Assert.Equal("source bytes", File.ReadAllText(source));
            }
            Assert.True(File.Exists(preparation.BundlePath));
        }
        finally
        {
            DeleteCreatedLink(path);
            DeleteBundle(preparation);
        }
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Typed link application rejects changed leaves missing recovery and cancellation before any effect")]
    [InlineData("file"), InlineData("different-link"), InlineData("exact-existing-link"), InlineData("absolute-link"), InlineData("no-recovery"), InlineData("cancelled")]
    public static async Task RejectsUnsafeCreate(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("library-link-blocked");
        using var locks = WorkspaceLockTestStore.Create("library-link-blocked-lock");
        temporary.CreateDirectory(".agents");
        var source = temporary.CreateFile("shared/team/.agents/a.md", "source bytes");
        var path = temporary.Combine(Destination);
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var effect = RelativeFileLinkEffect.Create(CanonicalRelativePath.Create(Destination), RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, RawTarget));
        var before = NoFollowLeafObservation.Missing(path);
        var input = Input(workspace, RecoveryBundleTarget.Create(effect, before));
        RecoveryBundlePreparation? preparation = null;
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new WorkspaceLockRequest(workspace, input.Command, input.OperationId), TestContext.Current.CancellationToken)).Lease);
        try
        {
            if (scenario != "no-recovery")
            {
                preparation = Assert.IsType<RecoveryBundlePreparation>((await RecoveryBundleStore.PrepareAsync(input, TestContext.Current.CancellationToken)).Preparation);
            }
            switch (scenario)
            {
                case "file": temporary.CreateFile(Destination, "local bytes"); break;
                case "different-link": temporary.CreateFileSymbolicLink(Destination, "different.md"); break;
                case "exact-existing-link": temporary.CreateFileSymbolicLink(Destination, RawTarget); break;
                case "absolute-link": temporary.CreateFileSymbolicLink(Destination, source); break;
            }
            var originalTarget = new FileInfo(path).LinkTarget;
            using var cancellation = new CancellationTokenSource();
            if (scenario == "cancelled")
            {
                cancellation.Cancel();
            }

            var result = await RelativeFileLinkApplier.ApplyAsync(new PhysicalPathResolver(), lease,
                new RelativeFileLinkApplicationInput { Effect = effect, Expected = before, RecoveryPreparation = preparation }, cancellation.Token);

            Assert.Equal(FilesystemEffectState.NotStarted, result.EffectState);
            Assert.Null(result.After);
            Assert.Equal(originalTarget, new FileInfo(path).LinkTarget);
            Assert.Equal("source bytes", File.ReadAllText(source));
            if (scenario == "file")
            {
                Assert.Equal("local bytes", File.ReadAllText(path));
            }
            if (scenario is "cancelled" or "no-recovery")
            {
                Assert.False(File.Exists(path));
                Assert.Equal(scenario == "cancelled" ? FilesystemNotStartedReason.Cancelled : FilesystemNotStartedReason.ContractRejected, result.NotStartedReason);
            }
        }
        finally
        {
            DeleteBundle(preparation);
        }
    }

    private static RecoveryBundleInput Input(CliWorkspace workspace, RecoveryBundleTarget target)
        => RecoveryBundleInput.Create(workspace, command: "library attach",
            attribution: RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Attach, workspace),
            operationId: Guid.NewGuid(), targets: [target]);

    private static void DeleteCreatedLink(string path)
    {
        if (new FileInfo(path).LinkTarget == RawTarget)
        {
            File.Delete(path);
        }
    }

    private static void DeleteBundle(RecoveryBundlePreparation? preparation)
    {
        if (preparation is not null && File.Exists(preparation.BundlePath))
        {
            File.Delete(preparation.BundlePath);
        }
    }
}
