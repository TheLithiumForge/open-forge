using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

using OpenForge.Cli.IntegrationTests.Commands.Update.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

public sealed class UpdateOperationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update creates a genuinely new source target while protecting existing lifecycle bytes"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task CreatesNewSourceTargetAlongsideProtectedLifecyclePublication()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-new-source-create");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var intended = workspace.ReadBytes(UpdateIntegrationWorkspace.RetiredCandidatePath);
        workspace.SeedGenuinelyNewSourceTarget();
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(intended, workspace.ReadBytes(UpdateIntegrationWorkspace.RetiredCandidatePath));
        Assert.Equal(UpdateLifecycleAction.Publish, result.Lifecycle.Action);
        Assert.Equal(UpdateLifecycleOutcome.Verified, result.Lifecycle.Outcome);
        Assert.Equal(UpdateVerificationState.Verified, result.Verification);
        Assert.Equal(UpdateRecoveryState.Retained, result.Recovery.State);
        Assert.Equal([UpdateIntegrationWorkspace.OwnershipPath], result.Recovery.ProtectedPaths);
        var after = workspace.SnapshotHashes();
        foreach (var (path, hash) in before)
        {
            if (path != UpdateIntegrationWorkspace.OwnershipPath)
            {
                Assert.Equal(hash, after[path]);
            }
        }

        var repeat = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Effects);
        Assert.Equal(UpdateLifecycleOutcome.AlreadyCurrent, repeat.Lifecycle.Outcome);
        Assert.Equal(after, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update restores a missing target with force alongside a protected safe replacement"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task ForcedMissingRestorationCoexistsWithProtectedReplacement()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-mixed-create-replace");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var restored = workspace.ReadBytes(UpdateIntegrationWorkspace.ManagedPath);
        var replaced = workspace.ReadBytes(UpdateIntegrationWorkspace.RetiredCandidatePath);
        workspace.RemoveManagedContent();
        workspace.SeedSafePreviousSourceVersion();
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request(force: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(restored, workspace.ReadBytes(UpdateIntegrationWorkspace.ManagedPath));
        Assert.Equal(replaced, workspace.ReadBytes(UpdateIntegrationWorkspace.RetiredCandidatePath));
        Assert.Equal(UpdateLifecycleOutcome.AlreadyCurrent, result.Lifecycle.Outcome);
        Assert.Equal(UpdateVerificationState.Verified, result.Verification);
        Assert.Equal(UpdateRecoveryState.Retained, result.Recovery.State);
        Assert.Equal(
            [UpdateIntegrationWorkspace.RetiredCandidatePath],
            result.Recovery.ProtectedPaths);
        var after = workspace.SnapshotHashes();
        foreach (var (path, hash) in before)
        {
            if (path != UpdateIntegrationWorkspace.OwnershipPath && path != UpdateIntegrationWorkspace.RetiredCandidatePath)
            {
                Assert.Equal(hash, after[path]);
            }
        }

        var repeat = await workspace.ExecuteAsync(workspace.Request(force: true));

        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Effects);
        Assert.Equal(UpdateLifecycleOutcome.AlreadyCurrent, repeat.Lifecycle.Outcome);
        Assert.Equal(after, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update real workspace automatic mode updates owned content without force or prune"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task AutomaticUpdatesOwnedContentWithoutForceOrPrune()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-automatic-divergence");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request(automatic: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.NotEmpty(result.Effects);
        Assert.NotEqual(before, workspace.SnapshotHashes());
        Assert.Equal(UpdateRecoveryState.Retained, result.Recovery.State);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update real workspace dry-run matches application planning without persistent effects"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task DryRunMatchesApplicationPlanWithoutPersistentEffects()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-dry-run");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(
            workspace.Request(
                mode: UpdateMode.DryRun,
                force: true,
                automatic: false));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(UpdateMode.DryRun, result.Mode);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(UpdateRecoveryState.NotCreated, result.Recovery.State);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update presents a pure dry-run projection and actual planned deletion count before confirmation"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task ConfirmationUsesPureDryRunProjectionAndDeletionCount()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-confirmation-preview");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.SeedHistoricalRetiredTarget();
        var before = workspace.SnapshotHashes();
        UpdateResult? preview = null;
        UpdateConfirmationFacts? facts = null;

        var result = await UpdateOperationFactory.Create(
                UpdateInteractionTestSupport.Confirmation(
                    accepted: false,
                    observe: (candidate, question) =>
                    {
                        preview = candidate;
                        facts = question;
                    }),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(
                    prune: true,
                    automatic: false,
                    allowsInteractiveConfirmation: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        var previewResult = Assert.IsType<UpdateResult>(preview);
        Assert.Equal(UpdateMode.DryRun, previewResult.Mode);
        Assert.False(previewResult.Force);
        Assert.True(previewResult.Prune);
        Assert.False(previewResult.Automatic);
        var confirmationFacts = Assert.IsType<UpdateConfirmationFacts>(facts);
        Assert.Equal(1, confirmationFacts.DeletionCount);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.True(workspace.Exists(UpdateIntegrationWorkspace.HistoricalTargetPath));
        Assert.False(workspace.RecoveryDirectoryExists());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update real workspace redirected human write requires automatic without a prompt"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task RedirectedHumanWriteRequiresAutomaticWithoutPrompt()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-redirected");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(
            workspace.Request(
                force: true,
                automatic: false,
                allowsInteractiveConfirmation: false));

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code.ToString().Contains("ConfirmationRequired", StringComparison.Ordinal));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update real workspace human refusal interrupts before lease and recovery"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task HumanRefusalInterruptsBeforeLeaseAndRecovery()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-refusal");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(
            workspace.Request(
                force: true,
                automatic: false,
                allowsInteractiveConfirmation: true),
            canPrompt: true,
            input: "n\n");

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(UpdateRecoveryState.NotCreated, result.Recovery.State);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update real workspace held lock blocks before every effect"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task HeldWorkspaceLockBlocksBeforeAnyEffect()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-lock");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        using var held = workspace.HoldExternalLock();
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request(force: true));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update real workspace prompt-time change blocks complete-plan revalidation before recovery"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task PromptTimeChangeBlocksCompletePlanRevalidationBeforeRecovery()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-revalidation");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();

        string? lateContents = null;
        var result = await workspace.ExecutePromptedAsync(
            workspace.Request(
                force: true,
                automatic: false,
                allowsInteractiveConfirmation: true),
            () => lateContents = workspace.MutateManagedContentAgain());

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(
            [UpdateFindingCode.TargetUnsafe, UpdateFindingCode.PlanBlocked],
            result.Findings.Select(finding => finding.Code));
        Assert.Empty(result.Effects);
        Assert.Equal(UpdateRecoveryState.NotCreated, result.Recovery.State);
        Assert.Null(result.Recovery.ResidualPath);
        Assert.False(workspace.RecoveryDirectoryExists());
        Assert.Equal(lateContents, workspace.ReadText(UpdateIntegrationWorkspace.ManagedPath));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update real workspace prepares one recovery bundle for all existing target effects"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task PreparesOneBundleForAllExistingTargetEffects()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-recovery-bundle");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        workspace.SeedHistoricalRetiredTarget();

        var result = await workspace.ExecuteAsync(
            workspace.Request(force: true, prune: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.NotEmpty(result.Effects);
        Assert.Equal(UpdateRecoveryState.Retained, result.Recovery.State);
        Assert.Equal(
            [
                UpdateIntegrationWorkspace.HistoricalTargetPath,
                UpdateIntegrationWorkspace.ManagedPath,
                UpdateIntegrationWorkspace.OwnershipPath,
            ],
            result.Recovery.ProtectedPaths);
        Assert.False(workspace.Exists(UpdateIntegrationWorkspace.HistoricalTargetPath));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update preserves ownership receipts and ignores descriptive release metadata"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task PreservesReceiptsAndIgnoresReleaseMetadata()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-ownership");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        workspace.SeedPreviousInventoryIdentity();
        var before = workspace.ReadText(UpdateIntegrationWorkspace.OwnershipPath);
        var result = await workspace.ExecuteAsync(workspace.Request());
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(before, workspace.ReadText(UpdateIntegrationWorkspace.OwnershipPath));
        Assert.Equal(UpdateLifecycleAction.Preserve, result.Lifecycle.Action);
        Assert.Equal(UpdateLifecycleOutcome.AlreadyCurrent, result.Lifecycle.Outcome);
        Assert.Equal(UpdateRecoveryState.Retained, result.Recovery.State);

        using var metadataOnly = UpdateIntegrationWorkspace.Create("update-operation-metadata-only");
        await metadataOnly.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        metadataOnly.SeedPreviousInventoryIdentity();
        var prior = metadataOnly.SnapshotHashes();
        var noOp = await metadataOnly.ExecuteAsync(metadataOnly.Request());
        Assert.Equal(CliSemanticStatus.Complete, noOp.Status);
        Assert.Empty(noOp.Effects);
        Assert.Equal(UpdateLifecycleAction.Preserve, noOp.Lifecycle.Action);
        Assert.Equal(UpdateLifecycleOutcome.AlreadyCurrent, noOp.Lifecycle.Outcome);
        Assert.Equal(UpdateVerificationState.Verified, noOp.Verification);
        Assert.Equal(UpdateRecoveryState.NotRequired, noOp.Recovery.State);
        Assert.Equal(prior, metadataOnly.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update real workspace semantic no-op and repeat write nothing"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task SemanticNoOpAndRepeatWriteNothing()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-no-op");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var first = await workspace.ExecuteAsync(workspace.Request());
        var afterFirst = workspace.SnapshotHashes();

        var second = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        Assert.Empty(first.Effects);
        Assert.Empty(second.Effects);
        Assert.Equal(afterFirst, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update real workspace malformed generated boundary blocks without repair"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task MalformedGeneratedBoundaryBlocksWithoutRepair()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-generated-boundary");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.BreakGeneratedBoundary();
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(
            workspace.Request(force: true, prune: true));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(
            [UpdateFindingCode.GeneratedRegionUnsafe],
            result.Findings.Select(finding => finding.Code));
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());

        using var absent = UpdateIntegrationWorkspace.Create("update-operation-generated-boundary-absent");
        await absent.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        absent.RemoveGeneratedBoundary();
        var absentBefore = absent.SnapshotHashes();

        var absentResult = await absent.ExecuteAsync(
            absent.Request(force: true, prune: true));

        Assert.Equal(CliSemanticStatus.Blocked, absentResult.Status);
        Assert.Equal(
            [UpdateFindingCode.GeneratedRegionUnsafe],
            absentResult.Findings.Select(finding => finding.Code));
        Assert.Empty(absentResult.Effects);
        Assert.Equal(absentBefore, absent.SnapshotHashes());

        using var invalidUtf8 = UpdateIntegrationWorkspace.Create("update-operation-generated-boundary-utf8");
        await invalidUtf8.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        invalidUtf8.CorruptGeneratedUtf8();
        var invalidUtf8Before = invalidUtf8.SnapshotHashes();

        var invalidUtf8Result = await invalidUtf8.ExecuteAsync(
            invalidUtf8.Request(force: true, prune: true));

        Assert.Equal(CliSemanticStatus.Blocked, invalidUtf8Result.Status);
        Assert.Equal(
            [UpdateFindingCode.GeneratedRegionUnsafe],
            invalidUtf8Result.Findings.Select(finding => finding.Code));
        Assert.Empty(invalidUtf8Result.Effects);
        Assert.Equal(invalidUtf8Before, invalidUtf8.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update real workspace untrusted lifecycle and invalid provenance remain write-free"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task UntrustedLifecycleAndInvalidProvenanceRemainWriteFree()
    {
        using var malformed = UpdateIntegrationWorkspace.Create("update-operation-untrusted-lifecycle");
        await malformed.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        malformed.SeedMalformedLifecycle();
        var malformedBefore = malformed.SnapshotHashes();

        var malformedResult = await malformed.ExecuteAsync(
            malformed.Request(force: true, prune: true));

        Assert.Equal(CliSemanticStatus.Complete, malformedResult.Status);
        Assert.Empty(malformedResult.Effects);
        Assert.Equal(malformedBefore, malformed.SnapshotHashes());

        using var invalidProvenance = UpdateIntegrationWorkspace.Create("update-operation-invalid-provenance");
        await invalidProvenance.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        invalidProvenance.SeedInvalidSourceProvenance();
        var provenanceBefore = invalidProvenance.SnapshotHashes();

        var provenanceResult = await invalidProvenance.ExecuteAsync(
            invalidProvenance.Request(force: true, prune: true));

        Assert.Equal(CliSemanticStatus.Complete, provenanceResult.Status);
        Assert.Empty(provenanceResult.Effects);
        Assert.Equal(provenanceBefore, invalidProvenance.SnapshotHashes());
        Assert.Equal(
            [UpdateFindingCode.OwnershipObservation],
            provenanceResult.Findings.Select(finding => finding.Code));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update real workspace end of input interrupts before lease and recovery"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task EndOfInputInterruptsBeforeLeaseAndRecovery()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-end-of-input");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(
            workspace.Request(
                force: true,
                automatic: false,
                allowsInteractiveConfirmation: true),
            canPrompt: true);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(UpdateRecoveryState.NotCreated, result.Recovery.State);
        Assert.Null(result.Recovery.ResidualPath);
        Assert.False(workspace.RecoveryDirectoryExists());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static JsonNode ReadRequiredSection(string lifecycle, string propertyName)
        => JsonNode.Parse(lifecycle)?[propertyName]?.DeepClone()
            ?? throw new InvalidOperationException($"The lifecycle fixture requires '{propertyName}'.");

    private static void AssertBlockedWriteFree(
        UpdateResult result,
        IReadOnlyDictionary<string, string> before,
        UpdateIntegrationWorkspace workspace)
    {
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
