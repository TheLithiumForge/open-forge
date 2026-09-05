using System.Text;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallPreservationIntegrationTests
{
    [Fact(DisplayName = "Install preserves a trusted scoped Framework target and blocks when it is missing"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task TrustedScopedTargetIsPreservedAndMissingTargetBlocksWithoutRootEffects()
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
        var scopedBytes = Encoding.UTF8.GetBytes(scopedBlock);
        var scopedPhysicalPath = workspace.Combine(scopedPath);
        File.WriteAllBytes(scopedPhysicalPath, Encoding.UTF8.GetBytes(scopedContents));
        try
        {
            var fingerprint = new MarkdownFingerprintReader().Read(scopedBytes);
            Assert.True(fingerprint.IsSemantic, fingerprint.Cause);
            var baselineFingerprint = Assert.IsType<string>(fingerprint.Sha256);

            var lifecycleStore = new LifecycleStore(new PhysicalPathResolver());
            var currentRead = await lifecycleStore.ReadAsync(
                workspace.Workspace,
                LifecycleSection.Framework,
                TestContext.Current.CancellationToken);
            Assert.Equal(LifecycleStoreReadState.Available, currentRead.State);
            var current = Assert.IsType<FrameworkLifecycleState>(currentRead.Framework);
            var scopedTarget = new FrameworkLifecycleTarget
            {
                Path = scopedPath,
                SourceAssetPath = scopedPath,
                Region = "open-forge",
                BaselineFingerprint = baselineFingerprint,
                FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
            };
            var extended = new FrameworkLifecycleState
            {
                Coverage = current.Coverage,
                Source = current.Source,
                Targets = current.Targets
                    .Append(scopedTarget)
                    .OrderBy(target => target.Path, StringComparer.Ordinal)
                    .ThenBy(target => target.Region, StringComparer.Ordinal)
                    .ToArray(),
                GeneratedRegions = current.GeneratedRegions,
            };

            var lifecyclePlan = lifecycleStore.PlanFrameworkUpdate(currentRead, extended);
            Assert.Equal(LifecycleWritePlanState.Planned, lifecyclePlan.State);
            var lifecycleChange = Assert.IsType<PlannedFileChange>(lifecyclePlan.Change);
            await File.WriteAllBytesAsync(
                workspace.Combine(LifecycleSchema.RelativePath),
                lifecycleChange.IntendedBytes.ToArray(),
                TestContext.Current.CancellationToken);

            var persistedRead = await lifecycleStore.ReadAsync(
                workspace.Workspace,
                LifecycleSection.Framework,
                TestContext.Current.CancellationToken);
            var persistedTarget = Assert.Single(
                Assert.IsType<FrameworkLifecycleState>(persistedRead.Framework).Targets,
                target => target.Path == scopedPath);
            Assert.Equal(scopedPath, persistedTarget.SourceAssetPath);
            Assert.Equal(baselineFingerprint, persistedTarget.BaselineFingerprint);
            Assert.Equal(LifecycleSchema.SemanticFingerprintKind, persistedTarget.FingerprintKind);

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

            Assert.Equal(CliSemanticStatus.Blocked, blocked.Status);
            var preservationFinding = Assert.Single(
                blocked.Findings,
                finding => finding.Code == InstallFindingCode.LifecycleBlocked);
            Assert.Equal(scopedPath, preservationFinding.Subject);
            Assert.Equal(beforeBlocked, WithoutPath(workspace.SnapshotHashes(), scopedPath));
            Assert.False(workspace.Exists(scopedPath));
            Assert.DoesNotContain(
                blocked.Findings,
                finding => finding.Code is InstallFindingCode.TargetOccupied
                    or InstallFindingCode.ManagedDivergence);
        }
        finally
        {
            if (File.Exists(scopedPhysicalPath))
            {
                File.Delete(scopedPhysicalPath);
            }
        }
    }

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
        var preparationResult = await new RecoveryBundleStore(new RecoveryBundleReader()).PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundlePreparationState.Prepared, preparationResult.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(preparationResult.Preparation);
        var bundleBeforeInstall = await File.ReadAllBytesAsync(
            preparation.BundlePath,
            TestContext.Current.CancellationToken);

        var candidateBefore = await new RecoveryBundleReader().ReadFinalAsync(
            workspace.Workspace,
            preparation.BundlePath,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, candidateBefore.State);
        Assert.Equal(IndexDefinitions.CommandIdentity, candidateBefore.Verified?.Command);
        Assert.Equal(1, await workspace.ReadRecoveryCandidateCountAsync(
            TestContext.Current.CancellationToken));

        using var standardInput = new StringReader(string.Empty);
        using var promptOutput = new StringWriter();
        var result = await InstallOperationFactory.Create(
                new CliInteractiveSession(
                    standardInput,
                    promptOutput,
                    canPrompt: false),
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
        var candidateAfter = await new RecoveryBundleReader().ReadFinalAsync(
            workspace.Workspace,
            preparation.BundlePath,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, candidateAfter.State);
        Assert.Equal(IndexDefinitions.CommandIdentity, candidateAfter.Verified?.Command);
        Assert.False(workspace.AgentsDirectoryExists());
        Assert.Equal(string.Empty, promptOutput.ToString());
    }

    [Fact(DisplayName = "Install preserves and ignores an unrelated recovery-store lookalike outside the recognized candidate set"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
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

    private static InstallOperation CreateAutomaticOperation(
        InstallOperationWorkspace workspace)
        => InstallOperationFactory.Create(
            new CliInteractiveSession(
                new StringReader(string.Empty),
                new StringWriter(),
                canPrompt: false),
            workspace.LockStoreRoot);

    private static IReadOnlyDictionary<string, string> WithoutPath(
        IReadOnlyDictionary<string, string> snapshot,
        string relativePath)
        => snapshot
            .Where(pair => !string.Equals(pair.Key, relativePath, StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
}
