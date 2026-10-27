using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Applicability;

internal static class RouteInspectSourceApplicabilityEvaluator
{
    internal static SourceApplicabilityResult? Evaluate(
        RouteInspectGraph graph,
        string sourcePath,
        IReadOnlyList<string>? workspaceRelativePaths)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        var chain = ReadLoadingChain(graph, sourcePath);
        if (chain is null)
        {
            return null;
        }

        var conditions = chain
            .Select(source => new SourceApplyToCondition(source.CanonicalPath, graph.ReadCondition(source)))
            .ToArray();
        return SourceApplicabilityEvaluator.Evaluate(conditions, workspaceRelativePaths ?? []);
    }

    internal static IReadOnlyList<RouteSource>? ReadLoadingChain(RouteInspectGraph graph, string path)
    {
        if (!graph.HasRowPolicy(path)) return RouteInspectChainReader.Read(graph, path);
        var chain = new List<RouteSource>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        while (seen.Add(path))
        {
            var source = graph.ProjectionSet.FindByPath(path);
            if (source is null) return null;
            if (source.Kind == RouteSourceKind.Loader) return chain.AsEnumerable().Reverse().ToArray();
            if (graph.Catalogue?.FindByPath(path)?.IncludedParentPath is not { } parent)
            {
                var structural = RouteInspectChainReader.Read(graph, path);
                return structural is null ? null : structural.Concat(chain.AsEnumerable().Reverse()).ToArray();
            }
            chain.Add(source);
            path = parent;
        }
        return null;
    }
}
