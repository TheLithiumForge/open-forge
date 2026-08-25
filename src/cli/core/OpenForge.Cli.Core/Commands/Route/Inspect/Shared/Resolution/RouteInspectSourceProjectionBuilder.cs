using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed class RouteInspectSourceProjectionBuilder
{
    internal async ValueTask<RouteSourceProjectionBuildResult> ReadAsync(
        SourceCatalogueSelection selection,
        SourceDocumentReader reader,
        CancellationToken cancellationToken)
    {
        var projectionModes = selection.Sources.ToDictionary(
            source => source.Identity.CanonicalBasePath,
            _ => RouteSourceLayerProjection.ExactLayers,
            StringComparer.Ordinal);
        var overwritePlans = new List<OverwritePlan>();
        foreach (var candidate in selection.Candidates
                     .Where(candidate => candidate.Form == SourceDocumentForm.OverwriteCompanion))
        {
            var bases = selection.Sources
                .Where(source => string.Equals(
                    source.Identity.AutomaticId,
                    SourceIdentity.DeriveId(candidate.CanonicalPath),
                    StringComparison.Ordinal))
                .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .ToArray();
            if (bases.Length >= 2)
            {
                foreach (var source in bases)
                {
                    projectionModes[source.Identity.CanonicalBasePath] = RouteSourceLayerProjection.BaseOnly;
                }

                overwritePlans.Add(new OverwritePlan(
                    candidate,
                    RouteOverwriteState.Ambiguous,
                    bases.Select(source => source.Identity.CanonicalBasePath).ToArray()));
                continue;
            }

            var isAdjacent = bases.Length == 1
                && string.Equals(
                    bases[0].Overwrite?.CanonicalPath,
                    candidate.CanonicalPath,
                    StringComparison.Ordinal);
            overwritePlans.Add(new OverwritePlan(
                candidate,
                isAdjacent ? RouteOverwriteState.Paired : RouteOverwriteState.Orphan,
                isAdjacent || bases.Length == 0
                    ? []
                    : [bases[0].Identity.CanonicalBasePath]));
        }

        var projector = new RouteSourceProjector();
        var projections = new List<RouteSourceProjection>();
        foreach (var source in selection.Sources)
        {
            projections.Add(await projector
                .ReadAsync(
                    source,
                    projectionModes[source.Identity.CanonicalBasePath],
                    reader,
                    cancellationToken)
                .ConfigureAwait(false));
        }

        var overwriteFacts = new List<RouteOverwriteFact>();
        var pairedPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var projection in projections)
        {
            if (projection.Source?.Overwrite is not { } overwrite)
            {
                continue;
            }

            pairedPaths.Add(overwrite.CanonicalLogicalPath);
            overwriteFacts.Add(new RouteOverwriteFact(
                RouteOverwriteState.Paired,
                overwrite,
                [projection.Source.CanonicalPath]));
        }

        var sourceLessReads = new List<SourceDocumentReadResult>();
        foreach (var plan in overwritePlans
                     .Where(plan => plan.State != RouteOverwriteState.Paired
                         && plan.Candidate.PhysicalState == PhysicalPathState.Contained
                         && !pairedPaths.Contains(plan.Candidate.CanonicalPath)))
        {
            var layer = ToLayer(plan.Candidate);
            var read = await reader.ReadAsync(layer, cancellationToken).ConfigureAwait(false);
            overwriteFacts.Add(new RouteOverwriteFact(
                plan.State,
                ToRouteDocument(read),
                plan.CandidateBasePaths));
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

    private sealed record OverwritePlan(
        SourceCandidate Candidate,
        RouteOverwriteState State,
        IReadOnlyList<string> CandidateBasePaths);
}
