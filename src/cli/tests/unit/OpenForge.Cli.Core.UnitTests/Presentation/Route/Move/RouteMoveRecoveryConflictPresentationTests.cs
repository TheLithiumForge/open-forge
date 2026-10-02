using System.Collections.Immutable;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Result;
using OpenForge.Cli.Core.Presentation.Route.Move;
using OpenForge.Cli.Core.Presentation.Route.Move.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Move;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Route.Move;

[Trait("Feature", "route-move"), Trait("Evidence", "Unit")]
public sealed class RouteMoveRecoveryConflictPresentationTests
{
    private const string RecoveryCandidatePath = "recovery/open-forge/recovery-candidate.zip";
    private const string RecoveryConflictMessage = "Recovery data from an earlier run exists at "
        + RecoveryCandidatePath
        + " and blocks this change. Nothing was changed.";
    private const string CleanupPreviewCommand = "open-forge cleanup --dry-run";
    private const string CleanupPreviewReason = "Review the recovery file and preview what cleanup would remove before deleting anything.";
    private const string RetainedCleanupReason = "Review and remove the reported recovery artifact after confirming the verified Route Move result.";

    [Fact(DisplayName = "Route Move preserves the recovery candidate through every text and JSON detail level"), Trait("Boundary", "Output")]
    public void RecoveryConflictProjectionIsStableAtEveryDetailLevel()
    {
        var result = Result(RecoveryConflict(CliSemanticStatus.Blocked, RecoveryCandidatePath));
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteMoveTestData.SourcePath, result.Source.Path);
        Assert.Equal(RouteMoveTestData.DestinationPath, result.Destination.Path);
        var captures = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var report = RouteMoveReportSelector.Select(result, new CliSelection(detail, null));
            var finding = Assert.Single(report.Findings);
            Assert.Equal(CliSemanticStatus.Blocked, report.Status);
            Assert.Equal(CliSubjectKind.File, finding.Subject.Kind);
            Assert.Equal(RecoveryCandidatePath, finding.Subject.Path);
            Assert.False(string.IsNullOrWhiteSpace(finding.Subject.Path));
            Assert.Equal(RecoveryConflictMessage, finding.Message);
            Assert.DoesNotContain(RouteMoveTestData.SourcePath, finding.Message, StringComparison.Ordinal);
            Assert.DoesNotContain(RouteMoveTestData.DestinationPath, finding.Message, StringComparison.Ordinal);
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
            Assert.Equal(RecoveryConflictMessage, jsonFinding.GetProperty("message").GetString());
            Assert.Equal("file", jsonFinding.GetProperty("subject").GetProperty("kind").GetString());
            var jsonSubjectPath = jsonFinding.GetProperty("subject").GetProperty("path").GetString();
            Assert.False(string.IsNullOrWhiteSpace(jsonSubjectPath));
            Assert.Equal(RecoveryCandidatePath, jsonSubjectPath);
            var action = Assert.Single(jsonFinding.GetProperty("actions").EnumerateArray());
            Assert.Equal("command", action.GetProperty("kind").GetString());
            Assert.Equal(CleanupPreviewCommand, action.GetProperty("command").GetString());
            Assert.Equal(CleanupPreviewReason, action.GetProperty("reason").GetString());
            Assert.Equal(CleanupPreviewCommand, root.GetProperty("next").GetProperty("command").GetString());
            Assert.Equal(CleanupPreviewReason, root.GetProperty("next").GetProperty("reason").GetString());
            var data = root.GetProperty("data");
            Assert.Equal(RouteMoveTestData.SourcePath, data.GetProperty("source").GetProperty("path").GetString());
            Assert.Equal(RouteMoveTestData.DestinationPath, data.GetProperty("destination").GetProperty("path").GetString());

