using OpenForge.Cli.Core.Commands.Context.Shared.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Context.Models.Result;

internal sealed record ContextSourceIdentity
{
    internal ContextSourceIdentity(string? id, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (id is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
        }

        Id = id;
        Path = path;
    }

    internal string? Id { get; }

    internal string Path { get; }
}

internal enum ContextRouteState
{
    Routed,
    Unrouted,
    Ambiguous,
    Unavailable,
}

internal sealed record ContextRequestedSource
{
    internal ContextRequestedSource(
        string supplied,
        SourceReferenceKind form,
        SourceReferenceResolutionState resolution,
        ContextSourceIdentity? source,
        ContextRouteState? routeState,
        IEnumerable<ContextSourceIdentity> candidates)
    {
        ArgumentNullException.ThrowIfNull(supplied);
        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The Context source-reference form is not defined.");
        }

        if (!Enum.IsDefined(resolution))
        {
            throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The Context source resolution is not defined.");
        }

        if (routeState is { } route && !Enum.IsDefined(route))
        {
            throw new ArgumentOutOfRangeException(nameof(routeState), routeState, "The Context route state is not defined.");
        }

        Supplied = supplied;
        Form = form;
        Resolution = resolution;
        Source = source;
        RouteState = routeState;
        Candidates = ContextResultCollections.Snapshot(candidates, nameof(candidates));
    }

    internal string Supplied { get; }

    internal SourceReferenceKind Form { get; }

    internal SourceReferenceResolutionState Resolution { get; }

    internal ContextSourceIdentity? Source { get; }

    internal ContextRouteState? RouteState { get; }

    internal IReadOnlyList<ContextSourceIdentity> Candidates { get; }
}

internal sealed record ContextSelection
{
    internal ContextSelection(
        IEnumerable<ContextRequestedSource> requestedSources,
        bool startupIncluded,
        bool additionsOnly,
        ContextLinkExpansion linkExpansion,
        int? sourceCount)
    {
        ArgumentNullException.ThrowIfNull(linkExpansion);
        if (sourceCount is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceCount), sourceCount, "The Context source count cannot be negative.");
        }

        RequestedSources = ContextResultCollections.Snapshot(requestedSources, nameof(requestedSources));
        StartupIncluded = startupIncluded;
        AdditionsOnly = additionsOnly;
        LinkExpansion = linkExpansion;
        SourceCount = sourceCount;
    }

    internal IReadOnlyList<ContextRequestedSource> RequestedSources { get; }

    internal bool StartupIncluded { get; }

    internal bool AdditionsOnly { get; }

    internal ContextLinkExpansion LinkExpansion { get; }

    internal int? SourceCount { get; }
}
