using System.CommandLine;
using OpenForge.Cli.Core.Commands.Stripping.Models.Binding;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Export.Models.Binding;

internal sealed record ExportSymbols
{
    public required Command Command { get; init; }
    public required Argument<string?> Destination { get; init; }
    public required StripSelectionSymbols Selection { get; init; }
}

internal sealed record ExportBindingComponents
{
    public required CliHelpContent Help { get; init; }
    public required ExportOperation Operation { get; init; }
}
