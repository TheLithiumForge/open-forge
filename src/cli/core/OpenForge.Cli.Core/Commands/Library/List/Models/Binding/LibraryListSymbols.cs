using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Library.List.Models.Binding;

internal sealed record LibraryListSymbols
{
    public required Command Command { get; init; }
}
