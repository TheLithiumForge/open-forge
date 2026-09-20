using System.Collections.Immutable;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed record RouteInitProspectiveTopology
{
    internal RouteInitProspectiveTopology(
        RouteInitCurrentCatalogueFacts current,
        GeneratedNavigationFormation formation,
        IEnumerable<SourceLogicalSource> addedSources)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(formation);
        Current = current;
        Formation = formation;
        AddedSources = new ReadOnlyCollection<SourceLogicalSource>(addedSources
            .Select(source => source ?? throw new ArgumentException(
                "Route Init prospective sources cannot contain null members.",
                nameof(addedSources)))
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray());
    }

    internal RouteInitCurrentCatalogueFacts Current { get; }

    internal GeneratedNavigationFormation Formation { get; }

    internal IReadOnlyList<SourceLogicalSource> AddedSources { get; }

    internal bool IsSafeForProjection => !Current.IsCancelled
        && Formation.Issues.All(issue => issue.Code == SourceCatalogueIssueCode.RootMissing)
        && Formation.Ambiguities.Count == 0
        && Formation.IntendedTargetCollisions.Count == 0
        && Formation.PhysicalAliasGroups.All(group =>
            group.Compatibility == GeneratedNavigationPhysicalAliasCompatibility.Compatible);
}

internal enum RouteInitProspectiveProjectionState
{
    Complete,
    Unavailable,
    Blocked,
}

internal sealed record RouteInitProspectiveProjection(
    RouteInitProspectiveProjectionState State,
    GeneratedNavigationProjection? Projection,
    string? Cause);

internal sealed record RouteInitProspectiveSourceContent
{
    internal RouteInitProspectiveSourceContent(
        SourceLogicalSource source,
        FileStateSnapshot before,
        ReadOnlySpan<byte> intendedBytes,
        string? sourceAssetPath,
        bool ownsGeneratedEntries)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(before);
        Source = source;
        Before = before;
        IntendedBytes = ImmutableArray.CreateRange(intendedBytes.ToArray());
        SourceAssetPath = sourceAssetPath;
        OwnsGeneratedEntries = ownsGeneratedEntries;
    }

    internal SourceLogicalSource Source { get; }

    internal FileStateSnapshot Before { get; }

    internal ImmutableArray<byte> IntendedBytes { get; }

    internal string? SourceAssetPath { get; }

    internal bool OwnsGeneratedEntries { get; }
}

internal sealed record RouteInitOwnershipEffectInput(
    OwnershipWritePlanResult WritePlan,
    FileStateSnapshot? Before);

internal sealed record RouteInitProspectiveEffectPlan
{
    internal RouteInitProspectiveEffectPlan(
        IEnumerable<PlannedDirectoryCreation> directoryCreations,
        IEnumerable<PlannedFileChange> fileChanges,
        IEnumerable<RecoveryBundleTarget> recoveryTargets,
        IEnumerable<RouteInitFrameworkManagedState> frameworkManagedStates)
    {
        DirectoryCreations = new ReadOnlyCollection<PlannedDirectoryCreation>(directoryCreations.ToArray());
        FileChanges = new ReadOnlyCollection<PlannedFileChange>(fileChanges.ToArray());
        RecoveryTargets = new ReadOnlyCollection<RecoveryBundleTarget>(recoveryTargets.ToArray());
        FrameworkManagedStates = new ReadOnlyCollection<RouteInitFrameworkManagedState>(frameworkManagedStates.ToArray());
    }

    internal IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations { get; }

    internal IReadOnlyList<PlannedFileChange> FileChanges { get; }

    internal IReadOnlyList<RecoveryBundleTarget> RecoveryTargets { get; }

    internal IReadOnlyList<RouteInitFrameworkManagedState> FrameworkManagedStates { get; }
}

internal enum RouteInitProspectiveEffectPlanState
{
    Complete,
    Blocked,
}

internal sealed record RouteInitProspectiveEffectPlanBuild(
    RouteInitProspectiveEffectPlanState State,
    RouteInitProspectiveEffectPlan? Plan,
    string? Cause);

internal sealed class RouteInitProspectiveTopologyPlanner
{
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly GeneratedNavigationProjector _projector = new();

