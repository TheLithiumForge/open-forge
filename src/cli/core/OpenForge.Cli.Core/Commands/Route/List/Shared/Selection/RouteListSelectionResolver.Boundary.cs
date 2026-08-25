using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal sealed partial class RouteListSelectionResolver
{
    private static bool WasCancelled(
        SourceCatalogue catalogue,
        RouteSourceProjectionSet projectionSet,
        SourceRouteFacts routeFacts,
        CancellationToken cancellationToken)
    {
        return cancellationToken.IsCancellationRequested
            || catalogue.IsCancelled
            || routeFacts.IsCancelled
            || projectionSet.Projections.Any(projection =>
                projection.BaseRead.Verification.State == SourceLayerVerificationState.Cancelled
                || projection.BaseRead.Read?.State == FileReadState.Cancelled
                || projection.OverwriteRead?.Verification.State == SourceLayerVerificationState.Cancelled
                || projection.OverwriteRead?.Read?.State == FileReadState.Cancelled);
    }

    private static void ValidateBoundary(
        RouteListRequest request,
        SourceCatalogue catalogue,
        RouteSourceProjectionSet projectionSet,
        SourceRouteFacts routeFacts)
    {
        if (!string.Equals(
                request.Workspace.LexicalRoot,
                catalogue.Workspace.LexicalRoot,
                StringComparison.Ordinal)
            || !PhysicalIdentityTracker.PathComparer.Equals(
                request.Workspace.PhysicalRoot,
                catalogue.Workspace.PhysicalRoot))
        {
            throw new ArgumentException("The route-list request and source catalogue must share one workspace.", nameof(catalogue));
        }

        foreach (var source in catalogue.Sources)
        {
            ValidateCatalogueLayer(catalogue, source.Base, source.Identity.AutomaticId);
            if (source.Overwrite is { } overwrite)
            {
                ValidateCatalogueLayer(catalogue, overwrite, source.Identity.AutomaticId);
            }
        }

        foreach (var projection in projectionSet.Projections)
        {
            var logicalSource = projection.LogicalSource;
            if (!ReferenceEquals(
                    catalogue.FindByPath(logicalSource.Identity.CanonicalBasePath),
                    logicalSource))
            {
                throw new ArgumentException("Every Route projection must contain a source-catalogue member.", nameof(projectionSet));
            }

            if (projection.Source is { } source
                && (!ReferenceEquals(projectionSet.FindByPath(source.CanonicalPath), source)
                    || !PhysicalIdentityTracker.PathComparer.Equals(
                        source.PhysicalPath,
                        logicalSource.Base.PhysicalPath)
                    || source.Overwrite is { } overwrite
                        && (logicalSource.Overwrite is null
                            || !PhysicalIdentityTracker.PathComparer.Equals(
                                overwrite.PhysicalPath,
                                logicalSource.Overwrite.PhysicalPath))))
            {
                throw new ArgumentException("Every projected Route source must retain its neutral physical identity.", nameof(projectionSet));
            }
        }

        foreach (var fact in routeFacts.RouteFacts)
        {
            var source = catalogue.FindByPath(fact.Identity.CanonicalBasePath);
            if (source is null || !ReferenceEquals(source.Identity, fact.Identity))
            {
                throw new ArgumentException("Every route fact must retain a source-catalogue identity.", nameof(routeFacts));
            }
        }

        foreach (var node in routeFacts.Topology.Nodes)
        {
            var source = catalogue.FindByPath(node.Identity.CanonicalBasePath);
            if (source is null || !ReferenceEquals(source.Identity, node.Identity))
            {
                throw new ArgumentException("Every route node must retain a source-catalogue identity.", nameof(routeFacts));
            }
        }
    }

    private static void ValidateCatalogueLayer(
        SourceCatalogue catalogue,
        SourceLayer layer,
        string automaticId)
    {
        var candidate = catalogue.FindCandidateByPath(layer.CanonicalPath);
        if (candidate is null
            || candidate.Form != layer.Form
            || !string.Equals(candidate.AutomaticId, automaticId, StringComparison.Ordinal)
            || candidate.PhysicalState != PhysicalPathState.Contained
            || candidate.PhysicalPath is null
            || !PhysicalIdentityTracker.PathComparer.Equals(candidate.PhysicalPath, layer.PhysicalPath))
        {
            throw new ArgumentException("Every logical source layer must retain one exact catalogue candidate identity.", nameof(catalogue));
        }
    }
}
