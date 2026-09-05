using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemoveSubjectResolutionTests
{
    [Fact(DisplayName = "Route Remove subject facts distinguish a leaf pair from a complete category inventory"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void SubjectFactsDistinguishLeafPairAndCategoryInventory()
    {
        var leaf = RouteRemoveTestData.LeafSubject();
        var category = RouteRemoveTestData.CategorySubject();

        Assert.Equal(RouteRemoveSubjectKind.Leaf, leaf.Kind);
        Assert.Equal(
            [RouteRemoveLayerKind.Base, RouteRemoveLayerKind.Overwrite],
            leaf.Layers.Select(layer => layer.Layer));
        Assert.Equal(
            [RouteRemoveItemKind.RoutedMarkdown, RouteRemoveItemKind.RoutedMarkdown],
            leaf.Items.Select(item => item.Kind));
        Assert.Equal(RouteRemoveSubjectKind.Category, category.Kind);
        Assert.Contains(category.Items, item => item.Kind == RouteRemoveItemKind.UnroutedMarkdown);
        Assert.Contains(category.Items, item => item.Kind == RouteRemoveItemKind.Resource);
        Assert.Equal("assets/settings.json", category.Items[^1].RelativePath);
    }

    [Fact(DisplayName = "Route Remove subject selection requires exactly one subject or boundary"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void SubjectSelectionRequiresExactlyOneResult()
    {
        var subject = new RouteRemoveResolvedSubject
        {
            Request = RouteRemoveTestData.Request(),
            Source = RouteRemoveTestData.Source(),
            Kind = RouteRemoveSubjectKind.Leaf,
            Catalogue = null!,
            SelectedSource = null!,
        };
        var boundary = RouteRemoveTestData.Formation();

        var selected = new RouteRemoveSubjectSelection(subject, null);
        var blocked = new RouteRemoveSubjectSelection(null, boundary);

        Assert.Same(subject, selected.Subject);
        Assert.Null(selected.Boundary);
        Assert.Null(blocked.Subject);
        Assert.Same(boundary, blocked.Boundary);
        Assert.Throws<ArgumentException>(() => new RouteRemoveSubjectSelection(null, null));
        Assert.Throws<ArgumentException>(() => new RouteRemoveSubjectSelection(subject, boundary));
    }

    [Fact(DisplayName = "Route Remove subject resolution preserves cancellation and exposed-path facts"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void SubjectResolutionPreservesExposureFacts()
    {
        var subject = new RouteRemoveResolvedSubject
        {
            Request = RouteRemoveTestData.Request(),
            Source = RouteRemoveTestData.Source(),
            Kind = RouteRemoveSubjectKind.Category,
            Catalogue = null!,
            SelectedSource = null!,
            NavigationExposure = new RouteRemoveNavigationExposure
            {
                ExposedPaths = [".agents/guidance/_guidance.md"],
                UnavailableParents = [".agents/loader.md"],
                IsCancelled = false,
            },
        };

        Assert.False(subject.NavigationExposure.IsCancelled);
        Assert.Equal([".agents/guidance/_guidance.md"], subject.NavigationExposure.ExposedPaths);
        Assert.Equal([".agents/loader.md"], subject.NavigationExposure.UnavailableParents);
    }
}
