using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectProfileModelTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect automatic reasons retain base order and place overwrite after the base")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void AutomaticReasonsRetainCanonicalOrder()
    {
        var reasons = new RouteInspectAutomaticReadings(
        [
            Reading(RouteInspectAutomaticReadingKind.ParentLoadNow, "parent", RouteInspectAutomaticReadingEvent.ExposingParentRead),
            Reading(
                RouteInspectAutomaticReadingKind.EntrypointKeepInMind,
                "parent",
                RouteInspectAutomaticReadingEvent.ExposingParentRead),
            Reading(RouteInspectAutomaticReadingKind.OverwriteAfterBase, "base", RouteInspectAutomaticReadingEvent.BaseRead),
        ]);

        Assert.Equal(
            [
                RouteInspectAutomaticReadingKind.ParentLoadNow,
                RouteInspectAutomaticReadingKind.EntrypointKeepInMind,
                RouteInspectAutomaticReadingKind.OverwriteAfterBase,
            ],
            reasons.Reasons.Select(reason => reason.Kind));
        Assert.Equal("parent", reasons.Reasons[0].RelatedSourceId);
        Assert.Equal("base", reasons.Reasons[^1].RelatedSourceId);

        var routedFileReasons = new RouteInspectAutomaticReadings(
        [
            Reading(RouteInspectAutomaticReadingKind.ParentLoadNow, "parent", RouteInspectAutomaticReadingEvent.ExposingParentRead),
            Reading(
                RouteInspectAutomaticReadingKind.RoutedFileKeepInMind,
                "parent",
                RouteInspectAutomaticReadingEvent.ExposingParentRead,
                RouteInspectAutomaticReadingEvent.LaterReview),
            Reading(RouteInspectAutomaticReadingKind.OverwriteAfterBase, "base", RouteInspectAutomaticReadingEvent.BaseRead),
        ]);

        Assert.Equal(
            [
                RouteInspectAutomaticReadingKind.ParentLoadNow,
                RouteInspectAutomaticReadingKind.RoutedFileKeepInMind,
                RouteInspectAutomaticReadingKind.OverwriteAfterBase,
            ],
            routedFileReasons.Reasons.Select(reason => reason.Kind));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect automatic reasons retain dual LoadNow and KeepInMind triggers")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void AutomaticReasonsRetainDualLoadNowAndKeepInMind()
    {
        var reasons = new RouteInspectAutomaticReadings(
        [
            Reading(RouteInspectAutomaticReadingKind.ParentLoadNow, "root", RouteInspectAutomaticReadingEvent.ExposingParentRead),
            Reading(
                RouteInspectAutomaticReadingKind.RoutedFileKeepInMind,
                "parent",
                RouteInspectAutomaticReadingEvent.ExposingParentRead,
                RouteInspectAutomaticReadingEvent.LaterReview),
        ]);

        Assert.Equal(2, reasons.Reasons.Count);
        Assert.Equal(RouteInspectAutomaticReadingKind.ParentLoadNow, reasons.Reasons[0].Kind);
        Assert.Equal(RouteInspectAutomaticReadingKind.RoutedFileKeepInMind, reasons.Reasons[1].Kind);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect automatic reasons allow OnDemand only without another base trigger")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void OnDemandDoesNotAccompanyAnotherBaseTrigger()
    {
        var onDemand = new RouteInspectAutomaticReadings(
        [
            Reading(RouteInspectAutomaticReadingKind.OnDemand, null, RouteInspectAutomaticReadingEvent.RouteSelected),
        ]);
        var onDemandWithOverwrite = new RouteInspectAutomaticReadings(
        [
            Reading(RouteInspectAutomaticReadingKind.OnDemand, null, RouteInspectAutomaticReadingEvent.RouteSelected),
            Reading(RouteInspectAutomaticReadingKind.OverwriteAfterBase, "base", RouteInspectAutomaticReadingEvent.BaseRead),
        ]);

        Assert.Equal([RouteInspectAutomaticReadingKind.OnDemand], onDemand.Reasons.Select(reason => reason.Kind));
        Assert.Equal(
            [
                RouteInspectAutomaticReadingKind.OnDemand,
                RouteInspectAutomaticReadingKind.OverwriteAfterBase,
            ],
            onDemandWithOverwrite.Reasons.Select(reason => reason.Kind));
        Assert.Throws<ArgumentException>(() => new RouteInspectAutomaticReadings(
        [
            Reading(RouteInspectAutomaticReadingKind.ParentLoadNow, "parent", RouteInspectAutomaticReadingEvent.ExposingParentRead),
            Reading(RouteInspectAutomaticReadingKind.OnDemand, null, RouteInspectAutomaticReadingEvent.RouteSelected),
        ]));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect axioms profile keeps inherited and local fact availability independent")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void AxiomsFactsAreIndependent()
    {
        var inherited = RouteInspectFact<RouteInspectAxiomsSources>.Unavailable("The inherited chain is unavailable.");
        var local = RouteInspectFact<RouteInspectAxiomsLocalState>.Available(RouteInspectAxiomsLocalState.NotApplicable);
        var profile = new RouteInspectAxiomsProfile(inherited, local);

        Assert.Equal(RouteInspectFactState.Unavailable, profile.Inherited.State);
        Assert.Null(profile.Inherited.Value);
        Assert.Equal(RouteInspectFactState.Value, profile.Local.State);
        Assert.Equal(RouteInspectAxiomsLocalState.NotApplicable, profile.Local.Value);

        var substantive = new RouteInspectAxiomsProfile(
            RouteInspectFact<RouteInspectAxiomsSources>.Available(new RouteInspectAxiomsSources(["loader", "root"])),
            RouteInspectFact<RouteInspectAxiomsLocalState>.Available(RouteInspectAxiomsLocalState.Substantive));

        var substantiveSources = Assert.IsType<RouteInspectAxiomsSources>(substantive.Inherited.Value);
        Assert.Equal(["loader", "root"], substantiveSources.SourceIds);
        Assert.Equal(RouteInspectAxiomsLocalState.Substantive, substantive.Local.Value);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect reading profile keeps refined automatic availability separate from task-start and later facts")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ReadingFactsRetainIndependentAvailability()
    {
        var reading = new RouteInspectReadingProfile(
            RouteInspectFact<bool>.Available(true),
            RouteInspectFact<RouteInspectAutomaticReadings>.Unavailable("Automatic trigger is unavailable."),
            RouteInspectFact<RouteInspectLaterReading>.NotApplicable("Later membership is not applicable."));

        Assert.Equal(RouteInspectFactState.Value, reading.TaskStart.State);
        Assert.True(reading.TaskStart.Value);
        Assert.Equal(RouteInspectFactState.Unavailable, reading.Automatic.State);
        Assert.Null(reading.Automatic.Value);
        Assert.Equal(RouteInspectFactState.NotApplicable, reading.Later.State);
        Assert.Null(reading.Later.Value);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route inspect axioms sources snapshot ordered identity provenance")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void AxiomsSourcesAreImmutableSnapshots()
    {
        var sourceIds = new List<string> { "loader", "root" };
        var sources = new RouteInspectAxiomsSources(sourceIds);
        sourceIds[0] = "changed";

        Assert.Equal(["loader", "root"], sources.SourceIds);
        Assert.True(((IList<string>)sources.SourceIds).IsReadOnly);
    }

    private static RouteInspectAutomaticReading Reading(
        RouteInspectAutomaticReadingKind kind,
        string? relatedSourceId,
        params RouteInspectAutomaticReadingEvent[] events)
    {
        return new RouteInspectAutomaticReading(kind, relatedSourceId, events);
    }
}
