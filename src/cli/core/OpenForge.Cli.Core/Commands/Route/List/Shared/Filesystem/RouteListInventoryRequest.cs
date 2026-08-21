using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed record RouteListInventoryRequest
{
    internal RouteListInventoryRequest(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        : this(workspace, [RouteListLogicalPath.AgentsRoot], cancellationToken)
    {
    }

    internal RouteListInventoryRequest(
        CliWorkspace workspace,
        IEnumerable<string> logicalRoots,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(logicalRoots);
        var roots = logicalRoots.ToArray();
        if (roots.Length == 0)
        {
            throw new ArgumentException("Route-list inventory requires at least one logical root.", nameof(logicalRoots));
        }

        if (roots.Any(root => !RouteListLogicalPath.IsCanonical(root)))
        {
            throw new ArgumentException("Every route-list inventory root must be a canonical .agents logical path.", nameof(logicalRoots));
        }

        var orderedRoots = roots.OrderBy(root => root, StringComparer.Ordinal).ToArray();
        for (var index = 0; index < orderedRoots.Length; index++)
        {
            if (index > 0 && IsSameOrDescendant(orderedRoots[index - 1], orderedRoots[index]))
            {
                throw new ArgumentException("Route-list inventory roots cannot duplicate or overlap.", nameof(logicalRoots));
            }
        }

        Workspace = workspace;
        LogicalRoots = new ReadOnlyCollection<string>(orderedRoots);
        CancellationToken = cancellationToken;
    }

    internal CliWorkspace Workspace { get; }

    internal IReadOnlyList<string> LogicalRoots { get; }

    internal CancellationToken CancellationToken { get; }

    private static bool IsSameOrDescendant(string possibleAncestor, string path)
    {
        return string.Equals(possibleAncestor, path, StringComparison.Ordinal)
            || path.StartsWith($"{possibleAncestor}/", StringComparison.Ordinal);
    }
}
