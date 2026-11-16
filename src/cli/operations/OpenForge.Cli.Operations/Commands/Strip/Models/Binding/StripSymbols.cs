using System.CommandLine;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Strip.Models.Binding;

internal sealed record StripSymbols
{
    public required Command Command { get; init; }
    public required Argument<string[]> Parts { get; init; }
    public required Option<string?> Flatten { get; init; }
    public required Option<bool> DryRun { get; init; }
    public required Option<bool> Automatic { get; init; }
}

internal sealed record StripBindingComponents
{
    public required CliHelpContent Help { get; init; }
    public required StripOperationRunner Runner { get; init; }
}
