using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning.Topology;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;

internal sealed class ExtensionUpdateIndependentMetadataProof
{
    internal IReadOnlySet<string> ReadOmittableRegions(
        GeneratedNavigationProjection projection,
        GeneratedNavigationFormation formation,
        IReadOnlyList<GeneratedNavigationRegionInput> regionInputs,
        IReadOnlyList<GeneratedNavigationMetadata> metadata,
        IReadOnlyDictionary<string, SourceAuthoredMetadataFacts> metadataByPath,
        IReadOnlyDictionary<string, string> documents,
        IReadOnlySet<string> packagePaths,
        IReadOnlySet<string> retiredPaths,
        ExtensionUpdateTopologyAdmission admission,
        GeneratedNavigationProjector projector)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(formation);
        ArgumentNullException.ThrowIfNull(regionInputs);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(metadataByPath);
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(packagePaths);
        ArgumentNullException.ThrowIfNull(retiredPaths);
        ArgumentNullException.ThrowIfNull(admission);
        ArgumentNullException.ThrowIfNull(projector);

        var affectedPaths = new HashSet<string>(packagePaths, StringComparer.Ordinal);
        affectedPaths.UnionWith(retiredPaths);
        affectedPaths.UnionWith(admission.Exclusions);
        affectedPaths.UnionWith(admission.Overrides.Keys);
        var affectedHosts = ReadAffectedHosts(formation, affectedPaths);
        var affectedClosure = ReadAffectedClosure(formation, affectedPaths, affectedHosts);
        var omitted = new HashSet<string>(StringComparer.Ordinal);
        foreach (var region in projection.Regions)
        {
            var malformedChildren = ReadOmittableMalformedChildren(
                region,
                formation,
                regionInputs,
                metadata,
                metadataByPath,
                documents,
                affectedClosure,
                projector);
            if (malformedChildren is not null)
            {
                _ = omitted.Add(region.CanonicalPath);
            }
        }

