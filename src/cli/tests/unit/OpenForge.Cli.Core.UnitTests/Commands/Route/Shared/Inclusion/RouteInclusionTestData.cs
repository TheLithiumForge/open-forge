using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Inclusion;

internal static class RouteInclusionTestData
{
    internal static CliInvocation Invocation()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-route-inclusion-unit"));
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        return new CliInvocation(new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(CliFormat.Json, CliDetail.Standard, null), CliTerminalMode.None,
            new CliWorkspaceRequest(root, root), workspace);
    }
}
