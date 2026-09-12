using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Binding;

internal sealed record RouteRemoveBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required RouteRemoveOperation Operation { get; init; }

    public required CliRendererSet<RouteRemoveResult> Renderers { get; init; }

    public CliDiagnosticRenderer<RouteRemoveResult>? DiagnosticRenderer { get; init; }
}
