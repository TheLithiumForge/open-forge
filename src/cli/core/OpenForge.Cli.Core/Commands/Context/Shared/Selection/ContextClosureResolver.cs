using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Request;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Selection;

internal sealed class ContextClosureResolver
{
    internal ContextClosureResolution Resolve(ContextRequest request, ContextGraph graph)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(graph);
        var requested = new ContextRequestedSourceResolver().Resolve(request, graph);
        if (requested.HasInvalid)
        {
            return new ContextClosureResolution(
                requestedSources: requested.Requests.Select(value => value.Result),
                startupSources: [],
                combinedSources: [],
                resultSources: [],
                findings: requested.Findings,
                startupIncluded: false,
                selectionComplete: false,
                selectionBlocked: requested.Blocked);
        }

        var startup = new ContextSelectionAccumulator();
        var loading = new ContextLoadingClosureResolver().Resolve(
            graph,
            requested.Requests,
            startup);
        var startupPaths = loading.Selection.StartupSources
            .Select(source => source.Source.CanonicalPath)
            .ToHashSet(StringComparer.Ordinal);
        IReadOnlyList<ContextSelectedGraphSource> result = request.AdditionsOnly
            ? loading.Selection.CombinedSources
                .Where(source => !startupPaths.Contains(source.Source.CanonicalPath))
                .ToArray()
            : loading.Selection.CombinedSources;
        return new ContextClosureResolution(
            requestedSources: requested.Requests.Select(value => value.Result),
            startupSources: loading.Selection.StartupSources,
            combinedSources: loading.Selection.CombinedSources,
            resultSources: result,
            findings: requested.Findings.Concat(loading.Findings),
            startupIncluded: !request.AdditionsOnly,
            selectionComplete: !loading.Incomplete && !loading.Blocked && !requested.Blocked,
            selectionBlocked: loading.Blocked || requested.Blocked);
    }
}
