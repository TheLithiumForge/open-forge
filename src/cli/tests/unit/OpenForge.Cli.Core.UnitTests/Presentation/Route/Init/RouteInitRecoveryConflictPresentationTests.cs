using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Result;
using OpenForge.Cli.Core.Presentation.Route.Init;
using OpenForge.Cli.Core.Presentation.Route.Init.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Route.Init;

[Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
public sealed class RouteInitRecoveryConflictPresentationTests
{
    private const string RequestedRouteId = "memory/project-alpha/documents";
    private const string RequestedRoutePath = ".agents/memory/project-alpha/documents/_documents.md";
    private const string RecoveryCandidatePath = "recovery/open-forge/recovery-candidate.zip";
    private const string RecoveryConflictMessage = "Recovery data from an earlier run exists at "
        + RecoveryCandidatePath
        + " and blocks this change. Nothing was changed.";
    private const string CleanupPreviewCommand = "open-forge cleanup --dry-run";
    private const string CleanupPreviewReason = "Review the recovery file and preview what cleanup would remove before deleting anything.";
    private const string RetainedCleanupReason = "Review and remove the reported recovery artifact after confirming the verified Route Init result.";

    [Fact(DisplayName = "Route Init preserves the recovery candidate through every text and JSON detail level"), Trait("Boundary", "Output")]
    public void RecoveryConflictProjectionIsStableAtEveryDetailLevel()
    {
        var result = Result(RecoveryConflict(RecoveryCandidatePath));
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RequestedRoutePath, result.Target.Path);
        var captures = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var report = RouteInitReportSelector.Select(result, new CliSelection(detail, null));
            var finding = Assert.Single(report.Findings);
            Assert.Equal(CliSemanticStatus.Blocked, report.Status);
            Assert.Equal(CliSubjectKind.File, finding.Subject.Kind);
            Assert.Equal(RecoveryCandidatePath, finding.Subject.Path);
            Assert.False(string.IsNullOrWhiteSpace(finding.Subject.Path));
            Assert.Equal(RecoveryConflictMessage, finding.Message);
            Assert.DoesNotContain(RequestedRoutePath, finding.Message, StringComparison.Ordinal);
            Assert.Equal(CleanupPreviewCommand, Assert.Single(finding.Actions).Command);
            Assert.Equal(CleanupPreviewReason, finding.Actions[0].Reason);
            Assert.Equal(CleanupPreviewCommand, report.Next?.Command);
            Assert.Equal(CleanupPreviewReason, report.Next?.Reason);

            var text = Render(result, CliFormat.Text, detail).PrimaryContent;
            var jsonText = Render(result, CliFormat.Json, detail).PrimaryContent;
            Assert.Contains(RecoveryCandidatePath, text, StringComparison.Ordinal);
            using var json = JsonDocument.Parse(jsonText);
            var root = json.RootElement;
            var jsonFinding = Assert.Single(root.GetProperty("findings").EnumerateArray());
            Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
            Assert.Equal("blocked", root.GetProperty("status").GetString());
            Assert.Equal("file", jsonFinding.GetProperty("subject").GetProperty("kind").GetString());
            var jsonSubjectPath = jsonFinding.GetProperty("subject").GetProperty("path").GetString();
            Assert.False(string.IsNullOrWhiteSpace(jsonSubjectPath));
            Assert.Equal(RecoveryCandidatePath, jsonSubjectPath);
            Assert.Equal(RecoveryConflictMessage, jsonFinding.GetProperty("message").GetString());
            var action = Assert.Single(jsonFinding.GetProperty("actions").EnumerateArray());
            Assert.Equal("command", action.GetProperty("kind").GetString());
            Assert.Equal(CleanupPreviewCommand, action.GetProperty("command").GetString());
            Assert.Equal(CleanupPreviewReason, action.GetProperty("reason").GetString());
            Assert.Equal(CleanupPreviewCommand, root.GetProperty("next").GetProperty("command").GetString());
            Assert.Equal(CleanupPreviewReason, root.GetProperty("next").GetProperty("reason").GetString());
            Assert.Equal(RequestedRoutePath, root.GetProperty("data").GetProperty("target").GetProperty("path").GetString());

            var detailName = detail.ToString().ToLowerInvariant();
            captures.Add($"recovery-conflict.{detailName}", text.Replace("\r\n", "\n", StringComparison.Ordinal));
            captures.Add($"recovery-conflict{CommandOutputSnapshot.JsonContentNameSegment}{detailName}", jsonText.Replace("\r\n", "\n", StringComparison.Ordinal));
        }

        CommandOutputSnapshot.MatchDetailSnapshot(captures);
    }

    [Theory(DisplayName = "Route Init requires an observed recovery candidate before it can form a conflict finding"), Trait("Boundary", "Output")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RecoveryConflictRequiresAnObservedTarget(string? target)
    {
        CliRenderedOutput? rendered = null;
        var exception = Assert.ThrowsAny<ArgumentException>(
            () => rendered = Render(
                Result(RecoveryConflict(target)),
                CliFormat.Json,
                CliDetail.Minimal));

        Assert.Equal("target", exception.ParamName);
        Assert.Null(rendered);
    }

    [Fact(DisplayName = "Route Init keeps optional targets for non-conflict findings"), Trait("Boundary", "Output")]
    public void NonConflictFindingStillAllowsMissingTarget()
    {
        var finding = new RouteInitFinding(
            RouteInitFindingCode.OperationFailed,
            "The operation failed.",
            target: null);

        Assert.Null(finding.Target);
    }

    [Fact(DisplayName = "Route Init non-conflict messages prefer the resolved path when the finding target is absent"), Trait("Boundary", "Output")]
    public void NonConflictMessageUsesResolvedPathWhenFindingTargetIsMissing()
    {
        const string requestedId = "memory/project-alpha/requested-route-id";
        const string resolvedPath = ".agents/memory/project-alpha/resolved-route.md";
        var result = Result(
            new RouteInitTarget(requestedId, requestedId, resolvedPath),
            new RouteInitFinding(
                RouteInitFindingCode.RecoveryUnavailable,
                "The recovery store could not be used.",
                target: null));
        var report = RouteInitReportSelector.Select(result, new CliSelection(CliDetail.Minimal, null));
        var finding = Assert.Single(report.Findings);

        Assert.Equal(
            "Recovery data could not be prepared at .agents/memory/project-alpha/resolved-route.md. Nothing was changed.",
            finding.Message);
        Assert.DoesNotContain(requestedId, finding.Message, StringComparison.Ordinal);
        Assert.Contains(requestedId, report.Headline.Sentence, StringComparison.Ordinal);
        Assert.DoesNotContain(resolvedPath, report.Headline.Sentence, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Init keeps failure guidance ahead of a recovery-conflict preview"), Trait("Boundary", "Output")]
    public void HigherSeverityFailureStillSelectsItsNextAction()
    {
        var result = Result(
            RecoveryConflict(RecoveryCandidatePath),
            new RouteInitFinding(
                RouteInitFindingCode.OperationFailed,
                "The operation failed.",
                RequestedRoutePath));
        var report = RouteInitReportSelector.Select(result, new CliSelection(CliDetail.Minimal, null));

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(CliSemanticStatus.Failed, report.Status);
        Assert.Equal("open-forge route init --detail debug", report.Next?.Command);
    }

    [Fact(DisplayName = "Route Init keeps its existing recovery-retention cleanup guidance"), Trait("Boundary", "Output")]
    public void RecoveryArtifactRetainedKeepsItsExistingCleanupAction()
    {
        var result = Result(
            new RouteInitRecovery(RouteInitRecoveryState.Retained, RecoveryCandidatePath),
            new RouteInitFinding(
                RouteInitFindingCode.RecoveryArtifactRetained,
                "The verified operation retained its recovery artifact.",
                RecoveryCandidatePath));
        var report = RouteInitReportSelector.Select(result, new CliSelection(CliDetail.Minimal, null));
        var finding = Assert.Single(report.Findings);

        Assert.Equal(CliSemanticStatus.Attention, report.Status);
        Assert.Equal("open-forge cleanup", Assert.Single(finding.Actions).Command);
        Assert.Equal(RetainedCleanupReason, finding.Actions[0].Reason);
        Assert.Equal("open-forge cleanup", report.Next?.Command);
        Assert.Equal(RetainedCleanupReason, report.Next?.Reason);
    }

    private static RouteInitFinding RecoveryConflict(string? target)
        => new(
            RouteInitFindingCode.RecoveryConflict,
            "An existing recovery candidate blocks the requested route change.",
            target);

    private static RouteInitResult Result(params RouteInitFinding[] findings)
        => Result(
            new RouteInitTarget(RequestedRouteId, RequestedRouteId, RequestedRoutePath),
            new RouteInitRecovery(RouteInitRecoveryState.NotRequired, null),
            findings);

    private static RouteInitResult Result(
        RouteInitTarget target,
        params RouteInitFinding[] findings)
        => Result(
            target,
            new RouteInitRecovery(RouteInitRecoveryState.NotRequired, null),
            findings);

    private static RouteInitResult Result(RouteInitRecovery recovery, params RouteInitFinding[] findings)
        => Result(
            new RouteInitTarget(RequestedRouteId, RequestedRouteId, RequestedRoutePath),
            recovery,
            findings);

    private static RouteInitResult Result(
        RouteInitTarget target,
        RouteInitRecovery recovery,
        params RouteInitFinding[] findings)
    {
        var formation = new RouteInitResultFormation(
            workspace: null,
            mode: RouteInitMode.Apply,
            scaffold: RouteInitScaffold.Generic,
            target: target,
            plan: new RouteInitPlanFacts(
                RouteInitPlanCompleteness.Complete,
                RouteInitPlanSafety.Safe),
            framework: null,
            entrypoints: [],
            effects: [],
            unchangedPaths: [],
            lifecycle: new RouteInitLifecycle(
                RouteInitLifecycleAction.None,
                RouteInitLifecycleOutcome.NotRequested),
            recovery: recovery,
            verification: RouteInitVerificationState.NotRequested,
            findings: findings);
        return new RouteInitResultBuilder().Build(formation);
    }

    private static CliRenderedOutput Render(
        RouteInitResult result,
        CliFormat format,
        CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<RouteInitResult>(
                result,
                new CliPresentation(format, detail, null)),
            RouteInitPresentation.Rendering);
}
