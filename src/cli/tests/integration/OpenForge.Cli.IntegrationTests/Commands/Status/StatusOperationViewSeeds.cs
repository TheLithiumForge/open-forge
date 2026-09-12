using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal static class StatusOperationViewSeeds
{
    internal static WorkspaceEntryStatusView WorkspaceEntry()
        => new(
            OperationalViewState.Complete,
            OperationalInstallationState.Installed,
            "AGENTS.md",
            ".agents/loader.md");

    internal static RecoveryResidualStatusView RecoveryResiduals()
        => new(OperationalViewState.Complete, []);

    internal static RouteStatusView Routes()
    {
        var zero = Measurement();
        return new RouteStatusView
        {
            State = OperationalViewState.Complete,
            SourceInventory = RouteSourceInventoryState.Present,
            InitialStartup = zero,
            CurrentStartup = zero,
            TotalAvailable = zero,
            Continuity = zero,
            ContinuitySources = [],
            InitialRootCategories = [],
            CurrentRootCategories = [],
            GeneratedNavigation = [],
        };
    }

    internal static FrameworkLifecycleStatusView FrameworkLifecycle()
        => new()
        {
            State = OperationalViewState.Complete,
            Presence = OperationalLifecyclePresenceState.Present,
            Lifecycle = OperationalLifecycleState.Trusted,
            SourceAvailability = OperationalSourceAvailability.Available,
            Targets = [],
        };

    internal static ExtensionLifecycleStatusView ExtensionLifecycle()
        => new()
        {
            State = OperationalViewState.Complete,
            Presence = OperationalLifecyclePresenceState.Present,
            Lifecycle = OperationalLifecycleState.Trusted,
            SourceAvailability = OperationalSourceAvailability.Available,
            Installed = [],
            Targets = [],
        };

    private static ContextMeasurementObservation Measurement()
    {
        var zero = new OperationalIntegerObservation(OperationalValueState.Available, 0);
        return new ContextMeasurementObservation(zero, zero, zero, zero);
    }
}
