using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

internal sealed class SourceCatalogueRequest
{

    internal SourceCatalogueRequest(WorkspaceLayout layout, IEnumerable<string> logicalRoots)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(logicalRoots);
        var roots = logicalRoots.ToArray();
        if (roots.Length == 0)
        {
            throw new ArgumentException("A source catalogue requires at least one logical root.", nameof(logicalRoots));
        }

        if (roots.Any(root => !SourceLogicalPath.IsCanonicalRoot(root) || !layout.SourceRoots.Contains(root) && !SourceLogicalPath.IsPrimarySource(layout, root)))
        {
            throw new ArgumentException("Every source catalogue root must be a canonical primary content location or descendant.", nameof(logicalRoots));
        }

        Layout = layout;
        LogicalRoots = new ReadOnlyCollection<string>(roots.OrderBy(root => root, StringComparer.Ordinal).ToArray());
    }

    internal WorkspaceLayout Layout { get; }

    internal CliWorkspace Workspace => Layout.Workspace;

    internal IReadOnlyList<string> LogicalRoots { get; }
}
