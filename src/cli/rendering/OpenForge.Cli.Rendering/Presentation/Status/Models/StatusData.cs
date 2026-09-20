using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Presentation.Status.Shared.Rendering;

namespace OpenForge.Cli.Core.Presentation.Status.Models;

internal sealed record StatusData
{
    public required StatusDataInstallation Installation { get; init; }

    public required StatusDataContext Context { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StatusDataStructure? Structure { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<StatusDataFrameworkFile>? FrameworkFiles { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<StatusDataEntrySection>? EntriesSections { get; init; }

    public required IReadOnlyList<StatusDataExtension> Extensions { get; init; }

    public required IReadOnlyList<StatusDataLibrary> Libraries { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StatusDataRecovery? Recovery { get; init; }

    [JsonIgnore]
    internal StatusDataTextFacts TextFacts { get; init; } = new();
}

internal sealed record StatusDataTextFacts
{
    public bool ShowOperationalSections { get; init; }
    public bool ShowStandard { get; init; }
    public bool ShowFull { get; init; }
    public bool ShowDifference { get; init; }
    public bool ShowMayLoadAgain { get; init; }
    public StatusIntegerValue EntriesCurrent { get; init; } = new(StatusValueState.NotApplicable, null);
    public StatusIntegerValue EntriesStale { get; init; } = new(StatusValueState.NotApplicable, null);
    public StatusIntegerValue EntriesMissing { get; init; } = new(StatusValueState.NotApplicable, null);
    public StatusIntegerValue FrameworkCurrent { get; init; } = new(StatusValueState.NotApplicable, null);
    public StatusIntegerValue FrameworkChanged { get; init; } = new(StatusValueState.NotApplicable, null);
    public StatusIntegerValue FrameworkMissing { get; init; } = new(StatusValueState.NotApplicable, null);
}

internal sealed record StatusDataInstallation
{
    public required string State { get; init; }

    [JsonIgnore]
    internal StatusInstallationState InstallationState { get; init; }

    public string? EntryPath { get; init; }

    public string? LoaderPath { get; init; }
}

internal sealed record StatusDataContext
{
    public required StatusDataStartup Startup { get; init; }

    public required StatusDataMeasurement AllRouted { get; init; }

    public decimal? StartupShare { get; init; }

    [JsonIgnore]
    internal StatusValueState StartupShareState { get; init; } = StatusValueState.NotApplicable;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<StatusDataContextSource>? MayLoadAgainSources { get; init; }
}

internal sealed record StatusDataStartup
{
    public required StatusDataMeasurement Shipped { get; init; }

    public required StatusDataMeasurement Current { get; init; }

    public required StatusDataMeasurement Difference { get; init; }

    public required StatusDataMeasurement MayLoadAgain { get; init; }
}

internal sealed record StatusDataMeasurement
{
    public long? Files { get; init; }
    [JsonIgnore]
    internal StatusValueState FilesState { get; init; } = StatusValueState.NotApplicable;

    public long? Characters { get; init; }
    [JsonIgnore]
    internal StatusValueState CharactersState { get; init; } = StatusValueState.NotApplicable;

    public long? Bytes { get; init; }
    [JsonIgnore]
    internal StatusValueState BytesState { get; init; } = StatusValueState.NotApplicable;

    public long? Tokens { get; init; }
    [JsonIgnore]
    internal StatusValueState TokensState { get; init; } = StatusValueState.NotApplicable;
}

internal sealed record StatusDataContextSource
{
    public required string SourceId { get; init; }

    public required long Bytes { get; init; }

    public required IReadOnlyList<StatusDataContextLayer> Layers { get; init; }
}

internal sealed record StatusDataContextLayer
{
    public required string Path { get; init; }

    public required long Bytes { get; init; }
}

internal sealed record StatusDataStructure
{
    public required StatusDataRootCategories RootCategories { get; init; }
}

internal sealed record StatusDataRootCategories
{
    public long? Count { get; init; }
    [JsonIgnore]
    internal StatusValueState CountState { get; init; } = StatusValueState.NotApplicable;

    public required IReadOnlyList<string> Added { get; init; }

    public required IReadOnlyList<string> Removed { get; init; }
}

internal sealed record StatusDataFrameworkFile
{
    public required string Path { get; init; }

    public required string State { get; init; }

    [JsonIgnore]
    internal StatusTargetState TargetState { get; init; }
}

internal sealed record StatusDataEntrySection
{
    public required string Path { get; init; }

    public required string State { get; init; }

    [JsonIgnore]
    internal StatusGeneratedNavigationState NavigationState { get; init; }
}

[JsonConverter(typeof(StatusDataExtensionJsonConverter))]
internal sealed record StatusDataExtension
{
    public required string Id { get; init; }

    public string? Version { get; init; }

    [JsonIgnore]
    public StatusDataManagedFileCounts? Files { get; init; }

    [JsonIgnore]
    public IReadOnlyList<StatusDataManagedFile>? FileRows { get; init; }

    [JsonIgnore]
    internal long? TextCurrentFiles { get; init; }

    [JsonIgnore]
    internal StatusValueState FilesState { get; init; } = StatusValueState.NotApplicable;
}

internal sealed record StatusDataManagedFileCounts
{
    public long? Current { get; init; }

    public long? Changed { get; init; }

    public long? Missing { get; init; }
}

internal sealed record StatusDataManagedFile
{
    public required string Path { get; init; }

    public required string State { get; init; }

    [JsonIgnore]
    internal StatusTargetState TargetState { get; init; }
}

[JsonConverter(typeof(StatusDataLibraryJsonConverter))]
internal sealed record StatusDataLibrary
{
    public required string Id { get; init; }

    public required string SourceRoot { get; init; }

    public required string DestinationRoot { get; init; }

    [JsonIgnore]
    public StatusDataLinkCounts? Links { get; init; }

    [JsonIgnore]
    public IReadOnlyList<StatusDataLibraryLink>? LinkRows { get; init; }

    [JsonIgnore]
    internal long? TextCurrentLinks { get; init; }

    [JsonIgnore]
    internal StatusValueState LinksState { get; init; } = StatusValueState.NotApplicable;
}

internal sealed record StatusDataLinkCounts
{
    public long? Current { get; init; }

    public long? Missing { get; init; }

    public long? Changed { get; init; }
}

internal sealed record StatusDataLibraryLink
{
    public required string Path { get; init; }

    public required string State { get; init; }

    [JsonIgnore]
    internal StatusTargetState TargetState { get; init; }

    public required string ExpectedTarget { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ObservedTarget { get; init; }
}

internal sealed record StatusDataRecovery
{
    public required IReadOnlyList<StatusDataRecoveryCandidate> Candidates { get; init; }
}

internal sealed record StatusDataRecoveryCandidate
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    [JsonIgnore]
    internal StatusRecoveryCandidateKind CandidateKind { get; init; }

    public required string Integrity { get; init; }

    [JsonIgnore]
    internal StatusRecoveryIntegrity IntegrityState { get; init; }
}
