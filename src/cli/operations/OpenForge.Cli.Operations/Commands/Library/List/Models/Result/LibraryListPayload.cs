using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;

namespace OpenForge.Cli.Core.Commands.Library.List.Models.Result;

internal sealed record LibraryListPayload
{
    public required LibraryListRecordView Record { get; init; }

    public required LibraryListView[] Libraries { get; init; }

    public required LibraryListInventoryState Inventory { get; init; }

    public required LibraryCoverage Coverage { get; init; }

    public required LibraryListFinding[] Findings { get; init; }
}
