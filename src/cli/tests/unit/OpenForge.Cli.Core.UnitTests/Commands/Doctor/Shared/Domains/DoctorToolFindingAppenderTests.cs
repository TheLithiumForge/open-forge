using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;
using OpenForge.Cli.Core.Presentation.Doctor;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Shared.Tools;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Domains;

[Trait("Feature", "tool-run"), Trait("Evidence", "Unit")]
public sealed class DoctorToolFindingAppenderTests
{
    [Theory(DisplayName = "Doctor renders the shared missing-tool fact, keeps coverage and counts its exact Install action")]
    [InlineData(false)]
    [InlineData(true)]
    public void ProjectsMissingTool(bool incomplete)
    {
        var seed = new ToolMissingReportSeed();
        var coverage = incomplete ? DoctorCoverageState.Incomplete : DoctorCoverageState.Complete;
        var counts = DoctorFindingAggregation.Count([]);
        var original = new DoctorResult
        {
            Status = CliSemanticStatus.Complete,
            Workspace = seed.Workspace,
            Next = null,
            Diagnosis = new()
            {
                Coverage = coverage,
                Counts = counts,
                Actions = [],
                Domains = [new()
                {
                    Domain = DoctorDomainKind.WorkspaceEntry, Boundary = new() { Kind = DoctorBoundaryKind.Workspace, Path = seed.Workspace.LexicalRoot },
                    Coverage = coverage, Lifecycle = null, SourceAvailability = null, Limitations = [], Counts = counts, Findings = [], Actions = [],
                }],
            },
        };
        var result = DoctorToolFindingAppender.Append(original, [seed.Failure]);
        Assert.Equal(incomplete ? CliSemanticStatus.Incomplete : CliSemanticStatus.Attention, result.Status);
        Assert.Equal(coverage, result.Diagnosis.Coverage);
        Assert.Equal(1, result.Diagnosis.Counts.Severity.Warning.Value);
        Assert.Equal(ToolContractCodes.RestoreCommand, Assert.Single(result.Diagnosis.Actions).Command);
        var report = DoctorPresentation.Rendering.Selector(result, new CliSelection(CliDetail.Full));
        var finding = Assert.Single(report.Findings);
        Assert.Equal(ToolContractCodes.DoctorDeclaredToolMissing, finding.Code);
        Assert.Equal(seed.Id, finding.Subject.Id);
        Assert.Null(finding.Subject.Path);
        var action = Assert.Single(finding.Actions);
        Assert.Equal(ToolContractCodes.RestoreCommand, action.Command);
        CommandOutputSnapshot.MatchSnapshot(string.Join('\n', finding.Title, finding.Message, action.Command, action.Reason), "missing-tool");
    }
}
