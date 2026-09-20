using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal static class RouteDoctorFactReader
{
    internal static IReadOnlyList<RouteDeclaredRootObservation> ReadDeclaredRoots(
        SourceRouteFacts routes)
    {
        var observations = routes.Issues
            .Where(issue => issue.Code == SourceRouteIssueCode.LoaderDestinationMissing)
            .Select(issue => RouteDeclaredRootObservation.Missing(
                issue.CanonicalPath,
                issue.RelatedPaths.Single()))
            .ToList();
        var factsByPath = routes.RouteFacts.ToDictionary(
            fact => fact.Identity.CanonicalBasePath,
            StringComparer.Ordinal);
        observations.AddRange(routes.Topology.LoaderRootPaths
            .Where(path => !factsByPath.TryGetValue(path, out var fact)
                || fact.State != SourceRouteState.Routed)
            .Select(path => RouteDeclaredRootObservation.Unreachable(
                path,
                SourceLogicalPath.LoaderPath)));
        return observations
            .OrderBy(observation => observation.State)
            .ThenBy(observation => observation.Path, StringComparer.Ordinal)
            .ToArray();
    }

    internal static IReadOnlyList<RouteMetadataObservation> ReadMetadata(
        RouteSourceInspection inspection)
    {
        return inspection.Sources
            .Where(source => !IsValidNativeSkill(source))
            .Select(source => new RouteMetadataObservation(
                source.Source.Identity.CanonicalBasePath,
                source.FrameworkMetadata))
            .ToArray();
    }

    internal static IReadOnlyList<RouteShapeObservation> ReadShape(
        RouteSourceInspection inspection)
    {
        var observations = new List<RouteShapeObservation>();
        observations.AddRange(inspection.Catalogue.Issues
            .Where(issue => issue.Code is SourceCatalogueIssueCode.EntrypointAmbiguous
                or SourceCatalogueIssueCode.EntrypointCompatibilityCollision)
            .Select(issue => RouteShapeObservation.EntrypointDuplicate(issue.RelatedPaths)));
        var validNativeSkills = inspection.Sources
            .Where(IsValidNativeSkill)
            .Select(source => (
                SkillPath: source.Source.Identity.CanonicalBasePath,
                ParentDirectory: SourceLogicalPath.ReadParent(
                    source.Source.Identity.CanonicalBasePath)))
            .ToArray();

        foreach (var fact in inspection.Routes.RouteFacts.Where(fact =>
                     fact.State == SourceRouteState.Unrouted))
        {
            if (IsNativeSkillResourcePath(
                    fact.Identity.CanonicalBasePath,
                    inspection.Sources,
                    validNativeSkills))
            {
                continue;
            }

            var node = inspection.Routes.Topology.FindByPath(fact.Identity.CanonicalBasePath);
            observations.Add(node is { ParentState: SourceRouteParentState.None, ChildPaths.Count: > 0 }
                ? RouteShapeObservation.Detached(
                    fact.Identity.CanonicalBasePath,
                    node.ChildPaths)
                : RouteShapeObservation.Unreachable(fact.Identity.CanonicalBasePath));
        }

        foreach (var source in inspection.Sources.Where(source =>
                     source.GeneratedEntries.State == SourceGeneratedEntriesState.Complete
                     && source.Layers[0].Text is not null))
        {
            var locations = new Utf8SourceMap(source.Layers[0].Text
                ?? throw new InvalidOperationException("A readable source layer requires retained text."));
            foreach (var entry in source.GeneratedEntries.Entries)
            {
                var resolved = SourceGeneratedDestinationResolver.Resolve(
                    source.Source.Identity.CanonicalBasePath,
                    source.Source.Base.Form == SourceDocumentForm.Loader,
                    entry.Destination);
                if (resolved is null
                    || inspection.Catalogue.FindCandidateByPath(resolved)?.Form
                        != SourceDocumentForm.OverwriteCompanion)
                {
                    continue;
                }

                observations.Add(RouteShapeObservation.OverwriteIndependentIndex(
                    source.Source.Identity.CanonicalBasePath,
                    resolved,
                    entry.Destination,
                    locations.Map(entry.Span.Start, entry.Span.Length)));
            }
        }

        return observations
            .OrderBy(observation => observation.Kind)
            .ThenBy(observation => observation.Path, StringComparer.Ordinal)
            .ThenBy(observation => observation.AuthoredValue, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsValidNativeSkill(RouteSourceObservation source)
    {
        return source.Source.Base.Form == SourceDocumentForm.Skill
            && source.AuthoredMetadata.State == SourceAuthoredMetadataState.Complete
            && source.IsReadable
            && source.Document is not null;
    }

    private static bool IsNativeSkillResourcePath(
        string path,
        IReadOnlyList<RouteSourceObservation> sources,
        IReadOnlyList<(string SkillPath, string ParentDirectory)> nativeSkills)
    {
        var source = sources.FirstOrDefault(candidate =>
            string.Equals(
                candidate.Source.Identity.CanonicalBasePath,
                path,
                StringComparison.Ordinal));
        if (source is null
            || source.Source.Base.Form == SourceDocumentForm.Skill
            || !source.IsReadable
            || source.Document is null
            || source.AuthoredMetadata.State == SourceAuthoredMetadataState.Malformed
            || source.FrameworkMetadata.State == FrameworkDocumentMetadataState.Malformed)
        {
            return false;
        }

        return nativeSkills.Any(skill =>
            !string.Equals(skill.SkillPath, path, StringComparison.Ordinal)
            && IsStrictlyUnderDirectory(path, skill.ParentDirectory));
    }

    private static bool IsStrictlyUnderDirectory(
        string path,
        string directory)
    {
        return path.Length > directory.Length
            && path[directory.Length] == '/'
            && string.Equals(
                path[..directory.Length],
                directory,
                StringComparison.Ordinal);
    }
}
