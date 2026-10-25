using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Selection;

internal static class IndexLoaderRoots
{
    internal static IReadOnlyList<string> Read(GeneratedNavigationFormation formation)
    {
        var layout = formation.Catalogue.Layout;
        var listed = formation.Catalogue.IncludedRows.Where(row => row.ParentPath == layout.LoaderPath)
            .Select(row => row.Path).ToHashSet(StringComparer.Ordinal);
        return formation.Topology.LoaderRootPaths.Where(path => !SourceLogicalPath.IsOutsideSource(layout, path) || listed.Contains(path))
            .Concat(layout.OutsideRoutes.Where(route => listed.Contains(route.CataloguePath)).Select(route => route.CataloguePath))
            .Distinct(StringComparer.Ordinal).ToArray();
    }
}
