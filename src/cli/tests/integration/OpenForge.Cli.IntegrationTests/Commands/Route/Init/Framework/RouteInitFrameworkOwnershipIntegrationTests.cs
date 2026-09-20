using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;

public sealed class RouteInitFrameworkOwnershipIntegrationTests
{
    private const string CanonicalWorkingPath = ".agents/memory/working/_working.md";
    private const string CompatibilityWorkingPath = ".agents/memory/working/index.md";
    private const string MemoryPath = ".agents/memory/_memory.md";
    private const string CanonicalWorkingLink = "working/_working.md";
    private const string CompatibilityWorkingLink = "working/index.md";
    private const string EntriesRegion = "entries";

    [Trait("Boundary", "OS")]
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
        var before = workspace.SnapshotHashesWithoutOwnership();
        var beforeLifecycle = workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.OwnershipPath);

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        Assert.Empty(result.Effects);
        Assert.Equal(RouteInitLifecycleAction.Preserve, result.Lifecycle.Action);
        Assert.Equal(RouteInitLifecycleOutcome.AlreadyCurrent, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        Assert.Equal(before, workspace.SnapshotHashesWithoutOwnership());
        Assert.Equal(beforeLifecycle, workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.OwnershipPath));
        AssertUserOwnedFinalEntrypoint(result, target, compatibility);

        var lifecycle = await ReadFrameworkAsync(workspace);
        Assert.DoesNotContain(target, lifecycle.Paths);
        Assert.DoesNotContain(lifecycle.Regions, item => IsRegion(item, target));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "The existing ownership lock is preserved by repeated no-op route observations")]
    [Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task OwnershipLockIsCreatedOnceAndIsStableOnRepeat()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-ownership-stable",
            TestContext.Current.CancellationToken);
        var target = await PrepareUntrackedSourceAsync(workspace, compatibility: false, staleEntries: false);

        var first = await ExecuteAsync(workspace, target);
        Assert.Equal(CliSemanticStatus.Complete, first.Status);

        var afterFirst = workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.OwnershipPath);
        Assert.False(string.IsNullOrWhiteSpace(afterFirst));

        var beforeRepeat = workspace.SnapshotHashes();
        var second = await ExecuteAsync(workspace, target);
        Assert.Equal(CliSemanticStatus.Complete, second.Status);

        // An identical intended ownership must plan Unchanged rather than a
        // rewrite, so the whole workspace including the lock is stable here.
        Assert.Equal(afterFirst, workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.OwnershipPath));
        Assert.Equal(beforeRepeat, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
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
        Assert.DoesNotContain(target, lifecycle.Paths);
        Assert.Single(lifecycle.Regions, item => IsRegion(item, target));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Framework missing copied managed source publishes whole-file and Entries ownership")]
    [Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task MissingCopiedManagedSourceRetainsEmbeddedProvenance()
    {
        const string target = "memory/release-notes/working";
        const string destination = ".agents/memory/release-notes/working/_working.md";
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-missing-managed-ownership",
            TestContext.Current.CancellationToken);

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var entrypoint = Assert.Single(result.Entrypoints, item => item.Path == destination);
        Assert.Equal(RouteInitEntrypointCurrent.Missing, entrypoint.Current);
        Assert.Equal(RouteInitEntrypointOwnership.Framework, entrypoint.Ownership);
        Assert.Equal(CanonicalWorkingPath, entrypoint.SourceAssetPath);
        Assert.Equal(RouteInitEntrypointOutcome.Created, entrypoint.Outcome);
        var lifecycle = await ReadFrameworkAsync(workspace);
        Assert.Contains(destination, lifecycle.Paths);
        Assert.Single(lifecycle.Regions, item => IsRegion(item, destination));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Framework trusted repeat retains proven whole-source ownership without effects")]
    [Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task TrustedRepeatRetainsProvenFrameworkOwnership()
    {
        const string target = "memory/release-notes/working";
        const string destination = ".agents/memory/release-notes/working/_working.md";
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
        Assert.Equal(CanonicalWorkingPath, entrypoint.SourceAssetPath);
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
        Assert.Equal(CanonicalWorkingPath, segment.SourceAssetPath);
    }

    private static async ValueTask<string> PrepareUntrackedSourceAsync(
        RouteInitFrameworkIntegrationWorkspace workspace,
        bool compatibility,
        bool staleEntries)
    {
        var target = compatibility ? CompatibilityWorkingPath : CanonicalWorkingPath;
        if (compatibility)
        {
            workspace.WriteText(target, workspace.ReadText(CanonicalWorkingPath));
            workspace.Delete(CanonicalWorkingPath);
            workspace.WriteText(
                MemoryPath,
                workspace.ReadText(MemoryPath).Replace(
                    CanonicalWorkingLink,
                    CompatibilityWorkingLink,
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

        var read = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace,
            TestContext.Current.CancellationToken);
        var current = read.Document.Framework ?? throw new InvalidOperationException("Expected Framework ownership.");
        var intended = current with
        {
            Paths = [.. current.Paths.Where(path => path != CanonicalWorkingPath)],
            Regions = [.. current.Regions.Where(region => region.Path != CanonicalWorkingPath)],
        };
        var plan = new WorkspaceOwnershipStore().PlanFrameworkOwnership(read, intended);
        workspace.WriteBytes(RouteInitFrameworkIntegrationWorkspace.OwnershipPath,
            Assert.IsType<OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files.PlannedFileChange>(plan.Change).IntendedBytes.ToArray());
        return target;
    }

    private static async ValueTask<FrameworkOwnership> ReadFrameworkAsync(RouteInitFrameworkIntegrationWorkspace workspace)
    {
        var read = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace,
            TestContext.Current.CancellationToken);
        Assert.True(read.IsTrustworthy);
        return Assert.IsType<FrameworkOwnership>(read.Document.Framework);
    }

    private static bool IsRegion(OwnedRegion region, string path)
        => region.Path == path && region.Region == EntriesRegion;

    private static ValueTask<RouteInitResult> ExecuteAsync(
        RouteInitFrameworkIntegrationWorkspace workspace,
        string target)
        => RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(target), TestContext.Current.CancellationToken);
}
