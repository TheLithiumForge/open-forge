using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Include.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Route.Include;

internal static class RouteIncludeBinding
{
    internal static RouteIncludeSymbols CreateSymbols(Command routeGroup)
    {
        var command = new Command(RouteIncludeDefinitions.Command.Name, RouteIncludeDefinitions.Command.Description);
        var file = new Argument<string?>(RouteIncludeDefinitions.File.Name) { Description = RouteIncludeDefinitions.File.Description, Arity = ArgumentArity.ZeroOrOne };
        var dryRun = new Option<bool>("--dry-run") { Description = RouteIncludeDefinitions.DryRun.Description, Arity = ArgumentArity.Zero };
        command.Arguments.Add(file);
        command.Options.Add(dryRun);
        var description = new Option<string?>("--description") { Description = RouteIncludeDefinitions.Description.Description, HelpName = "text", Arity = ArgumentArity.ZeroOrOne };
        var tag = new Option<string[]>("--tag") { Description = RouteIncludeDefinitions.Tag.Description, HelpName = "tag", Arity = ArgumentArity.ZeroOrMore, AllowMultipleArgumentsPerToken = false };
        var applyTo = new Option<string[]>("--apply-to") { Description = RouteIncludeDefinitions.ApplyTo.Description, HelpName = "glob", Arity = ArgumentArity.ZeroOrMore, AllowMultipleArgumentsPerToken = false };
        command.Options.Add(description);
        command.Options.Add(tag);
        command.Options.Add(applyTo);
        routeGroup.Subcommands.Add(command);
        return new RouteIncludeSymbols { Command = command, File = file, DryRun = dryRun, Description = description, Tag = tag, ApplyTo = applyTo };
    }

    internal static CliRequestBinding<RouteIncludeRequest, RouteIncludeResult> CreateRequestBinding(RouteIncludeSymbols symbols, RouteIncludeBindingComponents components)
        => new()
        {
            Command = symbols.Command, Help = components.Help, WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => RouteIncludeRequestBinder.Bind(parse.Result, invocation, symbols),
            InvalidResultFactory = RouteIncludeInvalidResultFactory.Create,
            Operation = components.Operation.ExecuteAsync,
        };
}
