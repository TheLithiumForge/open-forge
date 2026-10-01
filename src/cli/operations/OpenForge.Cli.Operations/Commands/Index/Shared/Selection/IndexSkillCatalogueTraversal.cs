using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Selection;

internal sealed class IndexSkillCatalogueTraversal
{
    private const string SkillsRootDirectory = SourceLogicalPath.AgentsRoot + "/skills";

    private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _cataloguePathsBySkillDirectory;
    private readonly IReadOnlySet<string> _rootedSkillPaths;

    internal IndexSkillCatalogueTraversal(GeneratedNavigationFormation formation)
    {
        ArgumentNullException.ThrowIfNull(formation);
        _cataloguePathsBySkillDirectory = BuildCatalogueLookup(formation);
        _rootedSkillPaths = ReadRootedPaths(formation);
    }

    internal IReadOnlyList<string> ReadCataloguePaths(SourceLogicalSource skill)
    {
        if (skill.Base.Form != SourceDocumentForm.Skill
            || !_rootedSkillPaths.Contains(skill.Identity.CanonicalBasePath))
        {
            return [];
        }

        return _cataloguePathsBySkillDirectory.GetValueOrDefault(
            SourceLogicalPath.ReadParent(skill.Identity.CanonicalBasePath)) ?? [];
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildCatalogueLookup(
        GeneratedNavigationFormation formation)
    {
        return formation.Sources
            .Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form))
            .GroupBy(source => ReadCatalogueParentDirectory(source.Identity.CanonicalBasePath), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(source => source.Identity.CanonicalBasePath)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);
    }

    private static HashSet<string> ReadRootedPaths(GeneratedNavigationFormation formation)
    {
        var skillsRoots = formation.Topology.LoaderRootPaths
            .Where(path => string.Equals(
                SourceLogicalPath.ReadParent(path),
                SkillsRootDirectory,
                StringComparison.Ordinal));
        var pending = new Queue<string>(skillsRoots);
        var rootedPaths = new HashSet<string>(StringComparer.Ordinal);
        while (pending.TryDequeue(out var path))
        {
            if (!rootedPaths.Add(path)
                || formation.Topology.FindByPath(path) is not { } node)
            {
                continue;
            }

            foreach (var childPath in node.ChildPaths)
            {
                pending.Enqueue(childPath);
            }
        }

        return rootedPaths;
    }

    private static string ReadCatalogueParentDirectory(string cataloguePath)
    {
        var representedDirectory = SourceLogicalPath.ReadParent(cataloguePath);
        return SourceLogicalPath.ReadParent(representedDirectory);
    }
}
