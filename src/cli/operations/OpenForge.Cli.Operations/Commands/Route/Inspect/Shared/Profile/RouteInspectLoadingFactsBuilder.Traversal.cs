using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile.Models;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed partial class RouteInspectLoadingFactsBuilder
{
    private void ReadLoaderStartup(ISet<string> paths, Queue<string> queue)
    {
        var loader = _graph.ProjectionSet.FindByPath(LoaderPath);
        if (loader is null)
        {
            _startupAvailable = false;
            _readingAvailable = false;
            return;
        }

        var read = ReadVisibleEntries(loader.CanonicalPath);
        if (!read.IsAvailable)
        {
            _startupAvailable = false;
            _readingAvailable = false;
            return;
        }

        foreach (var entry in read.Entries)
        {
            if (entry.LoadNow || entry.KeepInMind)
            {
                AddSource(paths, entry.TargetPath, queue, selected: false);
            }
        }
    }

    private void TraverseStartup(ISet<string> paths, Queue<string> queue)
    {
        while (queue.TryDequeue(out var parentPath))
        {
            CheckCancellation();
            var read = ReadVisibleEntries(parentPath);
            if (!read.IsAvailable)
            {
                _startupAvailable = false;
                _readingAvailable = false;
                continue;
            }

            foreach (var entry in read.Entries)
            {
                if (entry.LoadNow || entry.KeepInMind)
                {
                    AddSource(paths, entry.TargetPath, queue, selected: false);
                }
            }
        }
    }

    private HashSet<string> ReadSelectedLoading(IEnumerable<RouteSource> chain)
    {
        var descendants = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>(chain
            .Where(source => source.Kind == RouteSourceKind.Entrypoint)
            .Select(source => source.CanonicalPath));
        while (queue.TryDequeue(out var parentPath))
        {
            CheckCancellation();
            var read = ReadVisibleEntries(parentPath);
            if (!read.IsAvailable)
            {
                _selectedAvailable = false;
                _readingAvailable = false;
                continue;
            }

            foreach (var entry in read.Entries.Where(entry => entry.LoadNow || entry.KeepInMind))
            {
                if (descendants.Add(entry.TargetPath) && IsEntrypoint(entry.TargetPath))
                {
                    queue.Enqueue(entry.TargetPath);
                }
            }
        }

        return descendants;
    }

    private HashSet<string> ReadNarrowDescendants()
    {
        var descendants = new HashSet<string>(StringComparer.Ordinal);
        if (_selected.Kind != RouteSourceKind.Entrypoint)
        {
            return descendants;
        }

        var queue = new Queue<string>([_selected.CanonicalPath]);
        while (queue.TryDequeue(out var parentPath))
        {
            CheckCancellation();
            var read = ReadVisibleEntries(parentPath);
            if (!read.IsAvailable)
            {
                _narrowAvailable = false;
                continue;
            }

            foreach (var entry in read.Entries.Where(entry => entry.LoadNow))
            {
                descendants.Add(entry.TargetPath);
                if (IsEntrypoint(entry.TargetPath))
                {
                    queue.Enqueue(entry.TargetPath);
                }
            }
        }

        return descendants;
    }

    private void UpdateReadingAvailability()
    {
        if (_resolution.State == RouteInspectResolutionState.Incomplete
            || _selected.Metadata.State != RouteSourceMetadataState.Complete)
        {
            _readingAvailable = false;
            return;
        }

        var node = _graph.RouteFacts.Topology.FindByPath(_selected.CanonicalPath);
        if (node is null)
        {
            _readingAvailable = false;
            return;
        }

        if (node.ParentState == SourceRouteParentState.Resolved
            && !ReadVisibleEntries(node.ParentPaths[0]).IsAvailable)
        {
            _readingAvailable = false;
        }
    }
}
