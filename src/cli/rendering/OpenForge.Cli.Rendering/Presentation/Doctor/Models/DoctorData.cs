using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Rendering;

namespace OpenForge.Cli.Core.Presentation.Doctor.Models;

internal sealed record DoctorData
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName(DoctorDataJsonFields.RecoveryStores)]
    public IReadOnlyList<DoctorDataRecoveryStore>? RecoveryStores { get; init; }

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

    [JsonIgnore]
    internal string? NextLine { get; init; }
}

internal sealed record DoctorDataCategory
{
    public required string Name { get; init; }

    [JsonPropertyName(DoctorDataJsonFields.Coverage)]
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

internal sealed record DoctorDataRecoveryStore(
    [property: JsonPropertyName(DoctorDataJsonFields.Kind)] string Kind,
    [property: JsonPropertyName(DoctorDataJsonFields.Path)] string? Path,
    [property: JsonPropertyName(DoctorDataJsonFields.Coverage)] string Coverage,
    [property: JsonPropertyName(DoctorDataJsonFields.Candidates)] IReadOnlyList<DoctorDataRecoveryCandidate> Candidates,
    [property: JsonPropertyName(DoctorDataJsonFields.Cause)] string? Cause);

internal sealed record DoctorDataRecoveryCandidate([property: JsonPropertyName(DoctorDataJsonFields.Path)] string Path,
    [property: JsonPropertyName(DoctorDataJsonFields.Kind)] string Kind,
    [property: JsonPropertyName(DoctorDataJsonFields.Integrity)] string Integrity,
    [property: JsonPropertyName(DoctorDataJsonFields.Cause)] string? Cause);
