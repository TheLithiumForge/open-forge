using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Selection;

internal static class IndexLogicalSourceProjector
{
    internal static IndexLogicalSource Project(
        SourceLogicalSource source,
        GeneratedNavigationFormation formation)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(formation);
        var rooted = source.Base.Form == SourceDocumentForm.Loader
            || formation.Topology.ReadAbsoluteDepth(source.Identity.CanonicalBasePath) is not null;
        return new IndexLogicalSource(
            id: source.Identity.AutomaticId,
            path: source.Identity.CanonicalBasePath,
            scope: rooted ? IndexLogicalSourceScope.Rooted : IndexLogicalSourceScope.Detached);
    }
}
