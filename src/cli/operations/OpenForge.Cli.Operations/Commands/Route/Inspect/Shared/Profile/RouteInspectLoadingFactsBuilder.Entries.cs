using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile.Models;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Applicability;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed partial class RouteInspectLoadingFactsBuilder
{
    private RouteInspectVisibleEntriesRead ReadVisibleEntries(string parentPath)
    {
        if (_visibleEntries.TryGetValue(parentPath, out var cached))
        {
            return cached;
        }

        var parent = _graph.ProjectionSet.FindByPath(parentPath);
        var read = parent is null
            ? new RouteInspectVisibleEntriesRead { IsAvailable = false, Reason = "The exposing entrypoint is absent." }
            : ReadVisibleEntries(parent);
        _visibleEntries[parentPath] = read;
        return read;
    }

    private RouteInspectVisibleEntriesRead ReadVisibleEntries(RouteSource parent)
    {
        var generated = RouteInspectGeneratedEntriesReader.Read(parent);
        if (_graph.HasRowPolicy(parent.CanonicalPath) && parent.Base.ReadState == FileReadState.Complete && parent.Base.Body is { } body)
        {
            var parsed = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(body));
            generated = parsed.State == SourceGeneratedEntriesState.Complete
                ? RouteInspectGeneratedEntries.Available(parsed.Entries.Select(entry => new RouteInspectGeneratedEntry(
                    entry.Destination, entry.Tags.Select(tag => $"#{tag}"))))
                : RouteInspectGeneratedEntries.Unavailable(parsed.Cause ?? "The linked catalogue Entries section is unavailable.");
        }
        if (!generated.IsAvailable)
        {
            return new RouteInspectVisibleEntriesRead { IsAvailable = false, Reason = generated.ReadReason() };
        }

        var entries = new Dictionary<string, RouteInspectVisibleEntry>(StringComparer.Ordinal);
        foreach (var generatedEntry in generated.Entries)
        {
            var targetPath = SourceGeneratedDestinationResolver.Resolve(
                _graph.Layout,
                parent.CanonicalPath,
                parent.Kind == RouteSourceKind.Loader,
                generatedEntry.Destination)
                ?? SourceGeneratedDestinationResolver.ResolveLinked(_graph.Layout, parent.CanonicalPath,
                    parent.Kind == RouteSourceKind.Loader, generatedEntry.Destination);
            if (targetPath is not null && _graph.Catalogue?.Aliases.TryGetValue(targetPath, out var retained) is true)
                targetPath = retained;
            var linked = targetPath is not null && _graph.HasRowPolicy(targetPath);
            if (targetPath is null || !linked && !IsDirectChild(parent, targetPath)
                || linked && _graph.Catalogue?.FindByPath(targetPath)?.IncludedParentPath != parent.CanonicalPath)
            {
                continue;
            }

            var child = _graph.ProjectionSet.FindByPath(targetPath);
            if (child is null)
            {
                continue;
            }

            var loadNow = generatedEntry.HasTag(SourceLoadingClosureResolver.LoadNowTag);
            var keepInMind = generatedEntry.HasTag(SourceLoadingClosureResolver.KeepInMindTag);
            if (parent.Kind != RouteSourceKind.Loader
                && !linked
                && (loadNow || keepInMind)
                && child.Metadata.State != RouteSourceMetadataState.Complete)
            {
                return new RouteInspectVisibleEntriesRead
                {
                    IsAvailable = false,
                    Reason = "A visible child source has unavailable loading metadata.",
                };
            }

            if (parent.Kind != RouteSourceKind.Loader && !linked)
            {
                loadNow &= HasTag(child, SourceLoadingClosureResolver.LoadNowTag);
                keepInMind &= HasTag(child, SourceLoadingClosureResolver.KeepInMindTag);
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
            return _graph.RouteFacts.Topology.LoaderRootPaths.Contains(targetPath, StringComparer.Ordinal);
        }

        var node = _graph.RouteFacts.Topology.FindByPath(parent.CanonicalPath);
        return node is not null && node.ChildPaths.Contains(targetPath, StringComparer.Ordinal);
    }

    private bool IsEntrypoint(string path)
    {
        return _graph.ProjectionSet.FindByPath(path)?.Kind == RouteSourceKind.Entrypoint
            || _graph.Catalogue?.FindByPath(path)?.IsIncludedCatalogue is true;
    }

    private void AddSource(
        ISet<string> paths,
        string path,
        Queue<string>? entrypointQueue,
        bool selected)
    {
        var source = _graph.ProjectionSet.FindByPath(path);
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

        if (paths.Add(path) && IsEntrypoint(path))
        {
            entrypointQueue?.Enqueue(path);
        }
    }

    private bool ShouldReadAutomaticEntry(
        string path,
        RouteInspectLoadingSet target,
        bool hasLoadingTag)
    {
        if (!hasLoadingTag)
        {
            return false;
        }

        var applicability = RouteInspectSourceApplicabilityEvaluator.Evaluate(_graph, path, _workingPaths);
        if (applicability is null)
        {
            MarkLoadingSetUnavailable(target);
            return false;
        }

        if (applicability.State is SourceApplicabilityState.Pending or SourceApplicabilityState.Invalid)
        {
            _workingPathsPending |= applicability.State == SourceApplicabilityState.Pending;
            MarkLoadingSetUnavailable(target);
        }

        return SourceLoadingClosureResolver.ShouldLoadAutomatically(hasLoadingTag, applicability.State);
    }

    private void MarkLoadingSetUnavailable(RouteInspectLoadingSet target)
    {
        switch (target)
        {
            case RouteInspectLoadingSet.Startup:
                _startupAvailable = false;
                return;
            case RouteInspectLoadingSet.Selected:
                _selectedAvailable = false;
                return;
            case RouteInspectLoadingSet.Narrow:
                _narrowAvailable = false;
                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(target),
                    target,
                    "The route-inspect loading set is not defined.");
        }
    }
}
