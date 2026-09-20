using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Presentation.Install;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Serialization.Shared.Assertions;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallOperationIntegrationTests
{
    private const string GeneratedTargetPath = ".agents/memory/_memory.md";
    private const string LifecyclePath = ".agents/open-forge.lifecycle.json";
    private const string OwnershipPath = InstallOperationWorkspace.OwnershipPath;
    private const string CurrentGeneratedEntry =
        "- [Accepted knowledge that should remain current](crystallized/_crystallized.md)";
    private const string StaleGeneratedEntry = "- [Stale generated entry](stale.md)";

    private static IReadOnlyList<string> CompleteEmbeddedSourceAssetPaths { get; } =
        InstallOperationWorkspace.EmbeddedPayloadPaths
            .Append("AGENTS.md")
            .Append("CLAUDE.md")
            .ToArray();

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install projects a fully populated typed result into the exact JSON envelope and nested values"), Trait("Feature", "install-presentation"), Trait("Evidence", "Integration")]
    public async Task CompleteTypedResultProjectsEveryNestedValue()
    {
        using var workspace = InstallOperationWorkspace.Create("install-json-projection");
        var result = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Confirmation(),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(automatic: true),
                TestContext.Current.CancellationToken);

        var rendered = RenderJson(result, CliDetail.Full);
        using var document = JsonDocument.Parse(rendered);
        var root = document.RootElement;

        Schema3Assertions.Envelope(root, "install", "completed", "full");
        Assert.Equal(workspace.PhysicalPath, root.GetProperty("workspace").GetProperty("path").GetString());
        var data = root.GetProperty("data");
        Assert.Equal("apply", data.GetProperty("mode").GetString());
        Assert.False(data.GetProperty("force").GetBoolean());
        Assert.True(data.GetProperty("automatic").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("source").GetProperty("inventoryFingerprint").GetString()));
        Assert.Equal(CompleteEmbeddedSourceAssetPaths.Count, data.GetProperty("source").GetProperty("assetCount").GetInt32());
        Assert.Equal("safe-absence", data.GetProperty("classification").GetString());
        var effects = root.GetProperty("effects");
        Assert.NotEmpty(effects.EnumerateArray());
        Assert.Contains(effects.EnumerateArray(), effect =>
            effect.GetProperty("kind").GetString() == "directory"
            && effect.GetProperty("action").GetString() == "created"
            && effect.GetProperty("outcome").GetString() == "done");
        Assert.Contains(effects.EnumerateArray(), effect =>
            effect.GetProperty("kind").GetString() == "file"
            && effect.GetProperty("action").GetString() == "created"
            && effect.GetProperty("outcome").GetString() == "done");
        var dataEffects = data.GetProperty("effects");
        Assert.NotEmpty(dataEffects.EnumerateArray());
        Assert.All(dataEffects.EnumerateArray(), effect =>
            Assert.All(effect.EnumerateObject(), property =>
                Assert.Contains(property.Name, new[] { "path", "sourceAssetPath" })));
        Assert.Contains(dataEffects.EnumerateArray(), effect =>
            effect.TryGetProperty("sourceAssetPath", out var sourceAssetPath)
            && sourceAssetPath.ValueKind == JsonValueKind.String
            && sourceAssetPath.GetString() is not null);
        Assert.True(data.GetProperty("footprint").GetProperty("sections").GetInt32() > 0);
        Assert.Equal("publish", data.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("verified", data.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal("verified", data.GetProperty("verification").GetString());
        Assert.Empty(root.GetProperty("findings").EnumerateArray());
        Assert.Equal("not-required", root.GetProperty("recovery").GetProperty("disposition").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install establishes the embedded Framework payload, bounded root blocks, provenance, and an exact no-op rerun"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task SafeAbsentAutomaticApplyIsExactAndIdempotent()
    {
        using var workspace = InstallOperationWorkspace.Create("install-safe-absent");
        var operation = InstallOperationFactory.Create(
            InstallInteractionTestSupport.Confirmation(),
            workspace.LockStoreRoot);

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
        Assert.Equal(1, Count(memory, "## Entries"));
        Assert.DoesNotContain("generated-index", memory, StringComparison.Ordinal);
        Assert.Contains(
            "- [Accepted knowledge that should remain current](crystallized/_crystallized.md)",
            memory,
            StringComparison.Ordinal);

        Assert.False(workspace.Exists(LifecyclePath));
        Assert.False(workspace.Exists(".agents/open-forge.libraries.json"));
        using (var ownership = JsonDocument.Parse(await workspace.ReadTextAsync(
                   OwnershipPath, TestContext.Current.CancellationToken)))
        {
            var framework = ownership.RootElement.GetProperty("framework");
            Assert.Equal("embedded-framework", framework.GetProperty("source").GetProperty("id").GetString());
            Assert.Equal(InstallOperationWorkspace.EmbeddedPayloadPaths,
                framework.GetProperty("paths").EnumerateArray().Select(path => path.GetString()));
            var regions = framework.GetProperty("regions").EnumerateArray().ToArray();
            Assert.Contains(regions, region => region.GetProperty("path").GetString() == "AGENTS.md"
                && region.GetProperty("region").GetString() == "open-forge");
            Assert.Contains(regions, region => region.GetProperty("path").GetString() == "CLAUDE.md"
                && region.GetProperty("region").GetString() == "open-forge");
            Assert.Contains(regions, region => region.GetProperty("region").GetString() == "entries");
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
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install records verified file receipts in the ownership lock"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task FreshInstallRecordsVerifiedFileReceiptsInOwnershipLock()
    {
        using var workspace = InstallOperationWorkspace.Create("install-ownership-receipts");
        var result = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Confirmation(),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(automatic: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        Assert.False(workspace.Exists(LifecyclePath));
        Assert.True(workspace.Exists(OwnershipPath));

        var verifiedFileReceipts = result.Facts.Effects
            .Where(effect => effect.Kind == InstallEffectKind.File)
            .Where(effect => effect.Outcome == InstallEffectOutcome.Verified)
            .Where(effect => effect.Path != OwnershipPath)
            .Where(effect => effect.Path is not ("AGENTS.md" or "CLAUDE.md"))
            .Select(effect => effect.Path)
            .Order(StringComparer.Ordinal)
            .ToArray();

        using var ownership = JsonDocument.Parse(await workspace.ReadTextAsync(
            OwnershipPath,
            TestContext.Current.CancellationToken));
        var root = ownership.RootElement;
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        var framework = root.GetProperty("framework");
        var ownedPaths = framework
            .GetProperty("paths")
            .EnumerateArray()
            .Select(path => path.GetString())
            .ToArray();

        Assert.Equal(verifiedFileReceipts, ownedPaths);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install dry-run forms the safe-absent result without workspace, lock, or recovery effects"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task DryRunIsFullyReadOnlyFromSafeAbsence()
    {
        using var workspace = InstallOperationWorkspace.Create("install-dry-run");
        var before = workspace.SnapshotHashes();
        var result = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Confirmation(),
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
    }

    [Trait("Boundary", "OS")]
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
        var result = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Confirmation(),
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
        var json = RenderJson(result, CliDetail.Full);
        using var parsed = JsonDocument.Parse(json);
        Assert.Equal("completed", parsed.RootElement.GetProperty("status").GetString());
        Assert.Equal("dry-run", parsed.RootElement.GetProperty("data").GetProperty("mode").GetString());
        Assert.Equal("not-required", parsed.RootElement.GetProperty("recovery").GetProperty("disposition").GetString());
        Assert.Equal(JsonValueKind.Null, parsed.RootElement.GetProperty("next").ValueKind);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install presents a pure dry-run projection and distinct replacement count before confirmation"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ConfirmationUsesPureDryRunProjectionAndTargetReplacementCount()
    {
        using var workspace = InstallOperationWorkspace.Create("install-confirmation-preview");
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload
            ?? throw new InvalidOperationException("The embedded Framework payload is unavailable.");
        var asset = payload.Find(GeneratedTargetPath)
            ?? throw new InvalidOperationException("The embedded generated target is unavailable.");
        var original = Encoding.UTF8.GetString(asset.Bytes.AsSpan());
        workspace.WriteText(
            GeneratedTargetPath,
            original.Replace(CurrentGeneratedEntry, StaleGeneratedEntry, StringComparison.Ordinal));
        var before = workspace.SnapshotHashes();
        InstallResult? preview = null;
        InstallConfirmationFacts? facts = null;
        var result = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Confirmation(
                    accepted: false,
                    observe: (candidate, question) =>
                    {
                        preview = candidate;
                        facts = question;
                    }),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(
                    force: true,
                    automatic: false,
                    allowsInteractiveConfirmation: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        var previewResult = Assert.IsType<InstallResult>(preview);
        Assert.Equal(InstallMode.DryRun, previewResult.Mode);
        Assert.True(previewResult.Force);
        Assert.False(previewResult.Automatic);
        var confirmationFacts = Assert.IsType<InstallConfirmationFacts>(facts);
        Assert.Equal(1, confirmationFacts.ReplacementCount);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(WorkspaceLockPathIdentity.StoreDirectory(workspace.LockStoreRoot)));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install force does not reconcile trusted managed divergence and preserves all bytes for update"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ForceApplyRemainsBlockedAfterManagedPayloadDivergence()
    {
        using var workspace = InstallOperationWorkspace.Create("install-managed-divergence");
        var operation = InstallOperationFactory.Create(
            InstallInteractionTestSupport.Confirmation(),
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
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install non-prompt human writes are invalid and prompt refusal is interrupted without writes"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task InteractionPolicyPreservesNoWriteBoundaries()
    {
        using var nonPromptWorkspace = InstallOperationWorkspace.Create("install-nonprompt-human");
        var nonPromptBefore = nonPromptWorkspace.SnapshotHashes();
        var nonPromptResult = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Confirmation(),
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

        using var refusalWorkspace = InstallOperationWorkspace.Create("install-prompt-refusal");
        var refusalBefore = refusalWorkspace.SnapshotHashes();
        var refusalResult = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Confirmation(accepted: false),
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
    }

    private static IReadOnlyList<string> InstalledPayloadPaths(
        InstallOperationWorkspace workspace)
        => Directory
            .EnumerateFiles(
                workspace.Combine(".agents"),
                "*",
                SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(workspace.PhysicalPath, path).Replace('\\', '/'))
            .Where(path => path is not (LifecyclePath or OwnershipPath))
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static int Count(string value, string fragment)
        => (value.Length - value.Replace(fragment, string.Empty, StringComparison.Ordinal).Length)
            / fragment.Length;

    private static string RenderJson(InstallResult result, CliDetail detail)
        => CommandOutputRenderers<InstallResult>.Render(
            new CliPresentationRequest<InstallResult>(
                result,
                new(CliFormat.Json, detail, null)),
            InstallPresentation.Rendering);

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
                    && effect.Path != OwnershipPath),
                effect => Assert.False(string.IsNullOrWhiteSpace(effect.SourceAssetPath)));
            Assert.All(
                facts.Effects.Where(effect => effect.Path == OwnershipPath),
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
