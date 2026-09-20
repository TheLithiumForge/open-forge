using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Create;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class RouteCreateBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<RouteCreateResult> Renderers = CommandOutputRenderers<RouteCreateResult>.From(RouteCreatePresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route create output preserves the created file when the later parent update is denied")]
    public async Task PartialWriteFailure()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-output-partial");
        workspace.SeedBase();
        workspace.OwnApplicationCreatedTarget();
        var parentBefore = workspace.ReadText(RouteCreateIntegrationWorkspace.ParentPath);
        RouteCreateResult result;
        using (var held = File.Open(workspace.Absolute(RouteCreateIntegrationWorkspace.ParentPath), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            result = await RouteCreateOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
        }
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.True(File.Exists(workspace.Absolute(RouteCreateIntegrationWorkspace.TargetPath)));
        Assert.Equal(parentBefore, workspace.ReadText(RouteCreateIntegrationWorkspace.ParentPath));
        Assert.NotNull(result.Recovery.ResidualPath);
        CaptureRecovery(workspace, result);
        Renderers.MatchDetails(result, "write-failed-partial", result.Recovery.ResidualPath);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route create output preserves creation, templates, previews and exact no-ops")]
    public async Task Creation()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var situation in new[] { "created", "created-from-template", "dry-run", "already-matching" })
        {
            using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-output");
            workspace.SeedBase();
            if (situation == "created-from-template")
            {
                workspace.SeedTemplate();
            }

            if (situation == "already-matching")
            {
                workspace.SeedCompleteTarget();
            }
            else
            {
                workspace.OwnApplicationCreatedTarget();
            }

            var before = workspace.SnapshotHashes();
            var result = await RouteCreateOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(workspace.Request(
                situation == "dry-run" ? RouteCreateMode.DryRun : RouteCreateMode.Apply,
                situation == "created-from-template" ? RouteCreateIntegrationWorkspace.TemplateId : null), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            if (situation is "dry-run" or "already-matching")
            {
                Assert.Equal(before, workspace.SnapshotHashes());
            }
            else
            {
                Assert.True(File.Exists(workspace.Absolute(RouteCreateIntegrationWorkspace.TargetPath)));
                Assert.Contains("[Project overview](overview.md)", workspace.ReadText(RouteCreateIntegrationWorkspace.ParentPath), StringComparison.Ordinal);
            }

            CaptureRecovery(workspace, result);
            Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route create output preserves target and parent safety failures without effects")]
    public async Task SafetyBoundary()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, status) in new[]
        {
            ("parent-missing", CliSemanticStatus.Blocked),
            ("exists-with-different-content", CliSemanticStatus.Blocked),
            ("template-unknown", CliSemanticStatus.Invalid),
            ("lock-held", CliSemanticStatus.Blocked),
            ("cancelled", CliSemanticStatus.Interrupted),
        })
        {
            using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-output-boundary");
            workspace.SeedBase();
            if (situation == "parent-missing") workspace.RemoveParent();
            if (situation == "exists-with-different-content") workspace.SeedDifferingTarget();
            using var cancellation = new CancellationTokenSource();
            if (situation == "cancelled") cancellation.Cancel();
            var before = workspace.SnapshotHashes();
            using var held = situation == "lock-held" ? workspace.HoldLock() : null;
            var result = await RouteCreateOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
                workspace.Request(templateReference: situation == "template-unknown" ? "missing-template" : null), cancellation.Token);
            Assert.Equal(status, result.Status);
            Assert.Equal(before, workspace.SnapshotHashes());
            CaptureRecovery(workspace, result);
            Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route create output preserves missing metadata and invalid targets at binding")]
    public async Task InvalidInput()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var situation in new[] { "missing-description", "missing-tag", "missing-both", "invalid-target" })
        {
            using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-output-input");
            workspace.SeedBase();
            var target = situation == "invalid-target" ? "../outside" : RouteCreateIntegrationWorkspace.TargetId;
            string[] description = situation is "missing-description" or "missing-both" ? [] : ["--description", "Project overview"];
            string[] tags = situation is "missing-tag" or "missing-both" ? [] : ["--tag", "Docs"];
            var before = workspace.SnapshotHashes();
            var isOptionalDryRun = situation is "missing-description" or "missing-tag" or "missing-both";
            var arguments = new List<string> { "route", "create", target };
            arguments.AddRange(description);
            arguments.AddRange(tags);
            if (isOptionalDryRun)
            {
                arguments.Add("--dry-run");
            }

            await new ReadCommandOutputCapture(workspace.Workspace.PhysicalRoot).MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = situation,
                Arguments = arguments.ToArray(),
                ExitCode = isOptionalDryRun ? 2 : 4,
            }, snapshotCollector: snapshots);
            Assert.Equal(before, workspace.SnapshotHashes());
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    private static void CaptureRecovery(RouteCreateIntegrationWorkspace workspace, RouteCreateResult result)
    {
        if (result.Recovery.ResidualPath is { } path)
        {
            workspace.TrackRecoveryPath(path);
            Assert.True(File.Exists(path));
        }
    }
}
