using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

internal static class GeneratedNavigationTestData
{
    internal static GeneratedNavigationFormation Formation(
        SourceRouteTopology topology,
        IEnumerable<SourceLogicalSource> sources)
    {
        var catalogue = Catalogue(sources);
        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);
        AssertTopology(topology, formation.Topology);
        return formation;
    }

    internal static SourceCatalogue Catalogue(
        IEnumerable<SourceLogicalSource> sources,
        IEnumerable<SourceCandidate>? additionalCandidates = null,
        IEnumerable<SourceCatalogueIssue>? issues = null,
        bool isCancelled = false)
    {
        var materializedSources = sources.ToArray();
        var candidates = materializedSources
            .SelectMany(SourceCandidates)
            .Concat(additionalCandidates ?? [])
            .ToArray();
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "generated-navigation-unit"));
        return new SourceCatalogue(
            workspace: new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace),
            candidates: candidates,
            sources: materializedSources,
            issues: issues ?? [],
            isCancelled: isCancelled);
    }

    internal static SourceCandidate Candidate(
        string canonicalPath,
        SourceDocumentForm form,
        string physicalPath,
        string? physicalParentPath = null)
    {
        return new SourceCandidate(
            canonicalPath: canonicalPath,
            form: form,
            automaticId: SourceIdentity.DeriveId(canonicalPath),
            physicalState: PhysicalPathState.Contained,
            physicalPath: physicalPath,
            physicalParentPath: physicalParentPath ?? ReadParent(physicalPath));
    }

    internal static SourceCandidate UnrecognizedCandidate(
        string canonicalPath,
        string physicalPath,
        string? physicalParentPath = null)
    {
        return new SourceCandidate(
            canonicalPath: canonicalPath,
            form: null,
            automaticId: null,
            physicalState: PhysicalPathState.Contained,
            physicalPath: physicalPath,
            physicalParentPath: physicalParentPath ?? ReadParent(physicalPath));
    }

    internal static SourceLogicalSource Source(
        string canonicalPath,
        SourceDocumentForm form,
        string? physicalPath = null,
        string? overwritePhysicalPath = null)
    {
        var baseLayer = new SourceLayer(
            canonicalPath: canonicalPath,
            physicalPath: physicalPath ?? Physical(canonicalPath),
            form: form,
            kind: SourceLayerKind.Base);
        SourceLayer? overwrite = null;
        if (overwritePhysicalPath is not null)
        {
            overwrite = new SourceLayer(
                canonicalPath: canonicalPath[..^".md".Length] + ".overwrite.md",
                physicalPath: overwritePhysicalPath,
                form: SourceDocumentForm.OverwriteCompanion,
                kind: SourceLayerKind.Overwrite);
        }

        return new SourceLogicalSource(
            identity: new SourceLogicalIdentity(
                SourceIdentity.DeriveId(canonicalPath)
                    ?? throw new ArgumentException(
                        "A generated navigation test source requires an automatic ID.",
                        nameof(canonicalPath)),
                canonicalPath),
            @base: baseLayer,
            overwrite: overwrite);
    }

    internal static string Physical(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "generated-navigation-unit",
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static IEnumerable<SourceCandidate> SourceCandidates(SourceLogicalSource source)
    {
        yield return Candidate(
            source.Base.CanonicalPath,
            source.Base.Form,
            source.Base.PhysicalPath);
        if (source.Overwrite is { } overwrite)
        {
            yield return Candidate(
                overwrite.CanonicalPath,
                overwrite.Form,
                overwrite.PhysicalPath);
        }
    }

    private static void AssertTopology(
        SourceRouteTopology expected,
        SourceRouteTopology actual)
    {
        Assert.Equal(expected.LoaderRootPaths, actual.LoaderRootPaths);
        Assert.Equal(
            expected.Nodes.Select(node => node.Identity.CanonicalBasePath),
            actual.Nodes.Select(node => node.Identity.CanonicalBasePath));
        foreach (var expectedNode in expected.Nodes)
        {
            var actualNode = actual.FindByPath(expectedNode.Identity.CanonicalBasePath);
            Assert.NotNull(actualNode);
            Assert.Same(expectedNode.Identity, actualNode.Identity);
            Assert.Equal(expectedNode.ParentState, actualNode.ParentState);
            Assert.Equal(expectedNode.ParentPaths, actualNode.ParentPaths);
            Assert.Equal(expectedNode.ChildPaths, actualNode.ChildPaths);
        }
    }

    private static string ReadParent(string path)
    {
        return Path.GetDirectoryName(path)
            ?? throw new ArgumentException("A generated navigation test physical path requires a parent.", nameof(path));
    }
}
