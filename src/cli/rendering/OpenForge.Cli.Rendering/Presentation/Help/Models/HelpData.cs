using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Help.Models;

internal sealed record HelpData
{
    public required string? Topic { get; init; }
    public required string? Markdown { get; init; }
    public required IReadOnlyList<string> Topics { get; init; }
    [JsonIgnore] internal bool Complete { get; init; }
}
