using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

internal static class RouteInspectPresentationTestData
{
    internal static RouteInspectResult CompleteResult()
    {
        var source = Source(withOverwrite: true);
        var selection = RouteInspectResolutionTestData.Selection(
            source,
            RouteInspectSelectionMethod.AutomaticId);
        return CreateResolved(
            CliSemanticStatus.Complete,
            source,
            selection,
            RichProfile(),
            [],
            [],
            null);
    }

    internal static RouteInspectResult ExactPathAttentionResult()
    {
        var source = Source();
        var selection = RouteInspectResolutionTestData.Selection(
            source,
            RouteInspectSelectionMethod.ExactPath);
        return CreateResolved(
            CliSemanticStatus.Attention,
            source,
            selection,
            RichProfile(),
            [AutomaticIdObservation()],
            [],
            null);
    }

    internal static RouteInspectResult InteractiveAttentionResult()
    {
        var source = Source();
        var selection = RouteInspectResolutionTestData.Selection(
            source,
            RouteInspectSelectionMethod.Interactive);
        return CreateResolved(
            CliSemanticStatus.Attention,
            source,
            selection,
            RichProfile(),
            [AutomaticIdObservation()],
            [],
            new CliNextAction(
                "open-forge route inspect \".agents/root/item/_item.md\"",
                "Rerun with the exact path for non-interactive use."));
    }

    internal static RouteInspectResult IncompleteResult()
    {
        var source = Source();
        var selection = RouteInspectResolutionTestData.Selection(
            source,
            RouteInspectSelectionMethod.AutomaticId);
        var profile = RouteInspectProfileTestData.Profile(new RouteInspectProfileSpec
        {
            Completeness = RouteInspectCompleteness.Incomplete,
            Measurements = RouteInspectProfileTestData.Measurements(new RouteInspectMeasurementsSpec
            {
                OwnSource = RouteInspectProfileTestData.UnavailableMeasurement(),
                SelectedClosure = RouteInspectProfileTestData.Measurement(1, 8, 8),
                TaskStartOverlap = RouteInspectProfileTestData.Measurement(0, 0, 0),
                SelectionAddition = RouteInspectProfileTestData.Measurement(1, 8, 8),
                LoadNowDescendants = RouteInspectProfileTestData.NotApplicableMeasurement(),
            }),
        });
        return CreateResolved(
            CliSemanticStatus.Incomplete,
            source,
            selection,
            profile,
            [],
            [new RouteInspectCondition(
                RouteInspectConditionCode.UnavailableFact,
                CliSemanticStatus.Incomplete,
                ".agents/root/item/_item.md",
                "The own-source measurement is unavailable.")],
            new CliNextAction(
                "open-forge doctor",
                "Review the unavailable route fact, then rerun route inspect."));
    }

    internal static RouteInspectResult InvalidResult()
    {
        var selection = RouteInspectResolutionTestData.InvalidReference("missing source");
        return RouteInspectResult.Create(
            CliSemanticStatus.Invalid,
            RouteInspectResolutionTestData.Workspace(),
            selection,
            null,
            null,
            [],
            [new RouteInspectCondition(
                RouteInspectConditionCode.InvalidSourceReference,
                CliSemanticStatus.Invalid,
                "missing source",
                "route inspect requires one known source reference.")],
            new CliNextAction(
                "open-forge route inspect --help",
                "Correct the named source or input, then rerun route inspect."));
    }

    internal static RouteInspectResult BlockedCollisionResult()
    {
        var candidates = new[]
        {
            ".agents/root/collision.md",
            ".agents/root/collision/_collision.md",
        };
        return RouteInspectResult.Create(
            CliSemanticStatus.Blocked,
            RouteInspectResolutionTestData.Workspace(),
            RouteInspectResolutionTestData.UnresolvedId("root/collision", candidates),
            null,
            null,
            [],
            [new RouteInspectCondition(
                RouteInspectConditionCode.AmbiguousSource,
                CliSemanticStatus.Blocked,
                "root/collision",
                "The automatic source ID is not unique.",
                candidates)],
            new CliNextAction(
                "open-forge route inspect \".agents/root/collision.md\"",
                "Rerun with one listed exact path to resolve the source collision."));
    }

