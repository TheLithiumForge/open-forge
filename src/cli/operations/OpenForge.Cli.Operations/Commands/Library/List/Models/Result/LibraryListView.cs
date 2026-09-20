using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;

namespace OpenForge.Cli.Core.Commands.Library.List.Models.Result;

internal sealed record LibraryListView
{
    public required string Id { get; init; }

    public required string SourceRoot { get; init; }
    public required string DestinationRoot { get; init; }

    public required LibrarySourceRootViewState SourceRootState { get; init; }

    public required LibraryListRegisteredPath[] Paths { get; init; }
}
