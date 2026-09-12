using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

namespace OpenForge.Cli.Core.Commands.Route.Create;

internal static class RouteCreateBinding
{
    internal static RouteCreateSymbols CreateSymbols(Command routeGroup)
    {
        var fileTarget = new Argument<string?>(RouteCreateDefinitions.FileTarget.Name)
        {
            Description = RouteCreateDefinitions.FileTarget.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var description = CreateSingleton(RouteCreateDefinitions.Description);
        var tag = new Option<string[]>(RouteCreateDefinitions.Tag.Name)
        {
            Description = RouteCreateDefinitions.Tag.Description,
            HelpName = RouteCreateDefinitions.Tag.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        var responsibility = CreateSingleton(RouteCreateDefinitions.Responsibility);
        var template = CreateSingleton(RouteCreateDefinitions.Template);
        var dryRun = new Option<bool>(RouteCreateDefinitions.DryRun.Name)
        {
            Description = RouteCreateDefinitions.DryRun.Description,
            Arity = ArgumentArity.Zero,
        };
        var command = new Command(
            RouteCreateDefinitions.CreateCommand.Name,
            RouteCreateDefinitions.CreateCommand.Description);
        command.Arguments.Add(fileTarget);
        command.Options.Add(description);
        command.Options.Add(tag);
        command.Options.Add(responsibility);
        command.Options.Add(template);
        command.Options.Add(dryRun);
        routeGroup.Subcommands.Add(command);
        return new RouteCreateSymbols
        {
            RouteGroup = routeGroup,
            CreateCommand = command,
            FileTarget = fileTarget,
            Description = description,
            Tag = tag,
            Responsibility = responsibility,
            Template = template,
            DryRun = dryRun,
            DelimiterPolicies = Array.AsReadOnly(
            [
                new CliDelimiterPolicy(
                    RouteCreateDefinitions.Tag.Name,
                    CliDelimiterShape.Equals),
            ]),
        };
    }

    internal static CliCommandBinding<RouteCreateRequest, RouteCreateResult> Close(
        RouteCreateSymbols symbols,
        RouteCreateBindingComponents components)
    {
        return new CliCommandBinding<RouteCreateRequest, RouteCreateResult>(
            symbols.CreateCommand,
            new CliCommandBindingComponents<RouteCreateRequest, RouteCreateResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = (parse, invocation) => RouteCreateRequestBinder.Bind(parse.Result, invocation, symbols),
                InvalidResultFactory = RouteCreateInvalidResultFactory.CreateContextualInvalidResult,
                Operation = components.Operation.ExecuteAsync,
                Renderers = components.Renderers,
                DiagnosticRenderer = components.DiagnosticRenderer,
            });
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
