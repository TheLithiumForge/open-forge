using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Library.Sync.Models;

internal sealed record LibrarySyncData
{
    public required string Mode { get; init; }

    public required string? Id { get; init; }

    public required string? SourceFolder { get; init; }

    public required string? DestinationFolder { get; init; }

    public required LibrarySyncDataPermissions Permissions { get; init; }

    public required IReadOnlyList<LibrarySyncDataEffect> Effects { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Unchanged { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LibrarySyncDataInventory? Inventory { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<LibrarySyncDataExpectedState>? ExpectedStates { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LibrarySyncDataVerification? Verification { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LibrarySyncDataRecovery? Recovery { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<LibrarySyncDataTextRow> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextSummaryLines { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetailLines { get; init; } = [];
}

internal sealed record LibrarySyncDataPermissions
{
    public required IReadOnlyList<string> Required { get; init; }

    public required IReadOnlyList<string> Missing { get; init; }

    public required IReadOnlyList<LibrarySyncDataPermissionScope> ProposedScopes { get; init; }

    public required IReadOnlyList<LibrarySyncDataPermissionScope> ApprovedScopes { get; init; }

    public required string Decision { get; init; }

    public required string Action { get; init; }

    public required string Outcome { get; init; }
}

internal sealed record LibrarySyncDataPermissionScope
{
    public required string Kind { get; init; }

    public required string Path { get; init; }
}

internal sealed record LibrarySyncDataEffect
{
    public required string Path { get; init; }

    public required string Action { get; init; }

    public required string Outcome { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Target { get; init; }

    [JsonIgnore]
    internal bool IsLink { get; init; }

    [JsonIgnore]
    internal bool IsSection { get; init; }

    [JsonIgnore]
    internal bool IsRecord { get; init; }

    [JsonIgnore]
    internal string? Suffix { get; init; }
}

internal sealed record LibrarySyncDataInventory
{
    public required string RootState { get; init; }

    public required string InventoryState { get; init; }

    public required IReadOnlyList<LibrarySyncDataInventoryPath> EligiblePaths { get; init; }

    public required IReadOnlyList<LibrarySyncDataExcludedPath> ExcludedPaths { get; init; }

    public required IReadOnlyList<LibrarySyncDataUnavailablePath> UnavailablePaths { get; init; }

    public required string? LexicalRoot { get; init; }

    public required string? PhysicalRoot { get; init; }

    public required bool? LexicallyContained { get; init; }

    public required bool? PhysicallyContained { get; init; }
}

internal sealed record LibrarySyncDataInventoryPath
{
    public required string SourcePath { get; init; }

    public required string DestinationPath { get; init; }

    public required string? SourceId { get; init; }
}

internal sealed record LibrarySyncDataExcludedPath
{
    public required string Path { get; init; }

    public required string Reason { get; init; }
}

internal sealed record LibrarySyncDataUnavailablePath
{
    public required string Path { get; init; }

    public required string Cause { get; init; }
}

internal sealed record LibrarySyncDataExpectedState
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required long? Length { get; init; }

    public required string? Sha256 { get; init; }

    public required string? Target { get; init; }
}

internal sealed record LibrarySyncDataVerification
{
    public required string State { get; init; }

    public required string RecordPublication { get; init; }

    public required bool? PublishedLast { get; init; }
}

internal sealed record LibrarySyncDataRecovery
{
    public required string State { get; init; }

    public required string? Path { get; init; }

    public required IReadOnlyList<LibrarySyncDataResidual> Residuals { get; init; }
}

internal sealed record LibrarySyncDataResidual
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string State { get; init; }
}

internal sealed record LibrarySyncDataTextRow
{
    public required string Path { get; init; }

    public required string Label { get; init; }

    public string? Suffix { get; init; }

    public string? Target { get; init; }

    public bool IsSection { get; init; }

    public bool IsRecord { get; init; }

    [JsonIgnore]
    internal bool IsStandalone { get; init; }
}
