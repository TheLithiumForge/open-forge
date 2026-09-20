using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Models.Binding;

internal sealed record LibraryDetachSymbols
{
    public required Command Command { get; init; }

    public required Argument<string?> LibraryId { get; init; }

    public required Option<bool> DryRun { get; init; }

    public required Option<bool> Automatic { get; init; }

    public required Option<string[]> Allow { get; init; }
}
