using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;

public sealed class RouteInitFrameworkAlignmentIntegrationTests
{
    [Theory(DisplayName = "Framework Route Init aligns zero, one, many, and consecutive scopes at each supported position"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    [InlineData("memory/crystallized/documents", ".agents/memory/crystallized/_crystallized.md", ".agents/memory/crystallized/documents/_documents.md", (int)CliSemanticStatus.Complete, 0)]
    [InlineData("memory/release-notes/crystallized/documents", ".agents/memory/release-notes/crystallized/_crystallized.md", ".agents/memory/release-notes/crystallized/documents/_documents.md", (int)CliSemanticStatus.Attention, 1)]
    [InlineData("memory/release-notes/august/crystallized/documents", ".agents/memory/release-notes/august/crystallized/_crystallized.md", ".agents/memory/release-notes/august/crystallized/documents/_documents.md", (int)CliSemanticStatus.Attention, 2)]
    [InlineData("memory/crystallized/release-notes/documents", ".agents/memory/crystallized/_crystallized.md", ".agents/memory/crystallized/release-notes/documents/_documents.md", (int)CliSemanticStatus.Attention, 1)]
    [InlineData("memory/crystallized/release-notes/august/documents", ".agents/memory/crystallized/_crystallized.md", ".agents/memory/crystallized/release-notes/august/documents/_documents.md", (int)CliSemanticStatus.Attention, 2)]
    public async Task AlignsScopesAndPreservesCanonicalChainOrder(
        string target,
        string expectedCrystallizedPath,
        string expectedDocumentsPath,
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
                expectedCrystallizedPath,
                expectedDocumentsPath,
            ],
            result.Framework!.Segments
                .Where(segment => segment.Role != RouteInitFrameworkSegmentRole.Scope)
                .Select(segment => segment.Path));
        Assert.Equal(
            [
                ".agents/memory/_memory.md",
                ".agents/memory/crystallized/_crystallized.md",
                ".agents/memory/crystallized/documents/_documents.md",
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

    [Theory(DisplayName = "Framework ID-form scopes use invariant rune slugging and retain digits"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    [InlineData("memory/Überblick 2026/crystallized/documents", "überblick-2026")]
    [InlineData("memory/Release__Notes/crystallized/documents", "release-notes")]
    public async Task IdFormScopeLabelsUseRuneAwareSlugs(
        string target,
        string expectedSlug)
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-rune-slugs",
            TestContext.Current.CancellationToken);

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
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

    [Fact(DisplayName = "Framework exact path input preserves a concrete scope label without slugging"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task ExactPathScopeIsNotSlugged()
    {
        const string target = ".agents/memory/Release Notes/crystallized/documents/_documents.md";
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-exact-path",
            TestContext.Current.CancellationToken);

        var result = await ExecuteAsync(workspace, target);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
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

    [Fact(DisplayName = "Framework Route Init rejects a missing exact compatibility entrypoint without canonicalizing it"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task MissingExactCompatibilityEntrypointIsReadOnly()
    {
        const string target = ".agents/memory/crystallized/documents/index.md";
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
        Assert.True(workspace.Exists(".agents/memory/crystallized/documents/_documents.md"));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(RouteInitRecoveryState.NotRequired, result.Recovery.State);
    }

    [Theory(DisplayName = "Framework alignment refuses ambiguous, reordered, nested-root, and trailing-scope targets without writes"),
     Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    [InlineData("memory/crystallized/crystallized/documents")]
    [InlineData("memory/documents/crystallized")]
    [InlineData("memory/directives/crystallized/documents")]
    [InlineData("memory/crystallized/documents/notes")]
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

    [Fact(DisplayName = "Framework creation publishes only managed and generated facts while preserving the complete empty Extensions section"),
     Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task SparseScopePublicationPreservesLifecycleEnvelopeAndOwnership()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-lifecycle-publication",
            TestContext.Current.CancellationToken);
        var beforeLifecycle = workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.LifecyclePath);
        using var beforeDocument = JsonDocument.Parse(beforeLifecycle);
        var beforeExtensions = beforeDocument.RootElement
            .GetProperty("extensions")
            .GetRawText();

        var result = await ExecuteAsync(
            workspace,
            "memory/release-notes/crystallized/documents");

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
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
            workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.LifecyclePath));
        Assert.Equal(
            ["schemaVersion", "fingerprintPolicy", "workspacePath", "framework", "extensions"],
            afterDocument.RootElement.EnumerateObject().Select(property => property.Name));
        var extensions = afterDocument.RootElement.GetProperty("extensions");
        Assert.Equal("complete", extensions.GetProperty("coverage").GetString());
        Assert.Empty(extensions.GetProperty("packages").EnumerateArray());
        Assert.Empty(extensions.GetProperty("paths").EnumerateArray());
        Assert.Equal(beforeExtensions, extensions.GetRawText());
        Assert.DoesNotContain(
            afterDocument.RootElement.GetProperty("framework")
                .GetProperty("targets")
                .EnumerateArray(),
            target => target.GetProperty("path").GetString()
                == ".agents/memory/release-notes/_release-notes.md");
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(
                TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Framework segment and effect provenance distinguish embedded managed assets from user-owned scopes"),
     Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task ManagedAndScopeProvenanceRemainDistinct()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-provenance",
            TestContext.Current.CancellationToken);
        var result = await ExecuteAsync(
            workspace,
            "memory/release-notes/crystallized/documents");

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
        var managedAsset = payload.Find(".agents/memory/crystallized/documents/_documents.md")
            ?? throw new InvalidOperationException("The canonical documents asset is unavailable.");
        Assert.Equal(
            managedAsset.Bytes.ToArray(),
            await File.ReadAllBytesAsync(
                workspace.Combine(".agents/memory/crystallized/documents/_documents.md"),
                TestContext.Current.CancellationToken));
        Assert.Contains(
            result.Entrypoints,
            entrypoint => entrypoint.Path == ".agents/memory/release-notes/crystallized/documents/_documents.md"
                && entrypoint.Ownership == RouteInitEntrypointOwnership.Framework
                && entrypoint.SourceAssetPath == ".agents/memory/crystallized/documents/_documents.md");
    }

    private static ValueTask<RouteInitResult> ExecuteAsync(
        RouteInitFrameworkIntegrationWorkspace workspace,
        string target,
        RouteInitMode mode = RouteInitMode.Apply)
        => RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(target, mode), TestContext.Current.CancellationToken);
}
