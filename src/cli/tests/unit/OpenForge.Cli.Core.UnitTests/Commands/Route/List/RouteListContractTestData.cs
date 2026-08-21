using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List;

internal static class RouteListContractTestData
{
    internal static CliWorkspace Workspace()
    {
        return new CliWorkspace(
            Path.GetTempPath(),
            Path.GetTempPath(),
            CliWorkspaceSelectionMethod.CurrentDirectory);
    }
}
