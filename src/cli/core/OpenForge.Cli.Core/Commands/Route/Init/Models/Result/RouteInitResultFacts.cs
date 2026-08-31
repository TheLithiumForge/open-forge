using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Result;

internal sealed record RouteInitTarget(
    string Requested,
    string? Id,
    string? Path);

internal sealed record RouteInitPlanFacts(
    RouteInitPlanCompleteness Completeness,
    RouteInitPlanSafety Safety);

internal sealed record RouteInitFrameworkSegment(
    string Path,
    RouteInitFrameworkSegmentRole Role,
    string? SourceAssetPath);

internal sealed record RouteInitFramework
{
    internal RouteInitFramework(
        string inventoryFingerprint,
        IEnumerable<RouteInitFrameworkSegment> segments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inventoryFingerprint);
        ArgumentNullException.ThrowIfNull(segments);
        InventoryFingerprint = inventoryFingerprint;
        Segments = segments
            .Select(segment => segment ?? throw new ArgumentException(
                "Route Init Framework segments cannot contain null members.",
                nameof(segments)))
            .ToImmutableArray();
    }

    internal string InventoryFingerprint { get; }

    internal ImmutableArray<RouteInitFrameworkSegment> Segments { get; }
}

internal sealed record RouteInitMetadata
{
    internal RouteInitMetadata(
        string description,
        RouteInitDescriptionSource descriptionSource,
        string? responsibility,
        RouteInitResponsibilitySource responsibilitySource,
        IEnumerable<string> tags,
        RouteInitTagsSource tagsSource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(tags);
        Description = description;
        DescriptionSource = descriptionSource;
        Responsibility = responsibility;
        ResponsibilitySource = responsibilitySource;
        Tags = tags
            .Select(tag => tag ?? throw new ArgumentException(
                "Route Init metadata tags cannot contain null members.",
                nameof(tags)))
            .ToImmutableArray();
        TagsSource = tagsSource;
    }

    internal string Description { get; }

    internal RouteInitDescriptionSource DescriptionSource { get; }

    internal string? Responsibility { get; }

    internal RouteInitResponsibilitySource ResponsibilitySource { get; }

    internal ImmutableArray<string> Tags { get; }

    internal RouteInitTagsSource TagsSource { get; }
}

internal sealed record RouteInitEntrypoint(
    string Id,
    string Path,
    RouteInitEntrypointForm Form,
    RouteInitEntrypointCurrent Current,
    RouteInitEntrypointOwnership Ownership,
    RouteInitMetadata? Metadata,
    string? SourceAssetPath,
    RouteInitEntrypointOutcome Outcome);

internal sealed record RouteInitEffectChange(
    string? Before,
    string Expected);

internal sealed record RouteInitEffect(
    string Path,
    RouteInitEffectKind Kind,
    RouteInitEffectAction Action,
    string? SourceAssetPath,
    RouteInitEffectChange? Change,
    RouteInitEffectOutcome Outcome,
    RouteInitEffectResidual Residual);

internal sealed record RouteInitLifecycle(
    RouteInitLifecycleAction Action,
    RouteInitLifecycleOutcome Outcome);

internal sealed record RouteInitRecovery(
    RouteInitRecoveryState State,
    string? ResidualPath);
