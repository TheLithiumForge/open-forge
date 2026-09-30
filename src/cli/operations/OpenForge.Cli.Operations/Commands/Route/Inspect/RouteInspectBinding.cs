using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;

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
        var workingPaths = RouteInspectSymbols.CreateWorkingPaths();
        var matchingFiles = new Option<bool>(RouteInspectDefinitions.MatchingFiles.Name)
        {
            Description = RouteInspectDefinitions.MatchingFiles.Description,
        };
        inspect.Options.Add(matchingFiles);
        inspect.Arguments.Add(sourceReferences);
        inspect.Options.Add(workingPaths);
        routeGroup.Subcommands.Add(inspect);
        return new RouteInspectSymbols(routeGroup, inspect, sourceReferences, workingPaths, matchingFiles);
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
        var matchingFiles = parseResult.GetValue(symbols.MatchingFiles);
        if (sourceReferences.Length != 1)
        {
            return CliBindResult<RouteInspectRequest, RouteInspectResult>.Invalid(
                RouteInspectBindingInputPolicy.CreateSourceCardinalityInvalidResult(
                    invocation,
                    sourceReferences,
                    matchingFiles));
        }

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("The route-inspect binding requires a selected workspace.");
        var workingPaths = parseResult.GetValue(symbols.WorkingPaths);
        if (workingPaths is { Length: 0 })
        {
            workingPaths = null;
        }

        if (workingPaths is { Length: > 0 })
        {
            var normalized = SourceWorkingPathNormalizer.Normalize(workspace.LexicalRoot, workingPaths);
            if (normalized.InvalidPaths.Count > 0)
            {
                return CliBindResult<RouteInspectRequest, RouteInspectResult>.Invalid(
                    RouteInspectBindingInputPolicy.CreateWorkingPathsInvalidResult(
                        workspace,
                        sourceReferences[0],
                        matchingFiles));
            }

            workingPaths = normalized.Paths.ToArray();
        }

        return CliBindResult<RouteInspectRequest, RouteInspectResult>.Bound(
            new RouteInspectRequest(
                workspace,
                sourceReferences[0],
                allowInteractiveSourceSelection: invocation.Presentation.Format == CliFormat.Text,
                workingPaths: workingPaths,
                matchingFiles: matchingFiles));
    }

    internal static CliRequestBinding<RouteInspectRequest, RouteInspectResult> CreateRequestBinding(
        RouteInspectSymbols symbols,
        RouteInspectBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        return new CliRequestBinding<RouteInspectRequest, RouteInspectResult>
        {
            Command = symbols.InspectCommand,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = CreateBinder(symbols),
            InvalidResultFactory = CreateInvalidResultFactory(symbols),
            Operation = components.Operation.Invoke,
        };
    }
}
