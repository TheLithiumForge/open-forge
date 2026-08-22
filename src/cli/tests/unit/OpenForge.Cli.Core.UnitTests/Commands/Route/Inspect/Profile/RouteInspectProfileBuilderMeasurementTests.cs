using System.Text;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectProfileBuilderMeasurementTests
{
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    [Fact(DisplayName = "Route inspect measures all five sets with Unicode scalars strict UTF-8 bytes and aggregate token ceiling")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void MeasurementsUseExactScalarsBytesAndAggregateTokens()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries(
                declarations: ["- [Root](root/_root.md) - #LoadNow"]));
        var rootBody = RouteInspectSourceTestData.BodyWithEntries("B");
        var targetBody = RouteInspectSourceTestData.BodyWithEntries("A😀é");
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = rootBody,
            });
        var target = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/target/_target.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = targetBody,
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, target],
            [root.CanonicalPath]);

        var profile = new RouteInspectProfileBuilder().Build(
            RouteInspectResolutionTestData.Resolved(graph, target.CanonicalPath),
            TestContext.Current.CancellationToken);

        AssertMeasurement(profile.Measurements.OwnSource, MeasurementForBodies([targetBody]));
        var selectedExpected = MeasurementForBodies([rootBody, targetBody]);
        AssertMeasurement(profile.Measurements.SelectedClosure, selectedExpected);
        AssertMeasurement(profile.Measurements.TaskStartOverlap, MeasurementForBodies([rootBody]));
        AssertMeasurement(profile.Measurements.SelectionAddition, MeasurementForBodies([targetBody]));
        AssertMeasurement(profile.Measurements.LoadNowDescendants, MeasurementForBodies([]));
        var selectedBodies = new[] { rootBody, targetBody };
        Assert.NotEqual(
            PerFileRoundedTokenSum(selectedBodies
                .Select(body => (long)body.EnumerateRunes().Count())
                .ToArray()),
            selectedExpected.EstimatedTokens);
    }

    [Fact(DisplayName = "Route inspect deduplicates repeated logical entries and repeated physical identities")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void MeasurementSetsDeduplicateLogicalAndPhysicalLayers()
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
                        "- [Target](target/_target.md) - #LoadNow",
                        "- [Target again](target/_target.md) - #LoadNow",
                    ]),
            });
        var target = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/target/_target.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("target"),
                Tags = ["Route", "LoadNow"],
            });
        var logicalGraph = RouteInspectResolutionTestData.Graph(
            [loader, root, target],
            [root.CanonicalPath]);
        var logicalProfile = Build(logicalGraph, target.CanonicalPath);

        Assert.Equal(2, logicalProfile.Measurements.SelectedClosure.Value!.PhysicalFileCount);

        var aliasedRoot = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/alias/_alias.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("X"),
                PhysicalPath = RouteInspectSourceTestData.PhysicalPath("shared/physical.md"),
            });
        var aliasedTarget = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/alias/target/_target.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("X"),
                PhysicalPath = RouteInspectSourceTestData.PhysicalPath("shared/physical.md"),
            });
        var aliasLoader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries(
                declarations: ["- [Alias](alias/_alias.md) - #LoadNow"]));
        var physicalGraph = RouteInspectResolutionTestData.Graph(
            [aliasLoader, aliasedRoot, aliasedTarget],
            [aliasedRoot.CanonicalPath]);
        var physicalProfile = Build(physicalGraph, aliasedTarget.CanonicalPath);

        Assert.Equal(1, physicalProfile.Measurements.SelectedClosure.Value!.PhysicalFileCount);
        Assert.Equal(0, physicalProfile.Measurements.SelectionAddition.Value!.PhysicalFileCount);
    }

    [Fact(DisplayName = "Route inspect selected closure retains base-first overwrites on ancestors and target")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void SelectedClosureIncludesAncestorOverwrites()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries(
                declarations: ["- [Root](root/_root.md) - #LoadNow"]));
        var rootBody = RouteInspectSourceTestData.BodyWithEntries("R");
        var targetBody = RouteInspectSourceTestData.BodyWithEntries("T");
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = rootBody,
                OverwritePath = ".agents/root/_root.overwrite.md",
                OverwriteBody = "r",
            });
        var target = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/target/_target.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = targetBody,
                OverwritePath = ".agents/root/target/_target.overwrite.md",
                OverwriteBody = "t",
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, target],
            [root.CanonicalPath]);

        var profile = Build(graph, target.CanonicalPath);

        AssertMeasurement(profile.Measurements.OwnSource, MeasurementForBodies([targetBody, "t"]));
        AssertMeasurement(
            profile.Measurements.SelectedClosure,
            MeasurementForBodies([rootBody, "r", targetBody, "t"]));
        AssertMeasurement(profile.Measurements.TaskStartOverlap, MeasurementForBodies([rootBody, "r"]));
        AssertMeasurement(profile.Measurements.SelectionAddition, MeasurementForBodies([targetBody, "t"]));
        AssertMeasurement(profile.Measurements.LoadNowDescendants, MeasurementForBodies([]));
        Assert.Equal(
            [RouteInspectLayerRole.Base, RouteInspectLayerRole.Overwrite],
            RouteInspectResolutionTestData.Identity(target, RouteInspectRouteState.Routed).PhysicalLayers.Select(layer => layer.Role));
    }

    [Fact(DisplayName = "Route inspect narrow LoadNow descendants exclude ordinary links on-demand descendants and unrelated global KeepInMind")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void NarrowLoadNowClosureExcludesNonLoadNowSources()
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
        var target = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/target/_target.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries(
                    declarations:
                    [
                        "- [Automatic](automatic.md) - #LoadNow",
                        "- [Ordinary](ordinary.md)",
                        "- [On demand](on-demand.md)",
                    ]),
            });
        var automatic = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/target/automatic.md",
                Kind = RouteSourceKind.Markdown,
                Body = "automatic",
                Tags = ["Route", "LoadNow"],
            });
        var ordinary = RouteInspectSourceTestData.Source(
            ".agents/root/target/ordinary.md",
            RouteSourceKind.Markdown,
            "ordinary");
        var onDemand = RouteInspectSourceTestData.Source(
            ".agents/root/target/on-demand.md",
            RouteSourceKind.Markdown,
            "on-demand");
        var unrelated = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/unrelated/_unrelated.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries(
                    declarations: ["- [Global continuity](global.md) - #KeepInMind"]),
            });
        var global = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/unrelated/global.md",
                Kind = RouteSourceKind.Markdown,
                Body = "global",
                Tags = ["Route", "KeepInMind"],
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, target, automatic, ordinary, onDemand, unrelated, global],
            [root.CanonicalPath]);

        var profile = Build(graph, target.CanonicalPath);

        Assert.Equal(3, profile.Measurements.SelectedClosure.Value!.PhysicalFileCount);
        Assert.Equal(1, profile.Measurements.LoadNowDescendants.Value!.PhysicalFileCount);
        Assert.Equal(1, profile.Measurements.TaskStartOverlap.Value!.PhysicalFileCount);
        Assert.Equal(2, profile.Measurements.SelectionAddition.Value!.PhysicalFileCount);
    }

    [Fact(DisplayName = "Route inspect preserves measured zero unavailable and not-applicable measurement states")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void MeasurementAvailabilityPreservesZeroUnavailableAndNotApplicable()
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
        var leaf = RouteInspectSourceTestData.Source(
            ".agents/root/leaf.md",
            RouteSourceKind.Markdown,
            "leaf");
        var completeGraph = RouteInspectResolutionTestData.Graph(
            [loader, root, leaf],
            [root.CanonicalPath]);

        var rootProfile = Build(completeGraph, root.CanonicalPath);
        Assert.Equal(RouteInspectFactState.Value, rootProfile.Measurements.LoadNowDescendants.State);
        AssertMeasurement(rootProfile.Measurements.LoadNowDescendants, new RouteInspectMeasurementExpectation
        {
            PhysicalFileCount = 0,
            UnicodeScalarCount = 0,
            Utf8ByteCount = 0,
            EstimatedTokens = 0,
        });

        var leafProfile = Build(completeGraph, leaf.CanonicalPath);
        Assert.Equal(RouteInspectFactState.NotApplicable, leafProfile.Measurements.LoadNowDescendants.State);
        Assert.Null(leafProfile.Measurements.LoadNowDescendants.Value);

        var unreadable = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/unreadable.md",
                Kind = RouteSourceKind.Markdown,
                MetadataState = RouteSourceMetadataState.ReadUnavailable,
                DocumentReadState = FileReadState.AccessDenied,
            });
        var incompleteGraph = RouteInspectResolutionTestData.Graph(
            [loader, root, unreadable],
            [root.CanonicalPath]);
        var selection = RouteInspectResolutionTestData.Selection(
            unreadable,
            RouteInspectSelectionMethod.AutomaticId);
        var identity = RouteInspectResolutionTestData.Identity(
            unreadable,
            RouteInspectRouteState.Routed);
        var incomplete = RouteInspectResolutionTestData.Create(new RouteInspectResolutionSpec
        {
            State = RouteInspectResolutionState.Incomplete,
            Selection = selection,
            Identity = identity,
            Graph = incompleteGraph,
            Issues = [RouteInspectResolutionTestData.Issue(
                RouteInspectResolutionIssueCode.ReadUnavailable,
                unreadable.CanonicalPath)],
        });

        var unavailableProfile = new RouteInspectProfileBuilder().Build(
            incomplete,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectFactState.Unavailable, unavailableProfile.Measurements.OwnSource.State);
        Assert.Null(unavailableProfile.Measurements.OwnSource.Value);
        Assert.Equal(RouteInspectCompleteness.Incomplete, unavailableProfile.Completeness);
    }

    private static RouteInspectProfile Build(RouteInspectGraph graph, string selectedPath)
    {
        return new RouteInspectProfileBuilder().Build(
            RouteInspectResolutionTestData.Resolved(graph, selectedPath),
            TestContext.Current.CancellationToken);
    }

    private static void AssertMeasurement(
        RouteInspectFact<RouteInspectMeasurement> fact,
        RouteInspectMeasurementExpectation expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        Assert.Equal(RouteInspectFactState.Value, fact.State);
        var measurement = Assert.IsType<RouteInspectMeasurement>(fact.Value);
        Assert.Equal(expected.PhysicalFileCount, measurement.PhysicalFileCount);
        Assert.Equal(expected.UnicodeScalarCount, measurement.UnicodeScalarCount);
        Assert.Equal(expected.Utf8ByteCount, measurement.Utf8ByteCount);
        Assert.Equal(expected.EstimatedTokens, measurement.EstimatedTokens);
    }

    private static long PerFileRoundedTokenSum(params long[] scalarCounts)
    {
        return scalarCounts.Sum(count => count / 4 + (count % 4 == 0 ? 0 : 1));
    }

    private static RouteInspectMeasurementExpectation MeasurementForBodies(IEnumerable<string> bodies)
    {
        var materializedBodies = bodies.ToArray();
        var unicodeScalarCount = materializedBodies
            .Sum(body => (long)body.EnumerateRunes().Count());
        var utf8ByteCount = materializedBodies
            .Sum(body => (long)StrictUtf8.GetByteCount(body));
        return new RouteInspectMeasurementExpectation
        {
            PhysicalFileCount = materializedBodies.LongLength,
            UnicodeScalarCount = unicodeScalarCount,
            Utf8ByteCount = utf8ByteCount,
            EstimatedTokens = unicodeScalarCount / 4 + (unicodeScalarCount % 4 == 0 ? 0 : 1),
        };
    }

    private sealed record RouteInspectMeasurementExpectation
    {
        internal required long PhysicalFileCount { get; init; }

        internal required long UnicodeScalarCount { get; init; }

        internal required long Utf8ByteCount { get; init; }

        internal required long EstimatedTokens { get; init; }
    }
}
