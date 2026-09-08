using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;

internal sealed record LibraryMutationMapping
{
    public required string SourcePath { get; init; }

    public required string DestinationPath { get; init; }

    public required string? SourceId { get; init; }

    public required string? ExpectedRelativeLink { get; init; }

    public required string? ObservedRelativeLink { get; init; }

    public required LibraryLinkViewState State { get; init; }

    public required LibraryComparisonRelation Relation { get; init; }
}
