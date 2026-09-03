using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;

namespace OpenForge.Cli.Core.Commands.Route.Move;

internal sealed class RouteMoveBinding(
    RouteMoveBindingValidator validator,
    RouteMoveInvalidResultFactory invalidResultFactory)
{
    private readonly RouteMoveBindingValidator _validator = validator;
    private readonly RouteMoveInvalidResultFactory _invalidResultFactory = invalidResultFactory;

    internal RouteMoveSymbols CreateSymbols(Command routeGroup)
    {
        ArgumentNullException.ThrowIfNull(routeGroup);
        var sourceReference = new Argument<string?>(RouteMoveDefinitions.SourceReference.Name)
        {
            Description = RouteMoveDefinitions.SourceReference.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var destinationTarget = new Argument<string?>(RouteMoveDefinitions.DestinationTarget.Name)
        {
            Description = RouteMoveDefinitions.DestinationTarget.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var dryRun = new Option<bool>(RouteMoveDefinitions.DryRun.Name)
        {
            Description = RouteMoveDefinitions.DryRun.Description,
            Arity = ArgumentArity.Zero,
        };
        var command = new Command(
            RouteMoveDefinitions.MoveCommand.Name,
            RouteMoveDefinitions.MoveCommand.Description);
        command.Arguments.Add(sourceReference);
        command.Arguments.Add(destinationTarget);
        command.Options.Add(dryRun);
        routeGroup.Subcommands.Add(command);
        return new RouteMoveSymbols(
            routeGroup,
            command,
            sourceReference,
            destinationTarget,
            dryRun);
    }

    internal CliBindResult<RouteMoveRequest, RouteMoveResult> Bind(
        ParseResult parseResult,
        CliInvocation invocation,
        RouteMoveSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(invocation);
        ArgumentNullException.ThrowIfNull(symbols);
        var sourceReference = parseResult.GetValue(symbols.SourceReference);
        var destinationTarget = parseResult.GetValue(symbols.DestinationTarget);
        var mode = parseResult.GetValue(symbols.DryRun)
            ? RouteMoveMode.DryRun
            : RouteMoveMode.Apply;
        var validation = _validator.Validate(
            new RouteMoveBindingInput
            {
                SourceReference = sourceReference,
                DestinationTarget = destinationTarget,
                ParserErrors = parseResult.Errors.Select(error => error.Message).ToArray(),
            },
            mode);
        if (validation.Failure is { } failure)
        {
            return CliBindResult<RouteMoveRequest, RouteMoveResult>.Invalid(
                _invalidResultFactory.Create(
                    new RouteMoveInvalidResultInput
                    {
                        Workspace = invocation.Workspace,
                        SourceReference = sourceReference ?? RouteMoveDefinitions.SourceReference.Name,
                        DestinationTarget = destinationTarget ?? RouteMoveDefinitions.DestinationTarget.Name,
                        Mode = mode,
                        Failure = failure,
                    }));
        }

        var facts = validation.Facts
            ?? throw new InvalidOperationException(
                "Valid Route Move binding validation requires bound facts.");
        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException(
                "The route-move binding requires a selected workspace.");
        return CliBindResult<RouteMoveRequest, RouteMoveResult>.Bound(
            new RouteMoveRequest(
                workspace,
                facts.SourceReference,
                facts.DestinationTarget,
                facts.Mode));
    }

    internal CliCommandBinding<RouteMoveRequest, RouteMoveResult> Close(
        RouteMoveSymbols symbols,
        RouteMoveBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        return new CliCommandBinding<RouteMoveRequest, RouteMoveResult>(
            symbols.MoveCommand,
            new CliCommandBindingComponents<RouteMoveRequest, RouteMoveResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = (parse, invocation) => Bind(parse.Result, invocation, symbols),
                InvalidResultFactory = input => _invalidResultFactory.Create(
                    new RouteMoveInvalidResultInput
                    {
                        Workspace = null,
                        SourceReference = RouteMoveDefinitions.SourceReference.Name,
                        DestinationTarget = RouteMoveDefinitions.DestinationTarget.Name,
                        Mode = RouteMoveMode.Apply,
                        Failure = new RouteMoveBindingFailure(
                            RouteMoveFindingCode.InvalidInput,
                            ReadContextualCause(input)),
                    }),
                Operation = components.Operation.ExecuteAsync,
                Renderers = components.Renderers,
                DiagnosticRenderer = components.DiagnosticRenderer,
            });
    }

    private static string ReadContextualCause(CliInvalidBindingInput input)
    {
        var diagnostics = input.InvalidInput.Diagnostics;
        if (diagnostics.Count == 0)
        {
            return "The Route Move command input is invalid.";
        }

        return diagnostics.Count == 1
            ? diagnostics[0]
            : string.Join(" ", diagnostics);
    }
}
