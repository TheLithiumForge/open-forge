using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.Framework.Workspace.Operational;

internal enum WorkspaceObservedPathState
{
    Present,
    Missing,
    Incomplete,
    Blocked,
}

internal sealed class WorkspacePathObserver(
    PhysicalPathResolver physicalPathResolver)
{
    internal WorkspacePathObservation ObserveRoot(CliWorkspace workspace)
        => Observe(workspace, ".", workspace.LexicalRoot);

    internal WorkspacePathObservation Observe(
        CliWorkspace workspace,
        string relativePath)
    {
        var lexicalPath = Path.Combine(
            workspace.LexicalRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        return Observe(workspace, relativePath, lexicalPath);
    }

    private WorkspacePathObservation Observe(
        CliWorkspace workspace,
        string observationPath,
        string lexicalPath)
    {
        var resolution = physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            lexicalPath);
        if (resolution.State != PhysicalPathState.Contained)
        {
            return WorkspacePathObservation.Unresolved(observationPath, resolution.State);
        }

        var component = LinkTargetReader.Read(resolution.GetContainedPhysicalPath());
        return WorkspacePathObservation.Contained(
            observationPath,
            component.State,
            component.Attributes);
    }

    internal static WorkspaceObservedPathState ReadFileState(
        WorkspacePathObservation observation)
        => ReadState(observation, expectDirectory: false);

    internal static WorkspaceObservedPathState ReadDirectoryState(
        WorkspacePathObservation observation)
        => ReadState(observation, expectDirectory: true);

    private static WorkspaceObservedPathState ReadState(
        WorkspacePathObservation observation,
        bool expectDirectory)
        => observation.Resolution switch
        {
            PhysicalPathState.Missing => WorkspaceObservedPathState.Missing,
            PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure
                => WorkspaceObservedPathState.Incomplete,
            PhysicalPathState.Dangling
                or PhysicalPathState.External
                or PhysicalPathState.Cycle
                or PhysicalPathState.Invalid
                or PhysicalPathState.Unsupported => WorkspaceObservedPathState.Blocked,
            PhysicalPathState.Contained => ReadComponent(observation, expectDirectory),
            _ => throw new ArgumentOutOfRangeException(
                nameof(observation),
                observation.Resolution,
                "The workspace path resolution state is not defined."),
        };

    private static WorkspaceObservedPathState ReadComponent(
        WorkspacePathObservation observation,
        bool expectDirectory)
        => observation.Component switch
        {
            PathComponentState.Ordinary when observation.Attributes is { } attributes
                && ((attributes & FileAttributes.Directory) != 0) == expectDirectory =>
                WorkspaceObservedPathState.Present,
            PathComponentState.Missing => WorkspaceObservedPathState.Missing,
            PathComponentState.Inaccessible or PathComponentState.InputOutputFailure =>
                WorkspaceObservedPathState.Incomplete,
            PathComponentState.Ordinary
                or PathComponentState.Link
                or PathComponentState.Unsupported => WorkspaceObservedPathState.Blocked,
            _ => throw new ArgumentOutOfRangeException(
                nameof(observation),
                observation.Component,
                "The workspace path component state is not defined."),
        };
}
