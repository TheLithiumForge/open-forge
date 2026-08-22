using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectProfileBuilderDeterminismTests
{
    [Fact(DisplayName = "Route inspect profile formation is deterministic for unchanged graph facts")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ProfileFormationIsDeterministic()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("loader"));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries(
                    "## Axioms\n\n- Root rule.",
                    ["- [Target](target.md) - #LoadNow"]),
            });
        var target = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/target.md",
                Kind = RouteSourceKind.Markdown,
                Body = "target",
                Tags = ["Route", "LoadNow"],
                OverwritePath = ".agents/root/target.overwrite.md",
                OverwriteBody = "custom",
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, target],
            [root.CanonicalPath]);
        var resolution = RouteInspectResolutionTestData.Resolved(graph, target.CanonicalPath);
        var builder = new RouteInspectProfileBuilder();

        var first = builder.Build(resolution, CancellationToken.None);
        var second = builder.Build(resolution, CancellationToken.None);

        Assert.Equal(first.Completeness, second.Completeness);
        Assert.Equal(first.Safety, second.Safety);
        AssertFactStateEqual(first.Reading.TaskStart, second.Reading.TaskStart);
        Assert.Equal(first.Reading.TaskStart.Value, second.Reading.TaskStart.Value);
        AssertFactStateEqual(first.Reading.Automatic, second.Reading.Automatic);
        AssertFactStateEqual(first.Reading.Later, second.Reading.Later);

        var firstReasons = first.Reading.Automatic.Value!.Reasons;
        var secondReasons = second.Reading.Automatic.Value!.Reasons;
        Assert.Equal(firstReasons.Count, secondReasons.Count);
        for (var index = 0; index < firstReasons.Count; index++)
        {
            var firstReason = firstReasons[index];
            var secondReason = secondReasons[index];
            Assert.Equal(firstReason.Kind, secondReason.Kind);
            Assert.Equal(firstReason.RelatedSourceId, secondReason.RelatedSourceId);
            Assert.Equal(firstReason.Events.Count, secondReason.Events.Count);
            for (var eventIndex = 0; eventIndex < firstReason.Events.Count; eventIndex++)
            {
                Assert.Equal(firstReason.Events[eventIndex], secondReason.Events[eventIndex]);
            }
        }

        var firstLater = first.Reading.Later.Value!;
        var secondLater = second.Reading.Later.Value!;
        Assert.Equal(firstLater.MayBeReadAgain, secondLater.MayBeReadAgain);
        Assert.Equal(firstLater.Occasions.Count, secondLater.Occasions.Count);
        for (var index = 0; index < firstLater.Occasions.Count; index++)
        {
            Assert.Equal(firstLater.Occasions[index], secondLater.Occasions[index]);
        }

        AssertMeasurementEqual(first.Measurements.OwnSource, second.Measurements.OwnSource);
        AssertMeasurementEqual(first.Measurements.SelectedClosure, second.Measurements.SelectedClosure);
        AssertMeasurementEqual(first.Measurements.TaskStartOverlap, second.Measurements.TaskStartOverlap);
        AssertMeasurementEqual(first.Measurements.SelectionAddition, second.Measurements.SelectionAddition);
        AssertMeasurementEqual(first.Measurements.LoadNowDescendants, second.Measurements.LoadNowDescendants);

        Assert.Equal(first.Topology.State, second.Topology.State);
        Assert.Equal(first.Topology.Reason, second.Topology.Reason);
        var firstTopology = first.Topology.Value!;
        var secondTopology = second.Topology.Value!;
        Assert.Equal(firstTopology.RootRoute, secondTopology.RootRoute);
        Assert.Equal(firstTopology.RouteChain.Count, secondTopology.RouteChain.Count);
        for (var index = 0; index < firstTopology.RouteChain.Count; index++)
        {
            Assert.Equal(firstTopology.RouteChain[index], secondTopology.RouteChain[index]);
        }

        Assert.Equal(firstTopology.ParentId, secondTopology.ParentId);
        Assert.Equal(firstTopology.Depth, secondTopology.Depth);
        Assert.Equal(firstTopology.Counts.State, secondTopology.Counts.State);
        Assert.Equal(firstTopology.Counts.Reason, secondTopology.Counts.Reason);
        if (firstTopology.Counts.Value is { } firstCounts)
        {
            var secondCounts = Assert.IsType<RouteInspectTopologyCounts>(secondTopology.Counts.Value);
            Assert.Equal(firstCounts.DirectRoutedFileCount, secondCounts.DirectRoutedFileCount);
            Assert.Equal(firstCounts.DirectEntrypointCount, secondCounts.DirectEntrypointCount);
            Assert.Equal(firstCounts.DescendantRoutedFileCount, secondCounts.DescendantRoutedFileCount);
            Assert.Equal(firstCounts.DescendantEntrypointCount, secondCounts.DescendantEntrypointCount);
        }
        else
        {
            Assert.Null(secondTopology.Counts.Value);
        }

        Assert.Equal(first.Axioms.State, second.Axioms.State);
        Assert.Equal(first.Axioms.Reason, second.Axioms.Reason);
        var firstAxioms = first.Axioms.Value!;
        var secondAxioms = second.Axioms.Value!;
        Assert.Equal(firstAxioms.Inherited.State, secondAxioms.Inherited.State);
        Assert.Equal(firstAxioms.Inherited.Reason, secondAxioms.Inherited.Reason);
        var firstInherited = firstAxioms.Inherited.Value!;
        var secondInherited = secondAxioms.Inherited.Value!;
        Assert.Equal(firstInherited.SourceIds.Count, secondInherited.SourceIds.Count);
        for (var index = 0; index < firstInherited.SourceIds.Count; index++)
        {
            Assert.Equal(firstInherited.SourceIds[index], secondInherited.SourceIds[index]);
        }

        Assert.Equal(firstAxioms.Local.State, secondAxioms.Local.State);
        Assert.Equal(firstAxioms.Local.Reason, secondAxioms.Local.Reason);
        Assert.Equal(firstAxioms.Local.Value, secondAxioms.Local.Value);
    }

    [Fact(DisplayName = "Route inspect profile formation observes cancellation before producing profile facts")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ProfileFormationHonorsCancellation()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("loader"));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("root"),
            });
        var graph = RouteInspectResolutionTestData.Graph([loader, root], [root.CanonicalPath]);
        var resolution = RouteInspectResolutionTestData.Resolved(graph, root.CanonicalPath);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            new RouteInspectProfileBuilder().Build(resolution, cancellation.Token));
    }

    [Fact(DisplayName = "Route inspect profile output exposes immutable reason provenance and topology collections")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ProfileOutputCollectionsAreImmutable()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("loader"));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("root"),
            });
        var graph = RouteInspectResolutionTestData.Graph([loader, root], [root.CanonicalPath]);
        var resolution = RouteInspectResolutionTestData.Resolved(graph, root.CanonicalPath);

        var profile = new RouteInspectProfileBuilder().Build(resolution, CancellationToken.None);

        Assert.True(((IList<RouteInspectAutomaticReading>)profile.Reading.Automatic.Value!.Reasons).IsReadOnly);
        Assert.True(((IList<RouteInspectAutomaticReadingEvent>)profile.Reading.Automatic.Value.Reasons[0].Events).IsReadOnly);
        Assert.True(((IList<string>)profile.Axioms.Value!.Inherited.Value!.SourceIds).IsReadOnly);
        Assert.True(((IList<string>)profile.Topology.Value!.RouteChain).IsReadOnly);
    }

    private static void AssertFactStateEqual<T>(RouteInspectFact<T> expected, RouteInspectFact<T> actual)
    {
        Assert.Equal(expected.State, actual.State);
        Assert.Equal(expected.Reason, actual.Reason);
    }

    private static void AssertMeasurementEqual(
        RouteInspectFact<RouteInspectMeasurement> expected,
        RouteInspectFact<RouteInspectMeasurement> actual)
    {
        Assert.Equal(expected.State, actual.State);
        Assert.Equal(expected.Reason, actual.Reason);
        if (expected.Value is null)
        {
            Assert.Null(actual.Value);
            return;
        }

        var actualMeasurement = Assert.IsType<RouteInspectMeasurement>(actual.Value);
        Assert.Equal(expected.Value.PhysicalFileCount, actualMeasurement.PhysicalFileCount);
        Assert.Equal(expected.Value.UnicodeScalarCount, actualMeasurement.UnicodeScalarCount);
        Assert.Equal(expected.Value.Utf8ByteCount, actualMeasurement.Utf8ByteCount);
        Assert.Equal(expected.Value.EstimatedTokens, actualMeasurement.EstimatedTokens);
    }
}
