using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;
using OpenForge.Cli.Core.Presentation.Status;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Shared.Tools;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status.Shared.Aggregation;

[Trait("Feature", "tool-run"), Trait("Evidence", "Unit")]
public sealed class StatusToolFindingAppenderTests
{
    [Fact(DisplayName = "Workspace Status renders the shared missing-tool code, identifier and exact team Install action")]
    public void ProjectsMissingTool()
    {
        var seed = new ToolMissingReportSeed();
        var original = StatusResultBuilder.Event(seed.Workspace, StatusFindingCode.ToolBackupPresent, null, "fixture");
        var result = StatusToolFindingAppender.Append(original, [seed.Failure]);
        var report = StatusPresentation.Rendering.Selector(result, new CliSelection(CliDetail.Full));
        var finding = Assert.Single(report.Findings, finding => finding.Code == ToolContractCodes.StatusDeclaredToolMissing);
        Assert.Equal(seed.Id, finding.Subject.Id);
        Assert.Null(finding.Subject.Path);
        Assert.Equal(ToolContractCodes.RestoreCommand, Assert.Single(finding.Actions).Command);
        Assert.Equal(ToolContractCodes.RestoreCommand, result.Next?.Command);
        CommandOutputSnapshot.MatchSnapshot(string.Join('\n', finding.Title, finding.Message, finding.Actions[0].Command, finding.Actions[0].Reason), "missing-tool");
    }

    [Fact(DisplayName = "A missing tool keeps a higher-priority blocked Status result and its next action")]
    public void KeepsBlockedResult()
    {
        var seed = new ToolMissingReportSeed();
        var original = StatusResultBuilder.Event(seed.Workspace, StatusFindingCode.WorkspaceUnsafe, null, "fixture");
        var result = StatusToolFindingAppender.Append(original, [seed.Failure]);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Same(original.Next, result.Next);
    }
}
