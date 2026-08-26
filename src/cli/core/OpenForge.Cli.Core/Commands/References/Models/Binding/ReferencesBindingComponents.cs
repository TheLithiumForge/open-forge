using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.References.Models.Binding;

internal sealed class ReferencesBindingComponents
{
    internal required CliHelpContent Help { get; init; }

    internal required ReferencesOperation Operation { get; init; }

    internal required CliRendererSet<ReferencesResult> Renderers { get; init; }

    internal CliDiagnosticRenderer<ReferencesResult>? DiagnosticRenderer { get; init; }
}
