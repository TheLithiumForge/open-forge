using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectResultBuilderTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect result builder forms a complete result with no conditions or next action")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void CompleteResultRetainsResolvedFacts()
    {
        var (graph, source, resolution) = ResolvedSource();
        var request = RouteInspectResolutionTestData.Request(source.Id);
        var profile = RouteInspectProfileTestData.Profile();

        var result = new RouteInspectResultBuilder().Build(request, resolution, profile);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Same(request.Workspace, result.Workspace);
        Assert.Same(resolution.Selection, result.Selection);
        Assert.Same(resolution.Identity, result.Identity);
        Assert.Same(profile, result.Profile);
        Assert.Empty(result.Observations);
        Assert.Empty(result.Conditions);
        Assert.Null(result.Next);
        Assert.Equal("route inspect", result.Command);
        Assert.Equal(1, result.SchemaVersion);
        Assert.Same(graph, resolution.Graph);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect exact-path selection makes a safe non-unique automatic ID attention without a next action")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ExactPathCollisionProducesAttentionWithoutAction()
    {
        var (graph, selected, resolution) = CollisionResolution(RouteInspectSelectionMethod.ExactPath);
        var result = BuildResult(selected, resolution, RouteInspectProfileTestData.Profile());

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        var observation = Assert.Single(result.Observations);
        Assert.Equal(RouteInspectObservationCode.AutomaticIdNotUnique, observation.Code);
        Assert.Equal("root/collision", observation.Subject);
        Assert.Equal(
            [
                ".agents/root/collision.md",
                ".agents/root/collision/_collision.md",
            ],
            observation.Paths);
        Assert.Empty(result.Conditions);
        Assert.Null(result.Next);
        Assert.Same(graph, resolution.Graph);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect interactive non-unique automatic ID attention has one exact-path next action")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InteractiveCollisionProducesOneNextAction()
    {
        (_, var selected, var resolution) = CollisionResolution(RouteInspectSelectionMethod.Interactive);

        var result = BuildResult(selected, resolution, RouteInspectProfileTestData.Profile());

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Contains(
            result.Observations,
            observation => observation.Code == RouteInspectObservationCode.AutomaticIdNotUnique);
        var next = Assert.IsType<CliNextAction>(result.Next);
        Assert.Contains("exact path", next.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.Single(new[] { next });
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect neutral compatibility detached and overwrite observations do not change complete status")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void NeutralObservationsRemainComplete()
    {
        var source = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/detached/index.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("detached"),
                OverwritePath = ".agents/detached/index.overwrite.md",
                OverwriteBody = "custom",
            });
        var graph = RouteInspectResolutionTestData.Graph([source], []);
        var resolution = RouteInspectResolutionTestData.Resolved(
            graph,
            source.CanonicalPath,
            RouteInspectSelectionMethod.ExactPath,
            RouteInspectRouteState.Detached);
        var result = BuildResult(source, resolution, RouteInspectProfileTestData.Profile());

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(
            [
                RouteInspectObservationCode.CompatibilityEntrypoint,
                RouteInspectObservationCode.DetachedSource,
                RouteInspectObservationCode.ValidOverwrite,
            ],
            result.Observations.Select(observation => observation.Code));
        Assert.Null(result.Next);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect known unrouted observation remains complete without route recommendation")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void NotRoutedObservationRemainsComplete()
    {
        var source = RouteInspectSourceTestData.Source(
            ".agents/unrouted.md",
            RouteSourceKind.Markdown,
            "unrouted");
        var graph = RouteInspectResolutionTestData.Graph([source], []);
        var resolution = RouteInspectResolutionTestData.Resolved(
            graph,
            source.CanonicalPath,
            routeState: RouteInspectRouteState.NotRouted);
        var result = BuildResult(source, resolution, RouteInspectProfileTestData.Profile());

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var observation = Assert.Single(result.Observations);
        Assert.Equal(RouteInspectObservationCode.NotRouted, observation.Code);
        Assert.Null(result.Next);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect unavailable fact forms an incomplete result with one typed condition")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void UnavailableProfileFactProducesIncomplete()
    {
        var (graph, source, resolution) = ResolvedSource();
        var request = RouteInspectResolutionTestData.Request(source.Id);
        var profile = RouteInspectProfileTestData.Profile(new RouteInspectProfileSpec
        {
            Completeness = RouteInspectCompleteness.Incomplete,
            Measurements = RouteInspectProfileTestData.Measurements(new RouteInspectMeasurementsSpec
            {
                OwnSource = RouteInspectProfileTestData.UnavailableMeasurement(),
                SelectedClosure = RouteInspectProfileTestData.Measurement(1, 1, 1),
                TaskStartOverlap = RouteInspectProfileTestData.Measurement(1, 1, 1),
                SelectionAddition = RouteInspectProfileTestData.Measurement(0, 0, 0),
                LoadNowDescendants = RouteInspectProfileTestData.NotApplicableMeasurement(),
            }),
        });

        var result = new RouteInspectResultBuilder().Build(request, resolution, profile);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        var condition = Assert.Single(result.Conditions);
        Assert.Equal(RouteInspectConditionCode.UnavailableFact, condition.Code);
        Assert.Equal(CliSemanticStatus.Incomplete, condition.Status);
        AssertDiagnostic(condition);
        Assert.Empty(result.Observations);
        AssertSafeRecoveryAction(result);
        Assert.Same(graph, resolution.Graph);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect incomplete resolution retains safe identity and unreadable-source condition")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void IncompleteResolutionProducesIncomplete()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("loader"));
        var source = RouteInspectSourceTestData.Source(new RouteInspectSourceSpec
        {
            Path = ".agents/root/_root.md",
            Kind = RouteSourceKind.Entrypoint,
            MetadataState = RouteSourceMetadataState.ReadUnavailable,
            DocumentReadState = FileReadState.AccessDenied,
        });
        var graph = RouteInspectResolutionTestData.Graph([loader, source], [source.CanonicalPath]);
        var selection = RouteInspectResolutionTestData.Selection(
            source,
            RouteInspectSelectionMethod.AutomaticId);
        var identity = RouteInspectResolutionTestData.Identity(
            source,
            RouteInspectRouteState.Routed);
        var resolution = RouteInspectResolutionTestData.Create(new RouteInspectResolutionSpec
        {
            State = RouteInspectResolutionState.Incomplete,
            Selection = selection,
            Identity = identity,
            Graph = graph,
            Issues = [RouteInspectResolutionTestData.Issue(
                RouteInspectResolutionIssueCode.ReadUnavailable,
                source.CanonicalPath)],
        });
        var profile = RouteInspectProfileTestData.Profile(new RouteInspectProfileSpec
        {
            Completeness = RouteInspectCompleteness.Incomplete,
        });

        var result = BuildResult(source, resolution, profile);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        var condition = Assert.Single(result.Conditions);
        Assert.Equal(RouteInspectConditionCode.UnreadableSource, condition.Code);
        Assert.Equal(source.CanonicalPath, condition.Subject);
        Assert.Equal(CliSemanticStatus.Incomplete, condition.Status);
        AssertDiagnostic(condition, source.CanonicalPath);
        Assert.NotNull(result.Profile);
        AssertSafeRecoveryAction(result);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect incomplete precedence wins over a safe non-unique-ID attention observation")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void IncompletePrecedenceWinsOverAttention()
    {
        var (graph, source, resolved) = CollisionResolution(
            RouteInspectSelectionMethod.ExactPath,
            selectedUnavailable: true);
        var resolution = RouteInspectResolutionTestData.Create(new RouteInspectResolutionSpec
        {
            State = RouteInspectResolutionState.Incomplete,
            Selection = resolved.Selection,
            Identity = resolved.Identity,
            Graph = graph,
            Issues = [RouteInspectResolutionTestData.Issue(
                RouteInspectResolutionIssueCode.ReadUnavailable,
                source.CanonicalPath)],
        });
        var profile = RouteInspectProfileTestData.Profile(new RouteInspectProfileSpec
        {
            Completeness = RouteInspectCompleteness.Incomplete,
        });

        var result = BuildResult(source, resolution, profile);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Contains(
            result.Observations,
            observation => observation.Code == RouteInspectObservationCode.AutomaticIdNotUnique);
        Assert.Contains(
            result.Conditions,
            condition => condition.Code == RouteInspectConditionCode.UnreadableSource);
        var unreadable = Assert.Single(
            result.Conditions,
            condition => condition.Code == RouteInspectConditionCode.UnreadableSource);
        AssertDiagnostic(unreadable, source.CanonicalPath);
        AssertSafeRecoveryAction(result);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect blocked precedence wins over incomplete conditions and retains one next action")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void BlockedPrecedenceWinsOverIncomplete()
    {
        var (_, source, resolution) = BlockedRouteSource();

        var result = BuildResult(source, resolution, null);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Null(result.Profile);
        var ambiguous = Assert.Single(
            result.Conditions,
            condition => condition.Code == RouteInspectConditionCode.AmbiguousRoute);
        var unreadable = Assert.Single(
            result.Conditions,
            condition => condition.Code == RouteInspectConditionCode.UnreadableSource);
        AssertDiagnostic(ambiguous, source.CanonicalPath);
        AssertDiagnostic(unreadable, source.CanonicalPath);
        Assert.All(
            result.Conditions,
            condition => Assert.True(
                condition.Status is CliSemanticStatus.Incomplete or CliSemanticStatus.Blocked));
        AssertAction(result, "route inspect", "exact");
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect unresolved automatic-ID collision is blocked with every candidate path")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void UnresolvedCollisionRetainsCandidatesAndNextAction()
    {
        var candidates = new[]
        {
            ".agents/root/collision.md",
            ".agents/root/collision/_collision.md",
        };
        var selection = RouteInspectResolutionTestData.UnresolvedId("root/collision", candidates);
        var resolution = RouteInspectResolutionTestData.Create(new RouteInspectResolutionSpec
        {
            State = RouteInspectResolutionState.Blocked,
            Selection = selection,
            Issues = [RouteInspectResolutionTestData.Issue(
                RouteInspectResolutionIssueCode.AmbiguousSource,
                "root/collision",
                candidates)],
        });
        var request = RouteInspectResolutionTestData.Request("root/collision");

        var result = new RouteInspectResultBuilder().Build(request, resolution, null);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Null(result.Profile);
        var condition = Assert.Single(result.Conditions);
        Assert.Equal(RouteInspectConditionCode.AmbiguousSource, condition.Code);
        Assert.Equal(candidates, condition.Paths);
        Assert.Equal(CliSemanticStatus.Blocked, condition.Status);
        AssertDiagnostic(condition, "root/collision");
        AssertAction(result, "route inspect", "exact");
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect invalid reference forms invalid result before profile formation")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InvalidReferenceProducesInvalid()
    {
        var selection = RouteInspectResolutionTestData.InvalidReference("bad reference");
        var resolution = RouteInspectResolutionTestData.Create(new RouteInspectResolutionSpec
        {
            State = RouteInspectResolutionState.Invalid,
            Selection = selection,
            Issues = [RouteInspectResolutionTestData.Issue(
                RouteInspectResolutionIssueCode.InvalidReference,
                "bad reference")],
        });

        var result = new RouteInspectResultBuilder().Build(
            RouteInspectResolutionTestData.Request("bad reference"),
            resolution,
            null);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Null(result.Identity);
        Assert.Null(result.Profile);
        var condition = Assert.Single(result.Conditions);
        Assert.Equal(RouteInspectConditionCode.InvalidSourceReference, condition.Code);
        Assert.Equal(CliSemanticStatus.Invalid, condition.Status);
        AssertDiagnostic(condition, "bad reference");
        AssertAction(result, "--help", "input");
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "CLI-EDGE-003 route inspect bounded operation failure forms failed without invoking a production trigger")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void BoundedFailureFormsFailedWithoutOperationTrigger()
    {
        var resolution = RouteInspectResolutionTestData.Create(new RouteInspectResolutionSpec
        {
            State = RouteInspectResolutionState.Failed,
            Selection = RouteInspectResolutionTestData.UnresolvedId("root/failure"),
            Issues = [RouteInspectResolutionTestData.Issue(
                RouteInspectResolutionIssueCode.OperationFailure,
                "root/failure")],
        });

        var result = new RouteInspectResultBuilder().Build(
            RouteInspectResolutionTestData.Request("root/failure"),
            resolution,
            null);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Null(result.Profile);
        var condition = Assert.Single(result.Conditions);
        Assert.Equal(RouteInspectConditionCode.OperationFailed, condition.Code);
        Assert.Equal(CliSemanticStatus.Failed, condition.Status);
        AssertDiagnostic(condition, "root/failure");
        AssertAction(result, "route inspect", "retry");
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect interruption forms interrupted result with one rerun action")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InterruptionFormsInterrupted()
    {
        var resolution = RouteInspectResolutionTestData.Create(new RouteInspectResolutionSpec
        {
            State = RouteInspectResolutionState.Interrupted,
            Selection = RouteInspectResolutionTestData.UnresolvedId("root/interrupted"),
            Issues = [RouteInspectResolutionTestData.Issue(
                RouteInspectResolutionIssueCode.Interrupted,
                "root/interrupted")],
        });

        var result = new RouteInspectResultBuilder().Build(
            RouteInspectResolutionTestData.Request("root/interrupted"),
            resolution,
            null);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        var condition = Assert.Single(result.Conditions);
        Assert.Equal(RouteInspectConditionCode.Interrupted, condition.Code);
        Assert.Equal(CliSemanticStatus.Interrupted, condition.Status);
        AssertDiagnostic(condition, "root/interrupted");
        AssertAction(result, "route inspect", "rerun");
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect result builder is deterministic and returns immutable observation and condition collections")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ResultBuilderIsDeterministicAndImmutable()
    {
        var (_, selected, resolution) = CollisionResolution(RouteInspectSelectionMethod.ExactPath);
        var request = RouteInspectResolutionTestData.Request(selected.CanonicalPath);
        var profile = RouteInspectProfileTestData.Profile();
        var builder = new RouteInspectResultBuilder();

        var first = builder.Build(request, resolution, profile);
        var second = builder.Build(request, resolution, profile);

        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.Observations.Count, second.Observations.Count);
        for (var index = 0; index < first.Observations.Count; index++)
        {
            var firstObservation = first.Observations[index];
            var secondObservation = second.Observations[index];
            Assert.Equal(firstObservation.Code, secondObservation.Code);
            Assert.Equal(firstObservation.Subject, secondObservation.Subject);
            Assert.Equal(firstObservation.Paths.Count, secondObservation.Paths.Count);
            for (var pathIndex = 0; pathIndex < firstObservation.Paths.Count; pathIndex++)
            {
                Assert.Equal(firstObservation.Paths[pathIndex], secondObservation.Paths[pathIndex]);
            }
        }

        Assert.Equal(first.Conditions.Count, second.Conditions.Count);
        for (var index = 0; index < first.Conditions.Count; index++)
        {
            var firstCondition = first.Conditions[index];
            var secondCondition = second.Conditions[index];
            Assert.Equal(firstCondition.Code, secondCondition.Code);
            Assert.Equal(firstCondition.Status, secondCondition.Status);
            Assert.Equal(firstCondition.Subject, secondCondition.Subject);
            Assert.Equal(firstCondition.Message, secondCondition.Message);
            Assert.Equal(firstCondition.Paths.Count, secondCondition.Paths.Count);
            for (var pathIndex = 0; pathIndex < firstCondition.Paths.Count; pathIndex++)
            {
                Assert.Equal(firstCondition.Paths[pathIndex], secondCondition.Paths[pathIndex]);
            }
        }

        Assert.Equal(first.Next?.Command, second.Next?.Command);
        Assert.Equal(first.Next?.Reason, second.Next?.Reason);
        Assert.True(((IList<RouteInspectObservation>)first.Observations).IsReadOnly);
        Assert.True(((IList<RouteInspectCondition>)first.Conditions).IsReadOnly);
    }

    private static RouteInspectResult BuildResult(
        RouteSource source,
        RouteInspectResolution resolution,
        RouteInspectProfile? profile)
    {
        return new RouteInspectResultBuilder().Build(
            RouteInspectResolutionTestData.Request(
                resolution.Selection.RequestedReference ?? source.Id),
            resolution,
            profile);
    }

    private static (RouteInspectGraph Graph, RouteSource Source, RouteInspectResolution Resolution) ResolvedSource()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("loader"));
        var source = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("root"),
            });
        var graph = RouteInspectResolutionTestData.Graph([loader, source], [source.CanonicalPath]);
        return (graph, source, RouteInspectResolutionTestData.Resolved(graph, source.CanonicalPath));
    }

    private static (RouteInspectGraph Graph, RouteSource Selected, RouteInspectResolution Resolution) CollisionResolution(
        RouteInspectSelectionMethod selectionMethod,
        bool selectedUnavailable = false)
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
        RouteSource selected;
        if (selectedUnavailable)
        {
            selected = RouteInspectSourceTestData.Source(new RouteInspectSourceSpec
            {
                Path = ".agents/root/collision.md",
                Kind = RouteSourceKind.Markdown,
                Body = "selected",
                MetadataState = RouteSourceMetadataState.ReadUnavailable,
                DocumentReadState = FileReadState.AccessDenied,
            });
        }
        else
        {
            selected = RouteInspectSourceTestData.Source(
                ".agents/root/collision.md",
                RouteSourceKind.Markdown,
                "selected");
        }

        var other = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/collision/_collision.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("other"),
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, selected, other],
            [root.CanonicalPath]);
        return (
            graph,
            selected,
            RouteInspectResolutionTestData.Resolved(
                graph,
                selected.CanonicalPath,
                selectionMethod));
    }

    private static (RouteInspectGraph Graph, RouteSource Source, RouteInspectResolution Resolution) BlockedRouteSource()
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
        var alternate = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/references.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("alternate"),
            });
        var source = RouteInspectSourceTestData.Source(new RouteInspectSourceSpec
        {
            Path = ".agents/root/target.md",
            Kind = RouteSourceKind.Markdown,
            MetadataState = RouteSourceMetadataState.ReadUnavailable,
            DocumentReadState = FileReadState.AccessDenied,
        });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, alternate, source],
            [root.CanonicalPath]);
        var selection = RouteInspectResolutionTestData.Selection(
            source,
            RouteInspectSelectionMethod.AutomaticId);
        var identity = RouteInspectResolutionTestData.Identity(
            source,
            RouteInspectRouteState.Ambiguous);
        var resolution = RouteInspectResolutionTestData.Create(new RouteInspectResolutionSpec
        {
            State = RouteInspectResolutionState.Blocked,
            Selection = selection,
            Identity = identity,
            Graph = graph,
            Issues =
            [
                RouteInspectResolutionTestData.Issue(
                    RouteInspectResolutionIssueCode.ReadUnavailable,
                    source.CanonicalPath),
                RouteInspectResolutionTestData.Issue(
                    RouteInspectResolutionIssueCode.AmbiguousRoute,
                    source.CanonicalPath),
            ],
        });
        return (graph, source, resolution);
    }

    private static void AssertDiagnostic(RouteInspectCondition condition, string? expectedSubject = null)
    {
        if (expectedSubject is not null)
        {
            Assert.Equal(expectedSubject, condition.Subject);
        }

        Assert.False(string.IsNullOrWhiteSpace(condition.Subject));
        Assert.False(string.IsNullOrWhiteSpace(condition.Message));
    }

    private static CliNextAction AssertNextReason(RouteInspectResult result)
    {
        var next = Assert.IsType<CliNextAction>(result.Next);
        Assert.False(string.IsNullOrWhiteSpace(next.Reason));
        return next;
    }

    private static void AssertAction(
        RouteInspectResult result,
        string commandFragment,
        string reasonFragment)
    {
        var next = AssertNextReason(result);
        Assert.Contains(commandFragment, next.Command, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(reasonFragment, next.Reason, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertSafeRecoveryAction(RouteInspectResult result)
    {
        var command = AssertNextReason(result).Command;
        Assert.True(
            command.Contains("doctor", StringComparison.OrdinalIgnoreCase)
            || command.Contains("route inspect", StringComparison.OrdinalIgnoreCase));
    }
}
