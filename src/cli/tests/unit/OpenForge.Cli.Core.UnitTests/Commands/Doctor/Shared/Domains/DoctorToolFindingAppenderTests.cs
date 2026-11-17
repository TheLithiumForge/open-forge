using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Observation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Domains;

public sealed class DoctorToolFindingAppenderTests
{
    [Theory(DisplayName = "Doctor reports Tool observations with targeted operations and no repair proposal"),
     InlineData((int)ToolObservationState.Missing, (int)DoctorFindingKind.ToolDeclaredMissing), InlineData((int)ToolObservationState.Changed, (int)DoctorFindingKind.ToolDrift),
     InlineData((int)ToolObservationState.Unsupported, (int)DoctorFindingKind.ToolUnsupported), InlineData((int)ToolObservationState.Unavailable, (int)DoctorFindingKind.ToolObservationUnavailable),
     Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
    public void DiagnosisIsReadOnly(int state, int code)
    {
        var result = DoctorToolFindingAppender.Append(Seed(), [ToolObservationSeeds.Create((ToolObservationState)state)]);
        var finding = Assert.Single(Assert.Single(result.Diagnosis.Domains).Findings);
        Assert.Equal((DoctorFindingKind)code, finding.Kind);
        Assert.Equal(DoctorSubjectKind.Tool, finding.Subject.Kind);
        Assert.Equal(DoctorResolutionLane.TargetedOperation, finding.Resolution);
        Assert.Null(finding.Proposal);
        Assert.Equal(DoctorNextActionKind.AcceptedOperation, Assert.Single(finding.Actions).Kind);
        Assert.Null(Assert.Single(finding.Actions).Operation);
        Assert.True(DoctorDiagnosis.ReadOnly);
        Assert.False(DoctorDiagnosis.ChangesMade);
    }

    private static DoctorResult Seed()
    {
        var counts = DoctorFindingAggregation.Count([]);
        return new()
        {
            Status = CliSemanticStatus.Complete,
            Workspace = null,
            Next = null,
            Diagnosis = new()
            {
                Coverage = DoctorCoverageState.Complete,
                Counts = counts,
                Actions = [],
                Domains = [new() { Domain = DoctorDomainKind.WorkspaceEntry, Boundary = new() { Kind = DoctorBoundaryKind.Workspace, Path = null },
                    Coverage = DoctorCoverageState.Complete, Lifecycle = null, SourceAvailability = null, Limitations = [], Counts = counts, Findings = [], Actions = [] }],
            },
        };
    }
}
