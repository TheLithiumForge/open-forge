using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;

namespace OpenForge.Cli.Core.Commands.Library.List.Models.Result;

internal sealed record LibraryListRegisteredPath
{
    public required string SourcePath { get; init; }

    public required string DestinationPath { get; init; }

    public required string? ExpectedRelativeLink { get; init; }

    public required string? SourceId { get; init; }

    public required LibraryLinkViewState State { get; init; }

    public required string? ObservedRelativeLink { get; init; }
}
