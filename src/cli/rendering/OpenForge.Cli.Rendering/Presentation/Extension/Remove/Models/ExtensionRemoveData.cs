using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Extension.Remove.Models;

internal sealed record ExtensionRemoveData
{
    public required string Mode { get; init; }
    public required bool Automatic { get; init; }
    public required IReadOnlyList<ExtensionRemoveDataPackage> Packages { get; init; }
    public required IReadOnlyList<string> Orphaned { get; init; }
    public required ExtensionRemoveDataPermissions Permissions { get; init; }
    public required IReadOnlyList<ExtensionRemoveDataEffect> Effects { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? RemovalOrder { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? EntriesUnchanged { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExtensionRemoveDataVerification? Verification { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExtensionRemoveDataRecovery? Recovery { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<ExtensionRemoveDataTextRow> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetails { get; init; } = [];
}

internal sealed record ExtensionRemoveDataPackage
{
    public required string Id { get; init; }
}

internal sealed record ExtensionRemoveDataPermissions
{
    public required IReadOnlyList<string> Required { get; init; }
    public required IReadOnlyList<string> Missing { get; init; }
    public required string Decision { get; init; }
    public required string Action { get; init; }
    public required string Outcome { get; init; }
}

internal sealed record ExtensionRemoveDataEffect
{
    public required string Path { get; init; }
    public required string Action { get; init; }
    public required string Outcome { get; init; }
    public required string? Owner { get; init; }
    public required IReadOnlyList<string> KeptFor { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? OwnersBefore { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? OwnersAfter { get; init; }

    [JsonIgnore]
    internal string Text { get; init; } = string.Empty;

    [JsonIgnore]
    internal string? Detail { get; init; }
}

internal sealed record ExtensionRemoveDataVerification
{
    public required string Targets { get; init; }
    public required string Entries { get; init; }
    public required string ExtensionRecord { get; init; }
}

internal sealed record ExtensionRemoveDataRecovery
{
    public required string State { get; init; }
    public required IReadOnlyList<string> ProtectedPaths { get; init; }
    public required string? Path { get; init; }
}

internal sealed record ExtensionRemoveDataTextRow(string Path, string Text, string? Detail = null);
