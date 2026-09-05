using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models;

internal enum RouteDeclaredRootState
{
    Missing,
    Unreachable,
}

internal sealed class RouteDeclaredRootObservation
{
    private RouteDeclaredRootObservation(
        RouteDeclaredRootState state,
        string path,
        string loaderPath)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The declared-root state is not defined.");
        }

        if (!SourceLogicalPath.IsCanonicalSource(path)
            || !SourceLogicalPath.IsCanonicalSource(loaderPath))
        {
            throw new ArgumentException("Declared-root observations require canonical source paths.");
        }

        State = state;
        Path = path;
        LoaderPath = loaderPath;
    }

    internal RouteDeclaredRootState State { get; }

    internal string Path { get; }

    internal string LoaderPath { get; }

    internal static RouteDeclaredRootObservation Missing(string path, string loaderPath)
        => new(RouteDeclaredRootState.Missing, path, loaderPath);

    internal static RouteDeclaredRootObservation Unreachable(string path, string loaderPath)
        => new(RouteDeclaredRootState.Unreachable, path, loaderPath);
}

internal enum RouteShapeObservationKind
{
    EntrypointDuplicate,
    Unreachable,
    Detached,
    OverwriteIndependentIndex,
}

internal sealed class RouteShapeObservation
{
    private RouteShapeObservation(
        RouteShapeObservationKind kind,
        string path,
        IReadOnlyList<string> relatedPaths,
        string? authoredValue,
        SourceLocation? location)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The route-shape observation kind is not defined.");
        }

        if (!SourceLogicalPath.IsCanonicalSource(path)
            || relatedPaths.Any(related => !SourceLogicalPath.IsCanonicalSource(related))
            || relatedPaths.Distinct(StringComparer.Ordinal).Count() != relatedPaths.Count)
        {
            throw new ArgumentException("Route-shape observations require unique canonical source paths.");
        }

        var compatible = kind switch
        {
            RouteShapeObservationKind.EntrypointDuplicate =>
                relatedPaths.Count >= 2 && authoredValue is null && location is null,
            RouteShapeObservationKind.Unreachable =>
                relatedPaths.Count == 0 && authoredValue is null && location is null,
            RouteShapeObservationKind.Detached =>
                relatedPaths.Count > 0 && authoredValue is null && location is null,
            RouteShapeObservationKind.OverwriteIndependentIndex =>
                relatedPaths.Count == 1
                && !string.IsNullOrWhiteSpace(authoredValue)
                && location is not null,
            _ => false,
        };
        if (!compatible)
        {
            throw new ArgumentException("The route-shape observation fields do not match their kind.", nameof(kind));
        }

        Kind = kind;
        Path = path;
        RelatedPaths = relatedPaths.ToArray();
        AuthoredValue = authoredValue;
        Location = location;
    }

    internal RouteShapeObservationKind Kind { get; }

    internal string Path { get; }

    internal IReadOnlyList<string> RelatedPaths { get; }

    internal string? AuthoredValue { get; }

    internal SourceLocation? Location { get; }

    internal static RouteShapeObservation EntrypointDuplicate(IEnumerable<string> paths)
    {
        var ordered = Materialize(paths);
        if (ordered.Count < 2)
        {
            throw new ArgumentException(
                "A duplicate entrypoint observation requires at least two paths.",
                nameof(paths));
        }

        return new(
            RouteShapeObservationKind.EntrypointDuplicate,
            ordered[0],
            ordered,
            authoredValue: null,
            location: null);
    }

    internal static RouteShapeObservation Unreachable(string path)
        => new(RouteShapeObservationKind.Unreachable, path, [], authoredValue: null, location: null);

    internal static RouteShapeObservation Detached(string path, IEnumerable<string> childPaths)
        => new(
            RouteShapeObservationKind.Detached,
            path,
            Materialize(childPaths),
            authoredValue: null,
            location: null);

    internal static RouteShapeObservation OverwriteIndependentIndex(
        string path,
        string overwritePath,
        string authoredValue,
        SourceLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        return new(
            RouteShapeObservationKind.OverwriteIndependentIndex,
            path,
            [overwritePath],
            authoredValue,
            location);
    }

    private static IReadOnlyList<string> Materialize(IEnumerable<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        return paths.OrderBy(path => path, StringComparer.Ordinal).ToArray();
    }
}
