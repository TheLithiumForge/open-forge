using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Shared;

namespace OpenForge.Cli.IntegrationTests.Framework.GeneratedNavigation;

public sealed class GeneratedNavigationFormationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Generated navigation formation derives real Loader roots without reading the Loader body or writing files")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Integration")]
    public async Task LoaderBodyIsIrrelevantAndFormationDoesNotWrite()
    {
        using var workspace = SourceIntegrationWorkspace.Create("generated-navigation-formation-loader");
        workspace.Write(SourceLogicalPath.LoaderPath, "arbitrary authored and stale generated text\n");
        workspace.Write(".agents/root/_root.md", "# Root\n");
        workspace.Write(".agents/root/child.md", "# Child\n");
        var before = workspace.SnapshotHashes();

        var formation = await ReadFormationAsync(workspace);

        Assert.NotNull(formation.Loader);
        Assert.Equal([".agents/root/_root.md"], formation.Topology.LoaderRootPaths);
        Assert.Equal(
            [".agents/root/child.md"],
            formation.Topology.FindByPath(".agents/root/_root.md")?.ChildPaths);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Generated navigation formation keeps missing-Loader root ambiguity and missing-intermediate trees detached")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Integration")]
    public async Task MissingLoaderAndIntermediateDoNotInventTopology()
    {
        using var workspace = SourceIntegrationWorkspace.Create("generated-navigation-formation-detached");
        workspace.Write(".agents/root/_root.md", "# Root canonical\n");
        workspace.Write(".agents/root/index.md", "# Root compatibility\n");
        workspace.Write(".agents/root/deep/leaf/_leaf.md", "# Detached leaf\n");
        var before = workspace.SnapshotHashes();

        var formation = await ReadFormationAsync(workspace);

        Assert.Null(formation.Loader);
        Assert.Empty(formation.Topology.LoaderRootPaths);
        var detached = formation.Topology.FindByPath(".agents/root/deep/leaf/_leaf.md");
        Assert.NotNull(detached);
        Assert.Equal(SourceRouteParentState.None, detached.ParentState);
        var ambiguity = Assert.Single(
            formation.Ambiguities,
            value => value.Kind == GeneratedNavigationFormationAmbiguityKind.RootEntrypoint);
        Assert.Equal(".agents/root", ambiguity.Subject);
        Assert.Equal(
            [".agents/root/_root.md", ".agents/root/index.md"],
            ambiguity.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Generated navigation formation blocks multiple real recognized entrypoints for one Loader root folder")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Integration")]
    public async Task RealRootEntrypointAmbiguityAdmitsNoLoaderRoot()
    {
        using var workspace = SourceIntegrationWorkspace.Create("generated-navigation-formation-root-ambiguity");
        workspace.Write(SourceLogicalPath.LoaderPath, "Loader body is irrelevant\n");
        workspace.Write(".agents/root/_root.md", "# Root canonical\n");
        workspace.Write(".agents/root/index.md", "# Root compatibility\n");
        var before = workspace.SnapshotHashes();

        var formation = await ReadFormationAsync(workspace);

        var ambiguity = Assert.Single(
            formation.Ambiguities,
            value => value.Kind == GeneratedNavigationFormationAmbiguityKind.RootEntrypoint);
        Assert.Equal(".agents/root", ambiguity.Subject);
        Assert.Equal(
            [".agents/root/_root.md", ".agents/root/index.md"],
            ambiguity.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Empty(formation.Topology.LoaderRootPaths);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Generated navigation formation proves compatible leaf aliases and blocks incompatible rooted aliases on the current host")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Integration")]
    public async Task RealSymbolicLinksProduceCompatibleAndIncompatibleAliasFacts()
    {
        using var workspace = SourceIntegrationWorkspace.Create("generated-navigation-formation-aliases");
        workspace.Write(SourceLogicalPath.LoaderPath, "Loader body is not formation authority\n");
        workspace.Write(".agents/root/_root.md", "# Root\n");
        workspace.Write("shared/leaf.md", "# Shared leaf\n");
        workspace.Write("shared/root.md", "# Shared root\n");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/root/a.md",
                workspace.Absolute("shared/leaf.md"),
                out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/root/z.md",
                workspace.Absolute("shared/leaf.md"),
                out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/alpha/_alpha.md",
                workspace.Absolute("shared/root.md"),
                out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/zeta/_zeta.md",
                workspace.Absolute("shared/root.md"),
                out _),
            "This integration case requires real symbolic-link support.");
        var before = workspace.SnapshotHashes();

        var formation = await ReadFormationAsync(workspace);

        Assert.Equal(2, formation.PhysicalAliasGroups.Count);
        var compatible = Assert.Single(
            formation.PhysicalAliasGroups,
            group => group.Compatibility == GeneratedNavigationPhysicalAliasCompatibility.Compatible);
        Assert.Equal(
            [".agents/root/a.md", ".agents/root/z.md"],
            compatible.Candidates.Select(candidate => candidate.CanonicalPath));
        var incompatible = Assert.Single(
            formation.PhysicalAliasGroups,
            group => group.Compatibility == GeneratedNavigationPhysicalAliasCompatibility.Incompatible);
        Assert.Equal(
            [".agents/alpha/_alpha.md", ".agents/zeta/_zeta.md"],
            incompatible.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Equal([".agents/root/_root.md"], formation.Topology.LoaderRootPaths);
        Assert.Contains(formation.Ambiguities, ambiguity =>
            ambiguity.Kind == GeneratedNavigationFormationAmbiguityKind.PhysicalAlias);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task<GeneratedNavigationFormation> ReadFormationAsync(
        SourceIntegrationWorkspace workspace)
    {
        var catalogue = await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(workspace.Workspace, [SourceLogicalPath.AgentsRoot]),
            TestContext.Current.CancellationToken);
        Assert.False(catalogue.IsCancelled);
        return new GeneratedNavigationFormationBuilder().Build(catalogue);
    }
}
