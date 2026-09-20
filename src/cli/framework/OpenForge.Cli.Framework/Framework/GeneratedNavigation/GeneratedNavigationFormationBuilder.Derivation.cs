using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation;

internal sealed partial class GeneratedNavigationFormationBuilder
{
    internal sealed class DerivedComponents
    {
        private readonly IReadOnlyDictionary<string, SourceLogicalSource> _sourcesByPath;

        internal DerivedComponents(
            SourceCatalogue catalogue,
            IReadOnlyList<SourceLogicalSource> intendedSources)
        {
            if (catalogue.IsCancelled)
            {
                throw new ArgumentException(
                    "Generated navigation formation rejects a cancelled source catalogue.",
                    nameof(catalogue));
            }

            Catalogue = catalogue;
            Sources = MaterializeIntendedSources(catalogue, intendedSources);
            _sourcesByPath = BuildSourceLookup(Sources);
            var retainedEvidence = ReadRetainedObservedEvidence(catalogue, Sources);
            Issues = retainedEvidence.Issues;

            var topologyBuilder = new SourceRouteTopologyBuilder();
            var unrootedTopology = topologyBuilder.Build(Sources, []);
            PhysicalAliasGroups = BuildAliasGroups(
                retainedEvidence.Candidates,
                retainedEvidence.SourcesByCandidatePath,
                _sourcesByPath,
                unrootedTopology);
            IntendedTargetCollisions = BuildIntendedTargetCollisions(
                Sources,
                retainedEvidence.Candidates,
                retainedEvidence.MemberCandidatesByPath.Keys);
            var ambiguities = BuildAmbiguities(
                    unrootedTopology,
                    _sourcesByPath,
                    retainedEvidence.MemberCandidatesByPath,
                    retainedEvidence.SourcesByCandidatePath,
                    PhysicalAliasGroups)
                .ToList();
            Loader = FindSource(SourceLogicalPath.LoaderPath);
            if (Loader is not null && Loader.Base.Form != SourceDocumentForm.Loader)
            {
                Loader = null;
            }

            var structuralRootPaths = ReadStructuralRootPaths(
                Sources,
                retainedEvidence.MemberCandidatesByPath,
                PhysicalAliasGroups,
                IntendedTargetCollisions,
                ambiguities);
            Topology = topologyBuilder.Build(Sources, Loader is null ? [] : structuralRootPaths);
            Ambiguities = new ReadOnlyCollection<GeneratedNavigationFormationAmbiguity>(
                ambiguities
                    .OrderBy(ambiguity => ambiguity.Kind)
                    .ThenBy(ambiguity => ambiguity.Subject, StringComparer.Ordinal)
                    .ToArray());
        }

        internal SourceCatalogue Catalogue { get; }

        internal IReadOnlyList<SourceLogicalSource> Sources { get; }

        internal IReadOnlyList<SourceCatalogueIssue> Issues { get; }

        internal SourceRouteTopology Topology { get; }

        internal SourceLogicalSource? Loader { get; }

        internal IReadOnlyList<GeneratedNavigationPhysicalAliasGroup> PhysicalAliasGroups { get; }

        internal IReadOnlyList<GeneratedNavigationIntendedTargetCollision> IntendedTargetCollisions { get; }

        internal IReadOnlyList<GeneratedNavigationFormationAmbiguity> Ambiguities { get; }

        internal SourceLogicalSource? FindSource(string canonicalPath)
        {
            return _sourcesByPath.GetValueOrDefault(canonicalPath);
        }

        private static IReadOnlyDictionary<string, SourceLogicalSource> BuildSourceLookup(
            IEnumerable<SourceLogicalSource> sources)
        {
            var sourcesByPath = new Dictionary<string, SourceLogicalSource>(StringComparer.Ordinal);
            foreach (var source in sources)
            {
                sourcesByPath.Add(source.Identity.CanonicalBasePath, source);
                if (source.Overwrite is { } overwrite)
                {
                    sourcesByPath.Add(overwrite.CanonicalPath, source);
                }
            }

            return new ReadOnlyDictionary<string, SourceLogicalSource>(sourcesByPath);
        }
    }
}
