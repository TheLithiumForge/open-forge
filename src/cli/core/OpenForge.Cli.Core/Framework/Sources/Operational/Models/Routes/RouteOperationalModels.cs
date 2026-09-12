using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

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
