using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Exclude.Models.Binding;

internal sealed record RouteExcludeBindingComponents
{
    public required CliHelpContent Help { get; init; }
    public required RouteExcludeOperation Operation { get; init; }
}
