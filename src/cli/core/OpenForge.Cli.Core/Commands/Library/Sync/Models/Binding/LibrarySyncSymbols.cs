using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Models.Binding;

internal sealed record LibrarySyncSymbols
{
    public required Command Command { get; init; }

    public required Argument<string?> LibraryId { get; init; }

    public required Option<bool> DryRun { get; init; }
}
