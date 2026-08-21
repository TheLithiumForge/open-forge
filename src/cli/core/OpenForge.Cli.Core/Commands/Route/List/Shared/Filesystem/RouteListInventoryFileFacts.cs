using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed record RouteListInventoryFileFact
{
    internal RouteListInventoryFileFact(
        string canonicalLogicalPath,
        string physicalPath,
        FileReadState readState,
        string? body)
    {
        if (!RouteListLogicalPath.IsCanonical(canonicalLogicalPath)
            || canonicalLogicalPath == RouteListLogicalPath.AgentsRoot)
        {
            throw new ArgumentException("The inventory file logical path is not canonical.", nameof(canonicalLogicalPath));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);
        var normalizedPhysicalPath = Path.GetFullPath(physicalPath);
        if (!Path.IsPathRooted(physicalPath)
            || !string.Equals(normalizedPhysicalPath, physicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("The inventory file physical path must be absolute and normalized.", nameof(physicalPath));
        }

        if (!Enum.IsDefined(readState))
        {
            throw new ArgumentOutOfRangeException(nameof(readState), readState, "The file read state is not defined.");
        }

        if ((readState == FileReadState.Complete) != (body is not null))
        {
            throw new ArgumentException("Only a complete inventory file read can carry a source body.", nameof(body));
        }

        CanonicalLogicalPath = canonicalLogicalPath;
        PhysicalPath = normalizedPhysicalPath;
        Form = ReadSourceForm(canonicalLogicalPath);
        ReadState = readState;
        Body = body;
    }

    internal string CanonicalLogicalPath { get; }

    internal string PhysicalPath { get; }

    internal RouteListSourceForm Form { get; }

    internal FileReadState ReadState { get; }

    internal string? Body { get; }

    private static RouteListSourceForm ReadSourceForm(string canonicalLogicalPath)
    {
        if (canonicalLogicalPath.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            return RouteListSourceForm.OverwriteCompanion;
        }

        if (string.Equals(canonicalLogicalPath, ".agents/loader.md", StringComparison.Ordinal))
        {
            return RouteListSourceForm.Loader;
        }

        var fileName = RouteListLogicalPath.ReadFileName(canonicalLogicalPath);
        if (string.Equals(fileName, "SKILL.md", StringComparison.Ordinal))
        {
            return RouteListSourceForm.Skill;
        }

        if (!RouteListSourceIdentity.IsRecognizedEntrypointPath(canonicalLogicalPath))
        {
            return RouteListSourceForm.Markdown;
        }

        var parentName = RouteListLogicalPath.ReadFileName(RouteListLogicalPath.ReadParent(canonicalLogicalPath));
        if (string.Equals(fileName, $"_{parentName}.md", StringComparison.Ordinal))
        {
            return RouteListSourceForm.CanonicalEntrypoint;
        }

        return fileName switch
        {
            "index.md" => RouteListSourceForm.IndexEntrypoint,
            "_index.md" => RouteListSourceForm.UnderscoreIndexEntrypoint,
            "references.md" => RouteListSourceForm.ReferencesEntrypoint,
            "_references.md" => RouteListSourceForm.UnderscoreReferencesEntrypoint,
            _ => throw new InvalidOperationException("The recognized entrypoint form is not defined."),
        };
    }
}

internal sealed class RouteListInventoryFileFacts
{
    private readonly IReadOnlyList<RouteListInventoryFileFact> _files;

    internal RouteListInventoryFileFacts(IEnumerable<RouteListInventoryFileFact> files)
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
        RouteListMetadataParser metadataParser,
        ICollection<RouteListFilesystemFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(metadataParser);
        ArgumentNullException.ThrowIfNull(findings);
        var byPath = _files.ToDictionary(file => file.CanonicalLogicalPath, StringComparer.Ordinal);
        var overwriteByBase = ReadOverwrites(byPath, findings);
        var ambiguousEntrypoints = ReadAmbiguousEntrypoints();
        var sources = new List<RouteListInventorySource>();
        foreach (var file in _files.Where(file => file.Form != RouteListSourceForm.OverwriteCompanion))
        {
            overwriteByBase.TryGetValue(file.CanonicalLogicalPath, out var overwrite);
            var metadata = ReadMetadata(file, overwrite is not null, metadataParser);
            AddMetadataFinding(file.CanonicalLogicalPath, metadata, findings);
            if (metadata.IsCompatibilityEntrypoint
                && metadata.State == RouteListMetadataState.Complete)
            {
                findings.Add(RouteListFilesystemFindingPolicy.CompatibilityEntrypoint(file.CanonicalLogicalPath));
            }

            var id = RouteListSourceIdentity.DeriveId(file.CanonicalLogicalPath);
            if (id is null)
            {
                findings.Add(RouteListFilesystemFindingPolicy.UnsupportedSourceForm(file.CanonicalLogicalPath));
                continue;
            }

            var overwriteRelation = overwrite is null
                ? null
                : new RouteListOverwriteRelation(overwrite.CanonicalLogicalPath, overwrite.PhysicalPath);
            var source = new RouteListSource(
                id,
                file.CanonicalLogicalPath,
                file.PhysicalPath,
                ReadSourceKind(file.Form, metadata),
                overwriteRelation?.CanonicalLogicalPath,
                ambiguousEntrypoints.Contains(file.CanonicalLogicalPath));
            sources.Add(new RouteListInventorySource(source, file.Form, metadata, overwriteRelation));
        }

        AddIdentityCollisionFindings(sources, findings);
        return sources;
    }

    private Dictionary<string, RouteListInventoryFileFact> ReadOverwrites(
        IReadOnlyDictionary<string, RouteListInventoryFileFact> byPath,
        ICollection<RouteListFilesystemFinding> findings)
    {
        var overwriteByBase = new Dictionary<string, RouteListInventoryFileFact>(StringComparer.Ordinal);
        foreach (var overwrite in _files.Where(file => file.Form == RouteListSourceForm.OverwriteCompanion))
        {
            var basePath = overwrite.CanonicalLogicalPath[..^".overwrite.md".Length] + ".md";
            if (byPath.TryGetValue(basePath, out var baseFile)
                && baseFile.Form != RouteListSourceForm.OverwriteCompanion)
            {
                overwriteByBase.Add(basePath, overwrite);
            }
            else
            {
                findings.Add(RouteListFilesystemFindingPolicy.OrphanOverwrite(overwrite.CanonicalLogicalPath));
            }
        }

        return overwriteByBase;
    }

    private HashSet<string> ReadAmbiguousEntrypoints()
    {
        return _files
            .Where(file => RouteListSourceFormFacts.IsEntrypoint(file.Form))
            .GroupBy(file => RouteListLogicalPath.ReadParent(file.CanonicalLogicalPath), StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Select(file => file.CanonicalLogicalPath))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static RouteListSourceMetadata ReadMetadata(
        RouteListInventoryFileFact file,
        bool isOverwritePresent,
        RouteListMetadataParser metadataParser)
    {
        var isCompatibility = RouteListSourceFormFacts.IsCompatibilityEntrypoint(file.Form);
        if (file.ReadState != FileReadState.Complete)
        {
            return RouteListSourceMetadata.WithoutValues(
                RouteListMetadataState.ReadUnavailable,
                isCompatibility,
                isOverwritePresent);
        }

        if (file.Form == RouteListSourceForm.Loader)
        {
            return RouteListSourceMetadata.WithoutValues(
                RouteListMetadataState.NotApplicable,
                isCompatibilityEntrypoint: false,
                isOverwritePresent);
        }

        if (file.Form == RouteListSourceForm.Skill)
        {
            return metadataParser.ParseSkill(file.Body!, isOverwritePresent);
        }

        if (file.Form == RouteListSourceForm.Markdown
            || RouteListSourceFormFacts.IsEntrypoint(file.Form))
        {
            return metadataParser.ParseOpenForge(
                file.Body!,
                isCompatibility,
                isOverwritePresent);
        }

        throw new ArgumentOutOfRangeException(nameof(file), file.Form, "The file is not a logical source base.");
    }

    private static RouteListSourceKind ReadSourceKind(
        RouteListSourceForm form,
        RouteListSourceMetadata metadata)
    {
        if (RouteListSourceFormFacts.IsEntrypoint(form))
        {
            return RouteListSourceKind.Entrypoint;
        }

        return form switch
        {
            RouteListSourceForm.Loader => RouteListSourceKind.Loader,
            RouteListSourceForm.Skill => metadata.State == RouteListMetadataState.Complete
                ? RouteListSourceKind.RoutedNative
                : RouteListSourceKind.Unrouted,
            RouteListSourceForm.Markdown => metadata.State == RouteListMetadataState.Complete
                ? RouteListSourceKind.RoutedLeaf
                : RouteListSourceKind.Unrouted,
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The form is not a logical source base."),
        };
    }

    private static void AddMetadataFinding(
        string subject,
        RouteListSourceMetadata metadata,
        ICollection<RouteListFilesystemFinding> findings)
    {
        if (metadata.State == RouteListMetadataState.Missing)
        {
            findings.Add(RouteListFilesystemFindingPolicy.MetadataMissing(subject));
        }
        else if (metadata.State == RouteListMetadataState.Malformed)
        {
            findings.Add(RouteListFilesystemFindingPolicy.MetadataMalformed(subject));
        }
    }

    private static void AddIdentityCollisionFindings(
        IEnumerable<RouteListInventorySource> sources,
        ICollection<RouteListFilesystemFinding> findings)
    {
        var firstSourceByPhysical = new Dictionary<string, string>(PhysicalIdentityTracker.PathComparer);
        foreach (var source in sources.OrderBy(item => item.Source.CanonicalPath, StringComparer.Ordinal))
        {
            if (!firstSourceByPhysical.TryAdd(source.Source.PhysicalPath, source.Source.CanonicalPath))
            {
                findings.Add(RouteListFilesystemFindingPolicy.IdentityCollision(source.Source.CanonicalPath));
            }
        }
    }

}
