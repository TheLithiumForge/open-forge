using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Result;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Result;

internal static class IndexCompletionProjector
{
    internal static IReadOnlyList<IndexMetadataCompletion> Metadata(IndexPlan plan, IReadOnlyDictionary<string, IndexRegionOutcome>? outcomes = null)
        => plan.Input.Projection.Maintenance?.Metadata.Select(item => new IndexMetadataCompletion(item.Path,
            [.. item.Completion.Fields.Select(field => field.ToString().ToLowerInvariant())], item.Completion.Description, item.Completion.Tags,
            ReadOutcome(plan, item.Path, outcomes))).ToArray() ?? [];

    internal static IReadOnlyList<IndexEntrypointCompletion> Entrypoints(IndexPlan plan, IReadOnlyDictionary<string, IndexRegionOutcome>? outcomes = null)
        => plan.Input.Projection.Maintenance?.Entrypoints.Select(item => new IndexEntrypointCompletion(item.Source.Identity.CanonicalBasePath,
            ReadOutcome(plan, item.Source.Identity.CanonicalBasePath, outcomes))).ToArray() ?? [];

    internal static IReadOnlyList<IndexFileDeletion> Deletions(IndexPlan plan, IReadOnlyDictionary<string, IndexRegionOutcome>? outcomes = null)
        => plan.Input.Projection.Maintenance?.Entrypoints.Where(item => item.DeleteBefore is not null).Select(item =>
        {
            var before = item.DeleteBefore ?? throw new InvalidOperationException();
            var path = Path.GetRelativePath(plan.Input.Request.Workspace.LexicalRoot, before.LogicalPath).Replace('\\', '/');
            return new IndexFileDeletion(path, ReadOutcome(plan, path, outcomes));
        }).ToArray() ?? [];

    private static IndexRegionOutcome ReadOutcome(IndexPlan plan, string path, IReadOnlyDictionary<string, IndexRegionOutcome>? outcomes)
        => outcomes?.TryGetValue(path, out var outcome) == true ? outcome : Initial(plan);

    private static IndexRegionOutcome Initial(IndexPlan plan)
        => plan.IsExecutable ? IndexPlanningOutcome.ReadInitial(plan.Input.Request.Mode) : IndexRegionOutcome.NotStarted;
}
