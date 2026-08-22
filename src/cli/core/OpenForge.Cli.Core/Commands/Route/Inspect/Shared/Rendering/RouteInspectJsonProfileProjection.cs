using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectJsonProfileProjection
{
    internal static RouteInspectJsonProfile Create(RouteInspectProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return new RouteInspectJsonProfile
        {
            Reading = Reading(profile.Reading),
            Measurements = Measurements(profile.Measurements),
            Topology = RouteInspectJsonFacts.Create(profile.Topology, Topology),
            Axioms = RouteInspectJsonFacts.Create(profile.Axioms, Axioms),
            Completeness = RouteInspectJsonNames.Completeness(profile.Completeness),
            Safety = RouteInspectJsonNames.Safety(profile.Safety),
        };
    }

    private static RouteInspectJsonReading Reading(RouteInspectReadingProfile reading)
    {
        return new RouteInspectJsonReading
        {
            TaskStart = RouteInspectJsonFacts.Boolean(reading.TaskStart),
            Automatic = RouteInspectJsonFacts.Create(reading.Automatic, AutomaticReadings),
            Later = RouteInspectJsonFacts.Create(reading.Later, LaterReading),
        };
    }

    private static RouteInspectJsonAutomaticReadings AutomaticReadings(
        RouteInspectAutomaticReadings readings)
    {
        return new RouteInspectJsonAutomaticReadings
        {
            Reasons = readings.Reasons.Select(AutomaticReading).ToArray(),
        };
    }

    private static RouteInspectJsonAutomaticReading AutomaticReading(
        RouteInspectAutomaticReading reading)
    {
        return new RouteInspectJsonAutomaticReading
        {
            Kind = RouteInspectJsonNames.ReadingKind(reading.Kind),
            RelatedSourceId = reading.RelatedSourceId,
            Events = reading.Events.Select(RouteInspectJsonNames.ReadingEvent).ToArray(),
        };
    }

    private static RouteInspectJsonLaterReading LaterReading(RouteInspectLaterReading reading)
    {
        return new RouteInspectJsonLaterReading
        {
            MayBeReadAgain = reading.MayBeReadAgain,
            Occasions = reading.Occasions.Select(RouteInspectJsonNames.LaterOccasion).ToArray(),
        };
    }

    private static RouteInspectJsonMeasurements Measurements(RouteInspectMeasurements measurements)
    {
        return new RouteInspectJsonMeasurements
        {
            OwnSource = RouteInspectJsonFacts.Measurement(measurements.OwnSource),
            SelectedClosure = RouteInspectJsonFacts.Measurement(measurements.SelectedClosure),
            TaskStartOverlap = RouteInspectJsonFacts.Measurement(measurements.TaskStartOverlap),
            SelectionAddition = RouteInspectJsonFacts.Measurement(measurements.SelectionAddition),
            LoadNowDescendants = RouteInspectJsonFacts.Measurement(measurements.LoadNowDescendants),
        };
    }

    private static RouteInspectJsonTopology Topology(RouteInspectTopology topology)
    {
        return new RouteInspectJsonTopology
        {
            RootRoute = topology.RootRoute,
            RouteChain = topology.RouteChain.ToArray(),
            ParentId = topology.ParentId,
            Depth = topology.Depth,
            Counts = RouteInspectJsonFacts.Counts(topology.Counts),
        };
    }

    private static RouteInspectJsonAxioms Axioms(RouteInspectAxiomsProfile axioms)
    {
        return new RouteInspectJsonAxioms
        {
            Inherited = RouteInspectJsonFacts.Create(
                axioms.Inherited,
                sources => new RouteInspectJsonAxiomsSources
                {
                    SourceIds = sources.SourceIds.ToArray(),
                }),
            Local = RouteInspectJsonFacts.LocalAxioms(axioms.Local),
        };
    }
}
