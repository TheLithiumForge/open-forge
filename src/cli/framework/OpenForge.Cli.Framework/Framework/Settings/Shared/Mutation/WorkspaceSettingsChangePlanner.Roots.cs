using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;

internal static partial class WorkspaceSettingsChangePlanner
{
    /// <summary>
    /// The settings change that registers <paramref name="path"/> in `roots`, or null when it is
    /// already registered. A missing settings file is created.
    /// </summary>
    internal static PlannedFileChange? PlanRootRegistration(WorkspaceSettingsRead observation, string path)
        => PlanRootBytes(observation, bytes => WorkspaceSettingsCodec.AddRoot(bytes, path));

    /// <summary>
    /// The settings change that removes <paramref name="path"/> from `roots`, or null when it was
    /// not registered.
    /// </summary>
    internal static PlannedFileChange? PlanRootDeregistration(WorkspaceSettingsRead observation, string path)
        => PlanRootBytes(observation, bytes => WorkspaceSettingsCodec.RemoveRoot(bytes, path));

    private static PlannedFileChange? PlanRootBytes(
        WorkspaceSettingsRead observation,
        Func<ReadOnlyMemory<byte>, byte[]?> change)
    {
        if (observation.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || observation.Snapshot is not { } snapshot)
        {
            throw new InvalidOperationException("Changing included roots requires safely observed authored settings.");
        }

        var intended = change(snapshot.Bytes.ToArray());
        if (intended is null)
        {
            return null;
        }

        return snapshot.Kind == FileExpectationKind.Missing
            ? PlannedFileChange.Create(snapshot.Expectation, intended)
            : PlannedFileChange.Replace(snapshot.Expectation, intended);
    }
}
