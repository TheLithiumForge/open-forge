using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Route.Update.Models;

internal sealed record RouteUpdateData
{
    public required string Mode { get; init; }

    public required RouteUpdateDataTarget Target { get; init; }

    public required IReadOnlyList<RouteUpdateDataChange> Changes { get; init; }

    public RouteUpdateDataTemplate? Template { get; init; }

    public string? ListedIn { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FrontmatterBefore { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FrontmatterAfter { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<string> TextMetadata { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<RouteUpdateDataEffect> TextEffects { get; init; } = [];

    [JsonIgnore]
    internal bool ShowNoChanges { get; init; }
}

internal sealed record RouteUpdateDataTarget
{
    public string? Id { get; init; }

    public string? Path { get; init; }
}

internal sealed record RouteUpdateDataChange
{
    public required string Field { get; init; }

    public string? Before { get; init; }

    public string? After { get; init; }
}

internal sealed record RouteUpdateDataTemplate
{
    public required string Id { get; init; }

    public required string Path { get; init; }

    public required bool Applied { get; init; }
}

internal sealed record RouteUpdateDataEffect
{
    public required string Path { get; init; }

    public required string Action { get; init; }

    public required string Outcome { get; init; }

    public string? Before { get; init; }

    public string? After { get; init; }
}
