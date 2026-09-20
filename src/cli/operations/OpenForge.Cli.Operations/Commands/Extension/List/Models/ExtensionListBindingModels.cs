using System.CommandLine;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.List.Models;

internal sealed record ExtensionListSymbols(
    Command ListCommand,
    Option<bool> Installed,
    Option<bool> Available,
    Option<string?> Source);

internal sealed class ExtensionListBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required ExtensionListOperation Operation { get; init; }
}
