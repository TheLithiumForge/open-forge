using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed class RouteListInventorySourceBuilder
{
    private readonly IReadOnlyList<RouteSourceDocument> _files;

    internal RouteListInventorySourceBuilder(IEnumerable<RouteSourceDocument> files)
    {
        ArgumentNullException.ThrowIfNull(files);
        _files = files
            .Select(file => file ?? throw new ArgumentException("Inventory file facts cannot contain null.", nameof(files)))
            .OrderBy(file => file.CanonicalLogicalPath, StringComparer.Ordinal)
            .ToArray();
        if (_files.Select(file => file.CanonicalLogicalPath).Distinct(StringComparer.Ordinal).Count() != _files.Count)
        {
            throw new ArgumentException("Inventory file facts require unique canonical paths.", nameof(files));
        }
    }

    internal IReadOnlyList<RouteListInventorySource> FormSources(
        RouteMetadataParser metadataParser,
        ICollection<RouteListFilesystemFinding> findings,
        ICollection<RouteOverwriteFact> overwriteFacts)
    {
        ArgumentNullException.ThrowIfNull(metadataParser);
        ArgumentNullException.ThrowIfNull(findings);
        ArgumentNullException.ThrowIfNull(overwriteFacts);
        var byPath = _files.ToDictionary(file => file.CanonicalLogicalPath, StringComparer.Ordinal);
        var overwriteByBase = ReadOverwrites(byPath, findings, overwriteFacts);
        var ambiguousEntrypoints = ReadAmbiguousEntrypoints();
        var sources = new List<RouteListInventorySource>();
        foreach (var file in _files.Where(file => file.Form != RouteSourceForm.OverwriteCompanion))
        {
            overwriteByBase.TryGetValue(file.CanonicalLogicalPath, out var overwrite);
            var metadata = ReadMetadata(file, overwrite is not null, metadataParser);
            RouteListInventoryFindingBuilder.AddMetadataFinding(file.CanonicalLogicalPath, metadata, findings);
            if (metadata.IsCompatibilityEntrypoint
                && metadata.State == RouteSourceMetadataState.Complete)
            {
                findings.Add(RouteListFilesystemFindingPolicy.CompatibilityEntrypoint(file.CanonicalLogicalPath));
            }

            if (RouteSourceIdentity.DeriveId(file.CanonicalLogicalPath) is null)
            {
                findings.Add(RouteListFilesystemFindingPolicy.UnsupportedSourceForm(file.CanonicalLogicalPath));
                continue;
            }

            var source = new RouteSource(
                file,
                metadata,
                ReadSourceKind(file.Form),
                overwrite,
                ambiguousEntrypoints.Contains(file.CanonicalLogicalPath));
            sources.Add(new RouteListInventorySource(source));
        }

        RouteListInventoryFindingBuilder.AddIdentityCollisionFindings(sources, findings);
        return sources;
    }

    private Dictionary<string, RouteSourceDocument> ReadOverwrites(
        IReadOnlyDictionary<string, RouteSourceDocument> byPath,
        ICollection<RouteListFilesystemFinding> findings,
        ICollection<RouteOverwriteFact> overwriteFacts)
    {
        var overwriteByBase = new Dictionary<string, RouteSourceDocument>(StringComparer.Ordinal);
        foreach (var overwrite in _files.Where(file => file.Form == RouteSourceForm.OverwriteCompanion))
        {
            var basePath = overwrite.CanonicalLogicalPath[..^".overwrite.md".Length] + ".md";
            if (byPath.TryGetValue(basePath, out var baseFile)
                && baseFile.Form != RouteSourceForm.OverwriteCompanion)
            {
                overwriteByBase.Add(basePath, overwrite);
            }
            else
            {
                findings.Add(RouteListFilesystemFindingPolicy.OrphanOverwrite(overwrite.CanonicalLogicalPath));
                overwriteFacts.Add(new RouteOverwriteFact(
                    RouteOverwriteState.Orphan,
                    overwrite,
                    []));
            }
        }

        return overwriteByBase;
    }

    private HashSet<string> ReadAmbiguousEntrypoints()
    {
        return _files
            .Where(file => RouteSourceFormClassifier.IsEntrypoint(file.Form))
            .GroupBy(file => RouteListLogicalPath.ReadParent(file.CanonicalLogicalPath), StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Select(file => file.CanonicalLogicalPath))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static RouteSourceMetadata ReadMetadata(
        RouteSourceDocument file,
        bool isOverwritePresent,
        RouteMetadataParser metadataParser)
    {
        var isCompatibility = RouteSourceFormClassifier.IsCompatibilityEntrypoint(file.Form);
        if (file.ReadState != FileReadState.Complete)
        {
            return RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.ReadUnavailable,
                isCompatibility,
                isOverwritePresent);
        }

        if (file.Form == RouteSourceForm.Loader)
        {
            return RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.NotApplicable,
                isCompatibilityEntrypoint: false,
                isOverwritePresent);
        }

        if (file.Form == RouteSourceForm.Skill)
        {
            return metadataParser.ParseSkill(file.Body!, isOverwritePresent);
        }

        if (file.Form == RouteSourceForm.Markdown
            || RouteSourceFormClassifier.IsEntrypoint(file.Form))
        {
            return metadataParser.ParseOpenForge(
                file.Body!,
                isCompatibility,
                isOverwritePresent);
        }

        throw new ArgumentOutOfRangeException(nameof(file), file.Form, "The file is not a logical source base.");
    }

    private static RouteSourceKind ReadSourceKind(RouteSourceForm form)
    {
        if (RouteSourceFormClassifier.IsEntrypoint(form))
        {
            return RouteSourceKind.Entrypoint;
        }

        return form switch
        {
            RouteSourceForm.Loader => RouteSourceKind.Loader,
            RouteSourceForm.Skill => RouteSourceKind.Native,
            RouteSourceForm.Markdown => RouteSourceKind.Markdown,
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The form is not a logical source base."),
        };
    }

}
