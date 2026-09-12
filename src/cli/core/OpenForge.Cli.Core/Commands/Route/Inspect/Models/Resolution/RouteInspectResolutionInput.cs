using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal sealed class RouteInspectResolutionInput
{
    internal RouteInspectResolutionInput(
        RouteInspectRequest request,
        SourceReferenceParseResult parsed,
        RouteInspectSelection unresolvedSelection,
        SourceCatalogue catalogue,
        SourceCatalogueSelection catalogueSelection,
        RouteSourceProjectionBuildResult projections,
        SourceRouteFacts routeFacts,
        PhysicalPathResolution? exactPathPhysical)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(parsed);
        ArgumentNullException.ThrowIfNull(unresolvedSelection);
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(catalogueSelection);
        ArgumentNullException.ThrowIfNull(projections);
        ArgumentNullException.ThrowIfNull(routeFacts);
        if ((parsed.Kind == SourceReferenceKind.SourcePath) != (exactPathPhysical is not null))
        {
            throw new ArgumentException(
                "An exact physical path is required only for a parsed source-path reference.",
                nameof(exactPathPhysical));
        }

        Request = request;
        Parsed = parsed;
        UnresolvedSelection = unresolvedSelection;
        Catalogue = catalogue;
        CatalogueSelection = catalogueSelection;
        Projections = projections;
        RouteFacts = routeFacts;
        ExactPathPhysical = exactPathPhysical;
    }

    internal RouteInspectRequest Request { get; }

    internal SourceReferenceParseResult Parsed { get; }

    internal RouteInspectSelection UnresolvedSelection { get; }

    internal SourceCatalogue Catalogue { get; }

    internal SourceCatalogueSelection CatalogueSelection { get; }

    internal RouteSourceProjectionBuildResult Projections { get; }

    internal SourceRouteFacts RouteFacts { get; }

    internal PhysicalPathResolution? ExactPathPhysical { get; }
}
