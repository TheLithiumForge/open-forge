using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.Framework.OperationalContributors;

internal static class OperationalLifecycleAbsenceProof
{
    internal static bool IsProven(OperationalLifecycleAbsenceEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        return evidence.WorkspaceState == OperationalViewState.Complete
            && evidence.Installation == OperationalInstallationState.Uninstalled
            && evidence.RouteState == OperationalViewState.Complete
            && evidence.SourceInventory == RouteSourceInventoryState.SafelyAbsent
            && evidence.RecoveryState == OperationalViewState.Complete
            && evidence.RecoveryCandidateCount == 0
            && evidence.FrameworkPresence == OperationalLifecyclePresenceState.Missing
            && evidence.ExtensionPresence == OperationalLifecyclePresenceState.Missing;
    }
}
