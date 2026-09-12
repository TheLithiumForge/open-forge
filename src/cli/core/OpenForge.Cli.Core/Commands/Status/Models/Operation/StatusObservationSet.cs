using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Status.Models.Operation;

internal sealed record StatusObservationSet(
    WorkspaceEntryStatusView WorkspaceEntry,
    RecoveryResidualStatusView RecoveryResiduals,
    RouteStatusView Routes,
    FrameworkLifecycleStatusView FrameworkLifecycle,
    ExtensionLifecycleStatusView ExtensionLifecycle,
    LibraryStatusView Libraries);
