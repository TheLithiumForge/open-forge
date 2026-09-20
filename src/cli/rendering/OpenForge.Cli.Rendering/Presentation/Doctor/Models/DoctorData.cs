using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Doctor.Models;

internal sealed record DoctorData
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<DoctorDataCategory>? Categories { get; init; }

    [JsonIgnore]
    internal string? ChecksLine { get; init; }

    [JsonIgnore]
    internal string? CoverageLine { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<string> IncompleteLines { get; init; } = [];

    [JsonIgnore]
    internal string? LaneLine { get; init; }

    [JsonIgnore]
    internal string? HintLine { get; init; }
}

internal sealed record DoctorDataCategory
{
    public required string Name { get; init; }

    public required string Coverage { get; init; }

    public required DoctorDataCounts Counts { get; init; }

    public required IReadOnlyList<string> Limitations { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DoctorDataLanes? Lanes { get; init; }
}

internal sealed record DoctorDataCounts
{
    public required long Errors { get; init; }

    public required long Warnings { get; init; }

    public required long Infos { get; init; }
}

internal sealed record DoctorDataLanes
{
    public required long SafeExact { get; init; }

    public required long GuidedChoice { get; init; }

    public required long TargetedOperation { get; init; }

    public required long ManualDecision { get; init; }

    public required long BlockedRepair { get; init; }
}
