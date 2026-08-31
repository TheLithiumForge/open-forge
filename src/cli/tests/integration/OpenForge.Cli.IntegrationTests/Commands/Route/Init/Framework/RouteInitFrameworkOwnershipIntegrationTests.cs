using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;

public sealed class RouteInitFrameworkOwnershipIntegrationTests
{
    private const string CanonicalDocumentsPath = ".agents/memory/crystallized/documents/_documents.md";
    private const string CompatibilityDocumentsPath = ".agents/memory/crystallized/documents/index.md";
    private const string CrystallizedPath = ".agents/memory/crystallized/_crystallized.md";
    private const string CanonicalDocumentsLink = "documents/_documents.md";
    private const string CompatibilityDocumentsLink = "documents/index.md";
    private const string GeneratedEntriesRegion = "entries";

    [Theory(DisplayName = "Framework alignment does not adopt an unchanged untracked canonical or compatibility source")]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task UntrackedUnchangedSourceRemainsUserOwned(bool compatibility)
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-untracked-unchanged",
            TestContext.Current.CancellationToken);
        var target = await PrepareUntrackedSourceAsync(workspace, compatibility, staleEntries: false);
        var before = workspace.SnapshotHashes();
        var beforeLifecycle = workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.LifecyclePath);

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        Assert.Empty(result.Effects);
        Assert.Equal(RouteInitLifecycleAction.Preserve, result.Lifecycle.Action);
        Assert.Equal(RouteInitLifecycleOutcome.AlreadyCurrent, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(beforeLifecycle, workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.LifecyclePath));
        AssertUserOwnedFinalEntrypoint(result, target, compatibility);

        var lifecycle = await ReadFrameworkAsync(workspace);
        Assert.DoesNotContain(lifecycle.Targets, item => IsTarget(item, target, region: null));
        Assert.DoesNotContain(lifecycle.Targets, item => IsTarget(item, target, GeneratedEntriesRegion));
        Assert.DoesNotContain(lifecycle.GeneratedRegions, item => IsRegion(item, target));
    }

    [Theory(DisplayName = "Framework alignment repairs only stale Entries for an untracked canonical or compatibility source")]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task UntrackedStaleSourcePublishesOnlyRegionOwnership(bool compatibility)
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-untracked-stale",
            TestContext.Current.CancellationToken);
        var target = await PrepareUntrackedSourceAsync(workspace, compatibility, staleEntries: true);
        var beforeDocument = new MarkdownDocumentParser().Parse(workspace.ReadText(target));
        var content = beforeDocument.GeneratedRegion.ContentSpan
            ?? throw new InvalidOperationException("The stale ownership fixture has no complete Entries content span.");
        var authoredPrefix = beforeDocument.Source[..content.Start];
        var authoredSuffix = beforeDocument.Source[content.End..];

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        Assert.Equal(RouteInitLifecycleAction.Publish, result.Lifecycle.Action);
        Assert.Equal(RouteInitLifecycleOutcome.Verified, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitRecoveryState.Removed, result.Recovery.State);
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        AssertUserOwnedFinalEntrypoint(result, target, compatibility);
        var effect = Assert.Single(result.Effects);
        Assert.Equal(target, effect.Path);
        Assert.Equal(RouteInitEffectKind.GeneratedRegion, effect.Kind);
        Assert.Equal(RouteInitEffectAction.Replace, effect.Action);
        Assert.Null(effect.SourceAssetPath);
        Assert.Equal(RouteInitEffectOutcome.Verified, effect.Outcome);

        var afterDocument = new MarkdownDocumentParser().Parse(workspace.ReadText(target));
        var afterContent = afterDocument.GeneratedRegion.ContentSpan
            ?? throw new InvalidOperationException("The repaired ownership fixture has no complete Entries content span.");
        Assert.Equal(authoredPrefix, afterDocument.Source[..afterContent.Start]);
        Assert.Equal(authoredSuffix, afterDocument.Source[afterContent.End..]);
        Assert.Contains("- none - No entries - #Empty", afterDocument.Source, StringComparison.Ordinal);
        Assert.DoesNotContain("Stale route entry", afterDocument.Source, StringComparison.Ordinal);

        var lifecycle = await ReadFrameworkAsync(workspace);
        Assert.DoesNotContain(lifecycle.Targets, item => IsTarget(item, target, region: null));
        var regionTarget = Assert.Single(
            lifecycle.Targets,
            item => IsTarget(item, target, GeneratedEntriesRegion));
        Assert.Null(regionTarget.SourceAssetPath);
        Assert.Equal(LifecycleSchema.ExactBytesFingerprintKind, regionTarget.FingerprintKind);
        Assert.Single(lifecycle.GeneratedRegions, item => IsRegion(item, target));
    }

    [Fact(DisplayName = "Framework missing copied managed source publishes whole and Entries lifecycle identity")]
    [Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task MissingCopiedManagedSourceRetainsEmbeddedProvenance()
    {
        const string target = "memory/release-notes/crystallized/documents";
        const string destination = ".agents/memory/release-notes/crystallized/documents/_documents.md";
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-missing-managed-ownership",
            TestContext.Current.CancellationToken);

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        var entrypoint = Assert.Single(result.Entrypoints, item => item.Path == destination);
        Assert.Equal(RouteInitEntrypointCurrent.Missing, entrypoint.Current);
        Assert.Equal(RouteInitEntrypointOwnership.Framework, entrypoint.Ownership);
        Assert.Equal(CanonicalDocumentsPath, entrypoint.SourceAssetPath);
        Assert.Equal(RouteInitEntrypointOutcome.Created, entrypoint.Outcome);
        var lifecycle = await ReadFrameworkAsync(workspace);
        var whole = Assert.Single(lifecycle.Targets, item => IsTarget(item, destination, region: null));
        Assert.Equal(CanonicalDocumentsPath, whole.SourceAssetPath);
        var region = Assert.Single(
            lifecycle.Targets,
            item => IsTarget(item, destination, GeneratedEntriesRegion));
        Assert.Null(region.SourceAssetPath);
        Assert.Single(lifecycle.GeneratedRegions, item => IsRegion(item, destination));
    }

    [Fact(DisplayName = "Framework trusted repeat retains proven whole-source ownership without effects")]
    [Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task TrustedRepeatRetainsProvenFrameworkOwnership()
    {
        const string target = "memory/release-notes/crystallized/documents";
        const string destination = ".agents/memory/release-notes/crystallized/documents/_documents.md";
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-trusted-repeat-ownership",
            TestContext.Current.CancellationToken);
        _ = await ExecuteAsync(workspace, target);
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(RouteInitLifecycleAction.Preserve, result.Lifecycle.Action);
        Assert.Equal(RouteInitLifecycleOutcome.AlreadyCurrent, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
        var entrypoint = Assert.Single(result.Entrypoints, item => item.Path == destination);
        Assert.Equal(RouteInitEntrypointCurrent.Existing, entrypoint.Current);
        Assert.Equal(RouteInitEntrypointOwnership.Framework, entrypoint.Ownership);
        Assert.Equal(CanonicalDocumentsPath, entrypoint.SourceAssetPath);
        Assert.Equal(RouteInitEntrypointOutcome.Unchanged, entrypoint.Outcome);
        var scope = Assert.Single(
            result.Entrypoints,
            item => item.Path == ".agents/memory/release-notes/_release-notes.md");
        Assert.Equal(RouteInitEntrypointOwnership.User, scope.Ownership);
        Assert.Null(scope.SourceAssetPath);
    }

    private static void AssertUserOwnedFinalEntrypoint(
        RouteInitResult result,
        string target,
        bool compatibility)
    {
        var entrypoint = Assert.Single(result.Entrypoints, item => item.Path == target);
        Assert.Equal(RouteInitEntrypointCurrent.Existing, entrypoint.Current);
        Assert.Equal(RouteInitEntrypointOwnership.User, entrypoint.Ownership);
        Assert.Null(entrypoint.SourceAssetPath);
        Assert.Equal(
            compatibility ? RouteInitEntrypointForm.Compatibility : RouteInitEntrypointForm.Canonical,
            entrypoint.Form);
        var segment = Assert.Single(result.Framework!.Segments, item => item.Path == target);
        Assert.Equal(RouteInitFrameworkSegmentRole.Managed, segment.Role);
        Assert.Equal(CanonicalDocumentsPath, segment.SourceAssetPath);
    }

    private static async ValueTask<string> PrepareUntrackedSourceAsync(
        RouteInitFrameworkIntegrationWorkspace workspace,
        bool compatibility,
        bool staleEntries)
    {
        var target = compatibility ? CompatibilityDocumentsPath : CanonicalDocumentsPath;
        if (compatibility)
        {
            workspace.WriteText(target, workspace.ReadText(CanonicalDocumentsPath));
            workspace.Delete(CanonicalDocumentsPath);
            workspace.WriteText(
                CrystallizedPath,
                workspace.ReadText(CrystallizedPath).Replace(
                    CanonicalDocumentsLink,
                    CompatibilityDocumentsLink,
                    StringComparison.Ordinal));
        }

        if (staleEntries)
        {
            workspace.WriteText(
                target,
                workspace.ReadText(target).Replace(
                    "- none - No entries - #Empty",
                    "- stale - Stale route entry - #Stale",
                    StringComparison.Ordinal));
        }

        var store = new LifecycleStore(new PhysicalPathResolver());
        var read = await store.ReadAsync(
            workspace.Workspace,
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);
        var current = read.Framework
            ?? throw new InvalidOperationException("The ownership fixture has no trusted Framework lifecycle state.");
        var identity = new FrameworkContentIdentity();
        var targets = current.Targets
            .Where(item => !string.Equals(item.Path, CanonicalDocumentsPath, StringComparison.Ordinal))
            .Select(item => compatibility
                    && IsTarget(item, CrystallizedPath, GeneratedEntriesRegion)
                ? CopyTarget(
                    item,
                    identity.ReadGeneratedEntriesFingerprint(
                        File.ReadAllBytes(workspace.Combine(CrystallizedPath)),
                        LifecycleSchema.ExactBytesFingerprintKind))
                : item)
            .OrderBy(item => item.Path, StringComparer.Ordinal)
            .ThenBy(item => item.Region, StringComparer.Ordinal)
            .ToArray();
        var generated = current.GeneratedRegions
            .Where(item => !string.Equals(item.Path, CanonicalDocumentsPath, StringComparison.Ordinal))
            .OrderBy(item => item.Path, StringComparer.Ordinal)
            .ThenBy(item => item.Region, StringComparer.Ordinal)
            .ToArray();
        var intended = new FrameworkLifecycleState
        {
            Coverage = current.Coverage,
            Source = current.Source,
            Targets = targets,
            GeneratedRegions = generated,
        };
        var plan = store.PlanFrameworkUpdate(read, intended);
        if (plan.State != LifecycleWritePlanState.Planned || plan.Change is not { } change)
        {
            throw new InvalidOperationException(
                $"The untracked ownership fixture could not form a lifecycle change: {plan.State}: {plan.Cause}");
        }

        workspace.WriteBytes(
            RouteInitFrameworkIntegrationWorkspace.LifecyclePath,
            change.IntendedBytes.ToArray());
        return target;
    }

    private static FrameworkLifecycleTarget CopyTarget(
        FrameworkLifecycleTarget source,
        string baselineFingerprint)
        => new()
        {
            Path = source.Path,
            SourceAssetPath = source.SourceAssetPath,
            Region = source.Region,
            BaselineFingerprint = baselineFingerprint,
            FingerprintKind = source.FingerprintKind,
        };

    private static async ValueTask<FrameworkLifecycleState> ReadFrameworkAsync(
        RouteInitFrameworkIntegrationWorkspace workspace)
    {
        var read = await new LifecycleStore(new PhysicalPathResolver()).ReadAsync(
            workspace.Workspace,
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);
        return read.State == LifecycleStoreReadState.Available && read.Framework is { } framework
            ? framework
            : throw new InvalidOperationException(
                $"The ownership fixture lifecycle is not available: {read.State}: {read.Cause}");
    }

    private static bool IsTarget(
        FrameworkLifecycleTarget target,
        string path,
        string? region)
        => string.Equals(target.Path, path, StringComparison.Ordinal)
            && string.Equals(target.Region, region, StringComparison.Ordinal);

    private static bool IsRegion(FrameworkGeneratedRegion region, string path)
        => string.Equals(region.Path, path, StringComparison.Ordinal)
            && string.Equals(region.Region, GeneratedEntriesRegion, StringComparison.Ordinal);

    private static ValueTask<RouteInitResult> ExecuteAsync(
        RouteInitFrameworkIntegrationWorkspace workspace,
        string target)
        => RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(target), TestContext.Current.CancellationToken);
}
