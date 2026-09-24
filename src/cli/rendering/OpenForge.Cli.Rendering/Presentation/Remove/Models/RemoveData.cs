using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Remove.Models;

internal sealed record RemoveData
{
    public required string Kind { get; init; }

    public required string Target { get; init; }

    public required string Mode { get; init; }

    public required IReadOnlyList<string> Removed { get; init; }

    public required IReadOnlyList<RemoveDataEffect> Effects { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RecoveryPath { get; init; }

    public required string RecoveryDisposition { get; init; }

    [JsonIgnore]
    internal bool ShowEffects { get; init; }
}

internal sealed record RemoveDataEffect
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Action { get; init; }

    public required string Outcome { get; init; }
}
