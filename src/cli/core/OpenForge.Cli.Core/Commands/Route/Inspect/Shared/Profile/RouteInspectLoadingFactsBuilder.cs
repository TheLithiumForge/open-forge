using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed partial class RouteInspectLoadingFactsBuilder
{
    private const string LoaderPath = SourceLogicalPath.LoaderPath;
    private readonly RouteInspectResolution _resolution;
    private readonly RouteInspectGraph _graph;
    private readonly RouteInspectIdentity _identity;
    private readonly RouteSource _selected;
    private readonly CancellationToken _cancellationToken;
    private readonly Dictionary<string, RouteInspectVisibleEntriesRead> _visibleEntries = new(StringComparer.Ordinal);
    private bool _startupAvailable = true;
    private bool _selectedAvailable = true;
    private bool _narrowAvailable = true;
    private bool _readingAvailable = true;

    internal RouteInspectLoadingFactsBuilder(
        RouteInspectResolution resolution,
        CancellationToken cancellationToken)
    {
        _resolution = resolution;
        _graph = resolution.ReadGraph();
        _identity = resolution.ReadIdentity();
        _selected = _graph.ProjectionSet.FindByPath(_identity.CanonicalWorkspaceRelativePath)
            ?? throw new InvalidOperationException("The selected source is absent from the inspect projection set.");
        _cancellationToken = cancellationToken;
    }

    internal RouteInspectLoadingFacts Build()
    {
        CheckCancellation();
        var routeState = _identity.RouteState;
        ApplyResolutionAvailability();
        if (routeState == RouteInspectRouteState.NotRouted)
        {
            return NotRoutedFacts();
        }

        var chain = ReadChain(_selected.CanonicalPath);
        if (chain is null)
        {
            return UnresolvedFacts();
        }

        var selectedPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in chain)
        {
            AddSource(selectedPaths, source.CanonicalPath, null, selected: true);
        }

        foreach (var path in ReadSelectedLoadNow(chain))
        {
            AddSource(selectedPaths, path, null, selected: true);
        }

        var narrowDescendants = ReadNarrowDescendants();

        if (routeState == RouteInspectRouteState.Detached)
        {
            return CreateFacts(new RouteInspectLoadingFactInputs
            {
                SelectedApplicable = true,
                NarrowDescendantsApplicable = _selected.Kind == RouteSourceKind.Entrypoint,
                SelectedPaths = selectedPaths,
                LoadNowDescendantPaths = narrowDescendants,
            });
        }

        if (routeState != RouteInspectRouteState.Routed)
        {
            return UnresolvedFacts();
        }

        var startupPaths = new HashSet<string>(StringComparer.Ordinal);
        var requiredAncestors = new HashSet<string>(StringComparer.Ordinal);
        var startupQueue = new Queue<string>();
        ReadLoaderStartup(startupPaths, startupQueue);
        TraverseStartup(startupPaths, startupQueue);
        ReadGlobalContinuity(startupPaths, startupQueue, requiredAncestors);
        TraverseStartup(startupPaths, startupQueue);
        UpdateReadingAvailability();

        return CreateFacts(new RouteInspectLoadingFactInputs
        {
            StartupApplicable = true,
            SelectedApplicable = true,
            NarrowDescendantsApplicable = _selected.Kind == RouteSourceKind.Entrypoint,
            StartupPaths = startupPaths,
            SelectedPaths = selectedPaths,
            LoadNowDescendantPaths = narrowDescendants,
            RequiredAncestorPaths = requiredAncestors,
        });
    }

    private void ApplyResolutionAvailability()
    {
        if (_resolution.State != RouteInspectResolutionState.Incomplete)
        {
            return;
        }

        _startupAvailable = false;
        _selectedAvailable = false;
        _narrowAvailable = false;
        _readingAvailable = false;
    }

    private RouteInspectLoadingFacts CreateFacts(RouteInspectLoadingFactInputs input)
    {
        return new RouteInspectLoadingFacts
        {
            StartupApplicable = input.StartupApplicable,
            SelectedApplicable = input.SelectedApplicable,
            NarrowDescendantsApplicable = input.NarrowDescendantsApplicable,
            StartupAvailable = _startupAvailable,
            SelectedAvailable = _selectedAvailable,
            NarrowDescendantsAvailable = _narrowAvailable,
            ReadingAvailable = _readingAvailable,
            StartupPaths = input.StartupPaths.ToHashSet(StringComparer.Ordinal),
            SelectedPaths = input.SelectedPaths.ToHashSet(StringComparer.Ordinal),
            LoadNowDescendantPaths = input.LoadNowDescendantPaths.ToHashSet(StringComparer.Ordinal),
            RequiredAncestorPaths = input.RequiredAncestorPaths.ToHashSet(StringComparer.Ordinal),
            VisibleEntries = _visibleEntries
                .Where(pair => pair.Value.IsAvailable)
                .ToDictionary(pair => pair.Key, pair => pair.Value.Entries, StringComparer.Ordinal),
        };
    }

    private RouteInspectLoadingFacts NotRoutedFacts()
    {
        return new RouteInspectLoadingFacts
        {
            StartupApplicable = false,
            SelectedApplicable = false,
            NarrowDescendantsApplicable = false,
            StartupAvailable = true,
            SelectedAvailable = true,
            NarrowDescendantsAvailable = true,
            ReadingAvailable = true,
        };
    }

    private RouteInspectLoadingFacts UnresolvedFacts()
    {
        return new RouteInspectLoadingFacts
        {
            StartupApplicable = true,
            SelectedApplicable = true,
            NarrowDescendantsApplicable = _selected.Kind == RouteSourceKind.Entrypoint,
            StartupAvailable = false,
            SelectedAvailable = false,
            NarrowDescendantsAvailable = false,
            ReadingAvailable = false,
        };
    }
}