            var detailName = detail.ToString().ToLowerInvariant();
            captures.Add($"recovery-conflict.{detailName}", text.Replace("\r\n", "\n", StringComparison.Ordinal));
            captures.Add($"recovery-conflict{CommandOutputSnapshot.JsonContentNameSegment}{detailName}", jsonText.Replace("\r\n", "\n", StringComparison.Ordinal));
        }

        CommandOutputSnapshot.MatchDetailSnapshot(captures);
    }

    [Theory(DisplayName = "Route Move requires an observed recovery candidate before it can form a conflict finding"), Trait("Boundary", "Output")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RecoveryConflictRequiresAnObservedTarget(string? target)
    {
        CliRenderedOutput? rendered = null;
        var exception = Assert.ThrowsAny<ArgumentException>(
            () => rendered = Render(
                Result(RecoveryConflict(CliSemanticStatus.Blocked, target)),
                CliFormat.Json,
                CliDetail.Minimal));

        Assert.Equal("target", exception.ParamName);
        Assert.Null(rendered);
    }

    [Fact(DisplayName = "Route Move keeps optional targets for non-conflict findings"), Trait("Boundary", "Output")]
    public void NonConflictFindingStillAllowsMissingTarget()
    {
        var finding = new RouteMoveFinding(
            RouteMoveFindingCode.OperationFailed,
            CliSemanticStatus.Failed,
            null,
            "The operation failed.");

        Assert.Null(finding.Target);
    }

    [Fact(DisplayName = "Route Move keeps failure guidance ahead of a recovery-conflict preview"), Trait("Boundary", "Output")]
    public void HigherSeverityFailureStillSelectsItsNextAction()
    {
        var result = Result(
            RecoveryConflict(CliSemanticStatus.Blocked, RecoveryCandidatePath),
            RecoveryConflict(CliSemanticStatus.Failed, RouteMoveTestData.SourcePath, RouteMoveFindingCode.OperationFailed));
        var report = RouteMoveReportSelector.Select(result, new CliSelection(CliDetail.Minimal, null));

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(CliSemanticStatus.Failed, report.Status);
        Assert.Equal("open-forge route move --detail debug", report.Next?.Command);
    }

    [Fact(DisplayName = "Route Move keeps its existing recovery-retention cleanup guidance"), Trait("Boundary", "Output")]
    public void RecoveryArtifactRetainedKeepsItsExistingCleanupAction()
    {
        var finding = new RouteMoveFinding(
            RouteMoveFindingCode.RecoveryArtifactRetained,
            CliSemanticStatus.Attention,
            RecoveryCandidatePath,
            "The verified operation retained its recovery artifact.");
        var formation = RouteMoveTestData.Formation(RouteMoveMode.Apply, finding) with
        {
            Workspace = null,
            Recovery = new RouteMoveRecovery
            {
                State = RouteMoveRecoveryState.Retained,
                ResidualPath = RecoveryCandidatePath,
            },
        };
        var result = new RouteMoveResultBuilder().Build(formation);
        var report = RouteMoveReportSelector.Select(result, new CliSelection(CliDetail.Minimal, null));
        var reportFinding = Assert.Single(report.Findings);

        Assert.Equal(CliSemanticStatus.Attention, report.Status);
        Assert.Empty(reportFinding.Actions);
        Assert.Equal("open-forge cleanup", report.Next?.Command);
        Assert.Equal(RetainedCleanupReason, report.Next?.Reason);
    }

    private static RouteMoveFinding RecoveryConflict(
        CliSemanticStatus status,
        string? target,
        RouteMoveFindingCode code = RouteMoveFindingCode.RecoveryConflict)
        => new(code, status, target, "An existing recovery candidate blocks the requested route change.");

    private static RouteMoveResult Result(params RouteMoveFinding[] findings)
    {
        var formation = RouteMoveTestData.Formation(RouteMoveMode.Apply, findings) with
        {
            Workspace = null,
            Findings = ImmutableArray.CreateRange(findings),
        };
        return new RouteMoveResultBuilder().Build(formation);
    }

    private static CliRenderedOutput Render(RouteMoveResult result, CliFormat format, CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<RouteMoveResult>(result, new CliPresentation(format, detail, null)),
            RouteMovePresentation.Rendering);
}
