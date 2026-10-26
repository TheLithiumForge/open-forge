using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;

internal sealed class RouteScaffoldSourceBuilder
{
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();

    internal static string CanonicalPath(WorkspaceLayout layout, string folderId)
        => EntrypointPath(layout.SourcePath(ValidateFolderId(folderId)));

    private static string ValidateFolderId(string folderId)
    {
        if (!SourceIdentity.IsValidId(folderId))
            throw new ArgumentException("A scaffold folder requires a canonical source ID.", nameof(folderId));
        return folderId;
    }

    private static string EntrypointPath(string folder)
        => $"{folder}/{SourceFormClassifier.CanonicalEntrypointFileName(SourceLogicalPath.ReadFileName(folder))}";

    internal SourceLogicalSource Create(WorkspaceLayout layout, string canonicalPath)
    {
        if (!SourceFormClassifier.TryClassify(layout, canonicalPath, out var form)
            || form == SourceDocumentForm.OverwriteCompanion
            || SourceIdentity.DeriveId(layout, canonicalPath) is not { } id)
            throw new ArgumentException("An intended scaffold source requires a recognized path and identity.", nameof(canonicalPath));
        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, canonicalPath),
            new SourceLayer(
                canonicalPath,
                Path.GetFullPath(SourceLogicalPath.ToLexicalPath(layout.Workspace.PhysicalRoot, canonicalPath)),
                form,
                SourceLayerKind.Base));
    }

    internal GeneratedNavigationFormation Add(SourceCatalogue catalogue, IEnumerable<SourceLogicalSource> sources)
    {
        var proposed = sources.ToArray();
        RequireUnique(proposed);
        var existing = catalogue.Sources.Select(source => source.Identity.CanonicalBasePath).ToHashSet(StringComparer.Ordinal);
        return _formationBuilder.Build(catalogue, catalogue.Sources.Concat(
            proposed.Where(source => !existing.Contains(source.Identity.CanonicalBasePath))).ToArray());
    }

    internal GeneratedNavigationFormation Replace(
        SourceCatalogue catalogue,
        IEnumerable<SourceLogicalSource> sources,
        IEnumerable<string>? removedPaths = null)
    {
        var proposed = sources.ToArray();
        RequireUnique(proposed);
        var replaced = proposed.Select(source => source.Identity.CanonicalBasePath)
            .Concat(removedPaths ?? []).ToHashSet(StringComparer.Ordinal);
        return _formationBuilder.Build(catalogue, catalogue.Sources
            .Where(source => !replaced.Contains(source.Identity.CanonicalBasePath)).Concat(proposed).ToArray());
    }

    private static void RequireUnique(SourceLogicalSource[] sources)
    {
        if (sources.Select(source => source.Identity.CanonicalBasePath).Distinct(StringComparer.Ordinal).Count() != sources.Length)
            throw new ArgumentException("Intended scaffold sources require unique canonical paths.", nameof(sources));
    }
}
