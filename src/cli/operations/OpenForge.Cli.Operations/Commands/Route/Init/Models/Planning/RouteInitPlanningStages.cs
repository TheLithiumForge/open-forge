using OpenForge.Cli.Core.Framework.Ownership.Models;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;

internal sealed record RouteInitFrameworkPlanningBasis(
    RouteInitFrameworkAlignment Alignment,
    FrameworkPayload Payload,
    RouteInitFrameworkTrust Trust);

internal sealed record RouteInitInspectionFacts(
    RouteInitTargetFacts Target,
    RouteInitCurrentCatalogueFacts Catalogue,
    RouteInitCurrentStateFacts Current,
    RouteInitFrameworkPlanningBasis? Framework)
{
    public required WorkspaceSettingsRead Settings { get; init; }

    internal RouteInitRestoration? Restoration { get; init; }

    internal WorkspaceOwnershipRead? SharingOwnership { get; init; }
}

internal abstract record RouteInitInspectionResult;

internal sealed record RouteInitInspectionCompleted(RouteInitInspectionFacts Facts)
    : RouteInitInspectionResult;

internal sealed record RouteInitInspectionStopped(RouteInitPlanningBoundary Boundary)
    : RouteInitInspectionResult;

internal sealed record RouteInitIntendedEntrypoint(
    RouteInitCurrentChainEntry Chain,
    RouteInitProspectiveSourceContent Content,
    RouteInitMetadata? Metadata,
    RouteInitEntrypointOwnership Ownership)
{
    internal string? SourceAssetPath => Content.SourceAssetPath;

    internal SourceLogicalSource Source => Content.Source;
}

internal sealed record RouteInitIntendedChain
{
    internal RouteInitIntendedChain(IEnumerable<RouteInitIntendedEntrypoint> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Entries = entries
            .Select(entry => entry ?? throw new ArgumentException(
                "A Route Init intended chain cannot contain null entrypoints.",
                nameof(entries)))
            .ToImmutableArray();
    }

    internal ImmutableArray<RouteInitIntendedEntrypoint> Entries { get; }
}

internal abstract record RouteInitIntendedChainResult;

internal sealed record RouteInitIntendedChainCompleted(RouteInitIntendedChain Chain)
    : RouteInitIntendedChainResult;

internal sealed record RouteInitIntendedChainStopped(RouteInitPlanningBoundary Boundary)
    : RouteInitIntendedChainResult;

internal sealed record RouteInitProspectivePlanFacts
{
    internal RouteInitProspectivePlanFacts(
        RouteInitProspectiveTopology topology,
        GeneratedNavigationProjection projection,
        IEnumerable<FileStateSnapshot> directoryStates,
        IEnumerable<RouteInitProspectiveSourceContent> effectSources,
        RouteInitProspectiveEffectPlan effects)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(directoryStates);
        ArgumentNullException.ThrowIfNull(effectSources);
        ArgumentNullException.ThrowIfNull(effects);
        Topology = topology;
        Projection = projection;
        DirectoryStates = directoryStates.ToImmutableArray();
        EffectSources = effectSources.ToImmutableArray();
        Effects = effects;
    }

    internal RouteInitProspectiveTopology Topology { get; }

    internal GeneratedNavigationProjection Projection { get; }

    internal ImmutableArray<FileStateSnapshot> DirectoryStates { get; }

    internal ImmutableArray<RouteInitProspectiveSourceContent> EffectSources { get; }

    internal RouteInitProspectiveEffectPlan Effects { get; }
}

internal abstract record RouteInitProspectivePlanResult;

internal sealed record RouteInitProspectivePlanCompleted(RouteInitProspectivePlanFacts Facts)
    : RouteInitProspectivePlanResult;

internal sealed record RouteInitProspectivePlanStopped(RouteInitPlanningBoundary Boundary)
    : RouteInitProspectivePlanResult;

internal sealed record RouteInitPlanFinalizationFacts(
    RouteInitProspectiveEffectPlan Effects,
    OwnershipWritePlanState? OwnershipState);

internal abstract record RouteInitPlanFinalizationResult;

internal sealed record RouteInitPlanFinalizationCompleted(RouteInitPlanFinalizationFacts Facts)
    : RouteInitPlanFinalizationResult;

internal sealed record RouteInitPlanFinalizationStopped(RouteInitPlanningBoundary Boundary)
    : RouteInitPlanFinalizationResult;
