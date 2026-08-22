using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;

internal enum RouteTopologyParentState
{
    None,
    Resolved,
    Ambiguous,
}

internal sealed class RouteTopologyNode
{
    internal RouteTopologyNode(
        RouteSource source,
        RouteTopologyParentState parentState,
        IEnumerable<string> parentPaths,
        IEnumerable<string> childPaths)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!Enum.IsDefined(parentState))
        {
            throw new ArgumentOutOfRangeException(nameof(parentState), parentState, "The topology parent state is not defined.");
        }

        if (source.Kind is not (
            RouteSourceKind.Entrypoint
            or RouteSourceKind.Markdown
            or RouteSourceKind.Native))
        {
            throw new ArgumentException("A topology node requires a routed source.", nameof(source));
        }

        var parents = MaterializePaths(parentPaths, nameof(parentPaths));
        if (parentState == RouteTopologyParentState.None && parents.Count != 0
            || parentState == RouteTopologyParentState.Resolved && parents.Count != 1
            || parentState == RouteTopologyParentState.Ambiguous && parents.Count < 2)
        {
            throw new ArgumentException("The topology parent paths do not match the parent state.", nameof(parentPaths));
        }

        var children = MaterializePaths(childPaths, nameof(childPaths));
        if (source.Kind != RouteSourceKind.Entrypoint && children.Count != 0)
        {
            throw new ArgumentException("Only an entrypoint can have routed children.", nameof(childPaths));
        }

        if (parents.Contains(source.CanonicalPath, StringComparer.Ordinal)
            || children.Contains(source.CanonicalPath, StringComparer.Ordinal))
        {
            throw new ArgumentException("A topology node cannot be its own parent or child.");
        }

        Source = source;
        ParentState = parentState;
        ParentPaths = parents;
        ChildPaths = children;
    }

    internal RouteSource Source { get; }

    internal RouteTopologyParentState ParentState { get; }

    internal IReadOnlyList<string> ParentPaths { get; }

    internal string? ParentPath => ParentState == RouteTopologyParentState.Resolved
        ? ParentPaths[0]
        : null;

    internal IReadOnlyList<string> ChildPaths { get; }

    internal bool HasCompleteMetadata => Source.Metadata.State == RouteSourceMetadataState.Complete;

    private static IReadOnlyList<string> MaterializePaths(
        IEnumerable<string> paths,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(paths, parameterName);
        var materialized = paths.ToArray();
        if (materialized.Any(path => !RouteLogicalPath.IsCanonical(path))
            || materialized.Distinct(StringComparer.Ordinal).Count() != materialized.Length)
        {
            throw new ArgumentException("Topology relationship paths must be unique canonical logical paths.", parameterName);
        }

        return new ReadOnlyCollection<string>(materialized.OrderBy(path => path, StringComparer.Ordinal).ToArray());
    }

}
