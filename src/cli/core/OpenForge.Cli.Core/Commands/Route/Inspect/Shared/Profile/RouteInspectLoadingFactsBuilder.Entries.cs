using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed partial class RouteInspectLoadingFactsBuilder
{
    private RouteInspectVisibleEntriesRead ReadVisibleEntries(string parentPath)
    {
        if (_visibleEntries.TryGetValue(parentPath, out var cached))
        {
            return cached;
        }

        var parent = _graph.Catalogue.FindByPath(parentPath);
        var read = parent is null
            ? new RouteInspectVisibleEntriesRead { IsAvailable = false, Reason = "The exposing entrypoint is absent." }
            : ReadVisibleEntries(parent);
        _visibleEntries[parentPath] = read;
        return read;
    }

    private RouteInspectVisibleEntriesRead ReadVisibleEntries(RouteSource parent)
    {
        var generated = RouteInspectGeneratedEntriesReader.Read(parent);
        if (!generated.IsAvailable)
        {
            return new RouteInspectVisibleEntriesRead { IsAvailable = false, Reason = generated.Reason! };
        }

        var entries = new Dictionary<string, RouteInspectVisibleEntry>(StringComparer.Ordinal);
        foreach (var generatedEntry in generated.Entries)
        {
            var targetPath = ResolveDestination(
                RouteLogicalPath.ReadParent(parent.CanonicalPath),
                generatedEntry.Destination);
            if (targetPath is null || !IsDirectChild(parent, targetPath))
            {
                continue;
            }

            var child = _graph.Catalogue.FindByPath(targetPath);
            if (child is null)
            {
                continue;
            }

            var loadNow = generatedEntry.HasTag("LoadNow");
            var keepInMind = generatedEntry.HasTag("KeepInMind");
            if (parent.Kind != RouteSourceKind.Loader
                && (loadNow || keepInMind)
                && child.Metadata.State != RouteSourceMetadataState.Complete)
            {
                return new RouteInspectVisibleEntriesRead
                {
                    IsAvailable = false,
                    Reason = "A visible child source has unavailable loading metadata.",
                };
            }

            if (parent.Kind != RouteSourceKind.Loader
                && ((loadNow && !HasTag(child, "LoadNow"))
                    || (keepInMind && !HasTag(child, "KeepInMind"))))
            {
                continue;
            }

            var current = entries.GetValueOrDefault(targetPath);
            entries[targetPath] = new RouteInspectVisibleEntry(
                parent.CanonicalPath,
                targetPath,
                current?.LoadNow is true || loadNow,
                current?.KeepInMind is true || keepInMind);
        }

        return new RouteInspectVisibleEntriesRead
        {
            IsAvailable = true,
            Entries = entries.Values.ToArray(),
        };
    }

    private bool IsDirectChild(RouteSource parent, string targetPath)
    {
        if (parent.Kind == RouteSourceKind.Loader)
        {
            return _graph.Topology.LoaderRootPaths.Contains(targetPath, StringComparer.Ordinal);
        }

        var node = _graph.Topology.FindByPath(parent.CanonicalPath);
        return node is not null && node.ChildPaths.Contains(targetPath, StringComparer.Ordinal);
    }

    private bool IsEntrypoint(string path)
    {
        return _graph.Catalogue.FindByPath(path)?.Kind == RouteSourceKind.Entrypoint;
    }

    private bool IsRouted(RouteSource source)
    {
        return _graph.Topology.FindByPath(source.CanonicalPath) is not null
            && _graph.Topology.ReadAbsoluteDepth(source.CanonicalPath) is not null;
    }

    private void AddSource(
        ISet<string> paths,
        string path,
        Queue<string>? entrypointQueue,
        bool selected)
    {
        var source = _graph.Catalogue.FindByPath(path);
        if (source is null)
        {
            if (selected)
            {
                _selectedAvailable = false;
            }
            else
            {
                _startupAvailable = false;
            }

            return;
        }

        if (paths.Add(path) && source.Kind == RouteSourceKind.Entrypoint)
        {
            entrypointQueue?.Enqueue(path);
        }
    }
}
