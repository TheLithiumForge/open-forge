using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

public sealed class UpdateOperationIntegrationTests
{
    [Fact(DisplayName = "Update real workspace automatic mode preserves divergence without force or prune"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task AutomaticPreservesDivergenceWithoutForceOrPrune()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-automatic-divergence");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request(automatic: true));

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

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
        Assert.Equal(UpdateRecoveryState.Removed, result.Recovery.State);
        Assert.Equal(
            [
                UpdateIntegrationWorkspace.HistoricalTargetPath,
                UpdateIntegrationWorkspace.ManagedPath,
                UpdateIntegrationWorkspace.LifecyclePath,
            ],
            result.Recovery.ProtectedPaths);
        Assert.False(workspace.Exists(UpdateIntegrationWorkspace.HistoricalTargetPath));
    }

    [Fact(DisplayName = "Update real workspace publishes Framework lifecycle canonically while preserving extensions"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task PublishesFrameworkLifecycleCanonicallyAndPreservesExtensions()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-operation-lifecycle");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        workspace.SeedPreviousInventoryIdentity();
        var before = workspace.ReadText(UpdateIntegrationWorkspace.LifecyclePath);
        var extensionsBefore = ReadRequiredSection(before, "extensions");

        var result = await workspace.ExecuteAsync(workspace.Request(force: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var after = workspace.ReadText(UpdateIntegrationWorkspace.LifecyclePath);
        Assert.NotEqual(before, after);
        Assert.True(JsonNode.DeepEquals(extensionsBefore, ReadRequiredSection(after, "extensions")));
        Assert.Equal(UpdateLifecycleAction.Publish, result.Lifecycle.Action);
        Assert.Equal(UpdateLifecycleOutcome.Verified, result.Lifecycle.Outcome);

        using var lifecycleOnly = UpdateIntegrationWorkspace.Create("update-operation-lifecycle-only");
        await lifecycleOnly.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var installedSnapshot = lifecycleOnly.SnapshotHashes();
        var installedSource = lifecycleOnly.ReadLifecycleSourceIdentity();
        lifecycleOnly.SeedPreviousInventoryIdentity();
        var priorSnapshot = lifecycleOnly.SnapshotHashes();
        var priorSource = lifecycleOnly.ReadLifecycleSourceIdentity();

        Assert.Equal(installedSnapshot.Count, priorSnapshot.Count);
        foreach (var (path, hash) in installedSnapshot)
        {
            if (!string.Equals(path, UpdateIntegrationWorkspace.LifecyclePath, StringComparison.Ordinal))
            {
                Assert.Equal(hash, priorSnapshot[path]);
            }
        }

        Assert.Equal(installedSource.Id, priorSource.Id);
        Assert.Equal(installedSource.Version, priorSource.Version);
        Assert.NotEqual(installedSource.InventoryFingerprint, priorSource.InventoryFingerprint);

        var lifecycleOnlyResult = await lifecycleOnly.ExecuteAsync(lifecycleOnly.Request());

        Assert.Equal(CliSemanticStatus.Complete, lifecycleOnlyResult.Status);
        Assert.Empty(lifecycleOnlyResult.Effects);
        Assert.Equal(UpdateLifecycleAction.Publish, lifecycleOnlyResult.Lifecycle.Action);
        Assert.Equal(UpdateLifecycleOutcome.Verified, lifecycleOnlyResult.Lifecycle.Outcome);
        Assert.Equal(UpdateVerificationState.Verified, lifecycleOnlyResult.Verification);
        Assert.Equal(UpdateRecoveryState.Removed, lifecycleOnlyResult.Recovery.State);
        Assert.Equal(
            [UpdateIntegrationWorkspace.LifecyclePath],
            lifecycleOnlyResult.Recovery.ProtectedPaths);
        var publishedSource = lifecycleOnly.ReadLifecycleSourceIdentity();
        Assert.Equal(priorSource.Id, publishedSource.Id);
        Assert.Equal(priorSource.Version, publishedSource.Version);
        Assert.NotEqual(priorSource.InventoryFingerprint, publishedSource.InventoryFingerprint);
        var publishedSnapshot = lifecycleOnly.SnapshotHashes();

        var repeat = await lifecycleOnly.ExecuteAsync(lifecycleOnly.Request());

        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Effects);
        Assert.Equal(UpdateLifecycleOutcome.AlreadyCurrent, repeat.Lifecycle.Outcome);
        Assert.Equal(UpdateVerificationState.Verified, repeat.Verification);
        Assert.Equal(UpdateRecoveryState.NotRequired, repeat.Recovery.State);
        Assert.Equal(publishedSnapshot, lifecycleOnly.SnapshotHashes());
    }

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

    [Fact(DisplayName = "Update real workspace untrusted lifecycle and invalid provenance remain write-free"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task UntrustedLifecycleAndInvalidProvenanceRemainWriteFree()
    {
        using var malformed = UpdateIntegrationWorkspace.Create("update-operation-untrusted-lifecycle");
        await malformed.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        malformed.SeedMalformedLifecycle();
        var malformedBefore = malformed.SnapshotHashes();

        var malformedResult = await malformed.ExecuteAsync(
            malformed.Request(force: true, prune: true));

        AssertBlockedWriteFree(malformedResult, malformedBefore, malformed);

        using var invalidProvenance = UpdateIntegrationWorkspace.Create("update-operation-invalid-provenance");
        await invalidProvenance.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        invalidProvenance.SeedInvalidSourceProvenance();
        var provenanceBefore = invalidProvenance.SnapshotHashes();

        var provenanceResult = await invalidProvenance.ExecuteAsync(
            invalidProvenance.Request(force: true, prune: true));

        AssertBlockedWriteFree(provenanceResult, provenanceBefore, invalidProvenance);
        Assert.Equal(
            [UpdateFindingCode.SourceProvenanceInvalid],
            provenanceResult.Findings.Select(finding => finding.Code));
    }

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
