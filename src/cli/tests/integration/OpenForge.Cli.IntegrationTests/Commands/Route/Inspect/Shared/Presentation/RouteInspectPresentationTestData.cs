using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
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
}
