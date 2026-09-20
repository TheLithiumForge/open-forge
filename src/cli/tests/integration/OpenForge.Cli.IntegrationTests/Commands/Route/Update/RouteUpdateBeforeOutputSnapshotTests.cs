using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Update;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class RouteUpdateBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<RouteUpdateResult> Renderers = CommandOutputRenderers<RouteUpdateResult>.From(RouteUpdatePresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route update reports an unavailable ambiguous ID without interactive input")]
    public async Task AttemptedAmbiguousIdPrompt()
    {
        using var workspace = new RoutedOutputWorkspace();
        workspace.CollidingGuideId();
        var before = workspace.Snapshot();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "ambiguous-id-prompt",
            Arguments = ["route", "update", "docs/guide", "--description", "Updated guide"],
            ExitCode = 5,
            DiagnosticId = "route-update.route-ambiguous",
        }, snapshotCollector: null);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route update output preserves edited metadata when the later parent update is denied")]
    public async Task PartialWriteFailure()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-output-partial");
        var parentBefore = workspace.ReadText(RouteUpdateIntegrationWorkspace.ParentPath);
        RouteUpdateResult result;
        using (var held = File.Open(Path.Combine(workspace.Workspace.PhysicalRoot, RouteUpdateIntegrationWorkspace.ParentPath),
                   FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            result = await workspace.ExecuteAsync(workspace.Request(patch: RouteUpdateIntegrationWorkspace.DescriptionPatch("After overview")),
                TestContext.Current.CancellationToken);
        }
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Contains("After overview", workspace.ReadText(RouteUpdateIntegrationWorkspace.TargetPath), StringComparison.Ordinal);
        Assert.Equal(parentBefore, workspace.ReadText(RouteUpdateIntegrationWorkspace.ParentPath));
        Assert.NotNull(result.Recovery.ResidualPath);
        CaptureRecovery(workspace, result);
        Renderers.MatchDetails(result, "write-failed-partial", result.Recovery.ResidualPath);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route update output preserves metadata edits, guarded templates and previews")]
    public async Task MetadataAndTemplate()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, status) in new[]
        {
            ("description-changed", CliSemanticStatus.Complete),
            ("tags-replaced", CliSemanticStatus.Complete),
            ("responsibility-removed", CliSemanticStatus.Complete),
            ("template-applied", CliSemanticStatus.Complete),
            ("template-body-protected", CliSemanticStatus.Attention),
            ("no-change", CliSemanticStatus.Complete),
            ("dry-run", CliSemanticStatus.Complete),
        })
        {
            using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-output");
            if (situation is "template-applied" or "template-body-protected") workspace.SeedTemplate();
            if (situation == "template-applied") workspace.SeedEmptyBodyTarget();
            var patch = situation switch
            {
                "tags-replaced" => RouteUpdateIntegrationWorkspace.TagsPatch("Docs", "Guide"),
                "responsibility-removed" => RouteUpdateIntegrationWorkspace.ResponsibilityPatch(null),
                "no-change" => RouteUpdateIntegrationWorkspace.DescriptionPatch("Before overview"),
                _ => RouteUpdateIntegrationWorkspace.DescriptionPatch("After overview"),
            };
            var before = workspace.SnapshotHashes();
            var result = await workspace.ExecuteAsync(workspace.Request(patch: patch,
                templateReference: situation is "template-applied" or "template-body-protected" ? RouteUpdateIntegrationWorkspace.TemplateId : null,
                mode: situation == "dry-run" ? RouteUpdateMode.DryRun : RouteUpdateMode.Apply), TestContext.Current.CancellationToken);
            Assert.Equal(status, result.Status);
            if (situation is "no-change" or "dry-run") Assert.Equal(before, workspace.SnapshotHashes());
            if (situation == "template-applied") Assert.Contains(RouteUpdateIntegrationWorkspace.TemplateBody, workspace.ReadText(RouteUpdateIntegrationWorkspace.TargetPath), StringComparison.Ordinal);
            CaptureRecovery(workspace, result);
            Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route update output preserves unknown sources, held locks and cancellation")]
    public async Task SafetyBoundary()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, status) in new[]
        {
            ("unknown-source", CliSemanticStatus.Invalid),
            ("lock-held", CliSemanticStatus.Blocked),
            ("cancelled", CliSemanticStatus.Interrupted),
        })
        {
            using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-output-boundary");
            var before = workspace.SnapshotHashes();
            using var cancellation = new CancellationTokenSource();
            if (situation == "cancelled") cancellation.Cancel();
            using var held = situation == "lock-held" ? workspace.HoldLock() : null;
            var result = await workspace.ExecuteAsync(workspace.Request(sourceReference: situation == "unknown-source" ? "missing" : RouteUpdateIntegrationWorkspace.TargetId), cancellation.Token);
            Assert.Equal(status, result.Status);
            Assert.Equal(before, workspace.SnapshotHashes());
            CaptureRecovery(workspace, result);
            Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route update output rejects an invocation with no patch")]
    public async Task NoPatch()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-output-no-patch");
        var before = workspace.SnapshotHashes();
        await new ReadCommandOutputCapture(workspace.Workspace.PhysicalRoot).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "no-patch",
            Arguments = ["route", "update", RouteUpdateIntegrationWorkspace.TargetId],
            ExitCode = 4,
        }, snapshotCollector: null);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static void CaptureRecovery(RouteUpdateIntegrationWorkspace workspace, RouteUpdateResult result)
    {
        if (result.Recovery.ResidualPath is { } path)
        {
            workspace.TrackRecoveryPath(path);
            Assert.True(File.Exists(path));
        }
    }
}