    internal static RouteInspectResult FailedResult()
    {
        return EventResult(
            CliSemanticStatus.Failed,
            RouteInspectConditionCode.OperationFailed,
            "root/failure",
            "Route inspection failed while forming its result.",
            "open-forge route inspect",
            "Address the reported failure, then retry route inspect.");
    }

    internal static RouteInspectResult InterruptedResult()
    {
        return EventResult(
            CliSemanticStatus.Interrupted,
            RouteInspectConditionCode.Interrupted,
            "root/interrupted",
            "Route inspection was interrupted while forming its result.",
            "open-forge route inspect",
            "Rerun the same route-inspect request.");
    }

    internal static RouteInspectResult FallbackBlockedResult()
    {
        const string subject = "root/unsafe";
        return RouteInspectResult.Create(
            CliSemanticStatus.Blocked,
            RouteInspectResolutionTestData.Workspace(),
            RouteInspectResolutionTestData.UnresolvedId(subject),
            null,
            null,
            [],
            [new RouteInspectCondition(
                RouteInspectConditionCode.UnsafeSource,
                CliSemanticStatus.Blocked,
                subject,
                "The source physical boundary is unsafe.")],
            new CliNextAction(
                "open-forge doctor",
                "Review the blocked source boundary, then rerun route inspect."));
    }

    internal static RouteInspectResult DirectCorrectionBlockedResult()
    {
        const string subject = "root/ambiguous";
        return RouteInspectResult.Create(
            CliSemanticStatus.Blocked,
            RouteInspectResolutionTestData.Workspace(),
            RouteInspectResolutionTestData.UnresolvedId(subject),
            null,
            null,
            [],
            [new RouteInspectCondition(
                RouteInspectConditionCode.AmbiguousRoute,
                CliSemanticStatus.Blocked,
                subject,
                "The route relationship is structurally ambiguous.")],
            new CliNextAction(
                "open-forge route inspect \"root/ambiguous\"",
                "Rerun with the exact source path after resolving the ambiguous route."));
    }

    internal static RouteInspectResult HostileResult()
    {
        const string hostile = "hostile\0value\u0001\r\n\t Ω 東京 --workspace";
        return RouteInspectResult.Create(
            CliSemanticStatus.Invalid,
            RouteInspectResolutionTestData.Workspace(),
            RouteInspectResolutionTestData.InvalidReference(hostile),
            null,
            null,
            [],
            [new RouteInspectCondition(
                RouteInspectConditionCode.InvalidSourceReference,
                CliSemanticStatus.Invalid,
                hostile,
                $"The source value was rejected: {hostile}.")],
            new CliNextAction(
                "open-forge route inspect --help",
                "Correct the named source or input, then rerun route inspect."));
    }

    internal static RouteInspectResult ForStatus(CliSemanticStatus status)
    {
        return status switch
        {
            CliSemanticStatus.Complete => CompleteResult(),
            CliSemanticStatus.Attention => ExactPathAttentionResult(),
            CliSemanticStatus.Incomplete => IncompleteResult(),
            CliSemanticStatus.Invalid => InvalidResult(),
            CliSemanticStatus.Blocked => BlockedCollisionResult(),
            CliSemanticStatus.Failed => FailedResult(),
            CliSemanticStatus.Interrupted => InterruptedResult(),
            _ => throw new ArgumentOutOfRangeException(nameof(status), "The fixed status is not supported."),
        };
    }

    internal static CliPresentationRequest<RouteInspectResult> Presentation(
        RouteInspectResult result,
        CliView view = CliView.Expanded,
        CliOutputFormat format = CliOutputFormat.Human,
        CliVerbosity verbosity = CliVerbosity.Normal)
    {
        return new CliPresentationRequest<RouteInspectResult>(
            result,
            new CliPresentation(format, view, verbosity));
    }

