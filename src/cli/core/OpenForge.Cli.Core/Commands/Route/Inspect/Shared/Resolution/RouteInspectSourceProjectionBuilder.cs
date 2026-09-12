using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal static class RouteInspectSourceProjectionBuilder
{
    internal static async ValueTask<RouteSourceProjectionBuildResult> ReadAsync(
        SourceCatalogueSelection selection,
        SourceDocumentReader reader,
        CancellationToken cancellationToken)
    {
        var projector = new RouteSourceProjector();
        var projections = new List<RouteSourceProjection>();
        foreach (var source in selection.Sources)
        {
            projections.Add(await projector
                .ReadAsync(
                    source,
                    RouteSourceLayerProjection.ExactLayers,
                    reader,
                    cancellationToken)
                .ConfigureAwait(false));
        }

        var overwriteFacts = new List<RouteOverwriteFact>();
        var pairedPaths = selection.Sources.Where(source => source.Overwrite is not null)
            .Select(source => source.Overwrite?.CanonicalPath).ToHashSet(StringComparer.Ordinal);
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

        var sourceLessReads = new List<SourceDocumentReadResult>();
        foreach (var candidate in selection.Candidates.Where(candidate => candidate.Form == SourceDocumentForm.OverwriteCompanion
            && candidate.PhysicalState == PhysicalPathState.Contained && !pairedPaths.Contains(candidate.CanonicalPath)))
        {
            var layer = ToLayer(candidate);
            var read = await reader.ReadAsync(layer, cancellationToken).ConfigureAwait(false);
            var relatedPaths = selection.Sources
                .Where(source => string.Equals(source.Identity.AutomaticId, SourceIdentity.DeriveId(candidate.CanonicalPath), StringComparison.Ordinal))
                .Select(source => source.Identity.CanonicalBasePath).Order(StringComparer.Ordinal).ToArray();
            overwriteFacts.Add(new RouteOverwriteFact(RouteOverwriteState.Orphan, ToRouteDocument(read), relatedPaths));
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

    private static SourceLayer ToLayer(SourceCandidate candidate)
    {
        if (candidate.PhysicalState != PhysicalPathState.Contained
            || candidate.PhysicalPath is not { } physicalPath
            || candidate.Form is not { } form)
        {
            throw new InvalidOperationException(
                "A contained overwrite candidate requires a physical path and source form.");
        }

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
