using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Binding;

internal sealed class RouteInitBindingComponents
{
    internal required CliHelpContent Help { get; init; }

    internal required RouteInitOperation Operation { get; init; }

    internal required CliRendererSet<RouteInitResult> Renderers { get; init; }

    internal CliDiagnosticRenderer<RouteInitResult>? DiagnosticRenderer { get; init; }
}
