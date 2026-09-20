namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

internal sealed class RouteInspectReadingProfile
{
    internal RouteInspectReadingProfile(
        RouteInspectFact<bool> taskStart,
        RouteInspectFact<RouteInspectAutomaticReadings> automatic,
        RouteInspectFact<RouteInspectLaterReading> later)
    {
        ArgumentNullException.ThrowIfNull(taskStart);
        ArgumentNullException.ThrowIfNull(automatic);
        ArgumentNullException.ThrowIfNull(later);
        TaskStart = taskStart;
        Automatic = automatic;
        Later = later;
    }

    internal RouteInspectFact<bool> TaskStart { get; }

    internal RouteInspectFact<RouteInspectAutomaticReadings> Automatic { get; }

    internal RouteInspectFact<RouteInspectLaterReading> Later { get; }
}
