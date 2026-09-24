using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;

internal static class ExtensionRemoveExecutionPlanComposer
{
    internal static ExtensionRemoveExecutionPlan Compose(
        ExtensionRemovePlan content,
        ExtensionPermissionStage permission)
    {
        var settingsChange = ComposeSettingsChange(content, permission);
        var settingsRecoveryTarget = CreateSettingsRecoveryTarget(content, settingsChange);
        var directoryCreations = content.Effects
            .Select(effect => effect.DirectoryCreation)
            .OfType<PlannedDirectoryCreation>()
            .ToImmutableArray();
        IEnumerable<RecoveryBundleTarget> settingsTargets = settingsRecoveryTarget is { } settingsTarget
            ? [settingsTarget]
            : Array.Empty<RecoveryBundleTarget>();
        var contentTargets = content.Effects
            .Select(effect => effect.RecoveryTarget)
            .OfType<RecoveryBundleTarget>();
        IEnumerable<RecoveryBundleTarget> ownershipTargets = content.OwnershipRecoveryTarget is { } ownershipTarget
            ? [ownershipTarget]
            : Array.Empty<RecoveryBundleTarget>();
        var recoveryTargets = settingsTargets
            .Concat(contentTargets)
            .Concat(ownershipTargets)
            .ToImmutableArray();

        return new ExtensionRemoveExecutionPlan(
            Content: content,
            Permission: permission,
            SettingsChange: settingsChange,
            SettingsRecoveryTarget: settingsRecoveryTarget,
            DirectoryCreations: directoryCreations,
            RecoveryTargets: recoveryTargets);
    }

    private static PlannedFileChange? ComposeSettingsChange(
        ExtensionRemovePlan content,
        ExtensionPermissionStage permission)
    {
        var snapshot = content.SettingsObservation.Snapshot
            ?? throw new InvalidOperationException(
                "A safe Extension Remove settings observation requires an exact file snapshot.");
        var grantBytes = permission.Change?.IntendedBytes.ToArray()
            ?? content.SettingsChange?.IntendedBytes.ToArray()
            ?? snapshot.Bytes.ToArray();
        var withRemovals = WorkspaceSettingsCodec.AddRemovals(grantBytes, content.RemovalSelection);
        var intended = withRemovals ?? grantBytes;
        if (snapshot.Bytes.AsSpan().SequenceEqual(intended))
        {
            return null;
        }

        return snapshot.Kind switch
        {
            FileExpectationKind.Missing => PlannedFileChange.Create(snapshot.Expectation, intended),
            FileExpectationKind.File => PlannedFileChange.Replace(snapshot.Expectation, intended),
            _ => throw new InvalidOperationException(
                "A safe Extension Remove settings observation must be a file or proven absence."),
        };
    }

    private static RecoveryBundleTarget? CreateSettingsRecoveryTarget(
        ExtensionRemovePlan content,
        PlannedFileChange? change)
    {
        if (change is null)
        {
            return null;
        }

        var snapshot = content.SettingsObservation.Snapshot
            ?? throw new InvalidOperationException(
                "An Extension Remove settings write requires its exact prior snapshot.");
        return change.Kind == PlannedFileChangeKind.Create
            ? RecoveryBundleTarget.CreateReversible(change, snapshot)
            : RecoveryBundleTarget.Create(change, snapshot);
    }
}
