using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectProfileBuilderReadingTests
{
    [Fact(DisplayName = "Route inspect task-start membership is stable when the same source uses ID or exact-path selection")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void TaskStartMembershipDoesNotDependOnInspectionOperand()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries(
                declarations: ["- [Root](root/_root.md) - #LoadNow"]));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries(
                    declarations: ["- [Target](target.md) - #LoadNow"]),
                Tags = ["Route", "LoadNow"],
            });
        var target = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/target.md",
                Kind = RouteSourceKind.Markdown,
                Body = "target",
                Tags = ["Route", "LoadNow"],
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, target],
            [root.CanonicalPath]);

        var byId = Build(graph, target.CanonicalPath, RouteInspectSelectionMethod.AutomaticId);
        var byPath = Build(graph, target.CanonicalPath, RouteInspectSelectionMethod.ExactPath);

        Assert.True(byId.Reading.TaskStart.Value);
        Assert.True(byPath.Reading.TaskStart.Value);
        Assert.Equal(
            byId.Reading.Automatic.Value!.Reasons.Select(reason => reason.Kind),
            byPath.Reading.Automatic.Value!.Reasons.Select(reason => reason.Kind));
        Assert.Equal(
            byId.Measurements.SelectedClosure.Value!.UnicodeScalarCount,
            byPath.Measurements.SelectedClosure.Value!.UnicodeScalarCount);
        Assert.Equal(
            byId.Measurements.SelectionAddition.Value!.PhysicalFileCount,
            byPath.Measurements.SelectionAddition.Value!.PhysicalFileCount);
    }

    [Fact(DisplayName = "Route inspect startup roots and visible ordered LoadNow entries form the task-start closure")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void StartupClosureUsesLoaderRootsAndVisibleLoadNowEntries()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries(
                declarations: ["- [Root](root/_root.md) - #LoadNow"]));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries(
                    declarations:
                    [
                        "- [First](first.md) - #LoadNow",
                        "- [Second](second/_second.md) - #LoadNow",
                    ]),
                Tags = ["Route", "LoadNow"],
            });
        var first = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/first.md",
                Kind = RouteSourceKind.Markdown,
                Body = "first",
                Tags = ["Route", "LoadNow"],
            });
        var second = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/second/_second.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries(
                    declarations: ["- [Third](third.md) - #LoadNow"]),
                Tags = ["Route", "LoadNow"],
            });
        var third = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/second/third.md",
                Kind = RouteSourceKind.Markdown,
                Body = "third",
                Tags = ["Route", "LoadNow"],
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, first, second, third],
            [root.CanonicalPath]);

        var profile = Build(graph, third.CanonicalPath);

        Assert.True(profile.Reading.TaskStart.Value);
        var automatic = Assert.Single(profile.Reading.Automatic.Value!.Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.ParentLoadNow, automatic.Kind);
        Assert.Equal("root/second", automatic.RelatedSourceId);
        var later = profile.Reading.Later.Value!;
        Assert.False(later.MayBeReadAgain);
        Assert.Empty(later.Occasions);
        Assert.Equal(4, profile.Measurements.TaskStartOverlap.Value!.PhysicalFileCount);
        Assert.Equal(4, profile.Measurements.SelectedClosure.Value!.PhysicalFileCount);
        Assert.Equal(0, profile.Measurements.SelectionAddition.Value!.PhysicalFileCount);
    }

    [Fact(DisplayName = "Route inspect globally routed non-entrypoint KeepInMind retains its ancestor closure and every later occasion")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void GlobalRoutedFileKeepInMindRetainsAncestorsAndLaterOccasions()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries(
                declarations: ["- [Root](root/_root.md) - #LoadNow"]));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("root"),
                Tags = ["Route", "LoadNow"],
            });
        var ancestor = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/other/_other.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries(
                    declarations: ["- [Continuity](continuity.md) - #KeepInMind"]),
            });
        var continuity = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/other/continuity.md",
                Kind = RouteSourceKind.Markdown,
                Body = "continuity",
                Tags = ["Route", "KeepInMind"],
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, ancestor, continuity],
            [root.CanonicalPath]);

        var profile = Build(graph, continuity.CanonicalPath);

        Assert.True(profile.Reading.TaskStart.Value);
        var automatic = Assert.Single(profile.Reading.Automatic.Value!.Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.RoutedFileKeepInMind, automatic.Kind);
        Assert.Null(automatic.RelatedSourceId);
        Assert.Equal(
            [
                RouteInspectAutomaticReadingEvent.TaskReview,
                RouteInspectAutomaticReadingEvent.LaterReview,
            ],
            automatic.Events);
        Assert.True(profile.Reading.Later.Value!.MayBeReadAgain);
        Assert.Equal(
            [
                RouteInspectLaterReadOccasion.ContextRestoration,
                RouteInspectLaterReadOccasion.Handoff,
                RouteInspectLaterReadOccasion.Closeout,
                RouteInspectLaterReadOccasion.FollowupTransition,
            ],
            profile.Reading.Later.Value.Occasions);
        Assert.Equal(3, profile.Measurements.SelectedClosure.Value!.PhysicalFileCount);
        Assert.Equal(3, profile.Measurements.TaskStartOverlap.Value!.PhysicalFileCount);
        Assert.Equal(0, profile.Measurements.SelectionAddition.Value!.PhysicalFileCount);
    }

    [Fact(DisplayName = "Route inspect selected KeepInMind entrypoints retain task-start visibility and route selection reasons")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void SelectedKeepInMindEntrypointRetainsApplicableReasons()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries(
                declarations: ["- [Root](root/_root.md) - #LoadNow"]));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries(
                    declarations: ["- [Selected](selected/_selected.md) - #KeepInMind"]),
                Tags = ["Route", "LoadNow"],
            });
        var selected = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/selected/_selected.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("selected"),
                Tags = ["Route", "KeepInMind"],
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, selected],
            [root.CanonicalPath]);

        var profile = Build(graph, selected.CanonicalPath);

        Assert.True(profile.Reading.TaskStart.Value);
        var automatic = Assert.Single(profile.Reading.Automatic.Value!.Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.EntrypointKeepInMind, automatic.Kind);
        Assert.Null(automatic.RelatedSourceId);
        Assert.Equal(
            [
                RouteInspectAutomaticReadingEvent.TaskStartVisible,
                RouteInspectAutomaticReadingEvent.RouteSelected,
            ],
            automatic.Events);
        Assert.True(profile.Reading.Later.Value!.MayBeReadAgain);
        Assert.Equal(2, profile.Measurements.SelectedClosure.Value!.PhysicalFileCount);
    }

    [Fact(DisplayName = "Route inspect entrypoint KeepInMind selection preserves the selected reason while its ancestor is measured in the closure")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void KeepInMindEntrypointSelectionAndAncestorClosureRemainDistinct()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries(
                declarations: ["- [Root](root/_root.md) - #LoadNow"]));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("root"),
            });
        var scope = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/scope/_scope.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries(
                    declarations: ["- [Selected](selected/_selected.md) - #KeepInMind"]),
            });
        var selected = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/scope/selected/_selected.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries(
                    declarations: ["- [Leaf](leaf.md)"]),
                Tags = ["Route", "KeepInMind"],
            });
        var leaf = RouteInspectSourceTestData.Source(
            ".agents/root/scope/selected/leaf.md",
            RouteSourceKind.Markdown,
            "leaf");
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, scope, selected, leaf],
            [root.CanonicalPath]);

        var selectedProfile = Build(graph, selected.CanonicalPath, RouteInspectSelectionMethod.ExactPath);
        var leafProfile = Build(graph, leaf.CanonicalPath);

        var selectedReason = Assert.Single(selectedProfile.Reading.Automatic.Value!.Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.EntrypointKeepInMind, selectedReason.Kind);
        Assert.Equal([RouteInspectAutomaticReadingEvent.RouteSelected], selectedReason.Events);
        Assert.False(selectedProfile.Reading.TaskStart.Value);
        Assert.Equal(4, leafProfile.Measurements.SelectedClosure.Value!.PhysicalFileCount);
        Assert.Equal(3, leafProfile.Measurements.SelectionAddition.Value!.PhysicalFileCount);
        Assert.Equal(1, leafProfile.Measurements.TaskStartOverlap.Value!.PhysicalFileCount);
        Assert.Equal(RouteInspectAutomaticReadingKind.OnDemand, Assert.Single(leafProfile.Reading.Automatic.Value!.Reasons).Kind);
    }

    [Fact(DisplayName = "Route inspect LoadNow and routed-file KeepInMind reasons remain dual and overwrite remains last")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void DualLoadNowAndKeepInMindReasonsRemainOrdered()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("Loader"));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries(
                    declarations: ["- [Target](target.md) - #LoadNow #KeepInMind"]),
            });
        var target = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/target.md",
                Kind = RouteSourceKind.Markdown,
                Body = "target",
                Tags = ["Route", "LoadNow", "KeepInMind"],
                OverwritePath = ".agents/root/target.overwrite.md",
                OverwriteBody = "custom",
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, target],
            [root.CanonicalPath]);

        var profile = Build(graph, target.CanonicalPath);

        Assert.Equal(
            [
                RouteInspectAutomaticReadingKind.ParentLoadNow,
                RouteInspectAutomaticReadingKind.RoutedFileKeepInMind,
                RouteInspectAutomaticReadingKind.OverwriteAfterBase,
            ],
            profile.Reading.Automatic.Value!.Reasons.Select(reason => reason.Kind));
        Assert.Equal("root", profile.Reading.Automatic.Value.Reasons[0].RelatedSourceId);
        Assert.Equal(target.Id, profile.Reading.Automatic.Value.Reasons[^1].RelatedSourceId);
        Assert.True(profile.Reading.Later.Value!.MayBeReadAgain);
        Assert.Equal(2, profile.Measurements.OwnSource.Value!.PhysicalFileCount);
    }

    [Fact(DisplayName = "Route inspect OnDemand source with overwrite retains selection and immediate-after-base reasons")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void OnDemandOverwriteReasonsRemainCanonical()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("Loader"));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("root"),
            });
        var target = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/target.md",
                Kind = RouteSourceKind.Markdown,
                Body = "target",
                OverwritePath = ".agents/root/target.overwrite.md",
                OverwriteBody = "overwrite",
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, target],
            [root.CanonicalPath]);

        var profile = Build(graph, target.CanonicalPath);

        Assert.Equal(
            [
                RouteInspectAutomaticReadingKind.OnDemand,
                RouteInspectAutomaticReadingKind.OverwriteAfterBase,
            ],
            profile.Reading.Automatic.Value!.Reasons.Select(reason => reason.Kind));
        Assert.Equal(
            [RouteInspectAutomaticReadingEvent.RouteSelected],
            profile.Reading.Automatic.Value.Reasons[0].Events);
        Assert.Equal(
            [RouteInspectAutomaticReadingEvent.BaseRead],
            profile.Reading.Automatic.Value.Reasons[1].Events);
        Assert.False(profile.Reading.Later.Value!.MayBeReadAgain);
    }

    private static RouteInspectProfile Build(
        RouteInspectGraph graph,
        string selectedPath,
        RouteInspectSelectionMethod selectionMethod = RouteInspectSelectionMethod.AutomaticId)
    {
        var resolution = RouteInspectResolutionTestData.Resolved(graph, selectedPath, selectionMethod);
        return new RouteInspectProfileBuilder().Build(resolution, TestContext.Current.CancellationToken);
    }
}
