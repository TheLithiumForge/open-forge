using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Projection;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Projection;

internal sealed class FindProjectionSourceResolver
{
    internal FindProjectionSourceFacts Resolve(FindProjectionSourceInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var inspections = input.Inspections
            .Where(inspection => IsMatch(inspection, input.Match))
            .OrderBy(inspection => FindProjectionFindingPolicy.ReadLayerRank(inspection.Layer.Kind))
            .ThenBy(inspection => inspection.Layer.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        var source = inspections.FirstOrDefault()?.Source;
        var sourceIdentity = new FindSourceIdentity(input.Match.Id, input.Match.Path);
        return new FindProjectionSourceFacts(
            input.Match,
            sourceIdentity,
            source,
            ReadLayers(input.Match, source, inspections),
            ReadRoute(sourceIdentity, source, input.RouteFacts));
    }

    private static IReadOnlyList<FindProjectionLayer> ReadLayers(
        FindMatch match,
        SourceLogicalSource? source,
        IReadOnlyList<FindLayerInspectionFacts> inspections)
    {
        var layers = new List<FindProjectionLayer>
        {
            new(
                SourceLayerKind.Base,
                match.Path,
                ReadInspection(inspections, SourceLayerKind.Base, match.Path)),
        };
        var overwriteInspection = inspections.FirstOrDefault(inspection =>
            inspection.Layer.Kind == SourceLayerKind.Overwrite);
        if (source?.Overwrite is not null || overwriteInspection is not null)
        {
            layers.Add(new FindProjectionLayer(
                SourceLayerKind.Overwrite,
                source?.Overwrite?.CanonicalPath
                    ?? overwriteInspection?.Layer.CanonicalPath
                    ?? ReadOverwritePath(match.Path),
                overwriteInspection));
        }

        return layers;
    }

    private static FindLayerInspectionFacts? ReadInspection(
        IReadOnlyList<FindLayerInspectionFacts> inspections,
        SourceLayerKind kind,
        string path)
        => inspections.FirstOrDefault(inspection =>
            inspection.Layer.Kind == kind
            && string.Equals(inspection.Layer.CanonicalPath, path, StringComparison.Ordinal));

    private static FindSourceRouteProjection? ReadRoute(
        FindSourceIdentity sourceIdentity,
        SourceLogicalSource? source,
        SourceRouteFacts? routeFacts)
    {
        if (source?.Base.Form == SourceDocumentForm.Loader)
        {
            return new FindSourceRouteProjection(FindRouteState.Unrouted, null);
        }

        var routeFact = routeFacts?.RouteFacts.FirstOrDefault(fact =>
            string.Equals(fact.Identity.AutomaticId, sourceIdentity.Id, StringComparison.Ordinal)
            && string.Equals(fact.Identity.CanonicalBasePath, sourceIdentity.Path, StringComparison.Ordinal));
        if (routeFact is null)
        {
            return null;
        }

        return ReadRouteProjection(
            routeFact.State,
            routeFact.Route,
            sourceIdentity.Id);
    }

    internal static FindSourceRouteProjection? ReadRouteProjection(
        SourceRouteState state,
        string? route,
        string sourceId)
        => state switch
        {
            SourceRouteState.Routed when string.Equals(
                route,
                sourceId,
                StringComparison.Ordinal) => new FindSourceRouteProjection(
                    FindRouteState.Routed,
                    route),
            SourceRouteState.Routed => null,
            SourceRouteState.Unrouted => new FindSourceRouteProjection(
                FindRouteState.Unrouted,
                null),
            SourceRouteState.Ambiguous => null,
            SourceRouteState.Unavailable => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The source route state is not defined."),
        };

    private static bool IsMatch(
        FindLayerInspectionFacts inspection,
        FindMatch match)
        => string.Equals(
                inspection.Source.Identity.AutomaticId,
                match.Id,
                StringComparison.Ordinal)
            && string.Equals(
                inspection.Source.Identity.CanonicalBasePath,
                match.Path,
                StringComparison.Ordinal);

    private static string ReadOverwritePath(string basePath)
        => $"{basePath[..^3]}.overwrite.md";
}
