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
    [JsonIgnore] internal IReadOnlyList<IndexDataRegion> TextRegions { get; init; } = [];
    [JsonIgnore] internal IndexRecovery Recovery { get; init; } = IndexRecovery.NotRequired;
    [JsonIgnore] public bool NothingWritten { get; init; }
    [JsonIgnore] public int CurrentCount { get; init; }
}

internal sealed record IndexDataChange(string Path, int? Before, int? After)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Diff { get; init; }
}
internal sealed record IndexDataPath(string Path);
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
