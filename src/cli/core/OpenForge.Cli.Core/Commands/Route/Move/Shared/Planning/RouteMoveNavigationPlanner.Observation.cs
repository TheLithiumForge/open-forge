using System.Text;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed partial class RouteMoveNavigationPlanner
{
    internal RouteMoveNavigationPostMoveResult Observe(
        RouteMovePlan plan,
        RouteMoveResolvedSubject subject)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(subject);
        var currentSources = _sourceProjector.ProjectCurrent(subject);
        var formation = _formationBuilder.Build(subject.Catalogue, currentSources);
        if (formation.IntendedTargetCollisions.Count > 0 || formation.Ambiguities.Count > 0)
        {
            return Failed("The final Route Move navigation topology is ambiguous or colliding.");
        }

        if (!SourcesMatch(plan.Projection.Navigation.Request.IntendedSources, formation.Sources))
        {
            return Failed("The final Route Move navigation sources changed from the accepted topology.");
        }

        var projection = BuildPostMoveProjection(plan, formation);
        if (projection.Result is { } boundary)
        {
            return boundary;
        }

        foreach (var region in projection.Regions)
        {
            var observed = _regionPlanner.Plan(
                projection.Request
                    ?? throw new InvalidOperationException(
                        "A complete post-move navigation projection requires its request."),
                region);
            if (observed.State != GeneratedNavigationRegionState.Available
                || observed.Change?.RequiresUpdate == true)
            {
                return Failed(observed.Cause
                    ?? $"The final Route Move generated region '{observed.CanonicalPath}' differs from its authoritative projection.");
            }
        }

        return Verified();
    }

    private PostMoveProjection BuildPostMoveProjection(
        RouteMovePlan plan,
        GeneratedNavigationFormation formation)
    {
        var metadata = new List<GeneratedNavigationMetadata>();
        foreach (var source in formation.Sources)
        {
            var text = ReadCurrentText(source.Base.PhysicalPath);
            if (text is null)
            {
                return PostMoveProjection.Stop(
                    Failed("Final Route Move navigation metadata could not be read."));
            }

            metadata.Add(new GeneratedNavigationMetadata(
                source,
                _metadataParser.Parse(_markdownParser.Parse(text), source.Base.Form)));
        }

        var regions = ReadPostMoveRegions(plan, formation);
        if (regions is null)
        {
            return PostMoveProjection.Stop(
                Failed("A final Route Move generated region could not be read."));
        }

        return PostMoveProjection.Complete(
            new GeneratedNavigationProjectionRequest(formation, regions, metadata),
            regions);
    }

    private GeneratedNavigationRegionInput[]? ReadPostMoveRegions(
        RouteMovePlan plan,
        GeneratedNavigationFormation formation)
    {
        var values = new List<GeneratedNavigationRegionInput>();
        foreach (var expected in plan.Projection.Navigation.GeneratedNavigation.Regions)
        {
            var source = formation.FindSource(expected.Path);
            var text = source is null ? null : ReadCurrentText(source.Base.PhysicalPath);
            if (source is null || text is null)
            {
                return null;
            }

            values.Add(new GeneratedNavigationRegionInput(source, _markdownParser.Parse(text)));
        }

        return values.ToArray();
    }

    private static string? ReadCurrentText(string physicalPath)
    {
        try
        {
            return StrictUtf8.GetString(File.ReadAllBytes(physicalPath));
        }
        catch (Exception exception) when (exception is DecoderFallbackException
            or UnauthorizedAccessException
            or IOException)
        {
            return null;
        }
    }

    private static bool SourcesMatch(
        IEnumerable<SourceLogicalSource> expected,
        IEnumerable<SourceLogicalSource> actual)
    {
        var expectedValues = expected.OrderBy(SourceKey, StringComparer.Ordinal).ToArray();
        var actualValues = actual.OrderBy(SourceKey, StringComparer.Ordinal).ToArray();
        return expectedValues.Length == actualValues.Length
            && expectedValues.Zip(actualValues).All(pair => SourceMatches(pair.First, pair.Second));
    }

    private static bool SourceMatches(SourceLogicalSource expected, SourceLogicalSource actual)
    {
        if (expected.Identity.AutomaticId != actual.Identity.AutomaticId
            || expected.Identity.CanonicalBasePath != actual.Identity.CanonicalBasePath
            || !LayerMatches(expected.Base, actual.Base))
        {
            return false;
        }

        if (expected.Overwrite is null || actual.Overwrite is null)
        {
            return expected.Overwrite is null && actual.Overwrite is null;
        }

        return LayerMatches(expected.Overwrite, actual.Overwrite);
    }

    private static bool LayerMatches(SourceLayer expected, SourceLayer actual)
        => expected.CanonicalPath == actual.CanonicalPath
            && expected.PhysicalPath == actual.PhysicalPath
            && expected.Form == actual.Form
            && expected.Kind == actual.Kind;

    private static string SourceKey(SourceLogicalSource source)
        => source.Identity.CanonicalBasePath;

    private static RouteMoveNavigationPostMoveResult Verified()
        => new(RouteMoveNavigationPostMoveState.Verified, Cause: null);

    private static RouteMoveNavigationPostMoveResult Failed(string cause)
        => new(RouteMoveNavigationPostMoveState.Failed, cause);

    private sealed record PostMoveProjection(
        GeneratedNavigationProjectionRequest? Request,
        IReadOnlyList<GeneratedNavigationRegionInput> Regions,
        RouteMoveNavigationPostMoveResult? Result)
    {
        internal static PostMoveProjection Complete(
            GeneratedNavigationProjectionRequest request,
            IReadOnlyList<GeneratedNavigationRegionInput> regions)
            => new(request, regions, Result: null);

        internal static PostMoveProjection Stop(RouteMoveNavigationPostMoveResult result)
            => new(Request: null, [], result);
    }
}
