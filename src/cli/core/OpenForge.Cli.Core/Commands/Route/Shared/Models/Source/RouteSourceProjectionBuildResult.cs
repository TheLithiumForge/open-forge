using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

internal sealed class RouteSourceProjectionBuildResult
{
    internal RouteSourceProjectionBuildResult(
        RouteSourceProjectionSet projectionSet,
        IEnumerable<RouteSourceProjection> projections,
        IEnumerable<SourceDocumentReadResult> sourceLessReads,
        bool isCancelled)
    {
        ArgumentNullException.ThrowIfNull(projectionSet);
        ArgumentNullException.ThrowIfNull(projections);
        ArgumentNullException.ThrowIfNull(sourceLessReads);

        var orderedProjections = projections
            .Select(projection => projection ?? throw new ArgumentException("Projection results cannot contain null.", nameof(projections)))
            .OrderBy(projection => projection.LogicalSource.Identity.AutomaticId, StringComparer.Ordinal)
            .ThenBy(projection => projection.LogicalSource.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        var orderedReadResults = sourceLessReads
            .Select(read => read ?? throw new ArgumentException("Source-less read results cannot contain null.", nameof(sourceLessReads)))
            .OrderBy(read => read.Layer.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        var uniqueProjections = orderedProjections.ToHashSet(ReferenceEqualityComparer.Instance);
        if (uniqueProjections.Count != orderedProjections.Length
            || uniqueProjections.Count != projectionSet.Projections.Count
            || !uniqueProjections.SetEquals(projectionSet.Projections))
        {
            throw new ArgumentException("The projection result and projection set must contain the same projection instances.", nameof(projections));
        }

        if (orderedReadResults.Any(read => read.Layer.Kind != SourceLayerKind.Overwrite)
            || orderedReadResults.Select(read => read.Layer.CanonicalPath)
                .Distinct(StringComparer.Ordinal)
                .Count() != orderedReadResults.Length)
        {
            throw new ArgumentException("Source-less reads must describe unique overwrite layers.", nameof(sourceLessReads));
        }

        ValidateSourceLessReads(projectionSet, orderedReadResults, nameof(sourceLessReads));

        ProjectionSet = projectionSet;
        Projections = new ReadOnlyCollection<RouteSourceProjection>(orderedProjections);
        ReadResults = new ReadOnlyCollection<SourceDocumentReadResult>(orderedReadResults);
        IsCancelled = isCancelled;
    }

    internal RouteSourceProjectionSet ProjectionSet { get; }

    internal IReadOnlyList<RouteSourceProjection> Projections { get; }

    internal IReadOnlyList<SourceDocumentReadResult> ReadResults { get; }

    internal bool IsCancelled { get; }

    private static void ValidateSourceLessReads(
        RouteSourceProjectionSet projectionSet,
        IReadOnlyList<SourceDocumentReadResult> reads,
        string parameterName)
    {
        var sourceLessFacts = projectionSet.OverwriteFacts
            .Where(fact => fact.State != RouteOverwriteState.Paired)
            .OrderBy(fact => fact.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (sourceLessFacts.Length != reads.Count)
        {
            throw new ArgumentException("Every orphan or ambiguous overwrite fact requires one source-less read.", parameterName);
        }

        for (var index = 0; index < sourceLessFacts.Length; index++)
        {
            var fact = sourceLessFacts[index];
            var read = reads[index];
            if (!string.Equals(fact.CanonicalPath, read.Layer.CanonicalPath, StringComparison.Ordinal)
                || !string.Equals(fact.PhysicalPath, read.Layer.PhysicalPath, StringComparison.Ordinal))
            {
                throw new ArgumentException("A source-less read must match its orphan or ambiguous overwrite fact.", parameterName);
            }
        }
    }
}
