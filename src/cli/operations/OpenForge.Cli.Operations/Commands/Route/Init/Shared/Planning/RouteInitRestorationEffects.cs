using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal static class RouteInitRestorationEffects
{
    internal static RouteInitProspectiveEffectPlan Extend(RouteInitProspectiveEffectPlan effects, RouteInitRestoration? restoration)
    {
        if (restoration is null)
        {
            return effects;
        }

        var changes = effects.FileChanges.ToList();
        var recovery = effects.RecoveryTargets.ToList();
        var managed = effects.FrameworkManagedStates.ToList();
        var additions = new List<PlannedFileChange>();
        foreach (var file in restoration.Files.Where(file => file.Source is null && file.Before.Kind == FileExpectationKind.Missing))
        {
            additions.Add(PlannedFileChange.Create(file.Before.Expectation, file.IntendedBytes.Span));
            managed.Add(new RouteInitFrameworkManagedState(file.Asset.Path, file.Asset.Path, ownsGeneratedEntries: false));
        }

        if (restoration.SettingsChange is { } settingsChange)
        {
            additions.Add(settingsChange);
            recovery.Add(RecoveryBundleTarget.Create(settingsChange,
                restoration.Settings.Snapshot ?? throw new InvalidOperationException("A settings effect requires its exact prior snapshot.")));
        }

        var ownershipIndex = changes.FindIndex(change => change.LogicalPath.Replace('\\', '/')
            .EndsWith('/' + WorkspaceOwnershipDefinitions.RelativePath, StringComparison.Ordinal));
        changes.InsertRange(ownershipIndex < 0 ? changes.Count : ownershipIndex, additions);
        var recoveryByChange = recovery.ToDictionary(target => target.Change);
        return new RouteInitProspectiveEffectPlan(effects.DirectoryCreations, changes,
            changes.Where(recoveryByChange.ContainsKey).Select(change => recoveryByChange[change]), managed);
    }

    internal static ReadOnlyMemory<byte> Render(FrameworkPayloadAsset asset, FrontmatterForm form)
    {
        var rendered = WorkspacePayloadRenderer.Render(asset.Path, asset.Bytes.AsMemory(), form);
        return rendered.Bytes ?? throw new RouteInitPlanningException(
            RouteInitFindingCode.FrameworkPayloadInvalid,
            rendered.Cause ?? "A Framework restoration asset cannot be delivered in the workspace frontmatter form.",
            incomplete: false);
    }
}
