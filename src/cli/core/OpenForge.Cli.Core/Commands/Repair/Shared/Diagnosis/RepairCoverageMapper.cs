using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairCoverageMapper
{
    internal static RepairDiagnosisCoverage Read(DoctorObservation observation)
    {
        ArgumentNullException.ThrowIfNull(observation);
        var workspace = Read(observation.WorkspaceEntry.State);
        var routes = Read(observation.Routes.State);
        var references = Read(observation.LocalReferences.State);
        return new RepairDiagnosisCoverage(
            workspace,
            routes,
            references,
            ReadSelected(workspace, routes, references));
    }

    internal static RepairPostDiagnosisState ReadPostDiagnosisState(RepairDiagnosisCoverage coverage)
        => coverage.SelectedScope switch
        {
            RepairCoverageState.Complete => RepairPostDiagnosisState.Complete,
            RepairCoverageState.Incomplete or RepairCoverageState.NotRequested
                => RepairPostDiagnosisState.Incomplete,
            RepairCoverageState.Blocked => RepairPostDiagnosisState.Blocked,
            _ => throw new ArgumentOutOfRangeException(
                nameof(coverage),
                coverage.SelectedScope,
                "The Repair coverage state is not defined."),
        };

    private static RepairCoverageState Read(OperationalViewState state)
        => state switch
        {
            OperationalViewState.Complete => RepairCoverageState.Complete,
            OperationalViewState.Incomplete or OperationalViewState.Interrupted
                => RepairCoverageState.Incomplete,
            OperationalViewState.Blocked => RepairCoverageState.Blocked,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The view state is not defined."),
        };

    private static RepairCoverageState ReadSelected(params RepairCoverageState[] states)
    {
        if (states.Contains(RepairCoverageState.Blocked))
        {
            return RepairCoverageState.Blocked;
        }

        return states.Contains(RepairCoverageState.Incomplete)
            ? RepairCoverageState.Incomplete
            : RepairCoverageState.Complete;
    }
}
