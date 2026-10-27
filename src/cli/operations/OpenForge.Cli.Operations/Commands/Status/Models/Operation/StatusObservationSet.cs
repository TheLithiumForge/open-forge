using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Status.Models.Operation;

internal sealed record StatusObservationSet(
    WorkspaceLayout? Layout,
    WorkspaceEntryStatusView WorkspaceEntry,
    RecoveryResidualStatusView RecoveryResiduals,
    RouteStatusView Routes,
    FrameworkLifecycleStatusView FrameworkLifecycle,
    ExtensionLifecycleStatusView ExtensionLifecycle,
    LibraryStatusView Libraries)
{
    internal WorkspaceLayoutRead? LayoutRead { get; init; }

    internal RouteOutsideRoutesObservation? OutsideRoutes { get; init; }
}
