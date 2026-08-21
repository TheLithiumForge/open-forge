namespace OpenForge.Cli.Core.Commands.Route.List;

internal enum RouteListSelectionKind
{
    LoaderRoots,
    SourceId,
    SourcePath,
}

internal sealed class RouteListSelection
{
    internal RouteListSelection(
        object constructionToken,
        RouteListSelectionKind kind,
        string? attemptedId,
        string? attemptedPath,
        string? resolvedId,
        string? resolvedPath)
    {
        if (!Shared.Selection.RouteListSelectionFactory.OwnsConstructionToken(constructionToken))
        {
            throw new InvalidOperationException("Route-list selection construction is owned by its factory.");
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The route-list selection kind is not defined.");
        }

        ValidateIdentityShape(kind, attemptedId, attemptedPath, resolvedId, resolvedPath);
        Kind = kind;
        AttemptedId = attemptedId;
        AttemptedPath = attemptedPath;
        ResolvedId = resolvedId;
        ResolvedPath = resolvedPath;
    }

    internal RouteListSelectionKind Kind { get; }

    internal string? AttemptedId { get; }

    internal string? AttemptedPath { get; }

    internal string? ResolvedId { get; }

    internal string? ResolvedPath { get; }

    internal bool IsResolved => Kind == RouteListSelectionKind.LoaderRoots || ResolvedPath is not null;

    private static void ValidateIdentityShape(
        RouteListSelectionKind kind,
        string? attemptedId,
        string? attemptedPath,
        string? resolvedId,
        string? resolvedPath)
    {
        var hasResolvedIdentity = resolvedId is not null || resolvedPath is not null;
        if ((resolvedId is null) != (resolvedPath is null))
        {
            throw new ArgumentException("Resolved source ID and path must both be present or both be absent.");
        }

        switch (kind)
        {
            case RouteListSelectionKind.LoaderRoots:
                if (attemptedId is not null || attemptedPath is not null || hasResolvedIdentity)
                {
                    throw new ArgumentException("Loader-root selection cannot contain explicit source identity.");
                }

                break;
            case RouteListSelectionKind.SourceId:
                ArgumentException.ThrowIfNullOrWhiteSpace(attemptedId);
                if (attemptedPath is not null)
                {
                    throw new ArgumentException("Source-ID selection cannot contain an attempted path.");
                }

                break;
            case RouteListSelectionKind.SourcePath:
                ArgumentException.ThrowIfNullOrWhiteSpace(attemptedPath);
                if (attemptedId is not null)
                {
                    throw new ArgumentException("Source-path selection cannot contain an attempted ID.");
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "The route-list selection kind is not defined.");
        }

        if (resolvedId is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(resolvedId);
            ArgumentException.ThrowIfNullOrWhiteSpace(resolvedPath);
        }
    }
}
