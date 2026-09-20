using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Library.Inspect.Models;

internal sealed record LibraryInspectData
{
    public required string? Id { get; init; }

    public required string? SourceFolder { get; init; }

    public required string? DestinationFolder { get; init; }

    public required bool Current { get; init; }

    public required LibraryInspectDataFiles Files { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LibraryInspectDataInventory? Inventory { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<LibraryInspectDataFile> TextFiles { get; init; } = [];

    [JsonIgnore]
    internal bool ShowFiles { get; init; }

    [JsonIgnore]
    internal bool ShowTargets { get; init; }
}

internal sealed record LibraryInspectDataFile
{
    public required string SourcePath { get; init; }

    public required string DestinationPath { get; init; }

    public required string Relation { get; init; }

    public string? ExpectedTarget { get; init; }

    public string? ObservedTarget { get; init; }
}

internal sealed record LibraryInspectDataInventory
{
    public required int Eligible { get; init; }

    public required int Excluded { get; init; }
}

internal enum LibraryInspectDataFilesShape
{
    Differences,
    Paths,
    Targets,
}

[JsonConverter(typeof(OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Rendering.LibraryInspectDataFilesConverter))]
internal sealed class LibraryInspectDataFiles
{
    private LibraryInspectDataFiles(
        LibraryInspectDataFilesShape shape,
        IReadOnlyList<LibraryInspectDataFile> rows)
    {
        Shape = shape;
        Rows = rows;
    }

    internal LibraryInspectDataFilesShape Shape { get; }

    internal IReadOnlyList<LibraryInspectDataFile> Rows { get; }

    internal static LibraryInspectDataFiles From(
        LibraryInspectDataFilesShape shape,
        IReadOnlyList<LibraryInspectDataFile> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        if (!Enum.IsDefined(shape))
        {
            throw new ArgumentOutOfRangeException(nameof(shape), shape, "The Library Inspect file shape is not defined.");
        }

        return new(shape, rows);
    }
}
