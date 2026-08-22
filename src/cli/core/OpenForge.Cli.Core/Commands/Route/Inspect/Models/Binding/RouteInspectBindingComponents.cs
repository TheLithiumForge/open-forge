using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;

internal sealed class RouteInspectBindingComponents
{
    internal required CliHelpContent Help { get; init; }

    internal required RouteInspectOperation Operation { get; init; }

    internal required CliRendererSet<RouteInspectResult> Renderers { get; init; }

    internal CliDiagnosticRenderer<RouteInspectResult>? DiagnosticRenderer { get; init; }
}
