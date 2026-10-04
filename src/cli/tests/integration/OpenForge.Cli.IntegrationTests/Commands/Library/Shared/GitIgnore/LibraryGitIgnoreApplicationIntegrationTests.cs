using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.GitIgnore;

[Trait("Feature", "library-git-ignore"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class LibraryGitIgnoreApplicationIntegrationTests
{
    [Theory]
    [InlineData("create"), InlineData("replace"), InlineData("cancelled"), InlineData("changed-ignore"), InlineData("missing-ignore-recovery")]
    public static async Task WholePlanRequiresPairedIgnoreRecoveryAndPublishesOwnershipLast(string scenario)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        using var locks = WorkspaceLockTestStore.Create("library-ignore-application");
        workspace.Source("a.md");
        workspace.Directory("docs");
        workspace.Write(".agents/open-forge.json", """{"allowInstallPaths":["docs",".gitignore"]}""");
        if (scenario == "replace") workspace.Write(".gitignore", "# authored\n");
        var plan = await CapturePlan(workspace);
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(new(workspace.Workspace, "library attach", Guid.NewGuid()), TestContext.Current.CancellationToken)).Lease);
        var prepared = await LibraryMutationOperationSupport.PrepareRecoveryAsync(new()
        {
            Lease = lease,
            Command = "library attach",
            Operation = RecoveryBundleOperation.Attach,
            Permissions = plan.Permissions,
            Links = plan.Links,
            GeneratedRegions = plan.GeneratedRegions,
            GitIgnore = scenario == "missing-ignore-recovery" ? null : plan.GitIgnore,
            Ownership = plan.Input.Ownership,
            OwnershipChange = plan.OwnershipChange,
            Mappings = plan.Input.Mappings,
        }, TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var read = await RecoveryBundleReader.ReadFinalAsync(workspace.Workspace, preparation.BundlePath, TestContext.Current.CancellationToken);
            var verified = Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified);
            if (scenario != "missing-ignore-recovery")
            {
                var entry = Assert.Single(verified.Entries, entry => entry.TargetPath == ".gitignore");
                Assert.Equal(scenario == "replace" ? RecoveryEntryKind.OrdinaryReplace : RecoveryEntryKind.OrdinaryCreate, entry.Kind);
                Assert.Equal(scenario == "replace" ? RecoveryEntryStateKind.OrdinaryFile : RecoveryEntryStateKind.Missing, entry.Prior.Kind);
            }
            if (scenario == "changed-ignore") workspace.Write(".gitignore", "# changed before whole-plan validation\n");
            var before = workspace.Snapshot();
            using var cancellation = new CancellationTokenSource();
            if (scenario == "cancelled") await cancellation.CancelAsync();
            var execution = await LibraryAttachApplication.ApplyAsync(new LibraryAttachApplicationInput { Lease = lease, Plan = plan, RecoveryPreparation = preparation }, cancellation.Token);
            if (scenario is "create" or "replace")
            {
                Assert.Equal(FilesystemVerificationState.Verified, Assert.IsType<FileChangeReceipt>(execution.GitIgnore).VerificationState);
                Assert.Equal(FilesystemVerificationState.Verified, Assert.IsType<FileChangeReceipt>(execution.Record).VerificationState);
                Assert.Equal(LibraryRecordPublicationOrder.Last, execution.RecordPublicationOrder);
                Assert.Equal(new[] { "docs/a.md", ".gitignore", ".agents/open-forge.lock.json" }, Assert.IsType<LibrarySourceEffectScopeFacts>(execution.SourceEffectScope).AttemptedMutationTargets.Select(path => path.Value));
                Assert.Equal((scenario == "replace" ? "# authored\n" : string.Empty) + "# BEGIN OPEN FORGE LIBRARIES\n/docs/a.md\n# END OPEN FORGE LIBRARIES\n", File.ReadAllText(workspace.Absolute(".gitignore")));
            }
            else
            {
                Assert.Equal(before, workspace.Snapshot());
                Assert.Empty(execution.Links);
                Assert.Null(execution.GitIgnore);
                Assert.Null(execution.Record);
            }
            Assert.True(File.Exists(preparation.BundlePath));
            Assert.Equal(LibraryMutationWorkspace.SourceBytes, File.ReadAllText(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/a.md")));
        }
        finally
        {
            File.Delete(preparation.BundlePath);
        }
    }

    [Fact]
    public static async Task FailedIgnoreReplaceRetainsVerifiedLinksAndRecoveryWithoutPublishingOwnership()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement denial requires Windows file sharing.");
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        workspace.Write(".gitignore", "# authored\n");
        workspace.Write(".agents/open-forge.json", """{"allowInstallPaths":["docs",".gitignore"]}""");
        using var held = new FileStream(workspace.Absolute(".gitignore"), FileMode.Open, FileAccess.Read, FileShare.Read);
        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(workspace.Attach("team-knowledge", "docs", LibraryMode.Apply) with { GitIgnore = true }, TestContext.Current.CancellationToken);
        var recovery = Assert.IsType<string>(result.Result.Application.Recovery.Path);
        try
        {
            Assert.Equal(CliSemanticStatus.Failed, result.Status);
            Assert.Equal("../shared/team-knowledge/a.md", new FileInfo(workspace.Absolute("docs/a.md")).LinkTarget);
            Assert.Equal("# authored\n", File.ReadAllText(workspace.Absolute(".gitignore")));
            Assert.False(File.Exists(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath)));
            Assert.Null(result.Result.Application.RecordPublication.PublishedLast);
            Assert.True(File.Exists(recovery));
            var read = await RecoveryBundleReader.ReadFinalAsync(workspace.Workspace, recovery, TestContext.Current.CancellationToken);
            var entries = Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified).Entries;
            Assert.Equal(RecoveryEntryKind.OrdinaryReplace, Assert.Single(entries, entry => entry.TargetPath == ".gitignore").Kind);
            Assert.Equal(LibraryMutationWorkspace.SourceBytes, File.ReadAllText(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/a.md")));
        }
        finally
        {
            File.Delete(recovery);
        }
    }

    private static async Task<LibraryAttachPlan> CapturePlan(LibraryMutationWorkspace workspace)
    {
        LibraryAttachPlan? captured = null;
        var operation = new LibraryAttachOperation(workspace.Permissions, (_, plan, _, _) =>
        {
            captured = plan;
            return ValueTask.FromResult(CliPromptReply<bool>.Cancelled());
        });
        var result = await operation.ExecuteAsync(workspace.Attach("team-knowledge", "docs", LibraryMode.Apply) with { GitIgnore = true, Automatic = false, AllowPrompt = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        return Assert.IsType<LibraryAttachPlan>(captured);
    }
}
