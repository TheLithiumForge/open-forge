using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

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

        if (_selected.Kind != RouteSourceKind.Entrypoint
            && _selected.Metadata.Tags.Contains("KeepInMind", StringComparer.Ordinal)
            && IsRouted())
        {
            reasons.Add(new RouteInspectAutomaticReading(
                RouteInspectAutomaticReadingKind.RoutedFileKeepInMind,
                null,
                [
                    RouteInspectAutomaticReadingEvent.TaskReview,
                    RouteInspectAutomaticReadingEvent.LaterReview,
                ]));
        }

        if (_selected.Kind != RouteSourceKind.Entrypoint
            || !_selected.Metadata.Tags.Contains("KeepInMind", StringComparer.Ordinal))
        {
            return;
        }

        var events = new List<RouteInspectAutomaticReadingEvent>();
        if (_loading.StartupPaths.Contains(_selected.CanonicalPath))
        {
            events.Add(RouteInspectAutomaticReadingEvent.TaskStartVisible);
        }

        events.Add(RouteInspectAutomaticReadingEvent.RouteSelected);
        if (IsScopeSelected())
        {
            events.Add(RouteInspectAutomaticReadingEvent.ScopeSelected);
        }

        if (_loading.RequiredAncestorPaths.Contains(_selected.CanonicalPath))
        {
            events.Add(RouteInspectAutomaticReadingEvent.AncestorRequired);
        }

        reasons.Add(new RouteInspectAutomaticReading(
            RouteInspectAutomaticReadingKind.EntrypointKeepInMind,
            null,
            events));
    }

    private bool IsScopeSelected()
    {
        var node = _graph.RouteFacts.Topology.FindByPath(_selected.CanonicalPath);
        if (node is null || node.ParentState == SourceRouteParentState.None)
        {
            return true;
        }

        return _loading.RequiredAncestorPaths.Contains(_selected.CanonicalPath)
            || node.ParentState == SourceRouteParentState.Resolved
                && !_loading.StartupPaths.Contains(_selected.CanonicalPath)
                && _loading.StartupPaths.Contains(node.ParentPaths[0]);
    }

    private bool IsRouted()
    {
        var topology = _graph.RouteFacts.Topology;
        return topology.FindByPath(_selected.CanonicalPath) is not null
            && topology.ReadAbsoluteDepth(_selected.CanonicalPath) is not null;
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