        return omitted;
    }

    private static IReadOnlySet<string> ReadAffectedHosts(
        GeneratedNavigationFormation formation,
        IReadOnlySet<string> affectedPaths)
    {
        var affected = new HashSet<string>(StringComparer.Ordinal);
        foreach (var affectedPath in affectedPaths.Order(StringComparer.Ordinal))
        {
            if (formation.FindSource(affectedPath) is { } source)
            {
                AddSourceAndAncestors(formation, source, affected);
                continue;
            }

            if (!SourceFormClassifier.TryClassify(affectedPath, out var form)
                || form is SourceDocumentForm.Loader or SourceDocumentForm.OverwriteCompanion)
            {
                continue;
            }

            var containingDirectory = SourceLogicalPath.ReadParent(affectedPath);
            var representedParentDirectory = form == SourceDocumentForm.Markdown
                ? containingDirectory
                : SourceLogicalPath.ReadParent(containingDirectory);
            foreach (var host in formation.Sources.Where(source =>
                         SourceFormClassifier.IsEntrypoint(source.Base.Form)
                         && string.Equals(
                             SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath),
                             representedParentDirectory,
                             StringComparison.Ordinal)))
            {
                AddSourceAndAncestors(formation, host, affected);
            }
        }

        return affected;
    }

    private static void AddSourceAndAncestors(
        GeneratedNavigationFormation formation,
        SourceLogicalSource source,
        ISet<string> affected)
    {
        AddRegionHost(source, affected);
        if (source.Base.Form != SourceDocumentForm.Loader)
        {
            var node = formation.Topology.FindByPath(source.Identity.CanonicalBasePath);
            while (node?.ParentState == SourceRouteParentState.Resolved)
            {
                var parentPath = node.ParentPaths[0];
                if (formation.FindSource(parentPath) is { } parent)
                {
                    AddRegionHost(parent, affected);
                }

                node = formation.Topology.FindByPath(parentPath);
            }
        }

        if (formation.Loader is not null
            && formation.Topology.LoaderRootPaths.Contains(
                source.Identity.CanonicalBasePath,
                StringComparer.Ordinal))
        {
            _ = affected.Add(formation.Loader.Identity.CanonicalBasePath);
        }
    }

    private static IReadOnlySet<string> ReadAffectedClosure(
        GeneratedNavigationFormation formation,
        IReadOnlySet<string> affectedPaths,
        IReadOnlySet<string> affectedHosts)
    {
        var closure = new HashSet<string>(affectedPaths, StringComparer.Ordinal);
        foreach (var hostPath in affectedHosts)
        {
            _ = closure.Add(hostPath);
            if (formation.FindSource(hostPath) is not { } host)
            {
                continue;
            }

            var childPaths = ReadDirectChildPaths(formation, host);
            if (childPaths is null)
            {
                closure.UnionWith(formation.Sources.Select(
                    source => source.Identity.CanonicalBasePath));
                continue;
            }

            closure.UnionWith(childPaths);
        }

        return closure;
    }

    private static IReadOnlyList<string>? ReadOmittableMalformedChildren(
        GeneratedNavigationRegion region,
        GeneratedNavigationFormation formation,
        IReadOnlyList<GeneratedNavigationRegionInput> regionInputs,
        IReadOnlyList<GeneratedNavigationMetadata> metadata,
        IReadOnlyDictionary<string, SourceAuthoredMetadataFacts> metadataByPath,
        IReadOnlyDictionary<string, string> documents,
        IReadOnlySet<string> affectedClosure,
        GeneratedNavigationProjector projector)
    {
        if (region.State == GeneratedNavigationRegionState.Available
            || region.UnavailableReason != GeneratedNavigationRegionUnavailableReason.MetadataInvalid
            || affectedClosure.Contains(region.CanonicalPath))
        {
            return null;
        }

        var childPaths = ReadDirectChildPaths(formation, region.Source);
        if (childPaths is null)
        {
            return null;
        }

        var malformedChildren = new List<string>();
        foreach (var childPath in childPaths)
        {
            if (!metadataByPath.TryGetValue(childPath, out var childMetadata)
                || formation.FindSource(childPath) is not { } child
                || !documents.ContainsKey(childPath))
            {
                return null;
            }

            var form = child.Base.Form;
            if (childMetadata.State == SourceAuthoredMetadataState.Malformed)
            {
                if (affectedClosure.Contains(childPath)
                    || !(form == SourceDocumentForm.Markdown
                        || SourceFormClassifier.IsEntrypoint(form)))
                {
                    return null;
                }

                malformedChildren.Add(childPath);
                continue;
            }

            if (childMetadata.State == SourceAuthoredMetadataState.Missing
                && (form == SourceDocumentForm.Skill
                    || !(form == SourceDocumentForm.Markdown
                        || SourceFormClassifier.IsEntrypoint(form))))
            {
                return null;
            }

            if (childMetadata.State is SourceAuthoredMetadataState.NotApplicable
                || childMetadata.State == SourceAuthoredMetadataState.Complete
                    && childMetadata.Description is null)
            {
                return null;
            }
        }

        if (malformedChildren.Count == 0)
        {
            return null;
        }

        var regionInput = regionInputs.FirstOrDefault(input =>
            string.Equals(
                input.Source.Identity.CanonicalBasePath,
                region.CanonicalPath,
                StringComparison.Ordinal));
        if (regionInput is null)
        {
            return null;
        }

        var malformedSet = malformedChildren.ToHashSet(StringComparer.Ordinal);
        var proofMetadata = metadata
            .Select(value => malformedSet.Contains(value.Source.Identity.CanonicalBasePath)
                ? new GeneratedNavigationMetadata(
                    value.Source,
                    SourceAuthoredMetadataFacts.WithoutValues(
                        SourceAuthoredMetadataState.Missing))
                : value)
            .ToArray();
        var proof = projector.Project(new GeneratedNavigationProjectionRequest(
            formation,
            [regionInput],
            proofMetadata));
        return proof.Regions.Count == 1
            && string.Equals(
                proof.Regions[0].CanonicalPath,
                region.CanonicalPath,
                StringComparison.Ordinal)
            && proof.Regions[0].State == GeneratedNavigationRegionState.Available
            ? malformedChildren
            : null;
    }

    private static IReadOnlyList<string>? ReadDirectChildPaths(
        GeneratedNavigationFormation formation,
        SourceLogicalSource host)
    {
        var childPaths = host.Base.Form == SourceDocumentForm.Loader
            ? formation.Topology.LoaderRootPaths
            : formation.Topology.FindByPath(host.Identity.CanonicalBasePath)?.ChildPaths;
        if (childPaths is null)
        {
            return null;
        }

        var directChildren = new List<string>(childPaths.Count);
        foreach (var childPath in childPaths)
        {
            var child = formation.FindSource(childPath);
            if (child is null)
            {
                return null;
            }

            if (child.Base.Form != SourceDocumentForm.OverwriteCompanion)
            {
                directChildren.Add(childPath);
            }
        }

        return directChildren;
    }

    private static void AddRegionHost(
        SourceLogicalSource source,
        ISet<string> affected)
    {
        if (source.Base.Form == SourceDocumentForm.Loader
            || SourceFormClassifier.IsEntrypoint(source.Base.Form))
        {
            _ = affected.Add(source.Identity.CanonicalBasePath);
        }
    }
}
