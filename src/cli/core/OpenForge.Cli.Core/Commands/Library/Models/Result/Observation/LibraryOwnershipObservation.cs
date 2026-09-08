using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;

internal sealed record LibraryOwnershipObservation
{
    public required string Path { get; init; }

    public required LibraryOwnershipKind Kind { get; init; }

    public required string? OwnerId { get; init; }

    public required string? Cause { get; init; }
}
