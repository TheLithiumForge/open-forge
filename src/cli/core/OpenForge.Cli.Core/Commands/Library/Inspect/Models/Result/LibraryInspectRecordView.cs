using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;

internal sealed record LibraryInspectRecordView
{
    public required string Path { get; init; }

    public required LibraryRecordViewState State { get; init; }

    public required string? Id { get; init; }

    public required string? SourceRoot { get; init; }

    public required LibraryRegisteredPath[] RegisteredPaths { get; init; }
}
