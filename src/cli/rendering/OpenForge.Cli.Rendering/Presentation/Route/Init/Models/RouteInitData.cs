using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Route.Init.Models;

internal sealed record RouteInitData
{
    public required string Mode { get; init; }

    public required RouteInitDataTarget Target { get; init; }

    public required string Scaffold { get; init; }

    public required IReadOnlyList<RouteInitDataEntrypoint> Entrypoints { get; init; }

    public required IReadOnlyList<string> ListedIn { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RouteInitDataMetadata? Metadata { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LockPath { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<RouteInitDataSection>? Sections { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FrameworkFingerprint { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Verification { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<string> TextMetadata { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<RouteInitDataEntrypoint> TextEntrypoints { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<RouteInitDataSection> TextSections { get; init; } = [];

    [JsonIgnore]
    internal string? TextAdvisory { get; init; }

    [JsonIgnore]
    internal string? TextRecovery { get; init; }
}

internal sealed record RouteInitDataTarget
{
    public string? Id { get; init; }

    public string? Path { get; init; }
}

internal sealed record RouteInitDataEntrypoint
{
    public required string Path { get; init; }

    public required string Outcome { get; init; }

    public required bool NeedsAuthoring { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; init; }
}

internal sealed record RouteInitDataMetadata
{
    public required string Description { get; init; }

    public string? Responsibility { get; init; }

    public required IReadOnlyList<string> Tags { get; init; }

    public required RouteInitDataMetadataSources Sources { get; init; }
}

internal sealed record RouteInitDataMetadataSources
{
    public required string Description { get; init; }

    public required string Responsibility { get; init; }

    public required string Tags { get; init; }
}

internal sealed record RouteInitDataSection
{
    public required string Path { get; init; }

    public required string Before { get; init; }

    public required string After { get; init; }

    public required string Verification { get; init; }

    [JsonIgnore]
    internal string? TextBefore { get; init; }

    [JsonIgnore]
    internal string? TextAfter { get; init; }
}
