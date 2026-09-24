using OpenForge.Cli.Core.Commands.Route.Shared.Ownership;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed class RouteRemovePlanRevalidator(RouteRemovePlanBuilder planBuilder)
{
    private readonly RouteRemovePlanBuilder _planBuilder = planBuilder;

    internal async ValueTask<RouteRemovePlanRevalidation> RevalidateAsync(
        RouteRemovePlan plan,
        WorkspaceLockLease lease,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(lease);
        if (!lease.IsHeldFor(plan.Request.Workspace))
        {
            return Failed("Route Remove revalidation does not hold the selected workspace lease.");
        }

        RouteRemovePlanBuild fresh;
        try
        {
            fresh = await _planBuilder.BuildAsync(plan.Request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Interrupted();
        }
        catch (Exception)
        {
            return Failed("The complete Route Remove plan could not be rebuilt under the workspace lease.");
        }

        if (fresh.Formation.Findings.Any(finding =>
                finding.Code == RouteRemoveFindingCode.Interrupted))
        {
            return Interrupted();
        }

        if (fresh.Plan is not { } rebuilt || !Matches(plan, rebuilt))
        {
            return new RouteRemovePlanRevalidation(
                RouteRemovePlanRevalidationState.Changed,
                fresh.Formation.Findings.FirstOrDefault()?.Cause
                    ?? "The complete Route Remove plan changed before application.");
        }

        return new RouteRemovePlanRevalidation(RouteRemovePlanRevalidationState.Exact, cause: null);
    }

    private static bool Matches(RouteRemovePlan expected, RouteRemovePlan actual)
        => expected.Request == actual.Request
            && SubjectMatches(expected.Projection.Subject, actual.Projection.Subject)
            && OwnershipMatches(expected, actual)
            && expected.Projection.Settings.MatchesObservation(actual.Projection.Settings)
            && RemovalSelectionMatches(expected.Projection.RemovalSelection, actual.Projection.RemovalSelection)
            && expected.Projection.ContentPathsToRelease.SequenceEqual(
                actual.Projection.ContentPathsToRelease, StringComparer.Ordinal)
            && expected.Projection.ClaimsToRelease.SequenceEqual(actual.Projection.ClaimsToRelease)
            && OptionalFileChangesMatch(expected.Projection.SettingsChange, actual.Projection.SettingsChange)
            && OptionalFileChangesMatch(expected.Projection.OwnershipChange, actual.Projection.OwnershipChange)
            && ReferencesMatch(expected.Projection.References, actual.Projection.References)
            && NavigationMatches(expected.Projection.Navigation, actual.Projection.Navigation)
            && PreviewMatches(expected.Preview, actual.Preview)
            && FileChangesMatch(expected.Projection.FileChanges, actual.Projection.FileChanges)
            && expected.Projection.DirectoryDeletions.SequenceEqual(
                actual.Projection.DirectoryDeletions)
            && RecoveryTargetsMatch(
                expected.Projection.RecoveryTargets,
                actual.Projection.RecoveryTargets);

    private static bool SubjectMatches(
        RouteRemoveResolvedSubject expected,
        RouteRemoveResolvedSubject actual)
        => expected.Source == actual.Source
            && expected.Kind == actual.Kind
            && LogicalSourceMatches(expected.SelectedSource, actual.SelectedSource)
            && CatalogueMatches(expected.Catalogue, actual.Catalogue)
            && expected.NavigationExposure.IsCancelled == actual.NavigationExposure.IsCancelled
            && expected.NavigationExposure.ExposedPaths.SequenceEqual(
                actual.NavigationExposure.ExposedPaths)
            && expected.NavigationExposure.UnavailableParents.SequenceEqual(
                actual.NavigationExposure.UnavailableParents)
            && expected.Layers.Length == actual.Layers.Length
            && expected.Layers.Zip(actual.Layers).All(pair =>
                LayerMatches(pair.First.Layer, pair.Second.Layer)
                && SnapshotMatches(pair.First.Snapshot, pair.Second.Snapshot));

    private static bool CatalogueMatches(SourceCatalogue expected, SourceCatalogue actual)
        => expected.Workspace == actual.Workspace
            && expected.IsCancelled == actual.IsCancelled
            && expected.Candidates.Count == actual.Candidates.Count
            && expected.Candidates.Zip(actual.Candidates).All(pair =>
                pair.First.CanonicalPath == pair.Second.CanonicalPath
                && pair.First.Form == pair.Second.Form
                && pair.First.AutomaticId == pair.Second.AutomaticId
                && pair.First.PhysicalState == pair.Second.PhysicalState
                && pair.First.PhysicalPath == pair.Second.PhysicalPath
                && pair.First.PhysicalParentPath == pair.Second.PhysicalParentPath)
            && expected.Sources.Count == actual.Sources.Count
            && expected.Sources.Zip(actual.Sources).All(pair =>
                LogicalSourceMatches(pair.First, pair.Second))
            && expected.Issues.Count == actual.Issues.Count
            && expected.Issues.Zip(actual.Issues).All(pair =>
                pair.First.Stage == pair.Second.Stage
                && pair.First.Code == pair.Second.Code
                && pair.First.AttemptedCanonicalPath == pair.Second.AttemptedCanonicalPath
                && pair.First.RelatedPaths.SequenceEqual(pair.Second.RelatedPaths)
                && pair.First.ScopePhysicalPath == pair.Second.ScopePhysicalPath);

    private static bool LogicalSourceMatches(SourceLogicalSource expected, SourceLogicalSource actual)
        => expected.Identity.AutomaticId == actual.Identity.AutomaticId
            && expected.Identity.CanonicalBasePath == actual.Identity.CanonicalBasePath
            && LayerMatches(expected.Base, actual.Base)
            && (expected.Overwrite, actual.Overwrite) switch
            {
                (null, null) => true,
                ({ } left, { } right) => LayerMatches(left, right),
                _ => false,
            };

    private static bool LayerMatches(SourceLayer expected, SourceLayer actual)
        => expected.CanonicalPath == actual.CanonicalPath
            && expected.PhysicalPath == actual.PhysicalPath
            && expected.Form == actual.Form
            && expected.Kind == actual.Kind;

    private static bool SnapshotMatches(FileStateSnapshot expected, FileStateSnapshot actual)
        => expected.Expectation == actual.Expectation
            && expected.HasBytes == actual.HasBytes
            && expected.Bytes.AsSpan().SequenceEqual(actual.Bytes.AsSpan());

    private static bool OwnershipMatches(RouteRemovePlan expected, RouteRemovePlan actual)
    {
        var left = expected.Projection.Ownership;
        var right = actual.Projection.Ownership;
        return left.State == right.State
            && left.Cause == right.Cause
            && left.Snapshot?.Expectation == right.Snapshot?.Expectation
            && RouteOwnershipEvidence.Claims(left).SequenceEqual(RouteOwnershipEvidence.Claims(right));
    }

    private static bool ReferencesMatch(
        RouteRemoveReferencePlan expected,
        RouteRemoveReferencePlan actual)
        => expected.Catalogue.Coverage == actual.Catalogue.Coverage
            && expected.Catalogue.SelectedPaths.SequenceEqual(actual.Catalogue.SelectedPaths)
            && expected.Catalogue.Findings.SequenceEqual(actual.Catalogue.Findings)
            && expected.References.Coverage == actual.References.Coverage
            && expected.References.ScannedSourceCount == actual.References.ScannedSourceCount
            && expected.References.InspectedSourceCount == actual.References.InspectedSourceCount
            && expected.References.OccurrenceCount == actual.References.OccurrenceCount
            && expected.References.Detachments.SequenceEqual(actual.References.Detachments)
            && expected.Documents.Length == actual.Documents.Length
            && expected.Documents.Zip(actual.Documents).All(pair =>
                pair.First.SourcePath == pair.Second.SourcePath
                && SnapshotMatches(pair.First.Snapshot, pair.Second.Snapshot)
                && pair.First.IntendedText == pair.Second.IntendedText
                && pair.First.Edits.SequenceEqual(pair.Second.Edits));

    private static bool NavigationMatches(
        RouteRemoveNavigationPlan expected,
        RouteRemoveNavigationPlan actual)
        => expected.Request.IntendedSources.Length == actual.Request.IntendedSources.Length
            && expected.Request.IntendedSources.Zip(actual.Request.IntendedSources).All(pair =>
                LogicalSourceMatches(pair.First, pair.Second))
            && expected.GeneratedNavigation.Coverage == actual.GeneratedNavigation.Coverage
            && expected.GeneratedNavigation.Regions.Length == actual.GeneratedNavigation.Regions.Length
            && expected.GeneratedNavigation.Regions.Zip(actual.GeneratedNavigation.Regions).All(pair =>
                pair.First.Path == pair.Second.Path
                && pair.First.State == pair.Second.State
                && pair.First.Reasons.SequenceEqual(pair.Second.Reasons))
            && expected.DocumentEdits.Length == actual.DocumentEdits.Length
            && expected.DocumentEdits.Zip(actual.DocumentEdits).All(pair =>
                pair.First.LogicalPath == pair.Second.LogicalPath
                && pair.First.Location == pair.Second.Location
                && SnapshotMatches(pair.First.Snapshot, pair.Second.Snapshot)
                && pair.First.BeforeBytes.AsSpan().SequenceEqual(pair.Second.BeforeBytes.AsSpan())
                && pair.First.ExpectedBytes.AsSpan().SequenceEqual(pair.Second.ExpectedBytes.AsSpan()));

    private static bool PreviewMatches(
        RouteRemoveResultFormation expected,
        RouteRemoveResultFormation actual)
        => expected.Workspace == actual.Workspace
            && expected.Mode == actual.Mode
            && expected.Source == actual.Source
            && expected.Subject.Kind == actual.Subject.Kind
            && expected.Subject.Layers.SequenceEqual(actual.Subject.Layers)
            && expected.Subject.Items.SequenceEqual(actual.Subject.Items)
            && expected.Ownership.State == actual.Ownership.State
            && expected.Ownership.Framework == actual.Ownership.Framework
            && expected.Ownership.Extensions == actual.Ownership.Extensions
            && expected.Ownership.Claims.SequenceEqual(actual.Ownership.Claims)
            && expected.References.Coverage == actual.References.Coverage
            && expected.References.ScannedSourceCount == actual.References.ScannedSourceCount
            && expected.References.InspectedSourceCount == actual.References.InspectedSourceCount
            && expected.References.OccurrenceCount == actual.References.OccurrenceCount
            && expected.References.Detachments.SequenceEqual(actual.References.Detachments)
            && expected.GeneratedNavigation.Coverage == actual.GeneratedNavigation.Coverage
            && expected.GeneratedNavigation.Regions.Length == actual.GeneratedNavigation.Regions.Length
            && expected.GeneratedNavigation.Regions.Zip(actual.GeneratedNavigation.Regions).All(pair =>
                pair.First.Path == pair.Second.Path
                && pair.First.State == pair.Second.State
                && pair.First.Reasons.SequenceEqual(pair.Second.Reasons))
            && expected.Plan == actual.Plan
            && expected.Effects.SequenceEqual(actual.Effects)
            && expected.UnchangedPaths.SequenceEqual(actual.UnchangedPaths)
            && expected.Recovery.State == actual.Recovery.State
            && expected.Recovery.ProtectedPaths.SequenceEqual(actual.Recovery.ProtectedPaths)
            && expected.Recovery.ResidualPath == actual.Recovery.ResidualPath
            && expected.Verification == actual.Verification
            && PersistenceMatches(expected.Persistence, actual.Persistence)
            && expected.Findings.SequenceEqual(actual.Findings);

    private static bool PersistenceMatches(
        RouteRemovePersistence expected,
        RouteRemovePersistence actual)
        => expected.Settings.Outcome == actual.Settings.Outcome
            && expected.Settings.Path == actual.Settings.Path
            && expected.Settings.Categories.SequenceEqual(actual.Settings.Categories, StringComparer.Ordinal)
            && expected.Settings.Files.SequenceEqual(actual.Settings.Files, StringComparer.Ordinal)
            && expected.Settings.Directories.SequenceEqual(actual.Settings.Directories, StringComparer.Ordinal)
            && expected.Ownership.Outcome == actual.Ownership.Outcome
            && expected.Ownership.Claims.SequenceEqual(actual.Ownership.Claims);

    private static bool RemovalSelectionMatches(
        OpenForge.Cli.Core.Framework.Settings.Models.Mutation.WorkspaceRemovalSelection expected,
        OpenForge.Cli.Core.Framework.Settings.Models.Mutation.WorkspaceRemovalSelection actual)
        => expected.Categories.SequenceEqual(actual.Categories, StringComparer.Ordinal)
            && expected.Files.SequenceEqual(actual.Files, StringComparer.Ordinal)
            && expected.Directories.SequenceEqual(actual.Directories, StringComparer.Ordinal)
            && expected.Extensions.SequenceEqual(actual.Extensions, StringComparer.Ordinal)
            && expected.Libraries.SequenceEqual(actual.Libraries, StringComparer.Ordinal);

    private static bool OptionalFileChangesMatch(
        PlannedFileChange? expected,
        PlannedFileChange? actual)
        => expected is null && actual is null
            || expected is not null && actual is not null
                && expected.Kind == actual.Kind
                && expected.Expectation == actual.Expectation
                && expected.IntendedBytes.AsSpan().SequenceEqual(actual.IntendedBytes.AsSpan());

    private static bool FileChangesMatch(
        IReadOnlyList<PlannedFileChange> expected,
        IReadOnlyList<PlannedFileChange> actual)
        => expected.Count == actual.Count
            && expected.Zip(actual).All(pair =>
                pair.First.Kind == pair.Second.Kind
                && pair.First.Expectation == pair.Second.Expectation
                && pair.First.IntendedBytes.AsSpan().SequenceEqual(
                    pair.Second.IntendedBytes.AsSpan()));

    private static bool RecoveryTargetsMatch(
        IReadOnlyList<RecoveryBundleTarget> expected,
        IReadOnlyList<RecoveryBundleTarget> actual)
        => expected.Count == actual.Count
            && expected.Zip(actual).All(pair =>
                pair.First.Change.Kind == pair.Second.Change.Kind
                && pair.First.Change.Expectation == pair.Second.Change.Expectation
                && pair.First.Change.IntendedBytes.AsSpan().SequenceEqual(
                    pair.Second.Change.IntendedBytes.AsSpan())
                && pair.First.Before.Expectation == pair.Second.Before.Expectation
                && pair.First.Before.HasBytes == pair.Second.Before.HasBytes
                && pair.First.Before.Bytes.AsSpan().SequenceEqual(pair.Second.Before.Bytes.AsSpan())
                && pair.First.RequiresRecovery == pair.Second.RequiresRecovery);

    private static RouteRemovePlanRevalidation Interrupted()
        => new(
            RouteRemovePlanRevalidationState.Interrupted,
            "Complete Route Remove plan revalidation was interrupted.");

    private static RouteRemovePlanRevalidation Failed(string cause)
        => new(RouteRemovePlanRevalidationState.Failed, cause);
}
