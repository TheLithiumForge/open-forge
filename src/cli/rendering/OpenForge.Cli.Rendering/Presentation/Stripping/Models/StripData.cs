using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Stripping.Models;

// The plain-docs Interface contract's Results section defines these fields for uninstall and export.
internal sealed record StripData
{
    public required string Mode { get; init; }

    public required IReadOnlyList<string> Strip { get; init; }

    public string? Flatten { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Destination { get; init; }

    public required IReadOnlyList<StripDataPlanEntry> Plan { get; init; }

    public required IReadOnlyList<StripDataKept> Kept { get; init; }

    public required IReadOnlyList<StripDataCollision> Collisions { get; init; }

    public required bool Verified { get; init; }
}

internal sealed record StripDataPlanEntry
{
    public required string Action { get; init; }

    public required string Path { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? To { get; init; }

    public required string Reason { get; init; }

    public required IReadOnlyList<string> Parts { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Before { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? After { get; init; }

    public required string Outcome { get; init; }
}

internal sealed record StripDataKept(string Path, string Reason);

internal sealed record StripDataCollision(string Path, IReadOnlyList<string> Sources);
