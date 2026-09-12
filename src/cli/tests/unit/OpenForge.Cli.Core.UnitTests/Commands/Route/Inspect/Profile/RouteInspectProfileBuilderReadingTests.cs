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
        var byIdAutomatic = Assert.IsType<RouteInspectAutomaticReadings>(byId.Reading.Automatic.Value);
        var byPathAutomatic = Assert.IsType<RouteInspectAutomaticReadings>(byPath.Reading.Automatic.Value);
        var byIdSelectedClosure = Assert.IsType<RouteInspectMeasurement>(byId.Measurements.SelectedClosure.Value);
        var byPathSelectedClosure = Assert.IsType<RouteInspectMeasurement>(byPath.Measurements.SelectedClosure.Value);
        var byIdSelectionAddition = Assert.IsType<RouteInspectMeasurement>(byId.Measurements.SelectionAddition.Value);
        var byPathSelectionAddition = Assert.IsType<RouteInspectMeasurement>(byPath.Measurements.SelectionAddition.Value);
        Assert.Equal(
            byIdAutomatic.Reasons.Select(reason => reason.Kind),
            byPathAutomatic.Reasons.Select(reason => reason.Kind));
        Assert.Equal(
            byIdSelectedClosure.UnicodeScalarCount,
            byPathSelectedClosure.UnicodeScalarCount);
        Assert.Equal(
            byIdSelectionAddition.PhysicalFileCount,
            byPathSelectionAddition.PhysicalFileCount);
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
        var automaticReadings = Assert.IsType<RouteInspectAutomaticReadings>(profile.Reading.Automatic.Value);
        var automatic = Assert.Single(automaticReadings.Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.ParentLoadNow, automatic.Kind);
        Assert.Equal("root/second", automatic.RelatedSourceId);
        var later = Assert.IsType<RouteInspectLaterReading>(profile.Reading.Later.Value);
        var taskStartOverlap = Assert.IsType<RouteInspectMeasurement>(profile.Measurements.TaskStartOverlap.Value);
        var selectedClosure = Assert.IsType<RouteInspectMeasurement>(profile.Measurements.SelectedClosure.Value);
        var selectionAddition = Assert.IsType<RouteInspectMeasurement>(profile.Measurements.SelectionAddition.Value);
        Assert.False(later.MayBeReadAgain);
        Assert.Empty(later.Occasions);
        Assert.Equal(4, taskStartOverlap.PhysicalFileCount);
        Assert.Equal(4, selectedClosure.PhysicalFileCount);
        Assert.Equal(0, selectionAddition.PhysicalFileCount);
    }

    [Fact(DisplayName = "Route inspect inactive continuity requires selection and retains scoped refresh occasions")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InactiveContinuityRequiresScopeSelection()
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

        Assert.False(profile.Reading.TaskStart.Value);
        var automaticReadings = Assert.IsType<RouteInspectAutomaticReadings>(profile.Reading.Automatic.Value);
        var automatic = Assert.Single(automaticReadings.Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.RoutedFileKeepInMind, automatic.Kind);
        Assert.Equal("root/other", automatic.RelatedSourceId);
        Assert.Equal(
            [
                RouteInspectAutomaticReadingEvent.ExposingParentRead,
                RouteInspectAutomaticReadingEvent.LaterReview,
            ],
            automatic.Events);
        var later = Assert.IsType<RouteInspectLaterReading>(profile.Reading.Later.Value);
        var selectedClosure = Assert.IsType<RouteInspectMeasurement>(profile.Measurements.SelectedClosure.Value);
        var taskStartOverlap = Assert.IsType<RouteInspectMeasurement>(profile.Measurements.TaskStartOverlap.Value);
        var selectionAddition = Assert.IsType<RouteInspectMeasurement>(profile.Measurements.SelectionAddition.Value);
        Assert.True(later.MayBeReadAgain);
        Assert.Equal(
            [
                RouteInspectLaterReadOccasion.ContextRestoration,
                RouteInspectLaterReadOccasion.Handoff,
                RouteInspectLaterReadOccasion.Closeout,
                RouteInspectLaterReadOccasion.FollowupTransition,
            ],
            later.Occasions);
        Assert.Equal(3, selectedClosure.PhysicalFileCount);
        Assert.Equal(1, taskStartOverlap.PhysicalFileCount);
        Assert.Equal(2, selectionAddition.PhysicalFileCount);
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
        var automaticReadings = Assert.IsType<RouteInspectAutomaticReadings>(profile.Reading.Automatic.Value);
        var automatic = Assert.Single(automaticReadings.Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.EntrypointKeepInMind, automatic.Kind);
        Assert.Equal("root", automatic.RelatedSourceId);
        Assert.Equal(
            [
                RouteInspectAutomaticReadingEvent.ExposingParentRead,
                RouteInspectAutomaticReadingEvent.LaterReview,
            ],
            automatic.Events);
        var later = Assert.IsType<RouteInspectLaterReading>(profile.Reading.Later.Value);
        var selectedClosure = Assert.IsType<RouteInspectMeasurement>(profile.Measurements.SelectedClosure.Value);
        Assert.True(later.MayBeReadAgain);
        Assert.Equal(2, selectedClosure.PhysicalFileCount);
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

        var selectedAutomatic = Assert.IsType<RouteInspectAutomaticReadings>(selectedProfile.Reading.Automatic.Value);
        var selectedReason = Assert.Single(selectedAutomatic.Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.EntrypointKeepInMind, selectedReason.Kind);
        Assert.Equal([RouteInspectAutomaticReadingEvent.ExposingParentRead, RouteInspectAutomaticReadingEvent.LaterReview], selectedReason.Events);
        Assert.False(selectedProfile.Reading.TaskStart.Value);
        var leafSelectedClosure = Assert.IsType<RouteInspectMeasurement>(leafProfile.Measurements.SelectedClosure.Value);
        var leafSelectionAddition = Assert.IsType<RouteInspectMeasurement>(leafProfile.Measurements.SelectionAddition.Value);
        var leafTaskStartOverlap = Assert.IsType<RouteInspectMeasurement>(leafProfile.Measurements.TaskStartOverlap.Value);
        var leafAutomatic = Assert.IsType<RouteInspectAutomaticReadings>(leafProfile.Reading.Automatic.Value);
        Assert.Equal(4, leafSelectedClosure.PhysicalFileCount);
        Assert.Equal(3, leafSelectionAddition.PhysicalFileCount);
        Assert.Equal(1, leafTaskStartOverlap.PhysicalFileCount);
        Assert.Equal(RouteInspectAutomaticReadingKind.OnDemand, Assert.Single(leafAutomatic.Reasons).Kind);
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

        var automaticReadings = Assert.IsType<RouteInspectAutomaticReadings>(profile.Reading.Automatic.Value);
        var automaticReasons = automaticReadings.Reasons;
        Assert.Equal(
            [
                RouteInspectAutomaticReadingKind.ParentLoadNow,
                RouteInspectAutomaticReadingKind.RoutedFileKeepInMind,
                RouteInspectAutomaticReadingKind.OverwriteAfterBase,
            ],
            automaticReasons.Select(reason => reason.Kind));
        Assert.Equal("root", automaticReasons[0].RelatedSourceId);
        Assert.Equal(target.Id, automaticReasons[^1].RelatedSourceId);
        Assert.True(Assert.IsType<RouteInspectLaterReading>(profile.Reading.Later.Value).MayBeReadAgain);
        Assert.Equal(
            2,
            Assert.IsType<RouteInspectMeasurement>(profile.Measurements.OwnSource.Value).PhysicalFileCount);
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

        var automaticReadings = Assert.IsType<RouteInspectAutomaticReadings>(profile.Reading.Automatic.Value);
        Assert.Equal(
            [
                RouteInspectAutomaticReadingKind.OnDemand,
                RouteInspectAutomaticReadingKind.OverwriteAfterBase,
            ],
            automaticReadings.Reasons.Select(reason => reason.Kind));
        Assert.Equal(
            [RouteInspectAutomaticReadingEvent.RouteSelected],
            automaticReadings.Reasons[0].Events);
        Assert.Equal(
            [RouteInspectAutomaticReadingEvent.BaseRead],
            automaticReadings.Reasons[1].Events);
        Assert.False(Assert.IsType<RouteInspectLaterReading>(profile.Reading.Later.Value).MayBeReadAgain);
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
