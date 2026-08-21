using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Composition;

internal static class CliCompositionRoot
{
    internal static CliCoreApplication Create(CliProcessIdentity process)
    {
        ArgumentNullException.ThrowIfNull(process);
        var rootHelp = new CliHelpContent(
        [
            new CliHelpSection(
                "Product",
                $"  Open Forge CLI (`{CliSyntaxDefinitions.ExecutableName}`)."),
            new CliHelpSection(
                "Discovery",
                "  route list  List routed sources and descendants at a structural depth."),
        ]);
        var symbols = RouteListBinding.CreateSymbols();
        var binding = RouteListBinding.Close(
            symbols,
            RouteListHelpSections.CreateList(),
            RouteListBinding.CreateBinder(symbols),
            RouteListBinding.CreateInvalidResultFactory(),
            RouteListOperationFactory.Create(),
            new CliRendererSet<RouteListResult>(
                RouteListHumanRenderer.Render,
                RouteListJsonRenderer.Render),
            RouteListDiagnosticRenderer.Render);
        var tree = CliCommandTree.Create(
            rootHelp,
            [new CliRootBranch(
                symbols.RouteGroup,
                RouteListHelpSections.CreateRouteGroup(),
                symbols.DelimiterPolicies)],
            [binding]);
        var workspaceSelector = new CliWorkspaceSelector(new PhysicalPathResolver());
        return new CliCoreApplication(process, tree, workspaceSelector);
    }
}
