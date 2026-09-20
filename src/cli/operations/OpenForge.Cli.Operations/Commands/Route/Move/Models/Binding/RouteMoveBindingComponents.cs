using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Binding;

internal sealed record RouteMoveBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required RouteMoveOperation Operation { get; init; }
}
