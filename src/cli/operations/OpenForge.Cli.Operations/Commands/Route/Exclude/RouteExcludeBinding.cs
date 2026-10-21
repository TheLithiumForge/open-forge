using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Exclude.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Route.Exclude;

internal static class RouteExcludeBinding
{
    internal static RouteExcludeSymbols CreateSymbols(Command routeGroup)
    {
        var command = new Command(RouteExcludeDefinitions.Command.Name, RouteExcludeDefinitions.Command.Description);
        var file = new Argument<string?>(RouteExcludeDefinitions.File.Name) { Description = RouteExcludeDefinitions.File.Description, Arity = ArgumentArity.ZeroOrOne };
        var dryRun = new Option<bool>("--dry-run") { Description = RouteExcludeDefinitions.DryRun.Description, Arity = ArgumentArity.Zero };
        command.Arguments.Add(file);
        command.Options.Add(dryRun);
        routeGroup.Subcommands.Add(command);
        return new RouteExcludeSymbols { Command = command, File = file, DryRun = dryRun };
    }

    internal static CliRequestBinding<RouteExcludeRequest, RouteExcludeResult> CreateRequestBinding(RouteExcludeSymbols symbols, RouteExcludeBindingComponents components)
        => new()
        {
            Command = symbols.Command, Help = components.Help, WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => RouteExcludeRequestBinder.Bind(parse.Result, invocation, symbols),
            InvalidResultFactory = RouteExcludeInvalidResultFactory.Create,
            Operation = components.Operation.ExecuteAsync,
        };
}
