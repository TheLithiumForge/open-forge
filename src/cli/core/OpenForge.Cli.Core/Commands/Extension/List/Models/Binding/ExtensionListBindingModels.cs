using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.List.Models.Binding;

internal sealed record ExtensionListSymbols(
    Command ListCommand,
    Option<bool> Installed,
    Option<bool> Available,
    Option<string?> Source);

internal sealed class ExtensionListBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required ExtensionListOperation Operation { get; init; }

    public required CliRendererSet<ExtensionListResult> Renderers { get; init; }

    public CliDiagnosticRenderer<ExtensionListResult>? DiagnosticRenderer { get; init; }
}