    internal RouteInitProspectiveTopology Build(
        RouteInitCurrentCatalogueFacts current,
        IEnumerable<SourceLogicalSource> prospectiveSources)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(prospectiveSources);
        var proposed = prospectiveSources
            .Select(source => source ?? throw new ArgumentException(
                "Route Init prospective sources cannot contain null members.",
                nameof(prospectiveSources)))
            .ToArray();
        if (proposed.Select(source => source.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != proposed.Length)
        {
            throw new ArgumentException(
                "Route Init prospective sources require unique canonical paths.",
                nameof(prospectiveSources));
        }

        var observedByPath = current.ObservedSources.ToDictionary(
            source => source.Identity.CanonicalBasePath,
            StringComparer.Ordinal);
        var added = proposed
            .Where(source => !observedByPath.ContainsKey(source.Identity.CanonicalBasePath))
            .ToArray();
        var intended = current.ObservedSources
            .Concat(added)
            .ToArray();
        return new RouteInitProspectiveTopology(
            current,
            _formationBuilder.Build(current.Catalogue, intended),
            added);
    }

    internal RouteInitProspectiveProjection Project(
        RouteInitProspectiveTopology topology,
        IEnumerable<GeneratedNavigationRegionInput> regions,
        IEnumerable<GeneratedNavigationMetadata> metadata)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(regions);
        ArgumentNullException.ThrowIfNull(metadata);
        if (!topology.IsSafeForProjection)
        {
            return new RouteInitProspectiveProjection(
                RouteInitProspectiveProjectionState.Blocked,
                Projection: null,
                "The prospective route topology contains unavailable, ambiguous, colliding, or unsafe observed facts.");
        }

        var projection = _projector.Project(new GeneratedNavigationProjectionRequest(
            topology.Formation,
            regions,
            metadata));
        if (!projection.IsComplete)
        {
            var decisive = projection.Regions.First(region =>
                region.State != GeneratedNavigationRegionState.Available);
            return new RouteInitProspectiveProjection(
                IsUnsafe(decisive.UnavailableReason)
                    ? RouteInitProspectiveProjectionState.Blocked
                    : RouteInitProspectiveProjectionState.Unavailable,
                projection,
                decisive.Cause ?? "The prospective generated-navigation projection is unavailable.");
        }

