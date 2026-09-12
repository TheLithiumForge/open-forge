using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Models.Binding;

internal sealed record ExtensionCreateSymbols
{
    public required Command CreateCommand { get; init; }

    public required Argument<string?> StableId { get; init; }

    public required Option<string?> Path { get; init; }

    public required Option<string?> Name { get; init; }

    public required Option<string?> Description { get; init; }

    public required Option<string?> PackageVersion { get; init; }

    public required Option<string[]> Dependency { get; init; }

    public required Option<bool> Automatic { get; init; }

    public required Option<bool> DryRun { get; init; }

}

internal sealed class ExtensionCreateBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required ExtensionCreateOperation Operation { get; init; }

    public required CliRendererSet<ExtensionCreateResult> Renderers { get; init; }

    public CliDiagnosticRenderer<ExtensionCreateResult>? DiagnosticRenderer { get; init; }
}
