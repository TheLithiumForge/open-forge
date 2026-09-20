using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Extension.Update.Models;

internal sealed record ExtensionUpdateData
{
    public required string Mode { get; init; }
    public required bool Force { get; init; }
    public required bool Prune { get; init; }
    public required bool Automatic { get; init; }
    public required ExtensionUpdateDataSource? Source { get; init; }
    public required IReadOnlyList<ExtensionUpdateDataPackage> Packages { get; init; }
    public required string? PreviousContent { get; init; }
    public required ExtensionUpdateDataPermissions Permissions { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Unchanged { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Sections { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ExtensionUpdateDataDependency>? Dependencies { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ExtensionUpdateDataEffect>? Effects { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<ExtensionUpdateDataTextRow> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetails { get; init; } = [];
}

internal sealed record ExtensionUpdateDataSource
{
    public required string Kind { get; init; }
    public required string? Path { get; init; }
}

internal sealed record ExtensionUpdateDataPackage
{
    public required string Id { get; init; }
    public required string? From { get; init; }
    public required string? To { get; init; }
}

internal sealed record ExtensionUpdateDataPermissions
{
    public required IReadOnlyList<string> Required { get; init; }
    public required IReadOnlyList<string> Missing { get; init; }
    public required string Decision { get; init; }
    public required string Action { get; init; }
    public required string Outcome { get; init; }
}

internal sealed record ExtensionUpdateDataDependency
{
    public required string Id { get; init; }
    public required IReadOnlyList<string> Requires { get; init; }
}

internal sealed record ExtensionUpdateDataEffect
{
    public required string Path { get; init; }
    public required string? Before { get; init; }
    public required string? After { get; init; }
    public required ExtensionUpdateDataRelation Relation { get; init; }
    public required ExtensionUpdateDataVerification Verification { get; init; }
    public required ExtensionUpdateDataRecovery Recovery { get; init; }
}

internal sealed record ExtensionUpdateDataRelation
{
    public required string Current { get; init; }
    public required string Shipped { get; init; }
}

internal sealed record ExtensionUpdateDataVerification
{
    public required string Targets { get; init; }
    public required string Topology { get; init; }
    public required string ExtensionRecord { get; init; }
}

internal sealed record ExtensionUpdateDataRecovery
{
    public required string State { get; init; }
    public required string? Path { get; init; }
}

internal sealed record ExtensionUpdateDataTextRow(string Path, string Text, string? Detail = null);
