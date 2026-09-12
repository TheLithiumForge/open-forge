using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

internal sealed class SourceCatalogueRequest
{
    internal SourceCatalogueRequest(
        CliWorkspace workspace,
        IEnumerable<string> logicalRoots)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(logicalRoots);
        var roots = logicalRoots.ToArray();
        if (roots.Length == 0)
        {
            throw new ArgumentException("A source catalogue requires at least one logical root.", nameof(logicalRoots));
        }

        if (roots.Any(root => !SourceLogicalPath.IsCanonicalRoot(root)))
        {
            throw new ArgumentException("Every source catalogue root must be a canonical .agents root or descendant.", nameof(logicalRoots));
        }

        Workspace = workspace;
        LogicalRoots = new ReadOnlyCollection<string>(roots.OrderBy(root => root, StringComparer.Ordinal).ToArray());
    }

    internal CliWorkspace Workspace { get; }

    internal IReadOnlyList<string> LogicalRoots { get; }
}
