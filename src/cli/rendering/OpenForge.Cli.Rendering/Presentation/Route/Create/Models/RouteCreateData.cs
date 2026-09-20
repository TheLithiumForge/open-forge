using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Route.Create.Models;

internal sealed record RouteCreateData
{
    public required string Mode { get; init; }

    public required RouteCreateDataTarget Target { get; init; }

    public string? ListedIn { get; init; }

    public RouteCreateDataTemplate? Template { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RouteCreateDataMetadata? Metadata { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<RouteCreateDataSection>? Sections { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<string> TextMetadata { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextNextLines { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<RouteCreateDataSection> TextSections { get; init; } = [];
}

internal sealed record RouteCreateDataTarget
{
    public string? Id { get; init; }

    public string? Path { get; init; }
}

internal sealed record RouteCreateDataTemplate
{
    public required string Id { get; init; }

    public required string Path { get; init; }
}

internal sealed record RouteCreateDataMetadata
{
    public required string? Description { get; init; }

    public string? Responsibility { get; init; }

    public required IReadOnlyList<string> Tags { get; init; }
}

internal sealed record RouteCreateDataSection
{
    public required string Path { get; init; }

    public required string Before { get; init; }

    public required string After { get; init; }

    public required string Verification { get; init; }
}
