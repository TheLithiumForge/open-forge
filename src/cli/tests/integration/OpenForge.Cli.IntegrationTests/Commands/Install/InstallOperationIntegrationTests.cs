using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallOperationIntegrationTests
{
    private const string GeneratedTargetPath = ".agents/memory/_memory.md";
    private const string LifecyclePath = ".agents/open-forge.lifecycle.json";
    private const string CurrentGeneratedEntry =
        "- [Accepted knowledge that should remain current](crystallized/_crystallized.md)";
    private const string StaleGeneratedEntry = "- [Stale generated entry](stale.md)";

    private static IReadOnlyList<string> CompleteEmbeddedSourceAssetPaths { get; } =
        InstallOperationWorkspace.EmbeddedPayloadPaths
            .Append("AGENTS.md")
            .Append("CLAUDE.md")
            .ToArray();

    [Fact(DisplayName = "Install projects a fully populated typed result into the exact JSON envelope and nested values"), Trait("Feature", "install-presentation"), Trait("Evidence", "Integration")]
    public async Task CompleteTypedResultProjectsEveryNestedValue()
    {
        using var workspace = InstallOperationWorkspace.Create("install-json-projection");
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

        var document = OpenForge.Cli.Core.Commands.Install.Shared.Rendering.InstallJsonProjection.Create(result);

        Assert.Equal(1, document.SchemaVersion);
        Assert.Equal("install", document.Command);
        Assert.Equal("complete", document.Status);
        Assert.NotNull(document.Workspace);
        Assert.Equal(workspace.PhysicalPath, document.Workspace!.Path);
        Assert.Equal("explicit-workspace", document.Workspace.SelectedBy);
        Assert.Equal("apply", document.Result.Mode);
        Assert.False(document.Result.Force);
        Assert.True(document.Result.Automatic);
        Assert.NotNull(document.Result.Source);
        Assert.False(string.IsNullOrWhiteSpace(document.Result.Source!.InventoryFingerprint));
        Assert.Equal(CompleteEmbeddedSourceAssetPaths.Count, document.Result.Source.AssetCount);
        Assert.Equal("safe-absence", document.Result.Classification);
        Assert.NotNull(document.Result.Footprint);
        Assert.NotEmpty(document.Result.Effects);
        Assert.Contains(document.Result.Effects, effect =>
            effect.Kind == "directory"
            && effect.Action == "create"
            && effect.SourceAssetPath is null
            && effect.Outcome == "verified"
            && effect.Residual == "none");
        Assert.Contains(document.Result.Effects, effect =>
            effect.Kind == "file"
            && effect.Action == "create"
            && effect.SourceAssetPath is not null
            && effect.Outcome == "verified"
            && effect.Residual == "none");
        Assert.True(document.Result.Footprint!.GeneratedRegions > 0);
        Assert.Equal("publish", document.Result.Lifecycle.Action);
        Assert.Equal("verified", document.Result.Lifecycle.Outcome);
        Assert.Equal("not-required", document.Result.Recovery.State);
        Assert.Null(document.Result.Recovery.ResidualPath);
        Assert.Equal("verified", document.Result.Verification);
        Assert.Empty(document.Result.Findings);
        Assert.Null(document.Next);
    }

    [Fact(DisplayName = "Install establishes the embedded Framework payload, bounded root blocks, provenance, and an exact no-op rerun"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task SafeAbsentAutomaticApplyIsExactAndIdempotent()
    {
        using var workspace = InstallOperationWorkspace.Create("install-safe-absent");
        using var standardInput = new StringReader(string.Empty);
        using var promptOutput = new StringWriter();
        var session = new CliInteractiveSession(
            standardInput,
            promptOutput,
            canPrompt: false);
        var operation = InstallOperationFactory.Create(session, workspace.LockStoreRoot);

        var first = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        Assert.Empty(first.Findings);
        AssertInstallFacts(
            first,
            new InstallFactsExpectation
            {
                Classification = InstallManagementClassification.SafeAbsence,
                EffectOutcome = InstallEffectOutcome.Verified,
                LifecycleAction = InstallLifecycleAction.Publish,
                LifecycleOutcome = InstallLifecycleOutcome.Verified,
                RecoveryState = InstallResultRecoveryState.NotRequired,
                Verification = InstallResultVerificationState.Verified,
                RequireEffects = true,
            });
        Assert.Equal(string.Empty, promptOutput.ToString());
        Assert.True(workspace.AgentsDirectoryExists());
        Assert.Equal(
            InstallOperationWorkspace.EmbeddedPayloadPaths,
            InstalledPayloadPaths(workspace));

        var agents = await workspace.ReadTextAsync(
            "AGENTS.md",
            TestContext.Current.CancellationToken);
        var claude = await workspace.ReadTextAsync(
            "CLAUDE.md",
            TestContext.Current.CancellationToken);
        Assert.Equal(
            "<!-- open-forge:start -->\n\n"
            + "# Open Forge\n\n"
            + "Open Forge provides the working rules and context for this workspace.\n\n"
            + "Before starting a task, read `.agents/loader.md`.\n"
            + "Use it to select every relevant scope, including nested scopes.\n"
            + "Follow the loaded rules throughout the task.\n"
            + "<!-- open-forge:end -->\n",
            agents);
        Assert.Equal(
            "<!-- open-forge:start -->\n\n"
            + "@AGENTS.md\n"
            + "@.agents/loader.md\n"
            + "<!-- open-forge:end -->\n",
            claude);
        Assert.Equal(1, Count(agents, "<!-- open-forge:start -->"));
        Assert.Equal(1, Count(agents, "<!-- open-forge:end -->"));
        Assert.Equal(1, Count(claude, "<!-- open-forge:start -->"));
        Assert.Equal(1, Count(claude, "<!-- open-forge:end -->"));

        var memory = await workspace.ReadTextAsync(
            ".agents/memory/_memory.md",
            TestContext.Current.CancellationToken);
        Assert.Equal(1, Count(memory, "<!-- open-forge:generated-index:start -->"));
        Assert.Equal(1, Count(memory, "<!-- open-forge:generated-index:end -->"));
        Assert.Contains(
            "- [Accepted knowledge that should remain current](crystallized/_crystallized.md)",
            memory,
            StringComparison.Ordinal);

        using (var lifecycle = JsonDocument.Parse(await workspace.ReadTextAsync(
                   LifecyclePath,
                   TestContext.Current.CancellationToken)))
        {
            var framework = lifecycle.RootElement.GetProperty("framework");
            Assert.Equal("complete", framework.GetProperty("coverage").GetString());
            Assert.Equal(
                "embedded-framework",
                framework.GetProperty("source").GetProperty("id").GetString());

            var generatedRegionFound = false;
            var sourceBackedTargetCount = 0;
            foreach (var target in framework.GetProperty("targets").EnumerateArray())
            {
                var region = target.GetProperty("region");
                var sourceAssetPath = target.GetProperty("sourceAssetPath");
                if (region.ValueKind == JsonValueKind.Null)
                {
                    sourceBackedTargetCount++;
                    Assert.Equal(JsonValueKind.String, sourceAssetPath.ValueKind);
                    var sourcePath = sourceAssetPath.GetString()
                        ?? throw new InvalidOperationException(
                            "A source-backed Install target requires source provenance.");
                    Assert.False(string.IsNullOrWhiteSpace(sourcePath));
                    Assert.Contains(
                        sourcePath,
                        InstallOperationWorkspace.EmbeddedPayloadPaths
                            .Append("AGENTS.md")
                            .Append("CLAUDE.md"));
                }
                else
                {
                    generatedRegionFound = true;
                    Assert.Equal(JsonValueKind.Null, sourceAssetPath.ValueKind);
                }
            }

            Assert.True(generatedRegionFound);
            Assert.True(sourceBackedTargetCount >= 2);
        }

        Assert.Equal(0, await workspace.ReadRecoveryCandidateCountAsync(
            TestContext.Current.CancellationToken));
        Assert.False(workspace.RecoveryDirectoryExists());
        var afterFirst = workspace.SnapshotHashes();

        var second = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        Assert.Empty(second.Findings);
        AssertInstallFacts(
            second,
            new InstallFactsExpectation
            {
                Classification = InstallManagementClassification.TrustedExact,
                EffectOutcome = null,
                LifecycleAction = InstallLifecycleAction.Preserve,
                LifecycleOutcome = InstallLifecycleOutcome.AlreadyCurrent,
                RecoveryState = InstallResultRecoveryState.NotRequired,
                Verification = InstallResultVerificationState.Verified,
                RequireEffects = false,
            });
        Assert.Equal(afterFirst, workspace.SnapshotHashes());
        Assert.Equal(0, await workspace.ReadRecoveryCandidateCountAsync(
            TestContext.Current.CancellationToken));
        Assert.Equal(string.Empty, promptOutput.ToString());
    }

    [Fact(DisplayName = "Install dry-run forms the safe-absent result without workspace, lock, or recovery effects"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task DryRunIsFullyReadOnlyFromSafeAbsence()
    {
        using var workspace = InstallOperationWorkspace.Create("install-dry-run");
        var before = workspace.SnapshotHashes();
        using var standardInput = new StringReader(string.Empty);
        using var promptOutput = new StringWriter();
        var session = new CliInteractiveSession(
            standardInput,
            promptOutput,
            canPrompt: false);

        var result = await InstallOperationFactory.Create(
                session,
                workspace.LockStoreRoot)
            .ExecuteAsync(
            workspace.Request(
                mode: InstallMode.DryRun,
                automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        AssertInstallFacts(
            result,
            new InstallFactsExpectation
            {
                Classification = InstallManagementClassification.SafeAbsence,
                EffectOutcome = InstallEffectOutcome.Planned,
                LifecycleAction = InstallLifecycleAction.Publish,
                LifecycleOutcome = InstallLifecycleOutcome.Planned,
                RecoveryState = InstallResultRecoveryState.NotRequired,
                Verification = InstallResultVerificationState.NotRequested,
                RequireEffects = true,
            });
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.AgentsDirectoryExists());
        Assert.False(workspace.Exists("AGENTS.md"));
        Assert.False(workspace.Exists("CLAUDE.md"));
        Assert.Equal(0, await workspace.ReadRecoveryCandidateCountAsync(
            TestContext.Current.CancellationToken));
        Assert.False(workspace.RecoveryDirectoryExists());
        Assert.Equal(string.Empty, promptOutput.ToString());
    }

    [Fact(DisplayName = "Install force dry-run reports an existing safe generated target as one bounded replacement"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task EligibleForceReportsBoundedGeneratedRegionReplacement()
    {
        using var workspace = InstallOperationWorkspace.Create("install-generated-replacement");
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload
            ?? throw new InvalidOperationException("The embedded Framework payload is unavailable.");
        var asset = payload.Find(GeneratedTargetPath)
            ?? throw new InvalidOperationException("The embedded generated target is unavailable.");
        var original = Encoding.UTF8.GetString(asset.Bytes.AsSpan());
        Assert.Contains(CurrentGeneratedEntry, original, StringComparison.Ordinal);
        workspace.WriteText(
            GeneratedTargetPath,
            original.Replace(
                CurrentGeneratedEntry,
                StaleGeneratedEntry,
                StringComparison.Ordinal));
        using var standardInput = new StringReader(string.Empty);
        using var promptOutput = new StringWriter();

        var result = await InstallOperationFactory.Create(
                new CliInteractiveSession(
                    standardInput,
                    promptOutput,
                    canPrompt: false),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(
                    mode: InstallMode.DryRun,
                    force: true,
                    automatic: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(
            InstallManagementClassification.EligibleInitialOccupant,
            result.Facts.Classification);
        var effect = Assert.Single(
            result.Facts.Effects,
            candidate => candidate.Path == GeneratedTargetPath);
        Assert.Equal(InstallEffectKind.GeneratedRegion, effect.Kind);
        Assert.Equal(InstallEffectAction.Replace, effect.Action);
        Assert.Null(effect.SourceAssetPath);
        Assert.Equal(InstallEffectOutcome.Planned, effect.Outcome);
        Assert.Equal(InstallEffectResidual.None, effect.Residual);
        Assert.Equal(InstallResultRecoveryState.NotCreated, result.Facts.Recovery.State);
        Assert.Null(result.Facts.Recovery.ResidualPath);
        var document = OpenForge.Cli.Core.Commands.Install.Shared.Rendering.InstallJsonProjection.Create(result);
        var json = JsonSerializer.Serialize(
            document,
            InstallJsonContext.Default.InstallJsonDocument);
        using var parsed = JsonDocument.Parse(json);
        var recovery = parsed.RootElement.GetProperty("result").GetProperty("recovery");
        Assert.Equal("not-created", recovery.GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, recovery.GetProperty("residualPath").ValueKind);
        Assert.Equal(string.Empty, promptOutput.ToString());
    }

    [Fact(DisplayName = "Install force does not reconcile trusted managed divergence and preserves all bytes for update"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ForceApplyRemainsBlockedAfterManagedPayloadDivergence()
    {
        using var workspace = InstallOperationWorkspace.Create("install-managed-divergence");
        using var standardInput = new StringReader(string.Empty);
        using var promptOutput = new StringWriter();
        var operation = InstallOperationFactory.Create(
            new CliInteractiveSession(
                standardInput,
                promptOutput,
                canPrompt: false),
            workspace.LockStoreRoot);

        var first = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, first.Status);

        workspace.ReplaceInstalledText(
            ".agents/loader.md",
            "# User divergence\n");
        var beforeBlockedRun = workspace.SnapshotHashes();

        var result = await operation.ExecuteAsync(
            workspace.Request(
                force: true,
                automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == InstallFindingCode.ManagedDivergence);
        AssertInstallFacts(
            result,
            new InstallFactsExpectation
            {
                Classification = InstallManagementClassification.ManagedDivergence,
                EffectOutcome = null,
                LifecycleAction = InstallLifecycleAction.Preserve,
                LifecycleOutcome = InstallLifecycleOutcome.NotRequested,
                RecoveryState = InstallResultRecoveryState.NotRequired,
                Verification = InstallResultVerificationState.NotRequested,
                RequireEffects = false,
            });
        var next = Assert.IsType<CliNextAction>(result.Next);
        Assert.Equal("open-forge update", next.Command);
        Assert.Equal(beforeBlockedRun, workspace.SnapshotHashes());
        Assert.Equal(0, await workspace.ReadRecoveryCandidateCountAsync(
            TestContext.Current.CancellationToken));
        Assert.Equal(string.Empty, promptOutput.ToString());
    }

    [Fact(DisplayName = "Install non-prompt human writes are invalid and prompt refusal is interrupted without writes"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task InteractionPolicyPreservesNoWriteBoundaries()
    {
        using var nonPromptWorkspace = InstallOperationWorkspace.Create("install-nonprompt-human");
        var nonPromptBefore = nonPromptWorkspace.SnapshotHashes();
        using var nonPromptInput = new StringReader("unused\n");
        using var nonPromptOutput = new StringWriter();
        var nonPromptResult = await InstallOperationFactory.Create(
                new CliInteractiveSession(
                    nonPromptInput,
                    nonPromptOutput,
                    canPrompt: false),
                nonPromptWorkspace.LockStoreRoot)
            .ExecuteAsync(
                nonPromptWorkspace.Request(
                    automatic: false,
                    allowsInteractiveConfirmation: false),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, nonPromptResult.Status);
        Assert.Contains(
            nonPromptResult.Findings,
            finding => finding.Code == InstallFindingCode.ConfirmationRequired);
        AssertInstallFacts(
            nonPromptResult,
            new InstallFactsExpectation
            {
                Classification = InstallManagementClassification.SafeAbsence,
                EffectOutcome = InstallEffectOutcome.NotStarted,
                LifecycleAction = InstallLifecycleAction.Publish,
                LifecycleOutcome = InstallLifecycleOutcome.NotStarted,
                RecoveryState = InstallResultRecoveryState.NotRequired,
                Verification = InstallResultVerificationState.NotRequested,
                RequireEffects = true,
            });
        var nonPromptNext = Assert.IsType<CliNextAction>(nonPromptResult.Next);
        Assert.Equal("open-forge install --automatic", nonPromptNext.Command);
        Assert.Equal(nonPromptBefore, nonPromptWorkspace.SnapshotHashes());
        Assert.False(nonPromptWorkspace.AgentsDirectoryExists());
        Assert.Equal("unused", await nonPromptInput.ReadLineAsync(
            TestContext.Current.CancellationToken));
        Assert.Equal(string.Empty, nonPromptOutput.ToString());

        using var refusalWorkspace = InstallOperationWorkspace.Create("install-prompt-refusal");
        var refusalBefore = refusalWorkspace.SnapshotHashes();
        using var refusalInput = new StringReader("n\nremaining\n");
        using var refusalOutput = new StringWriter();
        var refusalResult = await InstallOperationFactory.Create(
                new CliInteractiveSession(
                    refusalInput,
                    refusalOutput,
                    canPrompt: true),
                refusalWorkspace.LockStoreRoot)
            .ExecuteAsync(
                refusalWorkspace.Request(
                    automatic: false,
                    allowsInteractiveConfirmation: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, refusalResult.Status);
        Assert.Contains(
            refusalResult.Findings,
            finding => finding.Code == InstallFindingCode.Interrupted);
        AssertInstallFacts(
            refusalResult,
            new InstallFactsExpectation
            {
                Classification = InstallManagementClassification.SafeAbsence,
                EffectOutcome = InstallEffectOutcome.NotStarted,
                LifecycleAction = InstallLifecycleAction.Publish,
                LifecycleOutcome = InstallLifecycleOutcome.NotStarted,
                RecoveryState = InstallResultRecoveryState.NotRequired,
                Verification = InstallResultVerificationState.NotRequested,
                RequireEffects = true,
            });
        Assert.Equal(refusalBefore, refusalWorkspace.SnapshotHashes());
        Assert.False(refusalWorkspace.AgentsDirectoryExists());
        Assert.NotEmpty(refusalOutput.ToString());
        Assert.Equal("remaining", await refusalInput.ReadLineAsync(
            TestContext.Current.CancellationToken));
    }

    private static IReadOnlyList<string> InstalledPayloadPaths(
        InstallOperationWorkspace workspace)
        => Directory
            .EnumerateFiles(
                workspace.Combine(".agents"),
                "*",
                SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(workspace.PhysicalPath, path).Replace('\\', '/'))
            .Where(path => path is not LifecyclePath)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static int Count(string value, string fragment)
        => (value.Length - value.Replace(fragment, string.Empty, StringComparison.Ordinal).Length)
            / fragment.Length;

    private static void AssertInstallFacts(
        InstallResult result,
        InstallFactsExpectation expectation)
    {
        var facts = result.Facts;
        Assert.NotNull(facts);
        Assert.NotNull(facts!.Source);
        Assert.False(string.IsNullOrWhiteSpace(facts.Source!.InventoryFingerprint));
        Assert.Equal(CompleteEmbeddedSourceAssetPaths.Count, facts.Source.AssetCount);
        Assert.NotNull(facts.Footprint);
        Assert.True(facts.Footprint!.PayloadFiles >= 0);
        Assert.True(facts.Footprint.ManagedRegions >= 0);
        Assert.True(facts.Footprint.GeneratedRegions >= 0);
        Assert.Equal(expectation.Classification, facts.Classification);
        Assert.NotNull(facts.Effects);
        Assert.Equal(expectation.RequireEffects, facts.Effects.Count != 0);
        if (expectation.EffectOutcome is { } outcome)
        {
            Assert.All(facts.Effects, effect => Assert.Equal(outcome, effect.Outcome));
            Assert.All(facts.Effects, effect => Assert.Equal(InstallEffectResidual.None, effect.Residual));
            Assert.Contains(facts.Effects, effect =>
                effect.Kind == InstallEffectKind.Directory
                && effect.Action == InstallEffectAction.Create
                && effect.SourceAssetPath is null);
            Assert.Contains(facts.Effects, effect =>
                effect.Kind == InstallEffectKind.File
                && effect.Action == InstallEffectAction.Create
                && effect.SourceAssetPath is not null);
            Assert.All(
                facts.Effects.Where(effect => effect.Kind == InstallEffectKind.Directory),
                effect => Assert.Null(effect.SourceAssetPath));
            Assert.All(
                facts.Effects.Where(effect => effect.Kind == InstallEffectKind.File
                    && effect.Path != LifecyclePath),
                effect => Assert.False(string.IsNullOrWhiteSpace(effect.SourceAssetPath)));
            Assert.All(
                facts.Effects.Where(effect => effect.Path == LifecyclePath),
                effect => Assert.Null(effect.SourceAssetPath));
        }

        Assert.Equal(expectation.LifecycleAction, facts.Lifecycle.Action);
        Assert.Equal(expectation.LifecycleOutcome, facts.Lifecycle.Outcome);
        Assert.Equal(expectation.RecoveryState, facts.Recovery.State);
        if (expectation.RecoveryState is InstallResultRecoveryState.NotRequired or InstallResultRecoveryState.NotCreated or InstallResultRecoveryState.Removed)
        {
            Assert.Null(facts.Recovery.ResidualPath);
        }

        Assert.Equal(expectation.Verification, facts.Verification.State);
    }
}

internal sealed record InstallFactsExpectation
{
    public required InstallManagementClassification Classification { get; init; }

    public required InstallEffectOutcome? EffectOutcome { get; init; }

    public required InstallLifecycleAction LifecycleAction { get; init; }

    public required InstallLifecycleOutcome LifecycleOutcome { get; init; }

    public required InstallResultRecoveryState RecoveryState { get; init; }

    public required InstallResultVerificationState Verification { get; init; }

    public required bool RequireEffects { get; init; }
}
