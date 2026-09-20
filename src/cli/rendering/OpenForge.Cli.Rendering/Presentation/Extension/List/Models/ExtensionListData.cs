using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Extension.List.Models;

internal sealed record ExtensionListData
{
    public ExtensionListDataSource? Source { get; init; }

    public required IReadOnlyList<ExtensionListDataInstalled> Installed { get; init; }

    public required IReadOnlyList<ExtensionListDataAvailable> Available { get; init; }

    [JsonIgnore]
    internal string? TextSourceLine { get; init; }

    [JsonIgnore]
    internal string InstalledHeading { get; init; } = global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleInstalled();

    [JsonIgnore]
    internal string? InstalledEmptyLine { get; init; }

    [JsonIgnore]
    internal string AvailableHeading { get; init; } = global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleAvailable();

    [JsonIgnore]
    internal string? AvailableEmptyLine { get; init; }

    [JsonIgnore]
    internal bool ShowInstalled { get; init; }

    [JsonIgnore]
    internal bool ShowAvailable { get; init; }

    [JsonIgnore]
    internal bool ShowSource { get; init; }

    [JsonIgnore]
    internal bool ShowInstalledDetails { get; init; }

    [JsonIgnore]
    internal bool ShowAvailableDependencies { get; init; }
}

internal sealed record ExtensionListDataSource(string? Kind, string Path);

internal sealed record ExtensionListDataInstalled
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public string? Note { get; init; }

    [JsonIgnore]
    internal ExtensionListDataInstalledFull? Full { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<string> TextCells { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetails { get; init; } = [];
}

internal sealed record ExtensionListDataInstalledFull
{
    public required int Files { get; init; }

    public required string? RecordedSource { get; init; }

    public required string Coverage { get; init; }
}

internal sealed record ExtensionListDataAvailable
{
    public required string Id { get; init; }

    public required string Version { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required int Packages { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Dependencies { get; init; }

    [JsonIgnore]
    public string? Installed { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<string> TextCells { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetails { get; init; } = [];
}