    private static RouteInspectResult CreateResolved(
        CliSemanticStatus status,
        RouteSource source,
        RouteInspectSelection selection,
        RouteInspectProfile profile,
        IEnumerable<RouteInspectObservation> observations,
        IEnumerable<RouteInspectCondition> conditions,
        CliNextAction? next)
    {
        return RouteInspectResult.Create(
            status,
            RouteInspectResolutionTestData.Workspace(),
            selection,
            RouteInspectResolutionTestData.Identity(source, RouteInspectRouteState.Routed),
            profile,
            observations,
            conditions,
            next);
    }

    private static RouteInspectResult EventResult(
        CliSemanticStatus status,
        RouteInspectConditionCode code,
        string subject,
        string message,
        string nextCommand,
        string nextReason)
    {
        return RouteInspectResult.Create(
            status,
            RouteInspectResolutionTestData.Workspace(),
            RouteInspectResolutionTestData.UnresolvedId(subject),
            null,
            null,
            [],
            [new RouteInspectCondition(code, status, subject, message)],
            new CliNextAction(nextCommand, nextReason));
    }

    private static RouteSource Source(bool withOverwrite = false)
    {
        return RouteInspectSourceTestData.Source(new RouteInspectSourceSpec
        {
            Path = ".agents/root/item/_item.md",
            Kind = RouteSourceKind.Entrypoint,
            Body = RouteInspectSourceTestData.BodyWithEntries("item"),
            OverwritePath = withOverwrite ? ".agents/root/item/_item.overwrite.md" : null,
            OverwriteBody = "custom item",
        });
    }

    private static RouteInspectProfile RichProfile()
    {
        return RouteInspectProfileTestData.Profile(new RouteInspectProfileSpec
        {
            TaskStart = RouteInspectFact<bool>.Available(true),
            Automatic = RouteInspectFact<RouteInspectAutomaticReadings>.Available(
                new RouteInspectAutomaticReadings(
                [new RouteInspectAutomaticReading(
                    RouteInspectAutomaticReadingKind.ParentLoadNow,
                    "root",
                    [RouteInspectAutomaticReadingEvent.ExposingParentRead])])),
            Later = RouteInspectFact<RouteInspectLaterReading>.Available(
                RouteInspectProfileTestData.ReadAgain()),
            Measurements = RouteInspectProfileTestData.Measurements(new RouteInspectMeasurementsSpec
            {
                OwnSource = RouteInspectProfileTestData.Measurement(2, 18, 24),
                SelectedClosure = RouteInspectProfileTestData.Measurement(4, 46, 64),
                TaskStartOverlap = RouteInspectProfileTestData.Measurement(3, 30, 40),
                SelectionAddition = RouteInspectProfileTestData.Measurement(1, 16, 24),
                LoadNowDescendants = RouteInspectProfileTestData.Measurement(0, 0, 0),
            }),
            Topology = RouteInspectFact<RouteInspectTopology>.Available(
                RouteInspectProfileTestData.Topology(new RouteInspectTopologySpec
                {
                    RootRoute = "root",
                    RouteChain = ["root", "item"],
                    ParentId = "root",
                    Depth = 2,
                    Counts = RouteInspectProfileTestData.Counts(2, 1, 4, 2),
                })),
            Axioms = RouteInspectFact<RouteInspectAxiomsProfile>.Available(
                RouteInspectProfileTestData.Axioms(
                    RouteInspectProfileTestData.Sources("loader", "root"),
                    RouteInspectFact<RouteInspectAxiomsLocalState>.Available(
                        RouteInspectAxiomsLocalState.Missing))),
        });
    }

    private static RouteInspectObservation AutomaticIdObservation()
    {
        return new RouteInspectObservation(
            RouteInspectObservationCode.AutomaticIdNotUnique,
            "root/item",
            "The automatic source ID is not unique.",
            [".agents/root/item.md", ".agents/root/item/_item.md"]);
    }
}
