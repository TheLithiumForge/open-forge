using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;

internal sealed record RouteInspectSymbols(
    Command RouteGroup,
    Command InspectCommand,
    Argument<string[]> SourceReferences,
    Option<string[]> WorkingPaths)
{
    internal static Option<string[]> CreateWorkingPaths()
        => new(RouteInspectDefinitions.WorkingPath.Name)
        {
            Description = RouteInspectDefinitions.WorkingPath.Description,
            HelpName = "path",
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
}
