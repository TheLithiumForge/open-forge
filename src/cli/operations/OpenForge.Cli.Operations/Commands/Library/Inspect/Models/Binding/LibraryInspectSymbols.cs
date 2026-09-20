using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Models.Binding;

internal sealed record LibraryInspectSymbols
{
    public required Command Command { get; init; }

    public required Argument<string?> LibraryId { get; init; }
}
