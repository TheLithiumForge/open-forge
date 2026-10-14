using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Discovery;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Sharing;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.RouteScaffolding;

[Trait("Feature", "route-scaffolding"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RouteScaffoldChainTests
{
    [Fact(DisplayName = "Scaffold discovery selects eligible ancestors and orders depth before ordinal path")]
    public void EligibleAncestorsAreOrdered()
    {
        var request = Request(
            [".agents/guidance/_guidance.md"],
            [".agents/guidance/z/b/note.md", ".agents/guidance/a/c/note.md", ".agents/unknown/deep/note.md"]);
        var result = new RouteScaffoldChainDiscovery().Discover(request);

        Assert.False(result.IsBlocked);
        Assert.Equal(
            ["guidance/a", "guidance/z", "guidance/a/c", "guidance/z/b"],
            result.Gaps.Select(gap => gap.FolderId));
    }

    [Fact(DisplayName = "Scaffold ordering compares canonical file paths when folder names share a prefix")]
    public void CanonicalPathOrderingHandlesFolderPrefixes()
    {
        var request = Request([".agents/guidance/_guidance.md"],
            [".agents/guidance/a/note.md", ".agents/guidance/a-b/note.md"]);
        Assert.Equal([".agents/guidance/a-b/_a-b.md", ".agents/guidance/a/_a.md"],
            new RouteScaffoldChainDiscovery().Discover(request).Gaps.Select(gap => gap.CanonicalPath));
    }

    [Fact(DisplayName = "Scaffold discovery honors explicit leaf selection and preserves compatibility entrypoints")]
    public void SelectedLeafKeepsCompatibility()
    {
        var request = Request(
            [".agents/guidance/_guidance.md", ".agents/guidance/team/index.md"],
            [".agents/guidance/team/deep/note.md", ".agents/guidance/other/deep/note.md"]);
        request = request with { EligibleSources = [request.EligibleSources[0]] };
        var result = new RouteScaffoldChainDiscovery().Discover(request);

        Assert.Equal("guidance/team/deep", Assert.Single(result.Gaps).FolderId);
        Assert.Empty(result.Boundaries);
    }

    [Fact(DisplayName = "Scaffold discovery prunes removed private and native package paths")]
    public void PoliciesPruneWholeChains()
    {
        var request = Request(
            [".agents/guidance/_guidance.md", ".agents/skills/_skills.md", ".agents/skills/tool/SKILL.md"],
            [".agents/guidance/removed/deep/note.md", ".agents/guidance/private/deep/note.md", ".agents/skills/tool/references/note.md"]);
        request = request with
        {
            Settings = WorkspaceSettingsDocument.Empty with { RemovedDirectories = [".agents/guidance/removed"] },
            Sharing = new SourceSharing([new SourceSharingRoute(".agents/guidance/private", ".agents/guidance/private/_private.md")]),
        };
        var result = new RouteScaffoldChainDiscovery().Discover(request);

        Assert.Empty(result.Gaps);
        Assert.Contains(result.Boundaries, boundary => boundary.Kind == RouteScaffoldBoundaryKind.SharedEntrypointMissing);
    }

    [Theory(DisplayName = "Scaffold discovery blocks duplicate entrypoints and occupied route identities")]
    [InlineData(".agents/guidance/team/index.md", ".agents/guidance/team/_team.md", (int)RouteScaffoldBoundaryKind.EntrypointAmbiguous)]
    [InlineData(".agents/guidance/team.md", ".agents/guidance/_guidance.md", (int)RouteScaffoldBoundaryKind.IdentityOccupied)]
    public void UnsafeIdentityBlocks(string first, string second, int expected)
    {
        var request = Request(
            [".agents/guidance/_guidance.md", first, second],
            [".agents/guidance/team/deep/note.md"]);
        var result = new RouteScaffoldChainDiscovery().Discover(request);

        Assert.True(result.IsBlocked);
        Assert.Contains(result.Boundaries, boundary => boundary.Kind == (RouteScaffoldBoundaryKind)expected);
    }

    [Fact(DisplayName = "Scaffold discovery never invents a root or scaffolds resources overwrites and removed categories")]
    public void NonEligiblePathsHaveNoGaps()
    {
        var request = Request(
            [".agents/guidance/_guidance.md"],
            [".agents/guidance/empty/note.overwrite.md", ".agents/unknown/deep/note.md"]);
        Assert.Empty(new RouteScaffoldChainDiscovery().Discover(request).Gaps);
        request = Request([".agents/guidance/_guidance.md"], [".agents/guidance/deep/note.md"]) with
        {
            Settings = WorkspaceSettingsDocument.Empty with { RemovedCategories = ["guidance"] },
        };
        Assert.Empty(new RouteScaffoldChainDiscovery().Discover(request).Gaps);
    }

    [Fact(DisplayName = "Recorded entrypoint file removal prunes all ancestors needed only by that source")]
    public void RemovedEntrypointDoesNotLeavePartialChain()
    {
        var request = Request([".agents/guidance/_guidance.md"], [".agents/guidance/team/deep/note.md"]) with
        {
            Settings = WorkspaceSettingsDocument.Empty with { RemovedFiles = [".agents/guidance/team/deep/_deep.md"] },
        };
        Assert.Empty(new RouteScaffoldChainDiscovery().Discover(request).Gaps);
    }

    [Fact(DisplayName = "Root duplication blocks selected scaffolding and an occupied target is never replaced")]
    public void RootAndTargetBoundariesAreExplicit()
    {
        var request = Request([".agents/guidance/_guidance.md", ".agents/guidance/index.md"], [".agents/guidance/deep/note.md"]);
        Assert.Contains(new RouteScaffoldChainDiscovery().Discover(request).Boundaries,
            boundary => boundary.Kind == RouteScaffoldBoundaryKind.EntrypointAmbiguous);
        request = Request([".agents/guidance/_guidance.md"], [".agents/guidance/deep/note.md"]) with
        {
            OccupiedPaths = [".agents/guidance/deep/_deep.md"],
        };
        Assert.Contains(new RouteScaffoldChainDiscovery().Discover(request).Boundaries,
            boundary => boundary.Kind == RouteScaffoldBoundaryKind.PathOccupied);
    }

    internal static RouteScaffoldDiscoveryRequest Request(string[] existingPaths, string[] selectedPaths)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "scaffold-unit"));
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var sources = existingPaths.Concat(selectedPaths).Distinct(StringComparer.Ordinal)
            .Where(path => !path.EndsWith(".overwrite.md", StringComparison.Ordinal))
            .Select(path => new RouteScaffoldSourceBuilder().Create(workspace, path)).ToImmutableArray();
        var candidates = existingPaths.Concat(selectedPaths).Distinct(StringComparer.Ordinal).Select(path =>
        {
            if (!SourceFormClassifier.TryClassify(path, out var form))
                throw new InvalidOperationException();
            var physicalPath = Path.GetFullPath(SourceLogicalPath.ToLexicalPath(root, path));
            return new SourceCandidate(path, form, SourceIdentity.DeriveId(path), PhysicalPathState.Contained,
                physicalPath, Path.GetDirectoryName(physicalPath));
        });
        return new RouteScaffoldDiscoveryRequest
        {
            Catalogue = new SourceCatalogue(workspace, candidates, sources, [], isCancelled: false),
            RecognizedRootPaths = sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form)
                && SourceLogicalPath.ReadParent(SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath)) == ".agents")
                .Select(source => source.Identity.CanonicalBasePath).ToImmutableArray(),
            EligibleSources = sources.Where(source => selectedPaths.Contains(source.Identity.CanonicalBasePath, StringComparer.Ordinal)).ToImmutableArray(),
            Settings = WorkspaceSettingsDocument.Empty,
        };
    }
}
