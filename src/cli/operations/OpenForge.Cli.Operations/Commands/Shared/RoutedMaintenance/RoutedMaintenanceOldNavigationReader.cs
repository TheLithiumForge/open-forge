using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Rename;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;

// Read-only Doctor observation. A familiar filename is a fact, never rename proof.
internal static class RoutedMaintenanceOldNavigationReader
{
    internal static ImmutableArray<RouteScaffoldOldNavigation> Read(SourceCatalogue catalogue)
        => [.. catalogue.Sources.Where(source => source.Base.Form == SourceDocumentForm.Markdown
                && SourceLogicalPath.ReadFileName(source.Identity.CanonicalBasePath).StartsWith('_'))
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .Select(source => new RouteScaffoldOldNavigation(source.Identity.CanonicalBasePath,
                $"_{SourceLogicalPath.ReadFileName(SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath))}.md"))];
}
