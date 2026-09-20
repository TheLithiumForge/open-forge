using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;

internal static class WorkspaceSettingsChangePlanner
{
    internal static PlannedFileChange? PlanGrant(WorkspaceSettingsRead observation, ImmutableArray<string> paths)
    {
        if (paths.IsEmpty)
        {
            return null;
        }
        if (observation.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || observation.Snapshot is not { } snapshot)
        {
            throw new InvalidOperationException("An explicit grant requires safely observed authored settings.");
        }
        ReadOnlyMemory<byte> intended = snapshot.Bytes.ToArray();
        foreach (var path in paths.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            if (WorkspaceSettingsCodec.AddAllowInstallPath(intended, path) is { } updated)
            {
                intended = updated;
            }
        }
        if (snapshot.Kind == FileExpectationKind.Missing)
        {
            return PlannedFileChange.Create(snapshot.Expectation, intended.ToArray());
        }
        return snapshot.Bytes.AsSpan().SequenceEqual(intended.Span)
            ? null
            : PlannedFileChange.Replace(snapshot.Expectation, intended.ToArray());
    }

    internal static (WorkspacePermissionAction Action, RecoveryBundleTarget? RecoveryTarget) ReadActionAndRecovery(
        WorkspaceSettingsRead? observation,
        PlannedFileChange? change)
    {
        var action = change?.Kind switch
        {
            null => WorkspacePermissionAction.None,
            PlannedFileChangeKind.Create => WorkspacePermissionAction.Create,
            PlannedFileChangeKind.Replace => WorkspacePermissionAction.Replace,
            _ => throw new InvalidOperationException("An explicit settings grant can only create or replace its authored file."),
        };
        RecoveryBundleTarget? recovery = null;
        if (change is not null && observation?.Snapshot is { } before)
        {
            recovery = change.Kind == PlannedFileChangeKind.Create
                ? RecoveryBundleTarget.CreateReversible(change, before)
                : RecoveryBundleTarget.Create(change, before);
        }
        return (action, recovery);
    }
}
