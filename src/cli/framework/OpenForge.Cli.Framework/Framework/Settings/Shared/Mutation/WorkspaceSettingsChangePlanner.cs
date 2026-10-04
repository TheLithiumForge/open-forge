using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;

internal static class WorkspaceSettingsChangePlanner
{
    internal static PlannedFileChange? PlanConfiguration(
        WorkspaceSettingsRead observation,
        WorkspaceRemovalSelection additions,
        WorkspaceRemovalSelection clear)
    {
        if (observation.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || observation.Snapshot is not { } snapshot)
            throw new InvalidOperationException("Configuration requires safely observed authored settings.");
        ReadOnlyMemory<byte> bytes = snapshot.Bytes.ToArray();
        bytes = WorkspaceSettingsCodec.ClearRemovals(bytes, clear) ?? bytes;
        bytes = WorkspaceSettingsCodec.AddRemovals(bytes, additions) ?? bytes;
        if (snapshot.Bytes.AsSpan().SequenceEqual(bytes.Span)) return null;
        return snapshot.Kind == FileExpectationKind.Missing
            ? PlannedFileChange.Create(snapshot.Expectation, bytes.ToArray())
            : PlannedFileChange.Replace(snapshot.Expectation, bytes.ToArray());
    }

    internal static PlannedFileChange? PlanRestoration(WorkspaceSettingsRead observation, WorkspaceRemovalSelection selection)
    {
        if (observation.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || observation.Snapshot is not { } snapshot)
        {
            throw new InvalidOperationException("Clearing removals requires safely observed authored settings.");
        }

        var expectedKind = observation.State == WorkspaceSettingsReadState.Absent
            ? FileExpectationKind.Missing : FileExpectationKind.File;
        if (snapshot.Kind != expectedKind)
        {
            throw new InvalidOperationException("The settings snapshot does not match its observation state.");
        }

        var intended = WorkspaceSettingsCodec.ClearRemovals(snapshot.Bytes.ToArray(), selection);
        return intended is null ? null : PlannedFileChange.Replace(snapshot.Expectation, intended);
    }

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

    internal static PlannedFileChange? PlanRemovals(
        WorkspaceSettingsRead observation,
        WorkspaceRemovalSelection selection)
    {
        if (observation.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || observation.Snapshot is not { } snapshot)
        {
            throw new InvalidOperationException("Recording removals requires safely observed authored settings.");
        }

        var expectedSnapshotKind = observation.State switch
        {
            WorkspaceSettingsReadState.Absent => FileExpectationKind.Missing,
            WorkspaceSettingsReadState.Complete => FileExpectationKind.File,
            _ => throw new InvalidOperationException("Recording removals requires safely observed authored settings."),
        };
        if (snapshot.Kind != expectedSnapshotKind)
        {
            throw new InvalidOperationException("The settings snapshot does not match its observation state.");
        }

        var intended = WorkspaceSettingsCodec.AddRemovals(snapshot.Bytes.ToArray(), selection);
        if (intended is null)
        {
            return null;
        }

        return snapshot.Kind switch
        {
            FileExpectationKind.Missing => PlannedFileChange.Create(snapshot.Expectation, intended),
            FileExpectationKind.File => PlannedFileChange.Replace(snapshot.Expectation, intended),
            _ => throw new InvalidOperationException("Recording removals requires a missing or ordinary settings file."),
        };
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
