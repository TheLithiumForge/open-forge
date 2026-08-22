using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;

internal sealed record RouteInspectBindingComponents(
    CliHelpContent Help,
    RouteInspectOperation Operation,
    CliRendererSet<RouteInspectResult> Renderers,
    CliDiagnosticRenderer<RouteInspectResult>? DiagnosticRenderer);
