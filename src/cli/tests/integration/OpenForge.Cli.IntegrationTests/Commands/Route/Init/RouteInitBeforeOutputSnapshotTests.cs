using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Init;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;
using OpenForge.Cli.IntegrationTests.Commands.Route.Init.Generic;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class RouteInitBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<RouteInitResult> Renderers = CommandOutputRenderers<RouteInitResult>.From(RouteInitPresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route init output preserves created child structure when an existing parent update is denied")]
    public async Task PartialWriteFailure()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-output-partial");
        var operation = RouteInitOperationFactory.Create(workspace.LockStoreRoot);
        var initial = await operation.ExecuteAsync(workspace.Request("documents", metadata:
            new RouteInitMetadataInput("Documents", true, "Owns documents.", ["Docs"])), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, initial.Status);
        const string parent = ".agents/documents/_documents.md";
        var before = workspace.ReadText(parent);
        RouteInitResult result;
        using (var held = File.Open(Path.Combine(workspace.Path, parent), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            result = await operation.ExecuteAsync(workspace.Request("documents/design"), TestContext.Current.CancellationToken);
        }
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.True(Directory.Exists(Path.Combine(workspace.Path, ".agents/documents/design")));
        Assert.Equal(before, workspace.ReadText(parent));
        Assert.NotNull(result.Recovery.ResidualPath);
        Assert.True(File.Exists(result.Recovery.ResidualPath));
        Renderers.MatchDetails(result, "write-failed-partial", result.Recovery.ResidualPath);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route init output preserves generic chain creation, explicit metadata and previews")]
    public async Task GenericScaffold()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, status) in new[]
        {
            ("new-chain", CliSemanticStatus.Complete),
            ("already-initialized", CliSemanticStatus.Complete),
            ("dry-run", CliSemanticStatus.Complete),
            ("explicit-metadata", CliSemanticStatus.Complete),
        })
        {
            using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-output");
            var operation = RouteInitOperationFactory.Create(workspace.LockStoreRoot);
            var metadata = new RouteInitMetadataInput("Documents", true, "Owns documents.", ["Docs"]);
            var target = situation is "explicit-metadata" or "already-initialized" ? "documents" : "documents/design";
            var request = workspace.Request(target,
                situation == "dry-run" ? RouteInitMode.DryRun : RouteInitMode.Apply,
                situation is "explicit-metadata" or "already-initialized" ? metadata : null);
            if (situation == "already-initialized")
            {
                var initial = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
                Assert.Equal(CliSemanticStatus.Complete, initial.Status);
                request = workspace.Request(target);
            }

            var before = workspace.SnapshotHashes();
            var result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(status, result.Status);
            if (situation is "already-initialized" or "dry-run")
            {
                Assert.Equal(before, workspace.SnapshotHashes());
            }
            else
            {
                Assert.True(workspace.Exists(".agents/documents/_documents.md"));
            }

            Renderers.MatchDetails(result, situation, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route init output preserves invalid targets and metadata without effects")]
    public async Task InvalidInput()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var situation in new[] { "invalid-target", "invalid-metadata" })
        {
            using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-output-invalid");
            var request = workspace.Request(situation == "invalid-target" ? "../outside" : "documents",
                metadata: situation == "invalid-metadata" ? new RouteInitMetadataInput("Documents", false, null, ["invalid tag"]) : null);
            var before = workspace.SnapshotHashes();
            var result = await RouteInitOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Invalid, result.Status);
            Assert.Equal(before, workspace.SnapshotHashes());
            Renderers.MatchDetails(result, situation, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route init output preserves Framework scaffolding and its installation requirement")]
    public async Task FrameworkScaffold()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, installed) in new[]
        {
            ("framework-scaffold", true),
            ("framework-not-installed", false),
        })
        {
            using var workspace = installed
                ? await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync("route-init-output-framework", TestContext.Current.CancellationToken)
                : RouteInitFrameworkIntegrationWorkspace.CreateEmpty("route-init-output-uninstalled");
            var before = workspace.SnapshotHashes();
            var result = await RouteInitOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
            Assert.Equal(installed ? CliSemanticStatus.Complete : CliSemanticStatus.Blocked, result.Status);
            if (!installed)
            {
                Assert.Equal(before, workspace.SnapshotHashes());
            }

            Renderers.MatchDetails(result, situation, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route init output preserves locking and pre-effect cancellation")]
    public async Task InterruptedBoundary()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, cancelled) in new[]
        {
            ("lock-held", false),
            ("cancelled", true),
        })
        {
            using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-output-boundary");
            using var cancellation = new CancellationTokenSource();
            if (cancelled)
            {
                cancellation.Cancel();
            }

            var before = workspace.SnapshotHashes();
            using var held = cancelled ? null : workspace.HoldLock();
            var result = await RouteInitOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(workspace.Request("documents"), cancellation.Token);
            Assert.Equal(cancelled ? CliSemanticStatus.Interrupted : CliSemanticStatus.Blocked, result.Status);
            Assert.Equal(before, workspace.SnapshotHashes());
            Renderers.MatchDetails(result, situation, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }
}
