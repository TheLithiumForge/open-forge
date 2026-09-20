using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;

internal sealed record LibraryPathComparison
{
    public required string SourcePath { get; init; }

    public required string DestinationPath { get; init; }

    public required string? SourceId { get; init; }

    public required LibraryComparisonRelation Relation { get; init; }

    public required LibraryRegisteredPath? Registered { get; init; }

    public required string? ObservedRelativeLink { get; init; }
}
