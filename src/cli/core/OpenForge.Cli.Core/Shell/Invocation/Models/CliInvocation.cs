using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Invocation.Models;

internal sealed record CliInvocation(
    CliProcessIdentity Process,
    CliPresentation Presentation,
    CliTerminalMode TerminalMode,
    CliWorkspaceRequest WorkspaceRequest,
    CliWorkspace? Workspace)
{
    internal CliView? SuppliedView { get; init; }
}
