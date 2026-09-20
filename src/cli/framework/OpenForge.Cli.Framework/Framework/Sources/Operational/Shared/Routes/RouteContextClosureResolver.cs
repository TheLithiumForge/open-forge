using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal sealed class RouteContextClosureResolver
{
    internal RouteContextClosure Resolve(RouteContextSet context)
    {
        var sourcesByPath = context.Sources.ToDictionary(
            source => source.Path,
            StringComparer.Ordinal);
        var resolution = new SourceLoadingClosureResolver().Resolve(
            new SourceLoadingClosureRequest
            {
                WorkspaceEntryPath = context.WorkspaceEntry.Path,
                Sources = context.Sources.Select(source => new SourceLoadingClosureSource
                {
                    Path = source.Path,
                    Form = source.Form,
                    RouteState = source.RouteState,
                    ParentPath = source.ParentPath,
                    Metadata = source.Metadata,
                    GeneratedEntries = source.GeneratedEntries,
                }).ToArray(),
                LoaderRootPaths = context.RootPaths,
            });
        return new RouteContextClosure
        {
            Startup = resolution.Startup.Select(selection =>
                FindSource(context, sourcesByPath, selection.Path)).ToArray(),
            Continuity = resolution.ContinuityPaths.Select(path =>
                FindSource(context, sourcesByPath, path)).ToArray(),
            IsComplete = context.IsComplete && resolution.Issues.Count == 0,
        };
    }

    private static RouteContextSource FindSource(
        RouteContextSet context,
        IReadOnlyDictionary<string, RouteContextSource> sourcesByPath,
        string path)
    {
        if (string.Equals(path, context.WorkspaceEntry.Path, StringComparison.Ordinal))
        {
            return context.WorkspaceEntry;
        }

        return sourcesByPath.TryGetValue(path, out var source)
            ? source
            : throw new InvalidOperationException(
                "A source loading closure selection must belong to the Route context set.");
    }
}