        return new RouteInitProspectiveProjection(
            RouteInitProspectiveProjectionState.Complete,
            projection,
            Cause: null);
    }

    private static bool IsUnsafe(GeneratedNavigationRegionUnavailableReason? reason)
        => reason is GeneratedNavigationRegionUnavailableReason.GeneratedRegionInvalid
            or GeneratedNavigationRegionUnavailableReason.GeneratedRegionLineEndingUnsupported
            or GeneratedNavigationRegionUnavailableReason.TopologyUnsafe
            or GeneratedNavigationRegionUnavailableReason.MetadataInvalid
            or GeneratedNavigationRegionUnavailableReason.MetadataUnrepresentable
            or GeneratedNavigationRegionUnavailableReason.DestinationUnsafe
            or GeneratedNavigationRegionUnavailableReason.DestinationConflict;

    internal RouteInitProspectiveEffectPlanBuild AssembleEffects(
        RouteInitProspectiveTopology topology,
        GeneratedNavigationProjection projection,
        IEnumerable<FileStateSnapshot> directoryStates,
        IEnumerable<RouteInitProspectiveSourceContent> sourceContents,
        RouteInitOwnershipEffectInput? ownershipEffect = null)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(directoryStates);
        ArgumentNullException.ThrowIfNull(sourceContents);
        if (!topology.IsSafeForProjection || !projection.IsComplete)
        {
            return BlockedEffects(
                "Route Init effects require one complete safe prospective topology and projection.");
        }

        try
        {
            var directories = directoryStates
                .Select(state => PlannedDirectoryCreation.Create(state.Expectation))
                .ToArray();
            var changes = new List<PlannedFileChange>();
            var recovery = new List<RecoveryBundleTarget>();
            var managed = new List<RouteInitFrameworkManagedState>();
            var regionsByPath = projection.Regions.ToDictionary(
                region => region.CanonicalPath,
                StringComparer.Ordinal);
            foreach (var content in sourceContents)
            {
                var member = topology.Formation.FindSource(content.Source.Identity.CanonicalBasePath);
                if (!ReferenceEquals(member, content.Source))
                {
                    return BlockedEffects(
                        "Every Route Init effect source must retain its exact prospective formation member.");
                }

                regionsByPath.TryGetValue(content.Source.Identity.CanonicalBasePath, out var region);
                var finalBytes = ReadFinalBytes(content, region);
                if (content.Before.Kind == FileExpectationKind.Missing)
                {
                    if (region is null)
                    {
                        return BlockedEffects(
                            "Every new Route Init entrypoint requires one complete destination-local generated region.");
                    }

                    changes.Add(PlannedFileChange.Create(
                        content.Before.Expectation,
                        finalBytes.AsSpan()));
                    if (content.SourceAssetPath is not null || content.OwnsGeneratedEntries)
                    {
                        managed.Add(new RouteInitFrameworkManagedState(
                            content.Source.Identity.CanonicalBasePath,
                            content.SourceAssetPath,
                            content.OwnsGeneratedEntries));
                    }

                    continue;
                }

                if (content.Before.Kind != FileExpectationKind.File || !content.Before.HasBytes)
                {
                    return BlockedEffects(
                        "An existing Route Init generated-region effect requires one exact prior file snapshot.");
                }

                if (region?.Change?.RequiresUpdate != true)
                {
                    continue;
                }

                var change = PlannedFileChange.ReplaceGeneratedRegion(
                    content.Before.Expectation,
                    finalBytes.AsSpan());
                changes.Add(change);
                recovery.Add(RecoveryBundleTarget.Create(change, content.Before));
                if (content.SourceAssetPath is not null || content.OwnsGeneratedEntries)
                {
                    managed.Add(new RouteInitFrameworkManagedState(
                        content.Source.Identity.CanonicalBasePath,
                        content.SourceAssetPath,
                        content.OwnsGeneratedEntries));
                }
            }

            if (ownershipEffect is not null)
            {
                var boundary = AddOwnershipEffect(ownershipEffect, changes, recovery);
                if (boundary is not null)
                {
                    return BlockedEffects(boundary);
                }
            }

            return new RouteInitProspectiveEffectPlanBuild(
                RouteInitProspectiveEffectPlanState.Complete,
                new RouteInitProspectiveEffectPlan(directories, changes, recovery, managed),
                Cause: null);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidOperationException)
        {
            return BlockedEffects($"The prospective Route Init effect plan is invalid: {exception.Message}");
        }
    }

    private static ImmutableArray<byte> ReadFinalBytes(
        RouteInitProspectiveSourceContent content,
        GeneratedNavigationRegion? region)
    {
        if (region is null)
        {
            return content.IntendedBytes;
        }

        if (region.State != GeneratedNavigationRegionState.Available
            || region.Change is not { } change)
        {
            throw new ArgumentException(
                "A Route Init effect region must carry one complete bounded change.",
                nameof(region));
        }

        return change.ExpectedDocumentBytes;
    }

    private static string? AddOwnershipEffect(
        RouteInitOwnershipEffectInput ownershipEffect,
        ICollection<PlannedFileChange> changes,
        ICollection<RecoveryBundleTarget> recovery)
    {
        return ownershipEffect.WritePlan.State switch
        {
            OwnershipWritePlanState.Unchanged
                or OwnershipWritePlanState.Skipped => null,
            OwnershipWritePlanState.Planned => AddPlannedOwnershipEffect(
                ownershipEffect,
                changes,
                recovery),
            _ => throw new ArgumentOutOfRangeException(
                nameof(ownershipEffect),
                ownershipEffect.WritePlan.State,
                "The ownership write-plan state is not defined."),
        };
    }

    private static string? AddPlannedOwnershipEffect(
        RouteInitOwnershipEffectInput ownershipEffect,
        ICollection<PlannedFileChange> changes,
        ICollection<RecoveryBundleTarget> recovery)
    {
        var change = ownershipEffect.WritePlan.Change
            ?? throw new ArgumentException(
                "A planned Route Init ownership effect requires one file change.",
                nameof(ownershipEffect));
        if (change.Kind == PlannedFileChangeKind.Create)
        {
            changes.Add(change);
            return null;
        }

        if (change.Kind != PlannedFileChangeKind.Replace
            || ownershipEffect.Before is not { } before
            || change.Expectation != before.Expectation
            || !before.HasBytes)
        {
            return "A Route Init ownership effect requires one exact existing-target recovery snapshot.";
        }

        changes.Add(change);
        recovery.Add(RecoveryBundleTarget.Create(change, before));
        return null;
    }

    private static RouteInitProspectiveEffectPlanBuild BlockedEffects(string cause)
        => new(RouteInitProspectiveEffectPlanState.Blocked, Plan: null, cause);
}
