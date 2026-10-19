using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;

internal static partial class WorkspaceSettingsChangePlanner
{
    internal static PlannedFileChange? PlanLinkScopeRemoval(
        WorkspaceSettingsRead observation, WorkspaceRemovalSelection selection, IReadOnlyList<string> removedPaths)
    {
        var before = ReadBytes(observation);
        ReadOnlyMemory<byte> bytes = WorkspaceSettingsCodec.AddRemovals(before, selection) ?? before;
        bytes = WorkspaceSettingsCodec.RemoveLinkScope(bytes, removedPaths) ?? bytes;
        return PlanLinkScopeBytes(observation, bytes.ToArray());
    }

    private static ReadOnlyMemory<byte> ReadBytes(WorkspaceSettingsRead observation)
    {
        if (observation.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || observation.Snapshot is not { } snapshot)
            throw new InvalidOperationException("Link scope changes require safely observed authored settings.");
        return snapshot.Bytes.ToArray();
    }

    private static PlannedFileChange? PlanLinkScopeBytes(WorkspaceSettingsRead observation, byte[]? intended)
    {
        if (intended is null) return null;
        var snapshot = observation.Snapshot ?? throw new InvalidOperationException("Settings require their observed snapshot.");
        if (snapshot.Bytes.AsSpan().SequenceEqual(intended)) return null;
        return snapshot.Kind == FileExpectationKind.Missing
            ? PlannedFileChange.Create(snapshot.Expectation, intended)
            : PlannedFileChange.Replace(snapshot.Expectation, intended);
    }
}
