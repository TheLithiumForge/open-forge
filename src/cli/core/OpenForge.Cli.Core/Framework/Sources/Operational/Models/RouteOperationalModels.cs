using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

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

    public required SourceCatalogue Catalogue { get; init; }

    public required SourceRouteFacts Routes { get; init; }

    public required IReadOnlyList<RouteMetadataObservation> Metadata { get; init; }

    public required IReadOnlyList<GeneratedNavigationTargetObservation> GeneratedNavigation { get; init; }
}
