using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Commands.Route.List.Topology;

namespace OpenForge.Cli.UnitTests.Commands.Route.List;

public sealed class RouteListSelectionRefinementTests
{
    [Fact(DisplayName = "Route-list selection factory forms Loader roots without source identity"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public void FactoryFormsLoaderRoots()
    {
        var selection = RouteListSelectionFactory.LoaderRoots();

        Assert.Equal(RouteListSelectionKind.LoaderRoots, selection.Kind);
        Assert.Null(selection.SourceId);
        Assert.Null(selection.SourcePath);
    }

    [Fact(DisplayName = "Route-list selection factory retains only an attempted source ID"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public void FactoryFormsAttemptedSourceId()
    {
        var selection = RouteListSelectionFactory.AttemptedSourceId("memory/working");

        Assert.Equal(RouteListSelectionKind.ExplicitSource, selection.Kind);
        Assert.Equal("memory/working", selection.SourceId);
        Assert.Null(selection.SourcePath);
    }

    [Fact(DisplayName = "Route-list selection factory retains only an attempted exact path"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public void FactoryFormsAttemptedExactPath()
    {
        var selection = RouteListSelectionFactory.AttemptedExactPath(".agents/memory/_memory.md");

        Assert.Equal(RouteListSelectionKind.ExplicitSource, selection.Kind);
        Assert.Null(selection.SourceId);
        Assert.Equal(".agents/memory/_memory.md", selection.SourcePath);
    }

    [Fact(DisplayName = "Route-list selection factory gives a resolved source both canonical identities"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public void FactoryFormsResolvedSource()
    {
        var selection = RouteListSelectionFactory.ResolvedSource(
            "memory",
            ".agents/memory/_memory.md");

        Assert.Equal(RouteListSelectionKind.ExplicitSource, selection.Kind);
        Assert.Equal("memory", selection.SourceId);
        Assert.Equal(".agents/memory/_memory.md", selection.SourcePath);
    }
}
