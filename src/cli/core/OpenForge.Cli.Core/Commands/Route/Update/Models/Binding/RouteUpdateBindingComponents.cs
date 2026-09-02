using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Binding;

internal sealed record RouteUpdateBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required RouteUpdateOperation Operation { get; init; }

    public required CliRendererSet<RouteUpdateResult> Renderers { get; init; }

    public CliDiagnosticRenderer<RouteUpdateResult>? DiagnosticRenderer { get; init; }
}
