using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Install.Models.Planning;

internal sealed record InstallLayoutSelection(
    WorkspaceLayoutRead Observation,
    WorkspaceLayout? IntendedLayout,
    InstallPlanningBoundary? Boundary);

internal sealed record InstallPlanningInput(
    InstallRequest Request,
    FrameworkPayload Payload,
    WorkspaceLayoutRead LayoutRead,
    WorkspaceLayout IntendedLayout);
