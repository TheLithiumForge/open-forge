using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Include.Models.Binding;

internal sealed record RouteIncludeBindingComponents
{
    public required CliHelpContent Help { get; init; }
    public required RouteIncludeOperation Operation { get; init; }
}
