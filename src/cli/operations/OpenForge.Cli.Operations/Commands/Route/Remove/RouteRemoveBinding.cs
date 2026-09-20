using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Remove;

internal sealed class RouteRemoveBinding(
    RouteRemoveBindingValidator validator,
    RouteRemoveInvalidResultFactory invalidResultFactory)
{
    private readonly RouteRemoveBindingValidator _validator = validator;
    private readonly RouteRemoveInvalidResultFactory _invalidResultFactory = invalidResultFactory;

    internal RouteRemoveSymbols CreateSymbols(Command routeGroup)
    {
        ArgumentNullException.ThrowIfNull(routeGroup);
        var sourceReference = new Argument<string?>(RouteRemoveDefinitions.SourceReference.Name)
        {
            Description = RouteRemoveDefinitions.SourceReference.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var dryRun = new Option<bool>(RouteRemoveDefinitions.DryRun.Name)
        {
            Description = RouteRemoveDefinitions.DryRun.Description,
            Arity = ArgumentArity.Zero,
        };
        var automatic = new Option<bool>(RouteRemoveDefinitions.Automatic.Name)
        {
            Description = RouteRemoveDefinitions.Automatic.Description,
            Arity = ArgumentArity.Zero,
        };
        var command = new Command(
            RouteRemoveDefinitions.RemoveCommand.Name,
            RouteRemoveDefinitions.RemoveCommand.Description);
        command.Arguments.Add(sourceReference);
        command.Options.Add(dryRun);
        command.Options.Add(automatic);
        routeGroup.Subcommands.Add(command);
        return new RouteRemoveSymbols(routeGroup, command, sourceReference, dryRun, automatic);
    }

    internal CliBindResult<RouteRemoveRequest, RouteRemoveResult> Bind(
        ParseResult parseResult,
        CliInvocation invocation,
        RouteRemoveSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(invocation);
        ArgumentNullException.ThrowIfNull(symbols);
        var sourceReference = parseResult.GetValue(symbols.SourceReference);
        var mode = parseResult.GetValue(symbols.DryRun)
            ? RouteRemoveMode.DryRun
            : RouteRemoveMode.Apply;
        var automatic = parseResult.GetValue(symbols.Automatic);
        var validation = _validator.Validate(
            new RouteRemoveBindingInput
            {
                SourceReference = sourceReference,
                ParserErrors = parseResult.Errors.Select(error => error.Message).ToArray(),
            },
            mode);
        if (validation.Failure is { } failure)
        {
            return CliBindResult<RouteRemoveRequest, RouteRemoveResult>.Invalid(
                _invalidResultFactory.Create(new RouteRemoveInvalidResultInput
                {
                    Workspace = invocation.Workspace,
                    SourceReference = sourceReference ?? RouteRemoveDefinitions.SourceReference.Name,
                    Mode = mode,
                    Failure = failure,
                }));
        }

        var facts = validation.Facts
            ?? throw new InvalidOperationException("Valid Route Remove binding validation requires bound facts.");
        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("The route-remove binding requires a selected workspace.");
        return CliBindResult<RouteRemoveRequest, RouteRemoveResult>.Bound(
            new RouteRemoveRequest(
                workspace,
                facts.SourceReference,
                facts.Mode,
                automatic,
                allowInteractiveSourceSelection: invocation.Presentation.Format == CliFormat.Text && !automatic,
                allowInteractiveConfirmation: invocation.Presentation.Format == CliFormat.Text && !automatic));
    }

    internal CliRequestBinding<RouteRemoveRequest, RouteRemoveResult> CreateRequestBinding(
        RouteRemoveSymbols symbols,
        RouteRemoveBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        return new CliRequestBinding<RouteRemoveRequest, RouteRemoveResult>
        {
            Command = symbols.RemoveCommand,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => Bind(parse.Result, invocation, symbols),
            InvalidResultFactory = input => _invalidResultFactory.Create(new RouteRemoveInvalidResultInput
            {
                Workspace = null,
                SourceReference = RouteRemoveDefinitions.SourceReference.Name,
                Mode = RouteRemoveMode.Apply,
                Failure = new RouteRemoveBindingFailure(
                    RouteRemoveFindingCode.InvalidInput,
                    ReadContextualCause(input)),
            }),
            Operation = components.Operation.ExecuteAsync,
        };
    }

    private static string ReadContextualCause(CliInvalidBindingInput input)
    {
        var diagnostics = input.InvalidInput.Diagnostics;
        if (diagnostics.Count == 0)
        {
            return "The Route Remove command input is invalid.";
        }

        return diagnostics.Count == 1 ? diagnostics[0] : string.Join(" ", diagnostics);
    }
}
