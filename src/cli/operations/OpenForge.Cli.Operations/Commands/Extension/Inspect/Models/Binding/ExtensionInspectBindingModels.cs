using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Binding;

internal sealed record ExtensionInspectSymbols(
    Command InspectCommand,
    Argument<string?> StableId,
    Option<string?> Source);

internal sealed class ExtensionInspectBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required ExtensionInspectOperation Operation { get; init; }
}
