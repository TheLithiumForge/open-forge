using OpenForge.Cli.Core.Commands.Status.Models.Operation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusLifecycleAbsenceResolver
{
    internal static bool IsProven(StatusObservationSet observations)
        => observations.WorkspaceEntry.State == OperationalViewState.Complete
            && observations.WorkspaceEntry.Installation == OperationalInstallationState.Uninstalled
            && observations.Routes.State == OperationalViewState.Complete
            && observations.Routes.SourceInventory == RouteSourceInventoryState.SafelyAbsent
            && observations.RecoveryResiduals.State == OperationalViewState.Complete
            && observations.RecoveryResiduals.Candidates.Count == 0
            && observations.FrameworkLifecycle.Presence == OperationalLifecyclePresenceState.Missing
            && observations.ExtensionLifecycle.Presence == OperationalLifecyclePresenceState.Missing;
}
