using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using System.Text;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallPreservationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install preserves unselected scoped ownership without gating on its missing content"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task UnselectedScopedOwnershipIsPreservedWithoutRootEffects()
    {
        using var workspace = InstallOperationWorkspace.Create("install-scoped-preservation");
        var operation = CreateAutomaticOperation(workspace);

        var first = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, first.Status);

        const string scopedPath = "scoped-framework.md";
        const string scopedBlock = "<!-- open-forge:start -->\n\n"
            + "@.agents/loader.md\n"
            + "<!-- open-forge:end -->\n";
        var scopedContents = OpenForgeDocumentSeed.Metadata(
            description: "Scoped agent instructions",
            tags: ["Scoped"],
            body: scopedBlock);
        var scopedPhysicalPath = workspace.Combine(scopedPath);
        File.WriteAllBytes(scopedPhysicalPath, Encoding.UTF8.GetBytes(scopedContents));
        try
        {
            var reader = await WorkspaceOwnershipReader.ReadAsync(
                new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
            var current = Assert.IsType<FrameworkOwnership>(reader.Document.Framework);
            var plan = new WorkspaceOwnershipStore().PlanFrameworkOwnership(reader,
                current with { Regions = [.. current.Regions, new OwnedRegion(scopedPath, "open-forge")] });
            var change = Assert.IsType<PlannedFileChange>(plan.Change);
            await File.WriteAllBytesAsync(workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath),
                [.. change.IntendedBytes], TestContext.Current.CancellationToken);

            var beforeNoOp = workspace.SnapshotHashes();
            var noOp = await operation.ExecuteAsync(
                workspace.Request(automatic: true),
                TestContext.Current.CancellationToken);

            Assert.True(
                noOp.Status == CliSemanticStatus.Complete,
                string.Join(" | ", noOp.Findings.Select(finding => $"{finding.Code}: {finding.Cause}")));
            Assert.Empty(noOp.Findings);
            Assert.Equal(beforeNoOp, workspace.SnapshotHashes());
            Assert.Equal(
                scopedContents,
                await workspace.ReadTextAsync(scopedPath, TestContext.Current.CancellationToken));

            File.Delete(scopedPhysicalPath);
            var beforeBlocked = WithoutPath(workspace.SnapshotHashes(), scopedPath);
            var blocked = await operation.ExecuteAsync(
                workspace.Request(automatic: true),
                TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, blocked.Status);
            Assert.Empty(blocked.Findings);
            Assert.Empty(blocked.Facts.Effects);
            Assert.Equal(beforeBlocked, WithoutPath(workspace.SnapshotHashes(), scopedPath));
            Assert.False(workspace.Exists(scopedPath));
        }
        finally
        {
            if (File.Exists(scopedPhysicalPath))
            {
                File.Delete(scopedPhysicalPath);
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install blocks a recognized Framework recovery bundle and preserves it"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task AutomaticInstallBlocksRecognizedFrameworkRecoveryCandidate()
    {
        using var workspace = InstallOperationWorkspace.Create("install-foreign-recovery");
        const string foreignPath = "foreign-install-input.md";
        var priorBytes = "foreign prior content\n"u8.ToArray();
        var intendedBytes = "foreign intended content\n"u8.ToArray();
        workspace.WriteText(foreignPath, Encoding.UTF8.GetString(priorBytes));

        var physicalForeignPath = workspace.Combine(foreignPath);
        var before = FileStateSnapshot.File(
            physicalForeignPath,
            physicalForeignPath,
            priorBytes);
        var change = PlannedFileChange.Replace(
            before.Expectation,
            intendedBytes);
        var input = RecoveryBundleInput.Create(
            workspace.Workspace,
            command: IndexDefinitions.CommandIdentity,
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                workspace.Workspace),
            operationId: Guid.NewGuid(),
            targets:
            [
                RecoveryBundleTarget.Create(change, before),
            ]);
        var preparationResult = await RecoveryBundleStore.PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundlePreparationState.Prepared, preparationResult.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(preparationResult.Preparation);
        var bundleBeforeInstall = await File.ReadAllBytesAsync(
            preparation.BundlePath,
            TestContext.Current.CancellationToken);

        var candidateBefore = await RecoveryBundleReader.ReadFinalAsync(
            workspace.Workspace,
            preparation.BundlePath,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, candidateBefore.State);
        Assert.Equal(IndexDefinitions.CommandIdentity, candidateBefore.Verified?.Command);
        Assert.Equal(1, await workspace.ReadRecoveryCandidateCountAsync(
            TestContext.Current.CancellationToken));

        var result = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Confirmation(),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(automatic: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(InstallFindingCode.RecoveryConflict, finding.Code);
        var summary = Assert.IsType<InstallOperationSummary>(result.Summary);
        Assert.Equal(InstallManagementState.Blocked, summary.ManagementState);
        Assert.False(summary.HasRetainedWorkspaceEffects);
        Assert.Empty(result.Facts.Effects);
        Assert.Equal(InstallResultRecoveryState.NotRequired, result.Facts.Recovery.State);
        Assert.Null(result.Facts.Recovery.ResidualPath);
        Assert.Equal(1, await workspace.ReadRecoveryCandidateCountAsync(
            TestContext.Current.CancellationToken));
        Assert.Equal(
            bundleBeforeInstall,
            await File.ReadAllBytesAsync(
                preparation.BundlePath,
                TestContext.Current.CancellationToken));
        var candidateAfter = await RecoveryBundleReader.ReadFinalAsync(
            workspace.Workspace,
            preparation.BundlePath,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, candidateAfter.State);
        Assert.Equal(IndexDefinitions.CommandIdentity, candidateAfter.Verified?.Command);
        Assert.False(workspace.AgentsDirectoryExists());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install preserves and ignores an unrelated recovery-store lookalike outside the recognized candidate set"),
     Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task AutomaticInstallPreservesUnrecognizedRecoveryLookalike()
    {
        using var workspace = InstallOperationWorkspace.Create("install-recovery-lookalike");
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
                Environment.SpecialFolderOption.Create)
            ?? throw new InvalidOperationException("Recovery storage is unavailable for lookalike evidence.");
        var workspaceDirectory = RecoveryBundlePathIdentity.WorkspaceDirectory(
            storeRoot,
            workspace.Workspace.PhysicalRoot);
        Directory.CreateDirectory(workspaceDirectory);
        var lookalikePath = Path.Combine(workspaceDirectory, "unrelated-recovery-note.txt");
        var lookalikeBytes = "preserve unrelated recovery-store state\n"u8.ToArray();
        File.WriteAllBytes(lookalikePath, lookalikeBytes);
        try
        {
            var result = await CreateAutomaticOperation(workspace).ExecuteAsync(
                workspace.Request(automatic: true),
                TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Empty(result.Findings);
            Assert.Equal(lookalikeBytes, await File.ReadAllBytesAsync(
                lookalikePath,
                TestContext.Current.CancellationToken));
        }
        finally
        {
            if (File.Exists(lookalikePath))
            {
                File.Delete(lookalikePath);
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Theory, InlineData("missing"), InlineData("invalid"), InlineData("directory"),
     Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task FreshInstallIgnoresLeftoverFilesAndUnavailableOwnership(string state)
    {
        using var workspace = InstallOperationWorkspace.Create("install-leftover-state");
        const string oldLifecycle = ".agents/open-forge.lifecycle.json";
        const string oldLibraries = ".agents/open-forge.libraries.json";
        workspace.WriteText(oldLifecycle, "malformed legacy content must remain untouched");
        workspace.WriteText(oldLibraries, "{different legacy library content}");
        if (state == "invalid")
        {
            workspace.WriteText(WorkspaceOwnershipDefinitions.RelativePath, "invalid receipt");
        }
        else if (state == "directory")
        {
            workspace.CreateDirectory(WorkspaceOwnershipDefinitions.RelativePath);
        }

        var before = workspace.SnapshotHashes();
        var result = await CreateAutomaticOperation(workspace).ExecuteAsync(
            workspace.Request(automatic: true), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        var after = workspace.SnapshotHashes();
        Assert.Equal(before[oldLifecycle], after[oldLifecycle]);
        Assert.Equal(before[oldLibraries], after[oldLibraries]);
        Assert.DoesNotContain(result.Facts.Effects, effect => effect.Path == oldLifecycle || effect.Path == oldLibraries);
        Assert.True(workspace.Exists(".agents/loader.md"));
        if (state == "directory")
        {
            Assert.True(Directory.Exists(workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath)));
            Assert.Equal(InstallLifecycleOutcome.NotRequested, result.Facts.Lifecycle.Outcome);
        }
        else
        {
            var receipt = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(),
                workspace.Workspace, TestContext.Current.CancellationToken);
            Assert.NotNull(receipt.Document.Framework);
            Assert.Single(result.Facts.Effects, effect => effect.Path == WorkspaceOwnershipDefinitions.RelativePath);
            Assert.Equal(InstallLifecycleOutcome.Verified, result.Facts.Lifecycle.Outcome);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task CurrentPayloadNoOpIgnoresReceiptReleaseMetadataAndOldFiles()
    {
        using var workspace = InstallOperationWorkspace.Create("install-current-payload");
        workspace.WriteText(".agents/open-forge.lifecycle.json", "{invalid}");
        workspace.WriteText(".agents/open-forge.libraries.json", "{invalid}");
        var operation = CreateAutomaticOperation(workspace);
        Assert.Equal(CliSemanticStatus.Complete, (await operation.ExecuteAsync(
            workspace.Request(automatic: true), TestContext.Current.CancellationToken)).Status);
        var read = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(),
            workspace.Workspace, TestContext.Current.CancellationToken);
        var framework = Assert.IsType<FrameworkOwnership>(read.Document.Framework);
        var plan = new WorkspaceOwnershipStore().PlanFrameworkOwnership(read,
            framework with { Source = new OwnedSource("older-release", "old-version") });
        var change = Assert.IsType<PlannedFileChange>(plan.Change);
        await File.WriteAllBytesAsync(workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath),
            [.. change.IntendedBytes], TestContext.Current.CancellationToken);
        var before = workspace.SnapshotHashes();

        var result = await operation.ExecuteAsync(workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Facts.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory, InlineData(".agents/loader.md"), InlineData(".agents/LOADER.md"),
     Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ForcePreservesExtensionOwnershipAtASelectedDestination(string path)
    {
        using var workspace = InstallOperationWorkspace.Create("install-extension-conflict");
        workspace.WriteText(WorkspaceOwnershipDefinitions.RelativePath,
            "{\"schemaVersion\":1,\"extensions\":[{\"id\":\"toolkit\",\"paths\":[\"" + path + "\"]}]}");
        workspace.WriteText(".agents/loader.md", "preserve another owner's content");
        var before = workspace.SnapshotHashes();

        var result = await CreateAutomaticOperation(workspace).ExecuteAsync(
            workspace.Request(force: true, automatic: true), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.OwnershipConflict);
        Assert.Empty(result.Facts.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static InstallOperation CreateAutomaticOperation(
        InstallOperationWorkspace workspace)
        => InstallOperationFactory.Create(
            InstallInteractionTestSupport.Confirmation(),
            workspace.LockStoreRoot);

    private static Dictionary<string, string> WithoutPath(
        IReadOnlyDictionary<string, string> snapshot,
        string relativePath)
        => snapshot
            .Where(pair => !string.Equals(pair.Key, relativePath, StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
}
