using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed partial class RouteRemoveNavigationPlanner
{
    internal RouteRemoveNavigationPostRemoveResult Observe(
        RouteRemovePlan plan,
        SourceCatalogue currentCatalogue,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(currentCatalogue);
        if (cancellationToken.IsCancellationRequested)
        {
            return Interrupted();
        }

        var formation = _formationBuilder.Build(currentCatalogue);
        if (formation.IntendedTargetCollisions.Count != 0 || formation.Ambiguities.Count != 0)
        {
            return Failed("The final Route Remove navigation topology is ambiguous or colliding.");
        }

        var selected = SelectAffectedRegions(plan, formation);
        if (selected.Result is { } selectionBoundary)
        {
            return selectionBoundary;
        }

        var request = plan.Projection.Navigation.Request;
        var documents = ReadDocuments(request, selected.Regions);
        if (documents.Boundary is { } documentBoundary)
        {
            return FromBoundary(documentBoundary);
        }

        var metadata = ReadMetadata(request, formation);
        if (metadata.Boundary is { } metadataBoundary)
        {
            return FromBoundary(metadataBoundary);
        }

        var regions = documents.Values
            .Select(document => new GeneratedNavigationRegionInput(
                document.Region.Source,
                _markdownParser.Parse(document.Text)))
            .ToArray();
        var projection = new GeneratedNavigationProjectionRequest(
            formation,
            regions,
            metadata.Values);
        foreach (var region in regions)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Interrupted();
            }

            var observed = _regionPlanner.Plan(projection, region);
            if (observed.State != GeneratedNavigationRegionState.Available
                || observed.Change?.RequiresUpdate == true)
            {
                return Failed(observed.Cause
                    ?? $"The final Route Remove generated region '{observed.CanonicalPath}' differs from its authoritative projection.");
            }
        }

        return Verified();
    }

    private static AffectedRegionSelection SelectAffectedRegions(
        RouteRemovePlan plan,
        GeneratedNavigationFormation formation)
    {
        var regions = new List<SelectedRegion>();
        foreach (var expected in plan.Projection.Navigation.GeneratedNavigation.Regions)
        {
            var source = formation.FindSource(expected.Path);
            if (source is null)
            {
                return AffectedRegionSelection.Stop(Failed(
                    $"The final Route Remove generated region '{expected.Path}' is unavailable."));
            }

            regions.Add(new SelectedRegion(source, expected.Reasons));
        }

        return AffectedRegionSelection.Complete(regions);
    }

    private static RouteRemoveNavigationPostRemoveResult FromBoundary(
        RouteRemoveResultFormation boundary)
    {
        var finding = boundary.Findings.FirstOrDefault();
        return finding?.Code == RouteRemoveFindingCode.Interrupted
            ? Interrupted()
            : Failed(finding?.Cause
                ?? "The final Route Remove generated navigation projection is unavailable.");
    }

    private static RouteRemoveNavigationPostRemoveResult Verified()
        => new(RouteRemoveNavigationPostRemoveState.Verified, Cause: null);

    private static RouteRemoveNavigationPostRemoveResult Failed(string cause)
        => new(RouteRemoveNavigationPostRemoveState.Failed, cause);

    private static RouteRemoveNavigationPostRemoveResult Interrupted()
        => new(
            RouteRemoveNavigationPostRemoveState.Interrupted,
            "Final Route Remove generated navigation observation was interrupted.");

    private sealed record AffectedRegionSelection(
        IReadOnlyList<SelectedRegion> Regions,
        RouteRemoveNavigationPostRemoveResult? Result)
    {
        internal static AffectedRegionSelection Complete(IReadOnlyList<SelectedRegion> regions)
            => new(regions, Result: null);

        internal static AffectedRegionSelection Stop(RouteRemoveNavigationPostRemoveResult result)
            => new([], result);
    }
}
