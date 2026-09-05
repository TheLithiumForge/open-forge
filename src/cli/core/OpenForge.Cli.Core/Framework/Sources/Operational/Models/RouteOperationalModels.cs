using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models;

internal sealed record OperationalIntegerObservation(
    OperationalValueState State,
    long? Value);

internal sealed record ContextMeasurementObservation(
    OperationalIntegerObservation Files,
    OperationalIntegerObservation Characters,
    OperationalIntegerObservation Utf8Bytes,
    OperationalIntegerObservation EstimatedTokens);

internal sealed record ContextLayerContributionObservation(
    string Path,
    long Utf8Bytes);

internal sealed record ContextSourceContributionObservation
{
    public required string SourceId { get; init; }

    public required long Utf8Bytes { get; init; }

    public required IReadOnlyList<ContextLayerContributionObservation> Layers { get; init; }
}

internal sealed record GeneratedNavigationTargetObservation(
    string Path,
    OperationalGeneratedNavigationState State);

internal sealed record DoctorGeneratedNavigationContent(
    SourceGeneratedEntriesFacts CurrentEntries,
    IReadOnlyList<GeneratedNavigationEntry> ExpectedEntries,
    IReadOnlyList<RouteGeneratedEntryComparison> EntryComparisons);

internal sealed record DoctorGeneratedNavigationUnavailability(
    GeneratedNavigationRegionUnavailableReason Reason,
    string? Cause);

internal sealed class DoctorGeneratedNavigationTargetObservation
{
    private DoctorGeneratedNavigationTargetObservation(
        string path,
        OperationalGeneratedNavigationState state,
        DoctorGeneratedNavigationContent content,
        DoctorGeneratedNavigationUnavailability? unavailability)
    {
        var isAvailable = state is OperationalGeneratedNavigationState.Current
            or OperationalGeneratedNavigationState.Changed;
        if (isAvailable != (unavailability is null))
        {
            throw new ArgumentException(
                "Generated-navigation availability must match its typed unavailable reason.",
                nameof(unavailability));
        }

        Path = path;
        State = state;
        Content = content;
        Unavailability = unavailability;
    }

    internal string Path { get; }

    internal OperationalGeneratedNavigationState State { get; }

    internal DoctorGeneratedNavigationContent Content { get; }

    internal DoctorGeneratedNavigationUnavailability? Unavailability { get; }

    internal SourceGeneratedEntriesFacts CurrentEntries => Content.CurrentEntries;

    internal IReadOnlyList<GeneratedNavigationEntry> ExpectedEntries => Content.ExpectedEntries;

    internal GeneratedNavigationRegionUnavailableReason? UnavailableReason =>
        Unavailability?.Reason;

    internal string? Cause => Unavailability?.Cause;

    internal static DoctorGeneratedNavigationTargetObservation Available(
        string path,
        OperationalGeneratedNavigationState state,
        DoctorGeneratedNavigationContent content)
    {
        if (state is not (OperationalGeneratedNavigationState.Current
            or OperationalGeneratedNavigationState.Changed))
        {
            throw new ArgumentException(
                "An available generated-navigation target must be current or changed.",
                nameof(state));
        }

        return new(path, state, content, unavailability: null);
    }

    internal static DoctorGeneratedNavigationTargetObservation Unavailable(
        string path,
        OperationalGeneratedNavigationState state,
        DoctorGeneratedNavigationContent content,
        DoctorGeneratedNavigationUnavailability unavailability)
    {
        if (state is OperationalGeneratedNavigationState.Current
            or OperationalGeneratedNavigationState.Changed)
        {
            throw new ArgumentException(
                "An unavailable generated-navigation target cannot be current or changed.",
                nameof(state));
        }

        return new(path, state, content, unavailability);
    }
}

internal enum RouteSourceInventoryState
{
    SafelyAbsent,
    Present,
    Incomplete,
    Blocked,
    Interrupted,
}

internal sealed record RouteMetadataObservation(
    string Path,
    FrameworkDocumentMetadataFacts Facts);

internal sealed record RouteStatusView
{
    public required OperationalViewState State { get; init; }

    public required RouteSourceInventoryState SourceInventory { get; init; }

    public required ContextMeasurementObservation InitialStartup { get; init; }

    public required ContextMeasurementObservation CurrentStartup { get; init; }

    public required ContextMeasurementObservation TotalAvailable { get; init; }

    public required ContextMeasurementObservation Continuity { get; init; }

    public required IReadOnlyList<ContextSourceContributionObservation> ContinuitySources { get; init; }

    public required IReadOnlyList<string> InitialRootCategories { get; init; }

    public required IReadOnlyList<string> CurrentRootCategories { get; init; }

    public required IReadOnlyList<GeneratedNavigationTargetObservation> GeneratedNavigation { get; init; }
}

internal sealed record RouteDoctorView
{
    public required OperationalViewState State { get; init; }

    public required RouteSourceInventoryState SourceInventory { get; init; }

    public required SourceCatalogue Catalogue { get; init; }

    public required SourceRouteFacts Routes { get; init; }

    public required IReadOnlyList<RouteMetadataObservation> Metadata { get; init; }

    public required RouteSourceLayerObservation WorkspaceEntry { get; init; }

    public required IReadOnlyList<RouteSourceObservation> Sources { get; init; }

    public required IReadOnlyList<DoctorGeneratedNavigationTargetObservation> GeneratedNavigation { get; init; }

    public required IReadOnlyList<RouteDeclaredRootObservation> DeclaredRoots { get; init; }

    public required IReadOnlyList<RouteShapeObservation> Shape { get; init; }
}
