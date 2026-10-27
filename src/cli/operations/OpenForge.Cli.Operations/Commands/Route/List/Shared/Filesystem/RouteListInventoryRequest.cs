using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed record RouteListInventoryRequest
{
    internal RouteListInventoryRequest(
        WorkspaceLayout layout,
        CancellationToken cancellationToken)
        : this(layout, layout.InventoryRoots, cancellationToken)
    {
    }

    internal RouteListInventoryRequest(
        WorkspaceLayout layout,
        IEnumerable<string> logicalRoots,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(logicalRoots);
        var roots = logicalRoots.ToArray();
        if (roots.Length == 0)
        {
            throw new ArgumentException("Route-list inventory requires at least one logical root.", nameof(logicalRoots));
        }

        if (roots.Any(root => !layout.InventoryRoots.Contains(root, StringComparer.Ordinal) && !SourceLogicalPath.IsPrimarySource(layout, root)))
        {
            throw new ArgumentException("Every route-list inventory root must be a canonical primary or registered content path.", nameof(logicalRoots));
        }

        var orderedRoots = roots.OrderBy(root => root, StringComparer.Ordinal).ToArray();
        for (var index = 0; index < orderedRoots.Length; index++)
        {
            if (index > 0 && IsSameOrDescendant(orderedRoots[index - 1], orderedRoots[index]))
            {
                throw new ArgumentException("Route-list inventory roots cannot duplicate or overlap.", nameof(logicalRoots));
            }
        }

        Layout = layout;
        LogicalRoots = new ReadOnlyCollection<string>(orderedRoots);
        CancellationToken = cancellationToken;
    }

    internal WorkspaceLayout Layout { get; }

    internal CliWorkspace Workspace => Layout.Workspace;

    internal IReadOnlyList<string> LogicalRoots { get; }

    internal CancellationToken CancellationToken { get; }

    private static bool IsSameOrDescendant(string possibleAncestor, string path)
    {
        return string.Equals(possibleAncestor, path, StringComparison.Ordinal)
            || path.StartsWith($"{possibleAncestor}/", StringComparison.Ordinal);
    }
}
