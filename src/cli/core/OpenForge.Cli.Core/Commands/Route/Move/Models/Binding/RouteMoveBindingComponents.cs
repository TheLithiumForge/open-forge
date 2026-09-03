using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Binding;

internal sealed record RouteMoveBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required RouteMoveOperation Operation { get; init; }

    public required CliRendererSet<RouteMoveResult> Renderers { get; init; }

    public CliDiagnosticRenderer<RouteMoveResult>? DiagnosticRenderer { get; init; }
}
