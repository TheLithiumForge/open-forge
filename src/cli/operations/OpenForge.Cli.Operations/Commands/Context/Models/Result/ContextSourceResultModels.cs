using OpenForge.Cli.Core.Commands.Context.Shared.Result;
namespace OpenForge.Cli.Core.Commands.Context.Models.Result;

internal sealed record ContextPathProjection
{
    internal ContextPathProjection(
        int position,
        int sourcePosition,
        string? id,
        string path,
        ContextSourceLayerKind layer,
        IEnumerable<ContextInclusionReason> inclusionReasons)
    {
        if (position < 1 || sourcePosition < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position, "Context path coordinates must be positive.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Enum.IsDefined(layer))
        {
            throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Context source layer is not defined.");
        }

        Position = position;
        SourcePosition = sourcePosition;
        Id = id;
        Path = path;
        Layer = layer;
        InclusionReasons = ContextResultCollections.Snapshot(inclusionReasons, nameof(inclusionReasons));
    }

    internal int Position { get; }

    internal int SourcePosition { get; }

    internal string? Id { get; }

    internal string Path { get; }

    internal ContextSourceLayerKind Layer { get; }

    internal IReadOnlyList<ContextInclusionReason> InclusionReasons { get; }
}

internal sealed record ContextLayer
{
    internal ContextLayer(
        int pathPosition,
        ContextSourceLayerKind kind,
        string path,
        IEnumerable<ContextInclusionReason> inclusionReasons,
        IEnumerable<ContextProjection> projections)
    {
        if (pathPosition < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pathPosition), pathPosition, "A Context path position must be positive.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Context source layer is not defined.");
        }

        PathPosition = pathPosition;
        Kind = kind;
        Path = path;
        InclusionReasons = ContextResultCollections.Snapshot(inclusionReasons, nameof(inclusionReasons));
        Projections = ContextResultCollections.Snapshot(projections, nameof(projections));
    }

    internal int PathPosition { get; }

    internal ContextSourceLayerKind Kind { get; }

    internal string Path { get; }

    internal IReadOnlyList<ContextInclusionReason> InclusionReasons { get; }

    internal IReadOnlyList<ContextProjection> Projections { get; }
}

internal sealed record ContextSource
{
    internal ContextSource(
        int position,
        string? id,
        string path,
        ContextRouteState routeState,
        string? route,
        string? scope,
        IEnumerable<ContextInclusionReason> inclusionReasons,
        IEnumerable<ContextLayer> layers,
        ContextSourceMetadata? metadata = null)
    {
        if (position < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position, "A Context source position must be positive.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Enum.IsDefined(routeState))
        {
            throw new ArgumentOutOfRangeException(nameof(routeState), routeState, "The Context route state is not defined.");
        }

        Position = position;
        Id = id;
        Path = path;
        RouteState = routeState;
        Route = route;
        Scope = scope;
        Metadata = metadata ?? ContextSourceMetadata.WithoutValues(ContextMetadataState.Unavailable);
        InclusionReasons = ContextResultCollections.Snapshot(inclusionReasons, nameof(inclusionReasons));
        Layers = ContextResultCollections.Snapshot(layers, nameof(layers));
    }

    internal int Position { get; }

    internal string? Id { get; }

    internal string Path { get; }

    internal ContextRouteState RouteState { get; }

    internal string? Route { get; }

    internal string? Scope { get; }

    internal ContextSourceMetadata Metadata { get; }

    internal IReadOnlyList<ContextInclusionReason> InclusionReasons { get; }

    internal IReadOnlyList<ContextLayer> Layers { get; }
}
