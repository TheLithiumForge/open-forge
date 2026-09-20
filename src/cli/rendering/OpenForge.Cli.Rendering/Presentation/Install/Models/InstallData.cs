using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Install.Models;

internal sealed record InstallData
{
    public required string Mode { get; init; }

    public required bool Force { get; init; }

    public required bool Automatic { get; init; }

    public string? Classification { get; init; }

    public InstallDataFootprint? Footprint { get; init; }

    public required string LockPath { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<InstallDataEffect>? Effects { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<InstallDataEffect> TextEffects { get; init; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InstallDataSource? Source { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InstallDataLifecycle? Lifecycle { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Verification { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<InstallDataTextRow> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextSummaryLines { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetailLines { get; init; } = [];

    [JsonIgnore]
    internal bool ShowNoChanges { get; init; }

    [JsonIgnore]
    internal bool SuppressInterruptedFinding { get; init; }

    [JsonIgnore]
    internal int CreatedFiles { get; init; }

    [JsonIgnore]
    internal int CreatedDirectories { get; init; }

    [JsonIgnore]
    internal int AddedSections { get; init; }

    [JsonIgnore]
    internal int ReplacedFiles { get; init; }

    [JsonIgnore]
    internal int CompletedChanges { get; init; }

    [JsonIgnore]
    internal int TotalChanges { get; init; }

    [JsonIgnore]
    internal CliSemanticStatus Status { get; init; }

    [JsonIgnore]
    internal InstallMode ResultMode { get; init; }

    [JsonIgnore]
    internal bool IsPreview { get; init; }

    [JsonIgnore]
    internal InstallResultRecoveryState RecoveryState { get; init; }

    [JsonIgnore]
    internal string? RecoveryPath { get; init; }

    [JsonIgnore]
    internal InstallResultVerificationState VerificationState { get; init; }

    [JsonIgnore]
    internal bool IsNoOp { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<string> BlockedPaths { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<InstallDataTextFindingIdentity> BlockedFindingIdentities { get; init; } = [];
}

internal sealed record InstallDataTextRow(string Path, string Wording);
internal sealed record InstallDataTextFindingIdentity(InstallFindingCode Code, string Path);

internal sealed record InstallDataFootprint
{
    public required int Files { get; init; }

    public required int? Directories { get; init; }

    public required int Sections { get; init; }
}

internal sealed record InstallDataSource
{
    public required string InventoryFingerprint { get; init; }

    public required int AssetCount { get; init; }
}

internal sealed record InstallDataEffect
{
    public required string Path { get; init; }

    [JsonIgnore]
    public string Kind { get; init; } = string.Empty;

    [JsonIgnore]
    public string Action { get; init; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SourceAssetPath { get; init; }

    [JsonIgnore]
    public string Outcome { get; init; } = string.Empty;

    [JsonIgnore]
    public string Residual { get; init; } = string.Empty;

    [JsonIgnore]
    internal InstallEffectKind ResultKind { get; init; }

    [JsonIgnore]
    internal InstallEffectAction ResultAction { get; init; }

    [JsonIgnore]
    internal InstallEffectOutcome ResultOutcome { get; init; }
}

internal sealed record InstallDataLifecycle
{
    public required string Action { get; init; }

    public required string Outcome { get; init; }
}
