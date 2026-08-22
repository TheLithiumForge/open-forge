using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed class RouteInspectCatalogueBuilder
{
    internal RouteSourceCatalogue Build(IReadOnlyList<RouteInspectInventoryFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);
        var baseFiles = files
            .Where(file => file.Form != RouteSourceForm.OverwriteCompanion)
            .Where(file => RouteSourceIdentity.DeriveId(file.CanonicalPath) is not null)
            .OrderBy(file => file.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        var overwriteFiles = files
            .Where(file => file.Form == RouteSourceForm.OverwriteCompanion)
            .OrderBy(file => file.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        var baseIds = baseFiles.ToDictionary(
            file => file.CanonicalPath,
            file => RouteSourceIdentity.DeriveId(file.CanonicalPath)!,
            StringComparer.Ordinal);
        var pairedOverwriteByBase = new Dictionary<string, RouteInspectInventoryFile>(StringComparer.Ordinal);
        var overwriteFacts = new List<RouteOverwriteFact>();
        var metadataParser = new RouteMetadataParser();
        foreach (var overwrite in overwriteFiles)
        {
            var overwriteId = RouteSourceIdentity.DeriveId(overwrite.CanonicalPath);
            var candidates = overwriteId is null
                ? []
                : baseFiles
                    .Where(file => string.Equals(baseIds[file.CanonicalPath], overwriteId, StringComparison.Ordinal))
                    .Select(file => file.CanonicalPath)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray();
            if (candidates.Length == 1)
            {
                pairedOverwriteByBase.Add(candidates[0], overwrite);
            }
            else
            {
                overwriteFacts.Add(new RouteOverwriteFact(
                    candidates.Length == 0 ? RouteOverwriteState.Orphan : RouteOverwriteState.Ambiguous,
                    RouteInspectCatalogueSourceFactory.ToDocument(overwrite),
                    candidates));
            }
        }

        var ambiguousEntrypoints = baseFiles
            .Where(file => RouteInspectSourcePolicy.IsEntrypoint(file.Form))
            .GroupBy(file => RouteLogicalPath.ReadParent(file.CanonicalPath), StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Select(file => file.CanonicalPath))
            .ToHashSet(StringComparer.Ordinal);
        var sources = new List<RouteSource>();
        foreach (var file in baseFiles)
        {
            pairedOverwriteByBase.TryGetValue(file.CanonicalPath, out var overwrite);
            sources.Add(RouteInspectCatalogueSourceFactory.Create(
                file,
                overwrite,
                ambiguousEntrypoints.Contains(file.CanonicalPath),
                metadataParser,
                overwriteFacts));
        }

        return new RouteSourceCatalogue(sources, overwriteFacts);
    }
}
