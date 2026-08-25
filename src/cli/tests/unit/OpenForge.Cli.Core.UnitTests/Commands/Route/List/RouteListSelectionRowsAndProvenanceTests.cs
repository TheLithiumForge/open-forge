using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List;

public sealed class RouteListSelectionRowsAndProvenanceTests
{
    [Fact(DisplayName = "Route list selection keeps attempted and resolved identity distinct")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void SelectionKeepsAttemptedAndResolvedIdentityDistinct()
    {
        var source = RouteSourceTestData.Source(
            ".agents/memory/_memory.md",
            RouteSourceKind.Entrypoint);
        var roots = RouteListSelectionFactory.LoaderRoots();
        var attemptedId = RouteListSelectionFactory.AttemptedId("memory");
        var attemptedPath = RouteListSelectionFactory.AttemptedPath(".agents/memory/_memory.md");
        var resolvedId = RouteListSelectionFactory.ResolvedId("memory", source);
        var resolvedPath = RouteListSelectionFactory.ResolvedPath(
            ".agents/memory/_memory.md",
            source);

        Assert.True(roots.IsResolved);
        Assert.Equal(RouteListSelectionKind.LoaderRoots, roots.Kind);
        Assert.Equal("memory", attemptedId.AttemptedId);
        Assert.Null(attemptedId.AttemptedPath);
        Assert.False(attemptedId.IsResolved);
        Assert.Equal(".agents/memory/_memory.md", attemptedPath.AttemptedPath);
        Assert.Null(attemptedPath.AttemptedId);
        Assert.False(attemptedPath.IsResolved);
        Assert.Equal("memory", resolvedId.ResolvedId);
        Assert.Equal(".agents/memory/_memory.md", resolvedId.ResolvedPath);
        Assert.Equal(".agents/memory/_memory.md", resolvedPath.AttemptedPath);
        Assert.Equal("memory", resolvedPath.ResolvedId);
        Assert.ThrowsAny<ArgumentException>(() => RouteListSelectionFactory.AttemptedId(""));
        Assert.ThrowsAny<ArgumentException>(() => RouteListSelectionFactory.ResolvedPath(
            ".agents/a.md",
            null!));
    }

    [Fact(DisplayName = "Route list rows enforce hierarchy kind and authored facts")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void RowsEnforceHierarchyKindAndAuthoredFacts()
    {
        var root = RootRow(directChildCount: 1);
        var child = RouteListRow.RoutedLeaf(
            "memory/child",
            ".agents/memory/child.md",
            "memory",
            ".agents/memory/_memory.md",
            1,
            1,
            "Child description",
            ["Memory"],
            new RouteListProvenance(
                RouteListSelectionProvenance.Descendant,
                RouteListSourceProvenance.AuthoredLeaf,
                hasOverwrite: true));

        Assert.Equal(RouteListRowKind.Entrypoint, root.Kind);
        Assert.Equal("memory", root.Id);
        Assert.Equal(".agents/memory/_memory.md", root.Path);
        Assert.Null(root.ParentId);
        Assert.Null(root.ParentPath);
        Assert.Equal(0, root.AbsoluteDepth);
        Assert.Equal(0, root.RelativeDepth);
        Assert.Equal("Memory description", root.Description);
        Assert.Equal(["Memory"], root.Tags);
        Assert.Equal(1, root.DirectChildCount);
        Assert.Equal(RouteListSelectionProvenance.LoaderRoot, root.Provenance.Selection);
        Assert.Equal(RouteListSourceProvenance.AuthoredEntrypoint, root.Provenance.Source);
        Assert.Equal(RouteListRowKind.RoutedLeaf, child.Kind);
        Assert.Null(child.DirectChildCount);
        Assert.Equal("memory", child.ParentId);
        Assert.Equal("memory/child", child.Id);
        Assert.Equal(".agents/memory/child.md", child.Path);
        Assert.Equal(".agents/memory/_memory.md", child.ParentPath);
        Assert.Equal(1, child.AbsoluteDepth);
        Assert.Equal(1, child.RelativeDepth);
        Assert.Equal("Child description", child.Description);
        Assert.Equal(["Memory"], child.Tags);
        Assert.Equal(RouteListSourceProvenance.AuthoredLeaf, child.Provenance.Source);
        Assert.Equal(RouteListSelectionProvenance.Descendant, child.Provenance.Selection);
        Assert.True(child.Provenance.HasOverwrite);
        Assert.Throws<ArgumentException>(() => RouteListRow.RoutedLeaf(
            "child",
            ".agents/child.md",
            "parent",
            null,
            1,
            1,
            "Description",
            [],
            child.Provenance));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteListRow.Entrypoint(
            "root",
            ".agents/root/_root.md",
            null,
            null,
            0,
            0,
            "Root",
            [],
            -1,
            root.Provenance));
        var nestedLoaderRoot = RouteListRow.Entrypoint(
            "root",
            ".agents/root/_root.md",
            "parent",
            ".agents/parent/_parent.md",
            1,
            0,
            "Root",
            [],
            0,
            new RouteListProvenance(
                RouteListSelectionProvenance.LoaderRoot,
                RouteListSourceProvenance.AuthoredEntrypoint,
                hasOverwrite: false));
        Assert.Equal("parent", nestedLoaderRoot.ParentId);
        Assert.Equal(1, nestedLoaderRoot.AbsoluteDepth);
        Assert.Throws<ArgumentException>(() => RouteListRow.Entrypoint(
            "root",
            ".agents/root/_root.md",
            "parent",
            ".agents/parent/_parent.md",
            0,
            0,
            "Root",
            [],
            0,
            new RouteListProvenance(
                RouteListSelectionProvenance.LoaderRoot,
                RouteListSourceProvenance.AuthoredEntrypoint,
                hasOverwrite: false)));

        var unknownCount = RootRow(
            RouteListSelectionProvenance.ExplicitRoot,
            directChildCount: null);
        Assert.Null(unknownCount.DirectChildCount);
        var nestedExplicit = RouteListRow.Entrypoint(
            "memory/nested",
            ".agents/memory/nested/_nested.md",
            "memory",
            ".agents/memory/_memory.md",
            1,
            0,
            "Nested",
            [],
            null,
            new RouteListProvenance(
                RouteListSelectionProvenance.ExplicitRoot,
                RouteListSourceProvenance.AuthoredEntrypoint,
                false));
        var detached = RouteListRow.Entrypoint(
            "workspace",
            ".agents/workspace/_workspace.md",
            null,
            null,
            null,
            0,
            "Detached",
            [],
            null,
            new RouteListProvenance(
                RouteListSelectionProvenance.DetachedRoot,
                RouteListSourceProvenance.AuthoredEntrypoint,
                false));
        Assert.Equal("memory", nestedExplicit.ParentId);
        Assert.Equal(1, nestedExplicit.AbsoluteDepth);
        Assert.Null(detached.AbsoluteDepth);
        var nestedDetached = RouteListRow.Entrypoint(
            "workspace/nested",
            ".agents/workspace/nested/_nested.md",
            "workspace",
            ".agents/workspace/_workspace.md",
            null,
            0,
            "Nested detached entrypoint",
            [],
            null,
            new RouteListProvenance(
                RouteListSelectionProvenance.DetachedRoot,
                RouteListSourceProvenance.AuthoredEntrypoint,
                false));
        var detachedLeaf = RouteListRow.RoutedLeaf(
            "workspace/leaf",
            ".agents/workspace/leaf.md",
            "workspace",
            ".agents/workspace/_workspace.md",
            null,
            0,
            "Detached leaf",
            [],
            new RouteListProvenance(
                RouteListSelectionProvenance.DetachedRoot,
                RouteListSourceProvenance.AuthoredLeaf,
                false));
        Assert.Equal("workspace", nestedDetached.ParentId);
        Assert.Null(nestedDetached.AbsoluteDepth);
        Assert.Equal(RouteListRowKind.RoutedLeaf, detachedLeaf.Kind);
        Assert.Null(detachedLeaf.AbsoluteDepth);
        Assert.Throws<ArgumentException>(() => RouteListRow.Entrypoint(
            "detached-shaped",
            ".agents/detached-shaped/_detached-shaped.md",
            null,
            null,
            null,
            0,
            "Invalid explicit root",
            [],
            null,
            new RouteListProvenance(
                RouteListSelectionProvenance.ExplicitRoot,
                RouteListSourceProvenance.AuthoredEntrypoint,
                false)));
        Assert.Throws<ArgumentException>(() => RouteListRow.RoutedLeaf(
            "memory/zero-depth",
            ".agents/memory/zero-depth.md",
            "memory",
            ".agents/memory/_memory.md",
            0,
            1,
            "Invalid descendant absolute depth",
            [],
            new RouteListProvenance(
                RouteListSelectionProvenance.Descendant,
                RouteListSourceProvenance.AuthoredLeaf,
                false)));
        Assert.Throws<ArgumentException>(() => RouteListRow.Entrypoint(
            "invalid-depth",
            ".agents/invalid-depth/_invalid-depth.md",
            null,
            null,
            1,
            0,
            "Invalid explicit root",
            [],
            null,
            new RouteListProvenance(
                RouteListSelectionProvenance.ExplicitRoot,
                RouteListSourceProvenance.AuthoredEntrypoint,
                false)));
    }

    [Fact(DisplayName = "Route list collections are immutable snapshots")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CollectionsAreImmutableSnapshots()
    {
        var tags = new[] { "Memory" };
        var row = RouteListRow.Entrypoint(
            "memory",
            ".agents/memory/_memory.md",
            null,
            null,
            0,
            0,
            "Memory",
            tags,
            0,
            new RouteListProvenance(
                RouteListSelectionProvenance.LoaderRoot,
                RouteListSourceProvenance.AuthoredEntrypoint,
                false));
        var rows = new[] { row };
        var evidence = new[] { "Complete." };
        var coverage = RouteListCoverage.Complete(
            RouteListDepth.Default,
            RouteListDepth.Default,
            1,
            1,
            evidence);
        var result = RouteListResult.Create(
            CliSemanticStatus.Complete,
            RouteListContractTestData.Workspace(),
            RouteListSelectionFactory.LoaderRoots(),
            coverage,
            rows,
            [],
            null);

        tags[0] = "Changed";
        rows[0] = ChildRow();
        evidence[0] = "Changed.";
        Assert.Equal(["Memory"], row.Tags);
        Assert.Same(row, Assert.Single(result.Rows));
        Assert.Equal(["Complete."], coverage.Evidence);
    }

    private static RouteListRow RootRow(
        RouteListSelectionProvenance selection = RouteListSelectionProvenance.LoaderRoot,
        int? directChildCount = 0,
        string id = "memory",
        string path = ".agents/memory/_memory.md")
    {
        return RouteListRow.Entrypoint(
            id,
            path,
            null,
            null,
            0,
            0,
            "Memory description",
            ["Memory"],
            directChildCount,
            new RouteListProvenance(
                selection,
                RouteListSourceProvenance.AuthoredEntrypoint,
                hasOverwrite: false));
    }

    private static RouteListRow ChildRow(
        string id = "memory/child",
        string path = ".agents/memory/child.md")
    {
        return RouteListRow.RoutedLeaf(
            id,
            path,
            "memory",
            ".agents/memory/_memory.md",
            1,
            1,
            "Child",
            ["Memory"],
            new RouteListProvenance(
                RouteListSelectionProvenance.Descendant,
                RouteListSourceProvenance.AuthoredLeaf,
                false));
    }

}
