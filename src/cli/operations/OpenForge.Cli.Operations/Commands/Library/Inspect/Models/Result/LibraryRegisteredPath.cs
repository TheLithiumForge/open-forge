
namespace OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;

internal sealed record LibraryRegisteredPath
{
    public required string SourcePath { get; init; }

    public required string DestinationPath { get; init; }

    public required string? ExpectedRelativeLink { get; init; }

    public required string? SourceId { get; init; }
}
