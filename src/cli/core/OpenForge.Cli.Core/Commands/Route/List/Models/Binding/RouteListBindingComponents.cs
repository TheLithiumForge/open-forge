using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.List.Models.Binding;

internal sealed class RouteListBindingComponents
{
    internal required CliHelpContent Help { get; init; }

    internal required RouteListOperation Operation { get; init; }

    internal required CliRendererSet<RouteListResult> Renderers { get; init; }

    internal CliDiagnosticRenderer<RouteListResult>? DiagnosticRenderer { get; init; }
}
