using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;

namespace OpenForge.Cli.Core.Presentation.Repair.Models;

internal sealed record RepairData
{
    public required string Mode { get; init; }

    public required string Selection { get; init; }

    public required IReadOnlyList<RepairDataRepair> Repairs { get; init; }

    public required IReadOnlyList<RepairDataRemaining> Remaining { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<RepairDataLibraryRecovery>? LibraryRecovery { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RepairDataDiagnosis? Diagnosis { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RepairDataVerification? Verification { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<RepairDataTextRow> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetailLines { get; init; } = [];

    [JsonIgnore]
    internal bool ShowNoChanges { get; init; }
}

internal sealed record RepairDataRepair
{
    public required string Path { get; init; }

    public required RepairDataLocation Location { get; init; }

    public required string From { get; init; }

    public required string To { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Before { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? After { get; init; }

    [JsonIgnore]
    internal RepairStepOutcome Outcome { get; init; }
}

[JsonConverter(typeof(OpenForge.Cli.Core.Presentation.Repair.Shared.Rendering.RepairDataCandidatesConverter))]
internal sealed class RepairDataCandidates
{
    private RepairDataCandidates(
        RepairDataCandidatesShape shape,
        IReadOnlyList<RepairDataCandidate> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Shape = shape;
        Items = items;
    }

    internal RepairDataCandidatesShape Shape { get; }

    internal IReadOnlyList<RepairDataCandidate> Items { get; }

    internal static RepairDataCandidates Count(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return new(RepairDataCandidatesShape.Count, new RepairDataCandidate[count]);
    }

    internal static RepairDataCandidates Paths(IReadOnlyList<RepairDataCandidate> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return new(RepairDataCandidatesShape.Paths, items);
    }

    internal static RepairDataCandidates Reasons(IReadOnlyList<RepairDataCandidate> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return new(RepairDataCandidatesShape.Reasons, items);
    }

    internal int ItemCount => Items.Count;
}

internal enum RepairDataCandidatesShape
{
    Count,
    Paths,
    Reasons,
}

internal sealed record RepairDataCandidate
{
    public required string Path { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Reasons { get; init; }
}

internal sealed record RepairDataRemaining
{
    public required string Path { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RepairDataLocation? Location { get; init; }

    public required string Kind { get; init; }

    public required RepairDataCandidates Candidates { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<RepairDataCandidate> TextCandidates { get; init; } = [];
}

internal sealed record RepairDataLocation(int Line, int Column);

internal sealed record RepairDataLibraryRecovery
{
    public required string Id { get; init; }

    public required string Path { get; init; }

    public required bool Selected { get; init; }

    [JsonIgnore]
    internal RepairStepOutcome Outcome { get; init; }
}

internal sealed record RepairDataDiagnosis
{
    public required string WorkspaceAndPath { get; init; }

    public required string RouteAndHeading { get; init; }

    public required string LocalReferences { get; init; }

    public required string SelectedScope { get; init; }
}

internal sealed record RepairDataVerification
{
    public required string Targets { get; init; }

    public required string ResultingBytes { get; init; }

    public required string PostConditions { get; init; }
}

internal sealed record RepairDataTextRow(string Path, string Wording);
