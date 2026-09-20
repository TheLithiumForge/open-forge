using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Update.Models;

internal sealed record UpdateData
{
    public required string Mode { get; init; }

    public required bool Force { get; init; }

    public required bool Prune { get; init; }

    public required bool Automatic { get; init; }

    public string? PreviousContent { get; init; }

    public required string LockPath { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<UpdateDataPath>? Unchanged { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<UpdateDataEntrySection>? EntriesSections { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<UpdateDataEffect>? Effects { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Verification { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<UpdateDataTextRow> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextSummaryLines { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetailLines { get; init; } = [];

    [JsonIgnore]
    internal bool ShowNoChanges { get; init; }

    [JsonIgnore]
    internal bool SuppressInterruptedFinding { get; init; }

    [JsonIgnore]
    internal int ChangedFiles { get; init; }

    [JsonIgnore]
    internal int ReplacedFiles { get; init; }

    [JsonIgnore]
    internal int RestoredFiles { get; init; }

    [JsonIgnore]
    internal int CreatedFiles { get; init; }

    [JsonIgnore]
    internal int DeletedFiles { get; init; }

    [JsonIgnore]
    internal int KeptFiles { get; init; }

    [JsonIgnore]
    internal int UnchangedFiles { get; init; }

    [JsonIgnore]
    internal int UpdatedSections { get; init; }

    [JsonIgnore]
    internal int CompletedChanges { get; init; }

    [JsonIgnore]
    internal int TotalChanges { get; init; }

    [JsonIgnore]
    internal CliSemanticStatus Status { get; init; }

    [JsonIgnore]
    internal UpdateMode ResultMode { get; init; }

    [JsonIgnore]
    internal UpdateRecoveryState RecoveryState { get; init; }

    [JsonIgnore]
    internal string? RecoveryPath { get; init; }

    [JsonIgnore]
    internal UpdateVerificationState VerificationState { get; init; }

    [JsonIgnore]
    internal bool IsNoOp { get; init; }
}

internal sealed record UpdateDataPath
{
    public required string Path { get; init; }
}

internal sealed record UpdateDataEntrySection
{
    public required string Path { get; init; }

    public required string State { get; init; }
}

internal sealed record UpdateDataEffect
{
    public required string Path { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Before { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? After { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SourceAssetPath { get; init; }

    public required UpdateDataRelation Relation { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UpdateDataSource? Source { get; init; }

    public required string Verification { get; init; }

    [JsonIgnore]
    internal UpdatePhysicalEffectAction ResultAction { get; init; }

    [JsonIgnore]
    internal UpdatePhysicalEffectOutcome ResultOutcome { get; init; }

    [JsonIgnore]
    internal UpdatePhysicalEffectResidual ResultResidual { get; init; }

    [JsonIgnore]
    internal bool IsRestore { get; init; }

    [JsonIgnore]
    internal bool IsSection { get; init; }

    [JsonIgnore]
    internal string? Reason { get; init; }
}

internal sealed record UpdateDataRelation
{
    public required string Current { get; init; }

    public required string Shipped { get; init; }
}

internal sealed record UpdateDataSource
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string Fingerprint { get; init; }
}

internal sealed record UpdateDataTextRow(string Path, string Wording);
