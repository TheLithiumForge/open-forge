using System.CommandLine;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Uninstall.Models.Binding;

internal sealed record UninstallSymbols
{
    public required Command Command { get; init; }
    public required Option<string[]> Strip { get; init; }
    public required Option<string?> Flatten { get; init; }
    public required Option<string[]> RemoveEdited { get; init; }
    public required Option<bool> DryRun { get; init; }
    public required Option<bool> Automatic { get; init; }
}

internal sealed record UninstallBindingComponents
{
    public required CliHelpContent Help { get; init; }
    public required UninstallOperation Operation { get; init; }
}
