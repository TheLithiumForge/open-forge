using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

namespace OpenForge.Cli.Core.Commands.Route.Update;

internal sealed partial class RouteUpdateBinding(RouteUpdateSymbols symbols)
{
    private readonly RouteUpdateSymbols _symbols = symbols;

    internal static RouteUpdateSymbols CreateSymbols(Command routeGroup)
    {
        var sourceReference = new Argument<string?>(RouteUpdateDefinitions.SourceReference.Name)
        {
            Description = RouteUpdateDefinitions.SourceReference.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var description = CreateSingleton(RouteUpdateDefinitions.Description);
        var tag = new Option<string[]>(RouteUpdateDefinitions.Tag.Name)
        {
            Description = RouteUpdateDefinitions.Tag.Description,
            HelpName = RouteUpdateDefinitions.Tag.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        var responsibility = CreateSingleton(RouteUpdateDefinitions.Responsibility);
        var template = CreateSingleton(RouteUpdateDefinitions.Template);
        var dryRun = new Option<bool>(RouteUpdateDefinitions.DryRun.Name)
        {
            Description = RouteUpdateDefinitions.DryRun.Description,
            Arity = ArgumentArity.Zero,
        };
        var command = new Command(
            RouteUpdateDefinitions.UpdateCommand.Name,
            RouteUpdateDefinitions.UpdateCommand.Description);
        command.Arguments.Add(sourceReference);
        command.Options.Add(description);
        command.Options.Add(tag);
        command.Options.Add(responsibility);
        command.Options.Add(template);
        command.Options.Add(dryRun);
        routeGroup.Subcommands.Add(command);
        return new RouteUpdateSymbols
        {
            RouteGroup = routeGroup,
            UpdateCommand = command,
            SourceReference = sourceReference,
            Description = description,
            Tag = tag,
            Responsibility = responsibility,
            Template = template,
            DryRun = dryRun,
            DelimiterPolicies = Array.AsReadOnly(
            [
                new CliDelimiterPolicy(
                    RouteUpdateDefinitions.Tag.Name,
                    CliDelimiterShape.Equals),
            ]),
        };
    }

    internal CliCommandBinding<RouteUpdateRequest, RouteUpdateResult> Bind(
        RouteUpdateBindingComponents components)
    {
        var invalidFactory = new RouteUpdateInvalidResultFactory();
        return new CliCommandBinding<RouteUpdateRequest, RouteUpdateResult>(
            _symbols.UpdateCommand,
            new CliCommandBindingComponents<RouteUpdateRequest, RouteUpdateResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = (parse, invocation) => BindRequest(
                    parse.Result,
                    parse.OriginalArguments,
                    invocation),
                InvalidResultFactory = invalidFactory.CreateContextual,
                Operation = components.Operation.ExecuteAsync,
                Renderers = components.Renderers,
                DiagnosticRenderer = components.DiagnosticRenderer,
            });
    }

    internal CliBindResult<RouteUpdateRequest, RouteUpdateResult> BindRequest(
        ParseResult parseResult,
        IReadOnlyList<string> originalArguments,
        CliInvocation invocation)
    {
        var input = ReadInput(parseResult, originalArguments, _symbols);
        var mode = parseResult.GetValue(_symbols.DryRun)
            ? RouteUpdateMode.DryRun
            : RouteUpdateMode.Apply;
        var validation = new RouteUpdateBindingValidator().Validate(input, mode);
        if (validation.Failure is { } failure)
        {
            return CliBindResult<RouteUpdateRequest, RouteUpdateResult>.Invalid(
                new RouteUpdateInvalidResultFactory().Create(
                    invocation.Workspace,
                    input,
                    mode,
                    failure));
        }

        var facts = validation.Facts
            ?? throw new InvalidOperationException(
                "Valid Route Update binding validation requires bound facts.");
        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException(
                "The route-update binding requires a selected workspace.");
        return CliBindResult<RouteUpdateRequest, RouteUpdateResult>.Bound(
            new RouteUpdateRequest(
                workspace: workspace,
                sourceReference: facts.Target,
                patch: facts.Patch,
                templateReference: facts.Template,
                mode: facts.Mode));
    }

    private static Option<string?> CreateSingleton(
        CliOptionDefinition<string?> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
        };

}
