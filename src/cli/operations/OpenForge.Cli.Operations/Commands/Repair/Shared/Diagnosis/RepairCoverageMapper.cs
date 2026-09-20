using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairCoverageMapper
{
    internal static RepairDiagnosisCoverage Read(DoctorDiagnosisRead diagnosis)
    {
        ArgumentNullException.ThrowIfNull(diagnosis);
        var coverage = Read(diagnosis.Observation);
        var workspaceAndPath = coverage.WorkspaceAndPath;
        var routeAndHeading = coverage.RouteAndHeading;
        var localReferences = coverage.LocalReferences;
        if (diagnosis.Result.Status == CliSemanticStatus.Blocked)
        {
            workspaceAndPath = RepairCoverageState.Blocked;
        }

        foreach (var finding in diagnosis.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings))
        {
            if (finding.Resolution != DoctorResolutionLane.BlockedRepair
                || ReadRepairDomain(finding.Kind) is not { } domain)
            {
                continue;
            }

            switch (domain)
            {
                case RepairDependencyDomain.WorkspaceContainment:
                    workspaceAndPath = RepairCoverageState.Blocked;
                    break;
                case RepairDependencyDomain.RouteAndHeading:
                    routeAndHeading = RepairCoverageState.Blocked;
                    break;
                case RepairDependencyDomain.LocalReference:
                    localReferences = RepairCoverageState.Blocked;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(domain), domain, "The Repair dependency domain is not defined.");
            }
        }

        return new RepairDiagnosisCoverage(
            workspaceAndPath,
            routeAndHeading,
            localReferences,
            ReadSelected(workspaceAndPath, routeAndHeading, localReferences));
    }

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

    private static RepairDependencyDomain? ReadRepairDomain(DoctorFindingKind kind)
        => kind switch
        {
            DoctorFindingKind.WorkspaceUnavailable
                or DoctorFindingKind.WorkspaceNotDirectory
                or DoctorFindingKind.WorkspaceAgentsMissing
                or DoctorFindingKind.WorkspaceAgentsInaccessible
                or DoctorFindingKind.WorkspaceLoaderMissing
                or DoctorFindingKind.WorkspaceLoaderUnreadable
                or DoctorFindingKind.WorkspaceLoaderMalformed
                or DoctorFindingKind.WorkspaceEntryMissing
                or DoctorFindingKind.WorkspaceEntryAmbiguous
                or DoctorFindingKind.WorkspaceEntryCompatibilityCollision
                or DoctorFindingKind.WorkspaceSourceIdCollision
                or DoctorFindingKind.WorkspacePathInvalid
                or DoctorFindingKind.WorkspacePathContainment
                or DoctorFindingKind.WorkspacePhysicalAlias
                or DoctorFindingKind.WorkspaceFrontmatterMalformed
                or DoctorFindingKind.WorkspaceFrontmatterDuplicate
                or DoctorFindingKind.WorkspaceParseIncomplete
                or DoctorFindingKind.WorkspaceUnsupportedSource
                or DoctorFindingKind.WorkspaceRootMissing
                or DoctorFindingKind.WorkspaceRootUnreachable
                or DoctorFindingKind.WorkspaceDetached
                => RepairDependencyDomain.WorkspaceContainment,
            DoctorFindingKind.RouteEntrypointMissing
                or DoctorFindingKind.RouteEntrypointDuplicate
                or DoctorFindingKind.RouteEscape
                or DoctorFindingKind.RouteUnreachable
                or DoctorFindingKind.RouteDetached
                or DoctorFindingKind.RouteMetadataRequiredMissing
                or DoctorFindingKind.RouteTitleInvalid
                or DoctorFindingKind.RouteAxiomsInvalid
                or DoctorFindingKind.RouteGeneratedRegionStale
                or DoctorFindingKind.RouteGeneratedRegionMissing
                or DoctorFindingKind.RouteGeneratedRegionMalformed
                or DoctorFindingKind.RouteGeneratedRegionMisplaced
                or DoctorFindingKind.RouteGeneratedRegionDuplicate
                or DoctorFindingKind.RouteGeneratedEntryMissing
                or DoctorFindingKind.RouteGeneratedEntryExtra
                or DoctorFindingKind.RouteGeneratedEntryOrder
                or DoctorFindingKind.RouteGeneratedEntryPath
                or DoctorFindingKind.RouteGeneratedEntryDescription
                or DoctorFindingKind.RouteGeneratedEntryTags
                or DoctorFindingKind.RouteOverwriteOrphan
                or DoctorFindingKind.RouteOverwriteIndependentIndex
                or DoctorFindingKind.RouteCompatibilityConflict
                => RepairDependencyDomain.RouteAndHeading,
            DoctorFindingKind.ReferenceTargetMissing
                or DoctorFindingKind.ReferenceFragmentMissing
                or DoctorFindingKind.ReferenceFragmentUnverified
                or DoctorFindingKind.ReferenceDestinationMalformed
                or DoctorFindingKind.ReferenceDestinationAbsolute
                or DoctorFindingKind.ReferenceDestinationQuery
                or DoctorFindingKind.ReferenceDestinationEncoding
                or DoctorFindingKind.ReferenceTargetOutsideWorkspace
                or DoctorFindingKind.ReferenceTargetPhysicalEscape
                or DoctorFindingKind.ReferenceTargetAlias
                or DoctorFindingKind.ReferenceTargetUnreadable
                or DoctorFindingKind.ReferenceTargetUnsupported
                or DoctorFindingKind.ReferenceSameTargetPath
                or DoctorFindingKind.ReferenceSameTargetCase
                or DoctorFindingKind.ReferenceSameTargetEncoding
                or DoctorFindingKind.ReferenceSameTargetFragment
                => RepairDependencyDomain.LocalReference,
            // Library, Framework, Extension, and Recovery findings belong to
            // other command-owned diagnosis domains, not ordinary Repair.
            _ => null,
        };

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
