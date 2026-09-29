using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;

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

        var chain = RouteInspectChainReader.Read(graph, sourcePath);
        if (chain is null)
        {
            return null;
        }

        var conditions = chain
            .Select(source => new SourceApplyToCondition(source.CanonicalPath, source.Metadata.ApplyTo))
            .ToArray();
        return SourceApplicabilityEvaluator.Evaluate(conditions, workspaceRelativePaths ?? []);
    }
}
