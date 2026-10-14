using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;

internal static class RoutedMaintenanceEffectAssembler
{
    internal static RoutedMaintenanceEffects Assemble(CliWorkspace workspace, RoutedMaintenanceIntent? intent, GeneratedNavigationProjection projection)
    {
        var changes = new Dictionary<string, PlannedFileChange>(StringComparer.Ordinal);
        var recovery = new Dictionary<string, RecoveryBundleTarget>(StringComparer.Ordinal);
        foreach (var entrypoint in intent?.Entrypoints ?? [])
        {
            var region = projection.Regions.Single(region => region.CanonicalPath == entrypoint.Source.Identity.CanonicalBasePath);
            var plan = new RouteScaffoldRenamePlanner().Complete(entrypoint, region);
            foreach (var change in plan.Changes) changes.Add(change.LogicalPath, change);
            foreach (var target in plan.RecoveryTargets) recovery.Add(target.Change.LogicalPath, target);
        }

        foreach (var metadata in intent?.Metadata ?? [])
        {
            var before = intent?.Originals[metadata.Path] ?? throw new InvalidOperationException();
            if (changes.ContainsKey(before.LogicalPath)) continue;
            var region = projection.Regions.FirstOrDefault(region => region.CanonicalPath == metadata.Path);
            var final = region?.Change?.ExpectedDocumentBytes ?? metadata.Completion.Bytes;
            var change = PlannedFileChange.Replace(before.Expectation, final.AsSpan());
            changes.Add(change.LogicalPath, change);
            recovery.Add(change.LogicalPath, RecoveryBundleTarget.Create(change, before));
        }

        foreach (var replacement in intent?.Replacements ?? new Dictionary<string, System.Collections.Immutable.ImmutableArray<byte>>())
        {
            var before = intent?.Originals[replacement.Key] ?? throw new InvalidOperationException();
            if (changes.ContainsKey(before.LogicalPath)) continue;
            var region = projection.Regions.FirstOrDefault(region => region.CanonicalPath == replacement.Key);
            var final = region?.Change?.ExpectedDocumentBytes ?? replacement.Value;
            var change = PlannedFileChange.Replace(before.Expectation, final.AsSpan());
            changes.Add(change.LogicalPath, change);
            recovery.Add(change.LogicalPath, RecoveryBundleTarget.Create(change, before));
        }

        foreach (var region in projection.Regions.Where(region => region.Change?.RequiresUpdate == true))
        {
            var path = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, region.CanonicalPath);
            if (changes.ContainsKey(path)) continue;
            var bytes = region.Change ?? throw new InvalidOperationException();
            var before = FileStateSnapshot.File(path, region.PhysicalPath, bytes.BeforeDocumentBytes.AsSpan());
            var change = PlannedFileChange.ReplaceGeneratedRegion(before.Expectation, bytes.ExpectedDocumentBytes.AsSpan());
            changes.Add(path, change);
            recovery.Add(path, RecoveryBundleTarget.Create(change, before));
        }

        var ordered = changes.Values.OrderBy(change => change.Kind == PlannedFileChangeKind.Create ? 0 : 1)
            .ThenBy(change => change.Kind == PlannedFileChangeKind.Create ? change.LogicalPath.Count(character => character is '/' or '\\') : 0)
            .ThenBy(change => change.LogicalPath, StringComparer.Ordinal).ToArray();
        return new([.. ordered], [.. ordered.Where(change => recovery.ContainsKey(change.LogicalPath)).Select(change => recovery[change.LogicalPath])]);
    }
}
