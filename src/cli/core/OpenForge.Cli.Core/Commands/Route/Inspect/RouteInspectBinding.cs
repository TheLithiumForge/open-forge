using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;

namespace OpenForge.Cli.Core.Commands.Route.Inspect;

internal static class RouteInspectBinding
{
    internal static RouteInspectSymbols CreateSymbols(Command routeGroup)
    {
        ArgumentNullException.ThrowIfNull(routeGroup);
        var sourceReferences = new Argument<string[]>(RouteInspectDefinitions.SourceReference.Name)
        {
            Description = RouteInspectDefinitions.SourceReference.Description,
            Arity = ArgumentArity.ZeroOrMore,
        };
        var inspect = new Command(
            RouteInspectDefinitions.InspectCommand.Name,
            RouteInspectDefinitions.InspectCommand.Description);
        inspect.Arguments.Add(sourceReferences);
        routeGroup.Subcommands.Add(inspect);
        return new RouteInspectSymbols(routeGroup, inspect, sourceReferences);
    }

    internal static CliRequestBinder<RouteInspectRequest, RouteInspectResult> CreateBinder(
        RouteInspectSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        return (parse, invocation) => Bind(parse.Result, invocation, symbols);
    }

    internal static CliContextualInvalidResultFactory<RouteInspectResult> CreateInvalidResultFactory(
        RouteInspectSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        return input => RouteInspectBindingInputPolicy.CreateWorkspaceInvalidResult(input, symbols);
    }

    internal static CliBindResult<RouteInspectRequest, RouteInspectResult> Bind(
        ParseResult parseResult,
        CliInvocation invocation,
        RouteInspectSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(invocation);
        ArgumentNullException.ThrowIfNull(symbols);
        var sourceReferences = parseResult.GetValue(symbols.SourceReferences) ?? [];
        if (sourceReferences.Length != 1)
        {
            return CliBindResult<RouteInspectRequest, RouteInspectResult>.Invalid(
                RouteInspectBindingInputPolicy.CreateSourceCardinalityInvalidResult(
                    invocation,
                    sourceReferences));
        }

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("The route-inspect binding requires a selected workspace.");
        return CliBindResult<RouteInspectRequest, RouteInspectResult>.Bound(
            new RouteInspectRequest(
                workspace,
                sourceReferences[0],
                allowInteractiveSourceSelection: invocation.Presentation.Format == CliOutputFormat.Human));
    }

    internal static CliCommandBinding<RouteInspectRequest, RouteInspectResult> Close(
        RouteInspectSymbols symbols,
        RouteInspectBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        return new CliCommandBinding<RouteInspectRequest, RouteInspectResult>(
            symbols.InspectCommand,
            new CliCommandBindingComponents<RouteInspectRequest, RouteInspectResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = CreateBinder(symbols),
                InvalidResultFactory = CreateInvalidResultFactory(symbols),
                Operation = components.Operation.Invoke,
                Renderers = components.Renderers,
                DiagnosticRenderer = components.DiagnosticRenderer,
            });
    }
}
