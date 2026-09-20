using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;

internal sealed record LibraryExpectedState
{
    public required LibraryExpectedStateKind Kind { get; init; }

    public required long? Length { get; init; }

    public required string? Sha256 { get; init; }

    public required string? RawRelativeTarget { get; init; }
}
