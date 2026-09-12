using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed class RouteListSourceProjectionBuilder
{
    internal async ValueTask<RouteSourceProjectionBuildResult> ReadAsync(
        SourceCatalogueSelection selection,
        SourceDocumentReader reader,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(reader);
        var projector = new RouteSourceProjector();
        var projections = new List<RouteSourceProjection>();
        foreach (var source in selection.Sources)
        {
            projections.Add(await projector
                .ReadAsync(source, RouteSourceLayerProjection.ExactLayers, reader, cancellationToken)
                .ConfigureAwait(false));
        }

        projections = ApplyEntrypointAmbiguity(projections);

        var overwriteFacts = new List<RouteOverwriteFact>();
        foreach (var projection in projections)
        {
            if (projection.Source?.Overwrite is not { } overwrite)
            {
                continue;
            }

            overwriteFacts.Add(new RouteOverwriteFact(
                RouteOverwriteState.Paired,
                overwrite,
                [projection.Source.CanonicalPath]));
        }

        var logicalOverwritePaths = selection.Sources
            .Select(source => source.Overwrite)
            .OfType<SourceLayer>()
            .Select(overwrite => overwrite.CanonicalPath)
            .ToHashSet(StringComparer.Ordinal);
        var sourceLessReads = new List<SourceDocumentReadResult>();
        foreach (var candidate in selection.Candidates
                     .Where(candidate => candidate.Form == SourceDocumentForm.OverwriteCompanion
                          && candidate.PhysicalState == PhysicalPathState.Contained
                          && !logicalOverwritePaths.Contains(candidate.CanonicalPath)))
        {
            var layer = ToLayer(candidate);
            var read = await reader.ReadAsync(layer, cancellationToken).ConfigureAwait(false);
            overwriteFacts.Add(new RouteOverwriteFact(
                RouteOverwriteState.Orphan,
                ToRouteDocument(read),
                []));
            sourceLessReads.Add(read);
        }

        var projectionSet = new RouteSourceProjectionSet(projections, overwriteFacts);
        return new RouteSourceProjectionBuildResult(
            projectionSet,
            projections,
            sourceLessReads,
            cancellationToken.IsCancellationRequested
                || projections.Any(IsCancelled)
                || sourceLessReads.Any(read => read.Verification.State == SourceLayerVerificationState.Cancelled));
    }

    private static bool IsCancelled(RouteSourceProjection projection)
    {
        return projection.BaseRead.Verification.State == SourceLayerVerificationState.Cancelled
            || projection.OverwriteRead?.Verification.State == SourceLayerVerificationState.Cancelled;
    }

    private static List<RouteSourceProjection> ApplyEntrypointAmbiguity(
        IReadOnlyList<RouteSourceProjection> projections)
    {
        var ambiguousPaths = projections
            .Where(projection => projection.Source is not null
                && SourceFormClassifier.IsEntrypoint(projection.LogicalSource.Base.Form))
            .GroupBy(
                projection => SourceLogicalPath.ReadParent(
                    projection.LogicalSource.Identity.CanonicalBasePath),
                StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Select(projection =>
                projection.LogicalSource.Identity.CanonicalBasePath))
            .ToHashSet(StringComparer.Ordinal);

        return projections
            .Select(projection => ambiguousPaths.Contains(
                    projection.LogicalSource.Identity.CanonicalBasePath)
                ? WithRouteAmbiguity(projection)
                : projection)
            .ToList();
    }

    private static RouteSourceProjection WithRouteAmbiguity(
        RouteSourceProjection projection)
    {
        var source = projection.Source
            ?? throw new InvalidOperationException("An ambiguous entrypoint projection requires a Route source.");
        var ambiguousSource = new RouteSource(
            source.Base,
            source.Metadata,
            source.Kind,
            source.Overwrite,
            isRouteAmbiguous: true);
        return new RouteSourceProjection(
            projection.LogicalSource,
            ambiguousSource,
            projection.BaseRead,
            projection.OverwriteRead);
    }

    private static SourceLayer ToLayer(SourceCandidate candidate)
    {
        var physicalPath = candidate.PhysicalPath
            ?? throw new InvalidOperationException("A contained overwrite candidate requires its physical path.");
        var form = candidate.Form
            ?? throw new InvalidOperationException("An overwrite candidate requires its recognized form.");
        return new SourceLayer(
            candidate.CanonicalPath,
            physicalPath,
            form,
            SourceLayerKind.Overwrite);
    }

    private static RouteSourceDocument ToRouteDocument(SourceDocumentReadResult read)
    {
        var fileRead = read.Read;
        var state = fileRead?.State ?? ReadUnavailableState(read.Verification.State);
        return new RouteSourceDocument(
            read.Layer.CanonicalPath,
            read.Layer.PhysicalPath,
            SourceDocumentForm.OverwriteCompanion,
            state,
            fileRead?.Value);
    }

    private static FileReadState ReadUnavailableState(SourceLayerVerificationState state)
    {
        return state == SourceLayerVerificationState.Cancelled
            ? FileReadState.Cancelled
            : FileReadState.InputOutputFailure;
    }
}
