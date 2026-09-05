using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.Framework.Workspace.Operational;

internal interface IWorkspaceEntryOperationalContributor
{
    ValueTask<WorkspaceEntryStatusView> ReadStatusAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken);

    ValueTask<WorkspaceEntryDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken);
}

internal sealed class WorkspaceEntryOperationalContributor(
    PhysicalPathResolver physicalPathResolver) : IWorkspaceEntryOperationalContributor
{
    internal ValueTask<WorkspaceEntryStatusView> ReadStatusAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var observation = Read(workspace, cancellationToken);
        return ValueTask.FromResult(new WorkspaceEntryStatusView(
            observation.State,
            observation.Installation,
            observation.EntryPath,
            observation.LoaderPath));
    }

    internal ValueTask<WorkspaceEntryDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var observation = Read(workspace, cancellationToken);
        return ValueTask.FromResult(new WorkspaceEntryDoctorView(
            observation.State,
            observation.Installation,
            observation.EntryPath,
            observation.LoaderPath));
    }

    ValueTask<WorkspaceEntryStatusView> IWorkspaceEntryOperationalContributor.ReadStatusAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => ReadStatusAsync(workspace, cancellationToken);

    ValueTask<WorkspaceEntryDoctorView> IWorkspaceEntryOperationalContributor.ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => ReadDoctorAsync(workspace, cancellationToken);

    private WorkspaceEntryObservation Read(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return new WorkspaceEntryObservation(
                OperationalViewState.Interrupted,
                OperationalInstallationState.Incomplete,
                null,
                null);
        }

        var loader = ReadPath(workspace, SourceLogicalPath.LoaderPath);
        if (loader == WorkspaceEntryPathState.Missing)
        {
            return new WorkspaceEntryObservation(
                OperationalViewState.Complete,
                OperationalInstallationState.Uninstalled,
                null,
                null);
        }

        var entry = ReadPath(workspace, SourceLogicalPath.WorkspaceEntryPath);
        if (loader == WorkspaceEntryPathState.Present
            && entry == WorkspaceEntryPathState.Present)
        {
            return new WorkspaceEntryObservation(
                OperationalViewState.Complete,
                OperationalInstallationState.Installed,
                SourceLogicalPath.WorkspaceEntryPath,
                SourceLogicalPath.LoaderPath);
        }

        var blocked = loader == WorkspaceEntryPathState.Blocked
            || entry == WorkspaceEntryPathState.Blocked;
        return new WorkspaceEntryObservation(
            blocked ? OperationalViewState.Blocked : OperationalViewState.Incomplete,
            blocked ? OperationalInstallationState.Blocked : OperationalInstallationState.Incomplete,
            entry == WorkspaceEntryPathState.Present ? SourceLogicalPath.WorkspaceEntryPath : null,
            loader == WorkspaceEntryPathState.Present ? SourceLogicalPath.LoaderPath : null);
    }

    private WorkspaceEntryPathState ReadPath(CliWorkspace workspace, string relativePath)
    {
        var lexicalPath = Path.Combine(
            workspace.LexicalRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        var resolution = physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            lexicalPath);
        if (resolution.State != PhysicalPathState.Contained)
        {
            return resolution.State switch
            {
                PhysicalPathState.Missing => WorkspaceEntryPathState.Missing,
                PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure
                    => WorkspaceEntryPathState.Incomplete,
                PhysicalPathState.Dangling
                    or PhysicalPathState.External
                    or PhysicalPathState.Cycle
                    or PhysicalPathState.Invalid
                    or PhysicalPathState.Unsupported => WorkspaceEntryPathState.Blocked,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(resolution),
                    resolution.State,
                    "The workspace-entry physical path state is not defined."),
            };
        }

        var component = LinkTargetReader.Read(resolution.GetContainedPhysicalPath());
        return component.State switch
        {
            PathComponentState.Ordinary when component.Attributes is { } attributes
                && (attributes & FileAttributes.Directory) == 0 => WorkspaceEntryPathState.Present,
            PathComponentState.Missing => WorkspaceEntryPathState.Missing,
            PathComponentState.Inaccessible or PathComponentState.InputOutputFailure
                => WorkspaceEntryPathState.Incomplete,
            PathComponentState.Ordinary
                or PathComponentState.Link
                or PathComponentState.Unsupported => WorkspaceEntryPathState.Blocked,
            _ => throw new ArgumentOutOfRangeException(
                nameof(component),
                component.State,
                "The workspace-entry component state is not defined."),
        };
    }

    private enum WorkspaceEntryPathState
    {
        Present,
        Missing,
        Incomplete,
        Blocked,
    }

    private sealed record WorkspaceEntryObservation(
        OperationalViewState State,
        OperationalInstallationState Installation,
        string? EntryPath,
        string? LoaderPath);
}
