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
public sealed class LibraryRelativeFileLinkDeleteSafetyIntegrationTests
{
    [Theory(DisplayName = "Link delete rechecks missing changed ordinary and generic occupants immediately before application")]
    [InlineData("missing"), InlineData("ordinary"), InlineData("different-link"), InlineData("absolute-link"), InlineData("parent")]
    public static async Task DoesNotDeleteChangedObject(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("library-link-delete-race");
        using var locks = WorkspaceLockTestStore.Create("library-link-delete-race-lock");
        var source = temporary.CreateFile("source/a.md", "source");
        if (scenario == "parent")
        {
            temporary.CreateDirectory("actual");
            temporary.CreateDirectorySymbolicLink(".agents", "actual");
        }
        else
        {
            temporary.CreateDirectory(".agents");
        }
        var path = temporary.Combine(".agents/a.md");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var link = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../source/a.md");
        var effect = RelativeFileLinkEffect.Delete(CanonicalRelativePath.Create(".agents/a.md"), link);
        var before = NoFollowLeafObservation.CreateRelativeFileLink(path, link);
        var input = RecoveryBundleInput.Create(workspace, command: "library detach",
            attribution: RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Detach, workspace),
            operationId: Guid.NewGuid(), targets: [RecoveryBundleTarget.Create(effect, before)]);
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new WorkspaceLockRequest(workspace, input.Command, input.OperationId), TestContext.Current.CancellationToken)).Lease);
        RecoveryBundlePreparation? preparation = null;
        try
        {
            preparation = Assert.IsType<RecoveryBundlePreparation>((await RecoveryBundleStore.PrepareAsync(input, TestContext.Current.CancellationToken)).Preparation);
            switch (scenario)
            {
                case "ordinary": temporary.CreateFile(".agents/a.md", "local"); break;
                case "different-link": temporary.CreateFileSymbolicLink(".agents/a.md", "../source/./a.md"); break;
                case "absolute-link": temporary.CreateFileSymbolicLink(".agents/a.md", source); break;
            }
            var rawBefore = new FileInfo(path).LinkTarget;

            var receipt = await RelativeFileLinkApplier.ApplyAsync(new PhysicalPathResolver(), lease,
                new RelativeFileLinkApplicationInput { Effect = effect, Expected = before, RecoveryPreparation = preparation }, TestContext.Current.CancellationToken);

            Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
            Assert.Null(receipt.After);
            Assert.Equal(rawBefore, new FileInfo(path).LinkTarget);
            Assert.Equal("source", File.ReadAllText(source));
            Assert.True(File.Exists(preparation.BundlePath));
            if (scenario == "ordinary")
            {
                Assert.Equal("local", File.ReadAllText(path));
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
}
