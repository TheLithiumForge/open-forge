using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Generic;

public sealed class GenericRouteInitSafetyIntegrationTests
{
    [Theory(DisplayName = "Generic Route Init rejects unsafe target forms before filesystem effects"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    [InlineData("loader")]
    [InlineData(".agents/memory")]
    [InlineData(".agents/memory/ordinary.md")]
    [InlineData("memory/../outside")]
    [InlineData("../outside")]
    public async Task UnsafeTargetFormsRemainReadOnly(string routeTarget)
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-invalid-target");
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            workspace.Request(routeTarget));

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(RouteInitFindingCode.InvalidTarget, Assert.Single(result.Findings).Code);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents"));
        Assert.Equal(RouteInitRecoveryState.NotRequired, result.Recovery.State);
    }

    [Fact(DisplayName = "Generic Route Init rejects a missing exact compatibility entrypoint without canonicalizing it"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task MissingExactCompatibilityEntrypointIsReadOnly()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-missing-exact-compatibility");
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            workspace.Request(".agents/memory/index.md"));

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(RouteInitFindingCode.InvalidTarget, Assert.Single(result.Findings).Code);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents/memory/index.md"));
        Assert.False(workspace.Exists(".agents/memory/_memory.md"));
        Assert.Equal(RouteInitRecoveryState.NotRequired, result.Recovery.State);
    }

    [Theory(DisplayName = "Generic Route Init rejects malformed explicit metadata before creating a target"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    [InlineData(" ", false, "Ready", "Docs", "description")]
    [InlineData("Ready", true, " ", "Docs", "responsibility")]
    [InlineData("Ready", false, null, "", "empty-tag")]
    [InlineData("Ready", false, null, "Docs|Docs", "duplicate-tag")]
    [InlineData("Ready", false, null, "Docs|Bad#Tag", "invalid-tag")]
    public async Task InvalidMetadataIsReadOnly(
        string description,
        bool responsibilitySpecified,
        string? responsibility,
        string tags,
        string _)
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-invalid-metadata");
        var metadata = new RouteInitMetadataInput(
            description,
            responsibilitySpecified,
            responsibility,
            tags.Split('|', StringSplitOptions.None));
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("documents", metadata: metadata));

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(RouteInitFindingCode.InvalidMetadata, Assert.Single(result.Findings).Code);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents"));
    }

    [Fact(DisplayName = "Generic Route Init blocks an ordinary-file route collision before creating descendants"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task OrdinaryFileCollisionIsBlockedWithoutWrites()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-ordinary-collision");
        workspace.WriteText(".agents/memory", "an ordinary file occupies the route folder\n");
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("memory/project"));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInitFindingCode.IdentityCollision, Assert.Single(result.Findings).Code);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents/memory/project"));
    }

    [Fact(DisplayName = "Generic Route Init blocks a physical directory alias that escapes the workspace"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task ExternalDirectoryAliasIsBlockedWithoutWrites()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-physical-alias");
        workspace.CreateDirectory(".agents");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(
                ".agents/memory",
                Path.GetTempPath(),
                out _));
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("memory/project"));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInitFindingCode.TargetUnsafe, Assert.Single(result.Findings).Code);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents/memory/project/_project.md"));
    }

    [Fact(DisplayName = "Generic Route Init blocks a malformed Loader generated boundary before any route write"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task MalformedLoaderBoundaryIsReadOnly()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-malformed-loader");
        workspace.WriteText(
            ".agents/loader.md",
            "# Loader\n\n<!-- open-forge:generated-index:start -->\n\n- malformed without end marker\n");
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("memory/project"));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInitFindingCode.LoaderUnsafe, Assert.Single(result.Findings).Code);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents/memory"));
    }

    [Fact(DisplayName = "Generic Route Init blocks a malformed existing generated region before creating a child"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task MalformedExistingGeneratedRegionIsReadOnly()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-malformed-generated-region");
        workspace.WriteText(
            ".agents/memory/_memory.md",
            "---\nopen-forge:\n  description: Memory\n  tags: [Memory]\n---\n\n# memory\n\n"
                + "## Entries\n\n"
                + "<!-- open-forge:generated-index:start -->\n"
                + "<!-- open-forge:generated-index:start -->\n"
                + "\n- malformed duplicate start marker\n"
                + "<!-- open-forge:generated-index:end -->\n");
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("memory/project"));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInitFindingCode.GeneratedRegionUnsafe, Assert.Single(result.Findings).Code);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents/memory/project/_project.md"));
    }

    [Fact(DisplayName = "Generic Route Init reports a held real workspace lock without creating a directory or recovery artifact"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task HeldWorkspaceLockIsReadOnly()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-lock");
        var before = workspace.SnapshotHashes();
        RouteInitResult result;
        await using (workspace.HoldLock())
        {
            result = await ExecuteAsync(
                workspace,
                workspace.Request("memory/project"));
        }

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInitFindingCode.WorkspaceLockUnavailable, Assert.Single(result.Findings).Code);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents"));
        Assert.Equal(RouteInitRecoveryState.NotCreated, result.Recovery.State);
        Assert.Equal(0, await workspace.RecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Generic Route Init cancellation before lease acquisition leaves the workspace untouched"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task PreCancelledRequestIsReadOnly()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-cancelled");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("memory/project"),
            cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(RouteInitFindingCode.Interrupted, Assert.Single(result.Findings).Code);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents"));
        Assert.Equal(RouteInitRecoveryState.NotRequired, result.Recovery.State);
    }

    private static async ValueTask<RouteInitResult> ExecuteAsync(
        GenericRouteInitIntegrationWorkspace workspace,
        RouteInitRequest request,
        CancellationToken? cancellationToken = null)
        => await RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(request, cancellationToken ?? TestContext.Current.CancellationToken);
}
