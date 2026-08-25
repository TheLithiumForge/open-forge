using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Find.Models.Binding;

internal sealed class FindBindingComponents
{
    internal required CliHelpContent Help { get; init; }

    internal required FindOperation Operation { get; init; }

    internal required CliRendererSet<FindResult> Renderers { get; init; }

    internal CliDiagnosticRenderer<FindResult>? DiagnosticRenderer { get; init; }
}
