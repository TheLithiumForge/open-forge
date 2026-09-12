using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed record RouteMoveSemanticPlanProjection
{
    private RouteMoveSemanticPlanProjection(
        SemanticPlanningProjection planning,
        SemanticReferenceProjection references,
        SemanticNavigationProjection navigation,
        SemanticExecutionProjection execution)
    {
        Planning = planning;
        References = references;
        Navigation = navigation;
        Execution = execution;
    }

    private SemanticPlanningProjection Planning { get; }

    private SemanticReferenceProjection References { get; }

    private SemanticNavigationProjection Navigation { get; }

    private SemanticExecutionProjection Execution { get; }

    internal static RouteMoveSemanticPlanProjection Create(RouteMovePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var projection = plan.Projection;
        return new RouteMoveSemanticPlanProjection(
            new SemanticPlanningProjection(
                ProjectLifecycle(projection.Ownership),
                projection.Destination.Destination,
                ProjectInventory(projection.Destination)),
            new SemanticReferenceProjection(
                ProjectReferences(projection.References),
                ProjectReferenceDocuments(projection.References),
                ProjectReferenceEdits(projection.References)),
            new SemanticNavigationProjection(
                ProjectNavigation(projection.Navigation),
                ProjectRegions(projection.Navigation),
                ProjectNavigationEdits(projection.Navigation)),
            new SemanticExecutionProjection(
                ProjectPreview(plan.Preview),
                ProjectEffects(projection),
                projection.RecoveryTargets.Select(target => new RecoveryObservation(
                target.Change.Kind,
                target.Change.Expectation,
                Bytes(target.Change.IntendedBytes),
                target.Before.Expectation,
                Bytes(target.Before.Bytes))).ToImmutableArray()));
    }

    internal bool Matches(RouteMoveSemanticPlanProjection actual)
    {
        ArgumentNullException.ThrowIfNull(actual);
        return Planning.Lifecycle.Matches(actual.Planning.Lifecycle)
            && Planning.Destination == actual.Planning.Destination
            && Planning.Inventory.SequenceEqual(actual.Planning.Inventory)
            && References.Observation.Matches(actual.References.Observation)
            && References.Documents.SequenceEqual(actual.References.Documents)
            && References.Edits.SequenceEqual(actual.References.Edits)
            && Navigation.Observation.Matches(actual.Navigation.Observation)
            && MatchesRegions(Navigation.Regions, actual.Navigation.Regions)
            && Navigation.Edits.SequenceEqual(actual.Navigation.Edits)
            && Execution.Preview.Matches(actual.Execution.Preview)
            && Execution.Effects.SequenceEqual(actual.Execution.Effects)
            && Execution.RecoveryTargets.SequenceEqual(actual.Execution.RecoveryTargets);
    }

    private static bool MatchesRegions(
        IReadOnlyList<NavigationRegionObservation> expected,
        IReadOnlyList<NavigationRegionObservation> actual)
        => expected.Count == actual.Count
            && expected.Zip(actual).All(pair => pair.First.Path == pair.Second.Path
                && pair.First.State == pair.Second.State
                && pair.First.Reasons.SequenceEqual(pair.Second.Reasons));

    private static LifecycleObservation ProjectLifecycle(LifecycleOwnershipReadResult value)
        => new(
            value.Framework.State,
            value.Framework.Cause,
            value.Extensions.State,
            value.Extensions.Cause,
            value.LifecycleFileExpectation,
            value.Claims);

    private static ImmutableArray<InventoryObservation> ProjectInventory(
        RouteMoveResolvedDestination destination)
        => destination.Inventory.Items.Select(item => new InventoryObservation(
            item.Kind,
            item.Layer,
            item.SourceId,
            item.SourcePath,
            item.RelativePath,
            item.Snapshot.Expectation)).ToImmutableArray();

    private static ReferenceObservation ProjectReferences(RouteMoveReferencePlan plan)
        => new(
            plan.Catalogue.Coverage,
            plan.Catalogue.SelectedPaths,
            plan.Catalogue.Findings,
            plan.References.Coverage,
            plan.References.ScannedSourceCount,
            plan.References.InspectedSourceCount,
            plan.References.OccurrenceCount,
            plan.References.Rewrites);

    private static ImmutableArray<ReferenceDocumentObservation> ProjectReferenceDocuments(
        RouteMoveReferencePlan plan)
        => plan.Documents.Select(document => new ReferenceDocumentObservation(
            document.SourcePath,
            document.DestinationSourcePath,
            document.Snapshot.Expectation,
            document.IntendedText)).ToImmutableArray();

    private static ImmutableArray<ReferenceEditObservation> ProjectReferenceEdits(
        RouteMoveReferencePlan plan)
        => plan.Documents.SelectMany(document => document.Edits.Select(edit =>
            new ReferenceEditObservation(
                document.SourcePath,
                document.DestinationSourcePath,
                edit.Location,
                edit.Before,
                edit.Expected))).ToImmutableArray();

    private static NavigationObservation ProjectNavigation(RouteMoveNavigationPlan plan)
        => new(
            plan.GeneratedNavigation.Coverage,
            plan.Request.IntendedSources.Select(source => new IntendedSourceObservation(
                source.Identity.AutomaticId,
                source.Identity.CanonicalBasePath,
                source.Base.CanonicalPath,
                source.Base.Form,
                source.Base.Kind,
                source.Overwrite?.CanonicalPath,
                source.Overwrite?.Form,
                source.Overwrite?.Kind)).ToImmutableArray());

    private static ImmutableArray<NavigationRegionObservation> ProjectRegions(
        RouteMoveNavigationPlan plan)
        => plan.GeneratedNavigation.Regions.Select(region => new NavigationRegionObservation(
            region.Path,
            region.State,
            region.Reasons)).ToImmutableArray();

    private static ImmutableArray<NavigationEditObservation> ProjectNavigationEdits(
        RouteMoveNavigationPlan plan)
        => plan.DocumentEdits.Select(edit => new NavigationEditObservation(
            edit.DestinationLogicalPath,
            edit.Snapshot.Expectation,
            edit.Location,
            Bytes(edit.BeforeBytes),
            Bytes(edit.ExpectedBytes))).ToImmutableArray();

    private static PreviewObservation ProjectPreview(RouteMoveResultFormation preview)
        => new(
            preview.Source,
            preview.Destination,
            preview.Subject.Kind,
            preview.Subject.Layers,
            preview.Subject.Items,
            preview.Plan,
            preview.UnchangedPaths,
            preview.Recovery.State,
            preview.Recovery.ProtectedPaths,
            preview.Recovery.ResidualPath,
            preview.Verification,
            preview.Findings);

    private static ImmutableArray<EffectObservation> ProjectEffects(
        RouteMovePlanProjectionInput projection)
    {
        var values = new List<EffectObservation>();
        values.AddRange(projection.DirectoryCreations.Select(value =>
            new EffectObservation("directory-create", value.Expectation, string.Empty)));
        values.AddRange(projection.FileChanges.Select(value =>
            new EffectObservation(value.Kind.ToString(), value.Expectation, Bytes(value.IntendedBytes))));
        values.AddRange(projection.DirectoryDeletions.Select(value =>
            new EffectObservation("directory-delete", value.Expectation, string.Empty)));
        return values.ToImmutableArray();
    }

    private static string Bytes(ImmutableArray<byte> value)
        => Convert.ToHexString(value.AsSpan());

}
