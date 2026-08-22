using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
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
                "  route list     List routed sources and descendants at a structural depth.\n"
                + "  route inspect  Explain one source's route behavior without returning authored content."),
        ]);
        var routeGroup = RouteBinding.CreateGroup();
        var listSymbols = RouteListBinding.CreateSymbols(routeGroup);
        var inspectSymbols = RouteInspectBinding.CreateSymbols(routeGroup);
        var listBinding = RouteListBinding.Close(
            listSymbols,
            new RouteListBindingComponents
            {
                Help = RouteListHelpSections.CreateList(),
                Operation = RouteListOperationFactory.Create(),
                Renderers = new CliRendererSet<RouteListResult>(
                    RouteListHumanRenderer.Render,
                    RouteListJsonRenderer.Render),
                DiagnosticRenderer = RouteListDiagnosticRenderer.Render,
            });
        var inspectBinding = RouteInspectBinding.Close(
            inspectSymbols,
            new RouteInspectBindingComponents
            {
                Help = RouteInspectHelpSections.CreateInspect(),
                Operation = RouteInspectOperationFactory.Create(),
                Renderers = new CliRendererSet<RouteInspectResult>(
                    RouteInspectHumanRenderer.Render,
                    RouteInspectJsonRenderer.Render),
                DiagnosticRenderer = RouteInspectDiagnosticRenderer.Render,
            });
        var tree = CliCommandTree.Create(
            rootHelp,
            [new CliRootBranch(
                routeGroup,
                RouteHelpSections.CreateGroup(),
                listSymbols.DelimiterPolicies)],
            [listBinding, inspectBinding]);
        var workspaceSelector = new CliWorkspaceSelector(new PhysicalPathResolver());
        return new CliCoreApplication(process, tree, workspaceSelector);
    }
}
