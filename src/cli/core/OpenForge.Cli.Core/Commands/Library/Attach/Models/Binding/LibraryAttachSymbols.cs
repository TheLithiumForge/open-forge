using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Models.Binding;

internal sealed record LibraryAttachSymbols
{
    public required Command Command { get; init; }

    public required Argument<string?> LibraryId { get; init; }

    public required Argument<string?> SourceRoot { get; init; }

    public required Option<string?> DestinationRoot { get; init; }

    public required Option<bool> DryRun { get; init; }
}
