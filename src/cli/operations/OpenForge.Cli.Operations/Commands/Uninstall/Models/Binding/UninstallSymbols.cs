using System.CommandLine;
using OpenForge.Cli.Core.Commands.Stripping.Models.Binding;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Uninstall.Models.Binding;

internal sealed record UninstallSymbols
{
    public required Command Command { get; init; }
    public required StripSelectionSymbols Selection { get; init; }
    public required Option<bool> Automatic { get; init; }
}

internal sealed record UninstallBindingComponents
{
    public required CliHelpContent Help { get; init; }
    public required UninstallOperation Operation { get; init; }
}
