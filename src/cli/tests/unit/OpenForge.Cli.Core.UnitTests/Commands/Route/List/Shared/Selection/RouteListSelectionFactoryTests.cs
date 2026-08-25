using System.Reflection;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List.Shared.Selection;

public sealed class RouteListSelectionFactoryTests
{
    [Fact(DisplayName = "Route-list selection policy classifies an omitted reference as Loader roots")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void OmittedReferenceUsesLoaderRootSelectionPolicy()
    {
        var selection = RouteListSelectionFactory.Attempted(null);

        Assert.Equal(RouteListSelectionKind.LoaderRoots, selection.Kind);
        Assert.Null(selection.AttemptedId);
        Assert.Null(selection.AttemptedPath);
        Assert.Null(selection.ResolvedId);
        Assert.Null(selection.ResolvedPath);
    }

    [Fact(DisplayName = "Route-list selection factory forms Loader roots without any attempted or resolved identity")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void LoaderRootsContainNoSourceIdentity()
    {
        var selection = RouteListSelectionFactory.LoaderRoots();

        Assert.Equal(RouteListSelectionKind.LoaderRoots, selection.Kind);
        Assert.Null(selection.AttemptedId);
        Assert.Null(selection.AttemptedPath);
        Assert.Null(selection.ResolvedId);
        Assert.Null(selection.ResolvedPath);
        Assert.True(selection.IsResolved);
    }

    [Fact(DisplayName = "Route-list selection factory retains only an attempted source ID")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void AttemptedIdRetainsOnlyId()
    {
        var selection = RouteListSelectionFactory.AttemptedId("memory/working");

        Assert.Equal(RouteListSelectionKind.SourceId, selection.Kind);
        Assert.Equal("memory/working", selection.AttemptedId);
        Assert.Null(selection.AttemptedPath);
        Assert.Null(selection.ResolvedId);
        Assert.Null(selection.ResolvedPath);
        Assert.False(selection.IsResolved);
    }

    [Fact(DisplayName = "Route-list selection factory retains only a canonical attempted exact path")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void AttemptedPathRetainsOnlyPath()
    {
        var selection = RouteListSelectionFactory.AttemptedPath(".agents/memory/_memory.md");

        Assert.Equal(RouteListSelectionKind.SourcePath, selection.Kind);
        Assert.Null(selection.AttemptedId);
        Assert.Equal(".agents/memory/_memory.md", selection.AttemptedPath);
        Assert.Null(selection.ResolvedId);
        Assert.Null(selection.ResolvedPath);
        Assert.False(selection.IsResolved);
    }

    [Fact(DisplayName = "Route-list selection factory gives a resolved ID selection both source identities")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ResolvedIdContainsBothIdentities()
    {
        var source = Source();
        var selection = RouteListSelectionFactory.ResolvedId("memory", source);

        Assert.Equal(RouteListSelectionKind.SourceId, selection.Kind);
        Assert.Equal("memory", selection.AttemptedId);
        Assert.Null(selection.AttemptedPath);
        Assert.Equal(source.Id, selection.ResolvedId);
        Assert.Equal(source.CanonicalPath, selection.ResolvedPath);
        Assert.True(selection.IsResolved);
    }

    [Fact(DisplayName = "Route-list selection factory gives a resolved path selection both source identities")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ResolvedPathContainsBothIdentities()
    {
        var source = Source();
        var selection = RouteListSelectionFactory.ResolvedPath(".agents/memory/_memory.md", source);

        Assert.Equal(RouteListSelectionKind.SourcePath, selection.Kind);
        Assert.Null(selection.AttemptedId);
        Assert.Equal(".agents/memory/_memory.md", selection.AttemptedPath);
        Assert.Equal(source.Id, selection.ResolvedId);
        Assert.Equal(source.CanonicalPath, selection.ResolvedPath);
        Assert.True(selection.IsResolved);
    }

    [Fact(DisplayName = "Route-list selection rejects construction outside its factory boundary")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void SelectionRejectsConstructionOutsideFactory()
    {
        var directStaticMethods = typeof(RouteListSelection)
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(method => method.DeclaringType == typeof(RouteListSelection)
                && method.ReturnType == typeof(RouteListSelection))
            .Select(method => method.Name)
            .ToArray();

        Assert.Empty(directStaticMethods);
        Assert.Throws<InvalidOperationException>(() => new RouteListSelection(
            new object(),
            RouteListSelectionKind.LoaderRoots,
            null,
            null,
            null,
            null));
    }

    [Fact(DisplayName = "Route-list selection factory rejects null or empty identity inputs")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void FactoryRejectsInvalidIdentityInputs()
    {
        Assert.ThrowsAny<ArgumentException>(() => RouteListSelectionFactory.AttemptedId(""));
        Assert.ThrowsAny<ArgumentException>(() => RouteListSelectionFactory.AttemptedPath(""));
        Assert.ThrowsAny<ArgumentException>(() => RouteListSelectionFactory.ResolvedId("memory", null!));
        Assert.ThrowsAny<ArgumentException>(() => RouteListSelectionFactory.ResolvedPath(".agents/memory.md", null!));
        Assert.ThrowsAny<ArgumentException>(() => RouteListSelectionFactory.ResolvedPath(
            "./.agents/memory/_memory.md",
            Source()));
        Assert.ThrowsAny<ArgumentException>(() => RouteListSelectionFactory.ResolvedId("other", Source()));
        Assert.ThrowsAny<ArgumentException>(() => RouteListSelectionFactory.ResolvedPath(
            ".agents/other.md",
            Source()));
    }

    private static RouteSource Source()
    {
        return RouteSourceTestData.Source(
            ".agents/memory/_memory.md",
            RouteSourceKind.Entrypoint);
    }
}
