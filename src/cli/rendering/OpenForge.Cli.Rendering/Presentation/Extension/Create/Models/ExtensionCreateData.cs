using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Extension.Create.Models;

internal sealed record ExtensionCreateData
{
    public required string Mode { get; init; }

    public string? Id { get; init; }

    public string? Folder { get; init; }

    public string? PackagePath { get; init; }

    public string? ManifestPath { get; init; }

    public string? ContentPath { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExtensionCreateDataManifest? Manifest { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ManifestContent { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<ExtensionCreateDataTextRow> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextManifest { get; init; } = [];

    [JsonIgnore]
    internal string? TextEditInstruction { get; init; }

    [JsonIgnore]
    internal bool ShowNoChanges { get; init; }
}

internal sealed record ExtensionCreateDataManifest
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required IReadOnlyList<string> Dependencies { get; init; }
}

internal sealed record ExtensionCreateDataManifestContent
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required IReadOnlyList<string> Dependencies { get; init; }
}

internal sealed record ExtensionCreateDataTextRow(string Path, string Wording);
