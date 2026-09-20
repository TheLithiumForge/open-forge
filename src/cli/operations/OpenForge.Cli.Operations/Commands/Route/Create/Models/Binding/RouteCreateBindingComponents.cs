using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Binding;

internal sealed record RouteCreateBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required RouteCreateOperation Operation { get; init; }
}
