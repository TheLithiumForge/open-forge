using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Extension.Inspect.Models;

internal sealed record ExtensionInspectData
{
    public required string Id { get; init; }

    public required ExtensionInspectDataVersion? Installed { get; init; }

    public required ExtensionInspectDataVersion? Available { get; init; }

    public required ExtensionInspectDataSource Source { get; init; }

    public required bool Matches { get; init; }

    public required IReadOnlyList<ExtensionInspectDataFile> Files { get; init; }

    public required IReadOnlyList<ExtensionInspectDataDependency> Dependencies { get; init; }

    public required string? Name { get; init; }

    public required string? Description { get; init; }

    public required string? ManifestPath { get; init; }

    public required IReadOnlyList<string> ResolutionOrder { get; init; }

    public required IReadOnlyList<string> RegisteredIn { get; init; }

    public required string? RecordCoverage { get; init; }

    [JsonIgnore]
    internal bool IncludeStandard { get; init; }

    [JsonIgnore]
    internal bool IncludeFull { get; init; }

    [JsonIgnore]
    internal string? TextSourceLine { get; init; }

    [JsonIgnore]
    internal string? TextSummaryLine { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<ExtensionInspectDataFile> TextFiles { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<IReadOnlyList<string>> TextDependencies { get; init; } = [];

    [JsonIgnore]
    internal string? TextManifestLine { get; init; }

    [JsonIgnore]
    internal string? TextResolutionLine { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<string> TextRegisteredIn { get; init; } = [];

    [JsonIgnore]
    internal string? TextCoverageLine { get; init; }
}

internal sealed record ExtensionInspectDataVersion
{
    public required string? Version { get; init; }
}

internal sealed record ExtensionInspectDataSource
{
    public required string? Kind { get; init; }

    public required string? Path { get; init; }

    [JsonIgnore]
    internal string TextPath { get; init; } = "unavailable";
}

internal sealed record ExtensionInspectDataFile
{
    public required string Path { get; init; }

    public required string Relation { get; init; }

    public string? InstalledSha256 { get; init; }

    public string? PackageSha256 { get; init; }

    [JsonIgnore]
    internal ExtensionInspectDataFileText Text { get; init; } = new();
}

internal sealed record ExtensionInspectDataFileText
{
    public string Severity { get; init; } = "Info";

    public string Message { get; init; } = "";
}

internal sealed record ExtensionInspectDataDependency
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string State { get; init; }
}
