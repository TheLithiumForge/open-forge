using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed class RouteInspectReadingProfileBuilder
{
    private static readonly RouteInspectLaterReadOccasion[] LaterOccasions =
    [
        RouteInspectLaterReadOccasion.ContextRestoration, RouteInspectLaterReadOccasion.Handoff,
        RouteInspectLaterReadOccasion.Closeout, RouteInspectLaterReadOccasion.FollowupTransition,
    ];

    private readonly RouteInspectGraph _graph;
    private readonly RouteInspectIdentity _identity;
    private readonly RouteInspectLoadingFacts _loading;
    private readonly RouteSource _selected;

    internal RouteInspectReadingProfileBuilder(
        RouteInspectResolution resolution,
        RouteInspectLoadingFacts loading)
    {
        _graph = resolution.ReadGraph();
        _identity = resolution.ReadIdentity();
        _loading = loading;
        _selected = _graph.ProjectionSet.FindByPath(_identity.CanonicalWorkspaceRelativePath)
            ?? throw new InvalidOperationException("The selected source is absent from the inspect projection set.");
    }

    internal RouteInspectReadingProfile Build()
    {
        var routeState = _identity.RouteState;
        if (routeState is RouteInspectRouteState.Detached or RouteInspectRouteState.NotRouted)
        {
            return NotApplicable();
        }

        if (!_loading.ReadingAvailable)
        {
            return Unavailable("The loading facts needed for reading classification are unavailable.");
        }

        var reasons = new List<RouteInspectAutomaticReading>();
        AddParentLoadNow(reasons);
        AddKeepInMind(reasons);
        if (reasons.Count == 0)
        {
            reasons.Add(new RouteInspectAutomaticReading(
                RouteInspectAutomaticReadingKind.OnDemand,
                null,
                [RouteInspectAutomaticReadingEvent.RouteSelected]));
        }

        if (_selected.Overwrite is not null)
        {
            reasons.Add(new RouteInspectAutomaticReading(
                RouteInspectAutomaticReadingKind.OverwriteAfterBase,
                _selected.Id,
                [RouteInspectAutomaticReadingEvent.BaseRead]));
        }

        var automatic = new RouteInspectAutomaticReadings(reasons);
        var taskStart = RouteInspectFact<bool>.Available(
            _loading.StartupPaths.Contains(_selected.CanonicalPath));
        var later = automatic.Reasons.Any(reason => reason.Kind is
                RouteInspectAutomaticReadingKind.EntrypointKeepInMind
                or RouteInspectAutomaticReadingKind.RoutedFileKeepInMind)
            ? RouteInspectFact<RouteInspectLaterReading>.Available(
                new RouteInspectLaterReading(true, LaterOccasions))
            : RouteInspectFact<RouteInspectLaterReading>.Available(
                new RouteInspectLaterReading(false, []));
        return new RouteInspectReadingProfile(
            taskStart,
            RouteInspectFact<RouteInspectAutomaticReadings>.Available(automatic),
            later);
    }

    private void AddParentLoadNow(ICollection<RouteInspectAutomaticReading> reasons)
    {
        foreach (var entry in _loading.VisibleEntries.Values
                     .SelectMany(entries => entries)
                     .Where(entry => entry.TargetPath == _selected.CanonicalPath && entry.LoadNow))
        {
            var parent = _graph.ProjectionSet.FindByPath(entry.ParentPath);
            if (parent is null)
            {
                continue;
            }

            if (reasons.Any(reason => reason.Kind == RouteInspectAutomaticReadingKind.ParentLoadNow))
            {
                continue;
            }

            reasons.Add(new RouteInspectAutomaticReading(
                RouteInspectAutomaticReadingKind.ParentLoadNow,
                parent.Id,
                [RouteInspectAutomaticReadingEvent.ExposingParentRead]));
        }
    }

    private void AddKeepInMind(ICollection<RouteInspectAutomaticReading> reasons)
    {
        if (_selected.Metadata.State != RouteSourceMetadataState.Complete)
        {
            return;
        }

        var entry = _loading.VisibleEntries.Values
            .SelectMany(entries => entries)
            .FirstOrDefault(entry => entry.TargetPath == _selected.CanonicalPath && entry.KeepInMind);
        if (entry is null)
        {
            return;
        }

        var parent = _graph.ProjectionSet.FindByPath(entry.ParentPath);
        reasons.Add(new RouteInspectAutomaticReading(
            _selected.Kind == RouteSourceKind.Entrypoint
                ? RouteInspectAutomaticReadingKind.EntrypointKeepInMind
                : RouteInspectAutomaticReadingKind.RoutedFileKeepInMind,
            parent?.Id,
            [
                RouteInspectAutomaticReadingEvent.ExposingParentRead,
                RouteInspectAutomaticReadingEvent.LaterReview,
            ]));
    }

    private static RouteInspectReadingProfile NotApplicable()
    {
        return new RouteInspectReadingProfile(
            RouteInspectFact<bool>.NotApplicable("Loader-rooted reading does not apply to this source."),
            RouteInspectFact<RouteInspectAutomaticReadings>.NotApplicable("Loader-rooted reading does not apply to this source."),
            RouteInspectFact<RouteInspectLaterReading>.NotApplicable("Loader-rooted reading does not apply to this source."));
    }

    private static RouteInspectReadingProfile Unavailable(string reason)
    {
        return new RouteInspectReadingProfile(
            RouteInspectFact<bool>.Unavailable(reason),
            RouteInspectFact<RouteInspectAutomaticReadings>.Unavailable(reason),
            RouteInspectFact<RouteInspectLaterReading>.Unavailable(reason));
    }
}
