using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Routing;

internal enum SourceRouteParentState
{
    None,
    Resolved,
    Ambiguous,
}

internal sealed class SourceRouteNode
{
    internal SourceRouteNode(
        SourceLogicalIdentity identity,
        SourceRouteParentState parentState,
        IEnumerable<string> parentPaths,
        IEnumerable<string> childPaths)
    {
        ArgumentNullException.ThrowIfNull(identity);
        if (!Enum.IsDefined(parentState))
        {
            throw new ArgumentOutOfRangeException(nameof(parentState), parentState, "The source route parent state is not defined.");
        }

        var parents = MaterializePaths(parentPaths, nameof(parentPaths));
        if (parentState == SourceRouteParentState.None && parents.Count != 0
            || parentState == SourceRouteParentState.Resolved && parents.Count != 1
            || parentState == SourceRouteParentState.Ambiguous && parents.Count < 2)
        {
            throw new ArgumentException("The source route parent paths do not match the parent state.", nameof(parentPaths));
        }

        var children = MaterializePaths(childPaths, nameof(childPaths));
        if (parents.Contains(identity.CanonicalBasePath, StringComparer.Ordinal)
            || children.Contains(identity.CanonicalBasePath, StringComparer.Ordinal))
        {
            throw new ArgumentException("A source route node cannot be its own parent or child.");
        }

        Identity = identity;
        ParentState = parentState;
        ParentPaths = parents;
        ChildPaths = children;
    }

    internal SourceLogicalIdentity Identity { get; }

    internal SourceRouteParentState ParentState { get; }

    internal IReadOnlyList<string> ParentPaths { get; }

    internal IReadOnlyList<string> ChildPaths { get; }

    private static IReadOnlyList<string> MaterializePaths(
        IEnumerable<string> paths,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(paths, parameterName);
        var materialized = paths.ToArray();
        if (materialized.Any(path => !SourceLogicalPath.IsCanonicalSource(path))
            || materialized.Distinct(StringComparer.Ordinal).Count() != materialized.Length)
        {
            throw new ArgumentException("Source route relationship paths must be unique canonical logical paths.", parameterName);
        }

        return new ReadOnlyCollection<string>(materialized.OrderBy(path => path, StringComparer.Ordinal).ToArray());
    }
}
