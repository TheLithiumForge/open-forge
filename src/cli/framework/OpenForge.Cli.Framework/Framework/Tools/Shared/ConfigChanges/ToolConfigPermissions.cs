using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

internal static class ToolConfigPermissions
{
    internal static IEnumerable<string> Destinations(ToolConfigPlan plan)
        => plan.Removals.Where(change => change.Addition is not ToolNativeCommandAddition && ChangesBytes(change.Current, change.Desired))
            .Select(change => ToolConfigEditor.PathOf(change.Addition))
            .Concat(plan.Changes.Where(change => change.Change is not ToolNativeConfigChange && ChangesBytes(change.Current, change.Desired))
                .Select(change => ToolRecipeExpansion.Expand(ToolConfigEditor.PathOf(change.Change), plan.Request.Context)));

    internal static IEnumerable<string> Destinations(ToolConfigRemovePlan plan)
        => plan.Changes.Where(change => change.Addition is not ToolNativeCommandAddition && ChangesBytes(change.Current, change.Desired))
            .Select(change => ToolConfigEditor.PathOf(change.Addition));

    internal static async Task<WorkspacePermissionEvaluation> EvaluateAsync(ToolRecipeContext context, IEnumerable<string> destinations, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var selected = destinations.ToArray();
        if (selected.Length == 0) return WorkspaceAllowList.Evaluate([], []);
        var scope = ToolFileReader.Scope(context.WorkspacePath);
        ImmutableArray<string> paths = [.. selected.Select(path => Path.GetRelativePath(scope.LexicalRoot, ToolFileReader.Target(scope, path))
            .Replace(Path.DirectorySeparatorChar, '/')).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
        var read = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(scope, token).ConfigureAwait(false);
        if (read.State is not (WorkspaceLayoutReadState.Ready or WorkspaceLayoutReadState.Uninstalled) || read.Layout is not { } layout)
            return new(paths, paths, WorkspacePermissionDecision.Required);
        ImmutableArray<string> required = [.. paths.Where(path => !layout.IsImplicitContentDestination(path))];
        ImmutableArray<string> allowed = [.. read.Settings.Document.AllowInstallPaths.Concat(context.AdditionalAllowInstallPaths).Distinct(StringComparer.Ordinal)];
        return WorkspaceAllowList.Evaluate(allowed, required);
    }

    internal static async Task RequireAsync(ToolRecipeContext context, IEnumerable<string> destinations, CancellationToken token)
    {
        var evaluation = await EvaluateAsync(context, destinations, token).ConfigureAwait(false);
        if (evaluation.Decision == WorkspacePermissionDecision.Required)
            throw new UnauthorizedAccessException("The current workspace destination permissions do not admit the configuration changes.");
    }

    private static bool ChangesBytes(ToolRecordedValue current, ToolRecordedValue desired)
        => current.Present != desired.Present || current.Present
            && (current.Bytes is not { } before || desired.Bytes is not { } after || !before.AsSpan().SequenceEqual(after));
}
