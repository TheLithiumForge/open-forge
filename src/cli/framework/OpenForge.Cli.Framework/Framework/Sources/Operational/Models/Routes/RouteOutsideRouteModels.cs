using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

internal sealed record RouteOutsideRouteObservation(
    WorkspaceOutsideRouteRead Directory,
    IReadOnlyList<string> SourcePaths,
    IReadOnlyList<string> EntrypointPaths,
    IReadOnlyList<SourceIncludedEntryRow> Rows)
{
    public bool? Listed { get; init; }
}

internal sealed record RouteOutsideLayerObservation(
    string PhysicalPath,
    IReadOnlyList<string> LogicalPaths,
    RouteContextSource Source,
    bool InSourceInventory,
    bool InLinkedFiles,
    bool InventoryStartup,
    bool InventoryContinuity,
    bool LinkedStartup,
    bool LinkedContinuity,
    ContextMeasurementObservation Measurement)
{
    public bool? LoadingStartup { get; init; }
    public bool? LoadingContinuity { get; init; }
}

internal sealed record RouteOutsideRoutesObservation
{
    public required WorkspaceLayoutRead LayoutRead { get; init; }
    public required bool RegistrationInventoryComplete { get; init; }
    public RouteSourceInspection? Inspection { get; init; }
    public RouteIncludedFilesObservation? IncludedFiles { get; init; }
    public SourceCatalogue? IncludedCatalogue { get; init; }
    public SourceLoadingClosureResolution? Loading { get; init; }
    public required ContextMeasurementObservation Startup { get; init; }
    public required ContextMeasurementObservation Continuity { get; init; }
    public required IReadOnlyList<RouteOutsideRouteObservation> Routes { get; init; }
    public required IReadOnlyList<RouteOutsideLayerObservation> Layers { get; init; }
    public required ContextMeasurementObservation Total { get; init; }
}
