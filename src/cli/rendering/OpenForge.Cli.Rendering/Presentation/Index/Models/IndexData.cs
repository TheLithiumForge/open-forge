using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;

namespace OpenForge.Cli.Core.Presentation.Index.Models;

internal sealed record IndexData
{
    public required string Mode { get; init; }
    public required IReadOnlyList<IndexDataChange> Changes { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<IndexDataPath>? Unchanged { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IndexDataSelection? Selection { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<IndexDataRegion>? Regions { get; init; }
    // Task 79 additions come after the existing members so the frozen order only grows at the end.
    public IReadOnlyList<IndexDataMetadata> Metadata { get; init; } = [];
    public IReadOnlyList<IndexDataEntrypoint> Entrypoints { get; init; } = [];
    [JsonIgnore] internal IReadOnlyList<IndexDataRegion> TextRegions { get; init; } = [];
    [JsonIgnore] internal IndexRecovery Recovery { get; init; } = IndexRecovery.NotRequired;
    [JsonIgnore] public bool NothingWritten { get; init; }
    [JsonIgnore] public int CurrentCount { get; init; }
    [JsonIgnore] internal IReadOnlyList<IndexDataEntrypoint> TextDeletions { get; init; } = [];
    [JsonIgnore] internal IReadOnlyDictionary<string, string> TextRenames { get; init; } = new Dictionary<string, string>();
}

internal sealed record IndexDataChange(string Path, int? Before, int? After)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Diff { get; init; }
}
internal sealed record IndexDataPath(string Path);
internal sealed record IndexDataMetadata(string Path, IReadOnlyList<string> Fields, string? Description, IReadOnlyList<string>? Tags, string Outcome);
internal sealed record IndexDataEntrypoint(string Path, string Outcome);
internal sealed record IndexDataSource(string Id, string Path);
internal sealed record IndexDataSelection(string Origin, string Scope, IReadOnlyList<IndexDataSource> Sources);
internal sealed record IndexDataRegion(string Path, string Action, string Outcome)
{
    [JsonIgnore] public int? Before { get; init; }
    [JsonIgnore] public int? After { get; init; }
    [JsonIgnore] public IndexRegionOutcome ResultOutcome { get; init; }
    [JsonIgnore] public IndexRegionAction ResultAction { get; init; }
    [JsonIgnore] public IReadOnlyList<string> Diff { get; init; } = [];
}
