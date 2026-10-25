using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;

// Exact ordered defaults from Metadata Completion in Index in the Forgiving Maintenance design.
internal static class RoutedMetadataDefaults
{
    private static readonly ImmutableDictionary<string, RoutedMetadataCategory> Categories = new Dictionary<string, RoutedMetadataCategory>(StringComparer.Ordinal)
    {
        [WorkspaceCategoryNames.Directives] = RoutedMetadataCategory.Directives,
        [WorkspaceCategoryNames.Guidance] = RoutedMetadataCategory.Guidance,
        [WorkspaceCategoryNames.Maps] = RoutedMetadataCategory.Maps,
        [WorkspaceCategoryNames.Patterns] = RoutedMetadataCategory.Patterns,
        [WorkspaceCategoryNames.Templates] = RoutedMetadataCategory.Templates,
        [WorkspaceLayoutNames.SkillsDirectoryName] = RoutedMetadataCategory.Skills,
        [WorkspaceCategoryNames.Extensions] = RoutedMetadataCategory.Extensions,
        [WorkspaceCategoryNames.Memory] = RoutedMetadataCategory.Memory,
    }.ToImmutableDictionary(StringComparer.Ordinal);

    private static readonly ImmutableDictionary<string, RoutedMetadataCategory> MemoryStates = new Dictionary<string, RoutedMetadataCategory>(StringComparer.Ordinal)
    {
        ["working"] = RoutedMetadataCategory.Working,
        ["emerging"] = RoutedMetadataCategory.Emerging,
        ["crystallized"] = RoutedMetadataCategory.Crystallized,
        ["archived"] = RoutedMetadataCategory.Archived,
    }.ToImmutableDictionary(StringComparer.Ordinal);

    private static readonly ImmutableDictionary<RoutedMetadataCategory, ImmutableArray<string>> CategoryTags = new Dictionary<RoutedMetadataCategory, ImmutableArray<string>>
    {
        [RoutedMetadataCategory.Directives] = [SourceLoadingClosureResolver.LoadNowTag, "Directive"],
        [RoutedMetadataCategory.DirectiveEntrypoint] = ["Directive"],
        [RoutedMetadataCategory.Guidance] = ["Guidance"],
        [RoutedMetadataCategory.Maps] = ["Map"],
        [RoutedMetadataCategory.Patterns] = ["Pattern"],
        [RoutedMetadataCategory.Templates] = ["Template"],
        [RoutedMetadataCategory.Skills] = ["Skill"],
        [RoutedMetadataCategory.Extensions] = ["Extension"],
        [RoutedMetadataCategory.Memory] = ["Memory"],
        [RoutedMetadataCategory.Working] = ["Memory", "Working", "Contextual"],
        [RoutedMetadataCategory.Emerging] = ["Memory", "Emerging", "Contextual"],
        [RoutedMetadataCategory.Crystallized] = ["Memory", "Crystallized"],
        [RoutedMetadataCategory.Archived] = ["Memory", "Archived", "Contextual", "Historical"],
        [RoutedMetadataCategory.Workspace] = ["Workspace"],
    }.ToImmutableDictionary();

    internal static ImmutableArray<string> Tags(RoutedMetadataCategory category)
        => CategoryTags.TryGetValue(category, out var tags)
            ? tags
            : throw new ArgumentOutOfRangeException(nameof(category), category, "The metadata category is not defined.");

    internal static ImmutableArray<string> Tags(SourceContentRoots roots, RoutedMetadataFileFacts file)
    {
        var root = file.LoaderRootEntrypointPath
            ?? throw new ArgumentException("Category defaults require an established Loader root.", nameof(file));
        var directory = SourceLogicalPath.ReadParent(root);
        var slug = SourceLogicalPath.ReadFileName(directory);
        if (!Categories.TryGetValue(slug, out var category)) return Tags(RoutedMetadataCategory.Workspace);
        if (category == RoutedMetadataCategory.Directives
            && SourceFormClassifier.TryClassify(roots, file.CanonicalPath, out var fileForm) && SourceFormClassifier.IsEntrypoint(fileForm))
            return Tags(RoutedMetadataCategory.DirectiveEntrypoint);
        if (category != RoutedMetadataCategory.Memory) return Tags(category);

        foreach (var entrypoint in file.RouteEntrypointPaths.OrderBy(path => path.Length))
        {
            if (!SourceFormClassifier.TryClassify(roots, entrypoint, out var form) || !SourceFormClassifier.IsEntrypoint(form)) continue;
            var stateDirectory = SourceLogicalPath.ReadParent(entrypoint);
            if (stateDirectory.StartsWith($"{directory}/", StringComparison.Ordinal)
                && IsAncestor(file.CanonicalPath, stateDirectory)
                && MemoryStates.TryGetValue(SourceLogicalPath.ReadFileName(stateDirectory), out var stateCategory))
                category = stateCategory;
        }

        return Tags(category);
    }

    internal static ImmutableArray<string> Tags(WorkspaceLayout layout, RoutedMetadataFileFacts file)
    {
        if (layout.TryGetMaintenanceBoundary(file.CanonicalPath, out var boundary) && boundary.Kind == WorkspaceMaintenanceBoundaryKind.Outside)
            return Tags(RoutedMetadataCategory.Workspace);
        return Tags(new SourceContentRoots(layout), file);
    }

    internal static bool IsAncestor(string path, string directory)
        => path.StartsWith($"{directory}/", StringComparison.Ordinal);
}
