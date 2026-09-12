using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Index.Models.Projection;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Index.Models.Planning;

internal sealed record IndexPlanningInput
{
    public required IndexRequest Request { get; init; }

    public required IndexProjectionFormation Projection { get; init; }
}

internal static class IndexPlanningOutcome
{
    internal static IndexRegionOutcome ReadInitial(IndexMode mode)
        => mode switch
        {
            IndexMode.Apply => IndexRegionOutcome.NotStarted,
            IndexMode.DryRun => IndexRegionOutcome.NotRequested,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The Index mode is not defined."),
        };
}

internal sealed record IndexPlan
{
    internal IndexPlan(
        IndexPlanningInput input,
        IEnumerable<IndexRegion> regions,
        IEnumerable<RecoveryBundleTarget> recoveryTargets)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Request);
        ArgumentNullException.ThrowIfNull(input.Projection);
        ArgumentNullException.ThrowIfNull(regions);
        ArgumentNullException.ThrowIfNull(recoveryTargets);
        var regionValues = regions
            .Select(region => region ?? throw new ArgumentException(
                "Index plan regions cannot contain null members.",
                nameof(regions)))
            .OrderBy(region => region.Source.Path, StringComparer.Ordinal)
            .ToArray();
        var targetValues = recoveryTargets
            .Select(target => target ?? throw new ArgumentException(
                "Index plan recovery targets cannot contain null members.",
                nameof(recoveryTargets)))
            .ToArray();

        ValidateRegions(input, regionValues);
        ValidateRecoveryTargets(input, regionValues, targetValues);
        Input = input;
        Regions = new ReadOnlyCollection<IndexRegion>(regionValues);
        RecoveryTargets = new ReadOnlyCollection<RecoveryBundleTarget>(targetValues);
        Updates = new ReadOnlyCollection<PlannedFileChange>(targetValues
            .Select(target => target.Change)
            .ToArray());
    }

    internal IndexPlanningInput Input { get; }

    internal IReadOnlyList<IndexRegion> Regions { get; }

    internal IReadOnlyList<PlannedFileChange> Updates { get; }

    internal IReadOnlyList<RecoveryBundleTarget> RecoveryTargets { get; }

    internal bool IsComplete => Input.Projection.IsComplete;

    internal bool IsNoOp => IsComplete && Updates.Count == 0;

    private static void ValidateRegions(
        IndexPlanningInput input,
        IReadOnlyList<IndexRegion> regions)
    {
        var projectedRegions = input.Projection.Regions;
        if (regions.Count != projectedRegions.Count)
        {
            throw new ArgumentException(
                "An Index plan requires one public region for every projected region.",
                nameof(regions));
        }

        for (var index = 0; index < projectedRegions.Count; index++)
        {
            ValidateRegion(input.Request.Mode, projectedRegions[index], regions[index]);
        }
    }

    private static void ValidateRegion(
        IndexMode mode,
        IndexProjectedRegion projected,
        IndexRegion region)
    {
        if (region.Source != projected.Source)
        {
            throw new ArgumentException(
                "An Index plan region must retain its projected logical source.",
                nameof(region));
        }

        switch (projected.Region.State)
        {
            case GeneratedNavigationRegionState.Available:
                ValidateAvailableRegion(mode, projected, region);
                return;
            case GeneratedNavigationRegionState.Unavailable:
                if (region.Action != IndexRegionAction.NotEstablished
                    || region.BeforeEntryCount is not null
                    || region.ExpectedEntryCount is not null
                    || region.Change is not null
                    || region.Outcome != IndexRegionOutcome.NotEstablished)
                {
                    throw new ArgumentException(
                        "An unavailable projection requires one not-established Index region.",
                        nameof(region));
                }

                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(projected),
                    projected.Region.State,
                    "The generated navigation region state is not defined.");
        }
    }

    private static void ValidateAvailableRegion(
        IndexMode mode,
        IndexProjectedRegion projected,
        IndexRegion region)
    {
        var change = projected.Region.Change
            ?? throw new ArgumentException(
                "An available projected region requires its bounded change.",
                nameof(projected));
        switch (change.Kind)
        {
            case GeneratedNavigationChangeKind.Unchanged:
                if (projected.BeforeEntryCount is null
                    || projected.ExpectedEntryCount is null
                    || region.Action != IndexRegionAction.Unchanged
                    || region.BeforeEntryCount != projected.BeforeEntryCount
                    || region.ExpectedEntryCount != projected.ExpectedEntryCount
                    || region.Change is not null
                    || region.Outcome != IndexRegionOutcome.AlreadyCurrent)
                {
                    throw new ArgumentException(
                        "An unchanged projection requires one already-current Index region with exact counts.",
                        nameof(region));
                }

                return;
            case GeneratedNavigationChangeKind.Update:
                var expectedOutcome = IndexPlanningOutcome.ReadInitial(mode);
                if (projected.ExpectedEntryCount is null
                    || region.Action != IndexRegionAction.Update
                    || region.BeforeEntryCount != projected.BeforeEntryCount
                    || region.ExpectedEntryCount != projected.ExpectedEntryCount
                    || region.Change is not { } publicChange
                    || !string.Equals(publicChange.BeforeBody, change.BeforeBody, StringComparison.Ordinal)
                    || !string.Equals(publicChange.ExpectedBody, change.ExpectedBody, StringComparison.Ordinal)
                    || region.Outcome != expectedOutcome)
                {
                    throw new ArgumentException(
                        "An updated projection requires one exact mode-specific Index update region.",
                        nameof(region));
                }

                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(projected),
                    change.Kind,
                    "The generated navigation change kind is not defined.");
        }
    }

    private static void ValidateRecoveryTargets(
        IndexPlanningInput input,
        IReadOnlyList<IndexRegion> regions,
        IReadOnlyList<RecoveryBundleTarget> recoveryTargets)
    {
        if (!input.Projection.IsComplete)
        {
            if (recoveryTargets.Count != 0)
            {
                throw new ArgumentException(
                    "An incomplete Index projection cannot expose recovery targets.",
                    nameof(recoveryTargets));
            }

            return;
        }

        var projectedUpdates = input.Projection.Regions
            .Where((_, index) => regions[index].Action == IndexRegionAction.Update)
            .ToArray();
        if (recoveryTargets.Count != projectedUpdates.Length)
        {
            throw new ArgumentException(
                "A complete Index plan requires one recovery target for every projected update.",
                nameof(recoveryTargets));
        }

        for (var index = 0; index < projectedUpdates.Length; index++)
        {
            ValidateRecoveryTarget(input.Request, projectedUpdates[index], recoveryTargets[index]);
        }
    }

    private static void ValidateRecoveryTarget(
        IndexRequest request,
        IndexProjectedRegion projected,
        RecoveryBundleTarget recoveryTarget)
    {
        var boundedChange = projected.Region.Change
            ?? throw new ArgumentException(
                "An executable Index update requires its bounded change.",
                nameof(projected));
        var logicalPath = SourceLogicalPath.ToLexicalPath(
            request.Workspace.LexicalRoot,
            projected.Source.Path);
        if (recoveryTarget.Change.Kind != PlannedFileChangeKind.ReplaceGeneratedRegion
            || !string.Equals(recoveryTarget.Change.LogicalPath, logicalPath, StringComparison.Ordinal)
            || !PhysicalIdentityTracker.PathComparer.Equals(
                recoveryTarget.Before.PhysicalPath,
                projected.Region.PhysicalPath)
            || !recoveryTarget.Before.Bytes.AsSpan().SequenceEqual(boundedChange.BeforeDocumentBytes.AsSpan())
            || !recoveryTarget.Change.IntendedBytes.AsSpan().SequenceEqual(boundedChange.ExpectedDocumentBytes.AsSpan()))
        {
            throw new ArgumentException(
                "An Index recovery target must retain the exact bounded existing-target replacement.",
                nameof(recoveryTarget));
        }
    }

}
