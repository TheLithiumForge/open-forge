
namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;

internal sealed record LibraryEligiblePath
{
    public required string SourcePath { get; init; }

    public required string DestinationPath { get; init; }

    public required string? SourceId { get; init; }
}
