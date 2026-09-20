using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal static class RouteStatusMetadataIssueReader
{
    internal static IReadOnlyList<GeneratedNavigationMetadataIssue> Read(
        GeneratedNavigationFormation formation,
        IReadOnlyDictionary<string, RouteSourceObservation> observations,
        GeneratedNavigationRegion region)
    {
        ArgumentNullException.ThrowIfNull(formation);
        ArgumentNullException.ThrowIfNull(observations);
        ArgumentNullException.ThrowIfNull(region);

        if (region.State != GeneratedNavigationRegionState.Unavailable
            || region.UnavailableReason != GeneratedNavigationRegionUnavailableReason.MetadataInvalid)
        {
            return [];
        }

        if (!TryReadDirectChildren(formation, region.Source, out var children))
        {
            return [];
        }

        var malformed = new List<(SourceLogicalSource Source, string Cause)>();
        foreach (var child in children)
        {
            if (!IsContained(formation.Catalogue, child)
                || !observations.TryGetValue(child.Identity.CanonicalBasePath, out var observation)
                || !ReferenceEquals(observation.Source, child)
                || !observation.IsReadable)
            {
                return [];
            }

            if (observation.AuthoredMetadata.State != SourceAuthoredMetadataState.Malformed)
            {
                continue;
            }

            if (!IsOrdinaryMetadataSource(child.Base.Form)
                || observation.FrameworkMetadata.State != FrameworkDocumentMetadataState.Malformed
                || ReadCause(observation) is not { } cause)
            {
                return [];
            }

            malformed.Add((child, cause));
        }

        if (malformed.Count == 0)
        {
            return [];
        }

        var malformedPaths = malformed
            .Select(value => value.Source.Identity.CanonicalBasePath)
            .ToHashSet(StringComparer.Ordinal);
        var counterfactual = observations.ToDictionary(
            pair => pair.Key,
            pair => malformedPaths.Contains(pair.Key)
                ? pair.Value with
                {
                    AuthoredMetadata = SourceAuthoredMetadataFacts.WithoutValues(
                        SourceAuthoredMetadataState.Missing),
                }
                : pair.Value,
            StringComparer.Ordinal);
        var reprojection = RouteGeneratedNavigationProjection.Project(
            formation,
            counterfactual,
            [region.Source]);
        var counterfactualRegion = reprojection.Regions
            .SingleOrDefault(value => value.CanonicalPath == region.CanonicalPath);
        if (counterfactualRegion?.State != GeneratedNavigationRegionState.Available)
        {
            return [];
        }

        return malformed
            .OrderBy(value => value.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .Select(value => new GeneratedNavigationMetadataIssue(
                value.Source.Identity.CanonicalBasePath,
                value.Cause))
            .ToArray();
    }

    private static bool TryReadDirectChildren(
        GeneratedNavigationFormation formation,
        SourceLogicalSource parent,
        out IReadOnlyList<SourceLogicalSource> children)
    {
        IReadOnlyList<string>? childPaths = parent.Base.Form == SourceDocumentForm.Loader
            ? formation.Topology.LoaderRootPaths
            : formation.Topology.FindByPath(parent.Identity.CanonicalBasePath)?.ChildPaths;
        if (childPaths is null)
        {
            children = [];
            return false;
        }

        var selected = new List<SourceLogicalSource>();
        var physicalPaths = new HashSet<string>(PhysicalIdentityTracker.PathComparer);
        foreach (var childPath in childPaths)
        {
            var child = formation.FindSource(childPath);
            if (child is null)
            {
                children = [];
                return false;
            }

            if (child.Base.Form == SourceDocumentForm.OverwriteCompanion)
            {
                continue;
            }

            if (PhysicalIdentityTracker.PathComparer.Equals(
                    child.Base.PhysicalPath,
                    parent.Base.PhysicalPath))
            {
                children = [];
                return false;
            }

            if (physicalPaths.Add(child.Base.PhysicalPath))
            {
                selected.Add(child);
            }
        }

        children = selected;
        return true;
    }

    private static bool IsContained(SourceCatalogue catalogue, SourceLogicalSource source)
        => IsContained(catalogue, source.Base.CanonicalPath)
            && (source.Overwrite is null
                || IsContained(catalogue, source.Overwrite.CanonicalPath));

    private static bool IsContained(SourceCatalogue catalogue, string canonicalPath)
        => catalogue.FindCandidateByPath(canonicalPath)?.PhysicalState == PhysicalPathState.Contained;

    private static bool IsOrdinaryMetadataSource(SourceDocumentForm form)
        => form == SourceDocumentForm.Markdown || SourceFormClassifier.IsEntrypoint(form);

    private static string? ReadCause(RouteSourceObservation observation)
        => observation.FrameworkMetadata.FailureKind switch
        {
            FrameworkDocumentMetadataFailureKind.Malformed =>
                "The source frontmatter or Open Forge metadata shape is malformed.",
            FrameworkDocumentMetadataFailureKind.Duplicate =>
                "The source frontmatter contains a duplicate Open Forge metadata key.",
            FrameworkDocumentMetadataFailureKind.None => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(observation),
                observation.FrameworkMetadata.FailureKind,
                "The Framework metadata failure kind is not defined."),
        };
}
