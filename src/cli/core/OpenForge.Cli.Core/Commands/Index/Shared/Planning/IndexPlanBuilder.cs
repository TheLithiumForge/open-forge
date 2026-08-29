using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Projection;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Planning;

internal sealed class IndexPlanBuilder
{
    internal IndexPlan Build(IndexPlanningInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Request);
        ArgumentNullException.ThrowIfNull(input.Projection);
        var regions = new List<IndexRegion>();
        var recoveryTargets = new List<RecoveryBundleTarget>();
        foreach (var projected in input.Projection.Regions)
        {
            var region = FormRegion(input, projected);
            regions.Add(region);
            if (input.Projection.IsComplete && region.Action == IndexRegionAction.Update)
            {
                recoveryTargets.Add(FormRecoveryTarget(input, projected));
            }
        }

        return new IndexPlan(
            input: input,
            regions: regions,
            recoveryTargets: recoveryTargets);
    }

    private static IndexRegion FormRegion(
        IndexPlanningInput input,
        IndexProjectedRegion projected)
    {
        return projected.Region.State switch
        {
            GeneratedNavigationRegionState.Available => FormAvailableRegion(input, projected),
            GeneratedNavigationRegionState.Unavailable => IndexRegion.NotEstablished(projected.Source),
            _ => throw new ArgumentOutOfRangeException(
                nameof(projected),
                projected.Region.State,
                "The generated navigation region state is not defined."),
        };
    }

    private static IndexRegion FormAvailableRegion(
        IndexPlanningInput input,
        IndexProjectedRegion projected)
    {
        var change = projected.Region.Change
            ?? throw new InvalidOperationException("An available projected region requires its bounded change.");
        return change.Kind switch
        {
            GeneratedNavigationChangeKind.Unchanged => IndexRegion.Unchanged(
                source: projected.Source,
                beforeEntryCount: projected.BeforeEntryCount
                    ?? throw new InvalidOperationException("An unchanged Index region requires its parsed before count."),
                expectedEntryCount: projected.ExpectedEntryCount
                    ?? throw new InvalidOperationException("An available Index region requires its expected count.")),
            GeneratedNavigationChangeKind.Update => IndexRegion.Update(
                projected.Source,
                new IndexRegionUpdate
                {
                    BeforeEntryCount = projected.BeforeEntryCount,
                    ExpectedEntryCount = projected.ExpectedEntryCount
                        ?? throw new InvalidOperationException("An available Index region requires its expected count."),
                    Change = new IndexChange(
                        beforeBody: change.BeforeBody,
                        expectedBody: change.ExpectedBody),
                    Outcome = IndexPlanningOutcome.ReadInitial(input.Request.Mode),
                }),
            _ => throw new ArgumentOutOfRangeException(
                nameof(projected),
                change.Kind,
                "The generated navigation change kind is not defined."),
        };
    }

    private static RecoveryBundleTarget FormRecoveryTarget(
        IndexPlanningInput input,
        IndexProjectedRegion projected)
    {
        var change = projected.Region.Change
            ?? throw new InvalidOperationException("An executable Index update requires its bounded change.");
        if (change.Kind != GeneratedNavigationChangeKind.Update)
        {
            throw new ArgumentException(
                "An Index recovery target requires an updated projected region.",
                nameof(projected));
        }

        var logicalPath = SourceLogicalPath.ToLexicalPath(
            input.Request.Workspace.LexicalRoot,
            projected.Source.Path);
        var before = FileStateSnapshot.File(
            logicalPath: logicalPath,
            physicalPath: projected.Region.PhysicalPath,
            bytes: change.BeforeDocumentBytes.AsSpan());
        var plannedChange = PlannedFileChange.ReplaceGeneratedRegion(
            expectation: before.Expectation,
            intendedDocumentBytes: change.ExpectedDocumentBytes.AsSpan());
        return RecoveryBundleTarget.Create(
            change: plannedChange,
            before: before);
    }
}
