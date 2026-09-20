using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;

namespace OpenForge.Cli.Core.Presentation.Library.List.Models;

internal sealed record LibraryListData
{
    public required IReadOnlyList<LibraryListDataLibrary> Libraries { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LibraryListDataRecordCoverage? RecordCoverage { get; init; }

    [JsonIgnore]
    internal LibraryListRecordView? Record { get; init; }

    [JsonIgnore]
    internal LibraryListInventoryState Inventory { get; init; }

    [JsonIgnore]
    internal LibraryCoverage Coverage { get; init; }

    [JsonIgnore]
    internal bool ShowLinks { get; init; }

    [JsonIgnore]
    internal bool ShowLinkTargets { get; init; }
}

internal sealed record LibraryListDataLibrary
{
    public required string Id { get; init; }
    public required string SourceFolder { get; init; }
    public required string DestinationFolder { get; init; }
    public required LibraryListDataLinks Links { get; init; }

    [JsonIgnore]
    internal LibrarySourceRootViewState SourceRootState { get; init; }
}

internal sealed record LibraryListDataRecordCoverage
{
    public required string Path { get; init; }
    public required string State { get; init; }
    public int? LibraryCount { get; init; }

    [JsonIgnore]
    internal LibraryRecordViewState ResultState { get; init; }
}

internal sealed record LibraryListDataLinkCounts
{
    internal LibraryListDataLinkCounts(int current, int missing, int changed, int unavailable)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(current);
        ArgumentOutOfRangeException.ThrowIfNegative(missing);
        ArgumentOutOfRangeException.ThrowIfNegative(changed);
        ArgumentOutOfRangeException.ThrowIfNegative(unavailable);
        Current = current;
        Missing = missing;
        Changed = changed;
        Unavailable = unavailable;
    }

    internal int Current { get; }
    internal int Missing { get; }
    internal int Changed { get; }
    internal int Unavailable { get; }
}

internal sealed record LibraryListDataLink
{
    public required string Path { get; init; }
    public required string State { get; init; }
    public string? ExpectedTarget { get; init; }
    public string? ObservedTarget { get; init; }
    public string? SourceId { get; init; }

    [JsonIgnore]
    internal LibraryLinkViewState ResultState { get; init; }
}

internal enum LibraryListDataLinksShape
{
    Counts,
    Paths,
    Targets,
}

[JsonConverter(typeof(OpenForge.Cli.Core.Presentation.Library.List.Shared.Rendering.LibraryListDataLinksConverter))]
internal sealed class LibraryListDataLinks
{
    private LibraryListDataLinks(
        LibraryListDataLinksShape shape,
        LibraryListDataLinkCounts counts,
        IReadOnlyList<LibraryListDataLink> rows)
    {
        Shape = shape;
        Counts = counts;
        Rows = rows;
    }

    internal LibraryListDataLinksShape Shape { get; }
    internal LibraryListDataLinkCounts Counts { get; }
    internal IReadOnlyList<LibraryListDataLink> Rows { get; }

    internal static LibraryListDataLinks FromCounts(LibraryListDataLinkCounts counts)
        => new(
            LibraryListDataLinksShape.Counts,
            counts,
            []);

    internal static LibraryListDataLinks FromPaths(
        LibraryListDataLinksShape shape,
        IReadOnlyList<LibraryListDataLink> rows,
        LibraryListDataLinkCounts counts)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(counts);
        if (shape is not (LibraryListDataLinksShape.Paths or LibraryListDataLinksShape.Targets))
        {
            throw new ArgumentOutOfRangeException(nameof(shape), shape, "A row shape is required.");
        }

        return new(shape, counts, rows);
    }
}



