using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;

internal sealed class GeneratedNavigationFormation
{
    private readonly GeneratedNavigationFormationBuilder.DerivedComponents _components;

    internal GeneratedNavigationFormation(
        GeneratedNavigationFormationBuilder.DerivedComponents components)
    {
        _components = components;
    }

    internal SourceCatalogue Catalogue => _components.Catalogue;

    internal IReadOnlyList<SourceLogicalSource> Sources => Catalogue.Sources;

    internal IReadOnlyList<SourceCatalogueIssue> Issues => Catalogue.Issues;

    internal SourceRouteTopology Topology => _components.Topology;

    internal SourceLogicalSource? Loader => _components.Loader;

    internal IReadOnlyList<GeneratedNavigationPhysicalAliasGroup> PhysicalAliasGroups => _components.PhysicalAliasGroups;

    internal IReadOnlyList<GeneratedNavigationFormationAmbiguity> Ambiguities => _components.Ambiguities;

    internal SourceLogicalSource? FindSource(string canonicalPath)
    {
        return Catalogue.FindByPath(canonicalPath);
    }
}
