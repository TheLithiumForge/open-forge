using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;

public sealed class RouteInitFrameworkAlignmentIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Framework Route Init aligns zero, one, many, and consecutive scopes at each supported position"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    // The reduced Core ships one managed segment below each root, so scopes can only
    // precede it. Cases that placed a scope between two managed segments are covered
    // again when Framework scaffolding reaches Extension-created routes (Task 48).
    [InlineData("memory/working", ".agents/memory/working/_working.md", (int)CliSemanticStatus.Complete, 0)]
    [InlineData("memory/release-notes/working", ".agents/memory/release-notes/working/_working.md", (int)CliSemanticStatus.Complete, 1)]
    [InlineData("memory/release-notes/august/working", ".agents/memory/release-notes/august/working/_working.md", (int)CliSemanticStatus.Complete, 2)]
    public async Task AlignsScopesAndPreservesCanonicalChainOrder(
        string target,
        string expectedManagedPath,
        int expectedStatusValue,
        int expectedScopeCount)
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-alignment",
            TestContext.Current.CancellationToken);
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal((CliSemanticStatus)expectedStatusValue, result.Status);
        Assert.Equal(target, result.Target.Requested);
        Assert.NotNull(result.Framework);
        Assert.Equal(
            [
                ".agents/memory/_memory.md",
                expectedManagedPath,
            ],
            result.Framework!.Segments
                .Where(segment => segment.Role != RouteInitFrameworkSegmentRole.Scope)
                .Select(segment => segment.Path));
        Assert.Equal(
            [
                ".agents/memory/_memory.md",
                ".agents/memory/working/_working.md",
            ],
            result.Framework.Segments
                .Where(segment => segment.Role != RouteInitFrameworkSegmentRole.Scope)
                .Select(segment => segment.SourceAssetPath));
        Assert.Equal(expectedScopeCount, result.Framework.Segments.Count(
            segment => segment.Role == RouteInitFrameworkSegmentRole.Scope));
        Assert.Equal(
            expectedScopeCount,
            result.Entrypoints.Count(entrypoint =>
                entrypoint.Ownership == RouteInitEntrypointOwnership.User
                && entrypoint.Current == RouteInitEntrypointCurrent.Missing));

        if (expectedScopeCount == 0)
        {
            Assert.Empty(result.Effects);
            Assert.Equal(RouteInitLifecycleAction.Preserve, result.Lifecycle.Action);
            Assert.Equal(RouteInitLifecycleOutcome.AlreadyCurrent, result.Lifecycle.Outcome);
            Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
            Assert.Equal(before, workspace.SnapshotHashes());
            Assert.Empty(result.Findings);
        }
        else
        {
            Assert.Contains(
                result.Findings,
                finding => finding.Code == RouteInitFindingCode.NeedsAuthoring);
            Assert.Contains(
                result.Effects,
                effect => effect.Kind == RouteInitEffectKind.Entrypoint
                    && effect.Action == RouteInitEffectAction.Create
                    && effect.SourceAssetPath is null
                    && effect.Outcome == RouteInitEffectOutcome.Verified);
            Assert.Equal(RouteInitLifecycleAction.Publish, result.Lifecycle.Action);
            Assert.Equal(RouteInitLifecycleOutcome.Verified, result.Lifecycle.Outcome);
            Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        }
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Framework ID-form scopes use invariant rune slugging and retain digits"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    [InlineData("memory/Überblick 2026/working", "überblick-2026")]
    [InlineData("memory/Release__Notes/working", "release-notes")]
    public async Task IdFormScopeLabelsUseRuneAwareSlugs(
        string target,
        string expectedSlug)
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-rune-slugs",
            TestContext.Current.CancellationToken);

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var scope = Assert.Single(
            result.Framework!.Segments,
            segment => segment.Role == RouteInitFrameworkSegmentRole.Scope);
        Assert.Equal($".agents/memory/{expectedSlug}/_{expectedSlug}.md", scope.Path);
        var entrypoint = Assert.Single(
            result.Entrypoints,
            candidate => candidate.Ownership == RouteInitEntrypointOwnership.User);
        Assert.Equal($".agents/memory/{expectedSlug}/_{expectedSlug}.md", entrypoint.Path);
        Assert.Null(entrypoint.SourceAssetPath);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Framework exact path input preserves a concrete scope label without slugging"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task ExactPathScopeIsNotSlugged()
    {
        const string target = ".agents/memory/Release Notes/working/_working.md";
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-exact-path",
            TestContext.Current.CancellationToken);

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(target, result.Target.Path);
        var scope = Assert.Single(
            result.Framework!.Segments,
            segment => segment.Role == RouteInitFrameworkSegmentRole.Scope);
        Assert.Equal(".agents/memory/Release Notes/_Release Notes.md", scope.Path);
        Assert.Contains(
            result.Entrypoints,
            entrypoint => entrypoint.Path == scope.Path
                && entrypoint.Ownership == RouteInitEntrypointOwnership.User);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Framework Route Init rejects a missing exact compatibility entrypoint without canonicalizing it"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task MissingExactCompatibilityEntrypointIsReadOnly()
    {
        const string target = ".agents/memory/working/index.md";
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-missing-exact-compatibility",
            TestContext.Current.CancellationToken);
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(RouteInitFindingCode.InvalidTarget, Assert.Single(result.Findings).Code);
        Assert.Empty(result.Effects);
        Assert.Equal(target, result.Target.Path);
        Assert.False(workspace.Exists(target));
        Assert.True(workspace.Exists(".agents/memory/working/_working.md"));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(RouteInitRecoveryState.NotRequired, result.Recovery.State);
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Framework alignment refuses ambiguous, reordered, nested-root, and trailing-scope targets without writes"),
     Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    [InlineData("memory/working/working")]
    [InlineData("memory/directives/working")]
    [InlineData("memory/working/notes")]
    public async Task UnsafeAlignmentsAreBlockedBeforeEffects(string target)
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-alignment-blocked",
            TestContext.Current.CancellationToken);
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteInitFindingCode.FrameworkAlignmentBlocked);
        Assert.Equal(RouteInitPlanSafety.Blocked, result.Plan.Safety);
        Assert.Equal(RouteInitLifecycleOutcome.NotStarted, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitVerificationState.NotRequested, result.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(
                TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Framework creation publishes only managed and generated facts while preserving the complete empty Extensions section"),
     Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task SparseScopePublicationPreservesLifecycleEnvelopeAndOwnership()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-lifecycle-publication",
            TestContext.Current.CancellationToken);
        var beforeLifecycle = workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.OwnershipPath);
        using var beforeDocument = JsonDocument.Parse(beforeLifecycle);
        var beforeExtensions = beforeDocument.RootElement
            .GetProperty("extensions")
            .GetRawText();

        var result = await ExecuteAsync(
            workspace,
            "memory/release-notes/working");

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteInitLifecycleAction.Publish, result.Lifecycle.Action);
        Assert.Equal(RouteInitLifecycleOutcome.Verified, result.Lifecycle.Outcome);
        Assert.Contains(
            result.Effects,
            effect => effect.Kind == RouteInitEffectKind.Entrypoint
                && effect.SourceAssetPath is null
                && effect.Path == ".agents/memory/release-notes/_release-notes.md");
        Assert.Contains(
            result.Effects,
            effect => effect.Kind == RouteInitEffectKind.GeneratedRegion
                && effect.SourceAssetPath is null);

        using var afterDocument = JsonDocument.Parse(
            workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.OwnershipPath));
        Assert.Equal(
            ["$schema", "schemaVersion", "framework", "extensions", "libraries"],
            afterDocument.RootElement.EnumerateObject().Select(property => property.Name));
        var extensions = afterDocument.RootElement.GetProperty("extensions");
        Assert.Empty(extensions.EnumerateArray());
        Assert.Equal(beforeExtensions, extensions.GetRawText());
        Assert.DoesNotContain(
            afterDocument.RootElement.GetProperty("framework")
                .GetProperty("paths")
                .EnumerateArray(),
            target => target.GetString()
                == ".agents/memory/release-notes/_release-notes.md");
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(
                TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Framework segment and effect provenance distinguish embedded managed assets from user-owned scopes"),
     Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task ManagedAndScopeProvenanceRemainDistinct()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-provenance",
            TestContext.Current.CancellationToken);
        var result = await ExecuteAsync(
            workspace,
            "memory/release-notes/working");

        Assert.NotNull(result.Framework);
        var scope = Assert.Single(
            result.Framework!.Segments,
            segment => segment.Role == RouteInitFrameworkSegmentRole.Scope);
        Assert.Null(scope.SourceAssetPath);
        Assert.All(
            result.Framework.Segments.Where(segment =>
                segment.Role is RouteInitFrameworkSegmentRole.InstalledRoot
                    or RouteInitFrameworkSegmentRole.Managed),
            segment => Assert.False(string.IsNullOrWhiteSpace(segment.SourceAssetPath)));
        Assert.All(
            result.Effects.Where(effect => effect.Kind == RouteInitEffectKind.Entrypoint),
            effect => Assert.Null(effect.SourceAssetPath));

        var payload = EmbeddedFrameworkPayloadReader.Read().Payload
            ?? throw new InvalidOperationException("The embedded Framework payload is unavailable.");
        var managedAsset = payload.Find(".agents/memory/working/_working.md")
            ?? throw new InvalidOperationException("The canonical Working asset is unavailable.");
        Assert.Equal(
            managedAsset.Bytes.ToArray(),
            await File.ReadAllBytesAsync(
                workspace.Combine(".agents/memory/working/_working.md"),
                TestContext.Current.CancellationToken));
        Assert.Contains(
            result.Entrypoints,
            entrypoint => entrypoint.Path == ".agents/memory/release-notes/working/_working.md"
                && entrypoint.Ownership == RouteInitEntrypointOwnership.Framework
                && entrypoint.SourceAssetPath == ".agents/memory/working/_working.md");
    }

    private static ValueTask<RouteInitResult> ExecuteAsync(
        RouteInitFrameworkIntegrationWorkspace workspace,
        string target,
        RouteInitMode mode = RouteInitMode.Apply)
        => RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(target, mode), TestContext.Current.CancellationToken);
}
