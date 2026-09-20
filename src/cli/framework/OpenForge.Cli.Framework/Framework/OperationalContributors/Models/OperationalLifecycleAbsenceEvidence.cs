using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Distribution.Operational;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.Framework.OperationalContributors.Models;

internal sealed class OperationalLifecycleAbsenceEvidence
{
    private OperationalLifecycleAbsenceEvidence()
    {
    }

    internal OperationalViewState WorkspaceState { get; private init; }

    internal OperationalInstallationState Installation { get; private init; }

    internal OperationalViewState RouteState { get; private init; }

    internal RouteSourceInventoryState SourceInventory { get; private init; }

    internal OperationalViewState RecoveryState { get; private init; }

    internal int RecoveryCandidateCount { get; private init; }

    internal OperationalLifecyclePresenceState FrameworkPresence { get; private init; }

    internal OperationalLifecyclePresenceState ExtensionPresence { get; private init; }

    internal static OperationalLifecycleAbsenceEvidence FromStatus(
        WorkspaceEntryStatusView workspace,
        RouteStatusView routes,
        RecoveryResidualStatusView recovery,
        FrameworkLifecycleStatusView framework,
        ExtensionLifecycleStatusView extensions)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(routes);
        ArgumentNullException.ThrowIfNull(recovery);
        ArgumentNullException.ThrowIfNull(framework);
        ArgumentNullException.ThrowIfNull(extensions);
        var evidence = new OperationalLifecycleAbsenceEvidence
        {
            WorkspaceState = workspace.State,
            Installation = workspace.Installation,
            RouteState = routes.State,
            SourceInventory = routes.SourceInventory,
            RecoveryState = recovery.State,
            RecoveryCandidateCount = recovery.Candidates.Count,
            FrameworkPresence = framework.Presence,
            ExtensionPresence = extensions.Presence,
        };
        evidence.Validate();
        return evidence;
    }

    internal static OperationalLifecycleAbsenceEvidence FromDoctor(
        WorkspaceEntryDoctorView workspace,
        RouteDoctorView routes,
        RecoveryResidualDoctorView recovery,
        FrameworkLifecycleDoctorView framework,
        ExtensionLifecycleDoctorView extensions)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(routes);
        ArgumentNullException.ThrowIfNull(recovery);
        ArgumentNullException.ThrowIfNull(framework);
        ArgumentNullException.ThrowIfNull(extensions);
        var evidence = new OperationalLifecycleAbsenceEvidence
        {
            WorkspaceState = workspace.State,
            Installation = workspace.Installation,
            RouteState = routes.State,
            SourceInventory = routes.SourceInventory,
            RecoveryState = recovery.State,
            RecoveryCandidateCount = recovery.Candidates.Count,
            FrameworkPresence = ReadPresence(framework.Ownership.State),
            ExtensionPresence = ReadPresence(extensions.Ownership.State),
        };
        evidence.Validate();
        return evidence;
    }

    private static OperationalLifecyclePresenceState ReadPresence(WorkspaceOwnershipReadState state)
        => state == WorkspaceOwnershipReadState.Absent ? OperationalLifecyclePresenceState.Missing
            : state == WorkspaceOwnershipReadState.Complete ? OperationalLifecyclePresenceState.Present
            : OperationalLifecyclePresenceState.Unavailable;

    private void Validate()
    {
        ValidateEnum(WorkspaceState, nameof(WorkspaceState));
        ValidateEnum(Installation, nameof(Installation));
        ValidateEnum(RouteState, nameof(RouteState));
        ValidateEnum(SourceInventory, nameof(SourceInventory));
        ValidateEnum(RecoveryState, nameof(RecoveryState));
        ValidateEnum(FrameworkPresence, nameof(FrameworkPresence));
        ValidateEnum(ExtensionPresence, nameof(ExtensionPresence));
        if (RecoveryCandidateCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RecoveryCandidateCount),
                RecoveryCandidateCount,
                "The recovery candidate count cannot be negative.");
        }
    }

    private static void ValidateEnum<T>(T value, string name)
        where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(name, value, $"The {typeof(T).Name} value is not defined.");
        }
    }
}
