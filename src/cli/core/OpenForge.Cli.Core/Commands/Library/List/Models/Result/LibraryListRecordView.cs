using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;

namespace OpenForge.Cli.Core.Commands.Library.List.Models.Result;

internal sealed record LibraryListRecordView
{
    public required string Path { get; init; }

    public required LibraryRecordViewState State { get; init; }

    public required int? LibraryCount { get; init; }
}
