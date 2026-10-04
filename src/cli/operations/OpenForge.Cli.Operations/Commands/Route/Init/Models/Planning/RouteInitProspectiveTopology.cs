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

using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;

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
