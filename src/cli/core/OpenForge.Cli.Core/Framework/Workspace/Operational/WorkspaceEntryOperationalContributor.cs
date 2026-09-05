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
    WorkspacePathObserver pathObserver) : IWorkspaceEntryOperationalContributor
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
        var root = pathObserver.ObserveRoot(workspace);
        var agents = pathObserver.Observe(workspace, SourceLogicalPath.AgentsRoot);
        var entry = pathObserver.Observe(workspace, SourceLogicalPath.WorkspaceEntryPath);
        var loader = pathObserver.Observe(workspace, SourceLogicalPath.LoaderPath);
        var observation = ReadDoctorObservation(
            agents,
            entry,
            loader,
            cancellationToken);
        return ValueTask.FromResult(new WorkspaceEntryDoctorView(
            new WorkspaceEntryDoctorSummary(
                observation.State,
                observation.Installation,
                observation.EntryPath,
                observation.LoaderPath),
            new WorkspacePathObservationSet(root, agents, entry, loader)));
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

        var loader = WorkspacePathObserver.ReadFileState(
            pathObserver.Observe(workspace, SourceLogicalPath.LoaderPath));
        if (loader == WorkspaceObservedPathState.Missing)
        {
            return new WorkspaceEntryObservation(
                OperationalViewState.Complete,
                OperationalInstallationState.Uninstalled,
                null,
                null);
        }

        var entry = WorkspacePathObserver.ReadFileState(
            pathObserver.Observe(workspace, SourceLogicalPath.WorkspaceEntryPath));
        if (loader == WorkspaceObservedPathState.Present
            && entry == WorkspaceObservedPathState.Present)
        {
            return new WorkspaceEntryObservation(
                OperationalViewState.Complete,
                OperationalInstallationState.Installed,
                SourceLogicalPath.WorkspaceEntryPath,
                SourceLogicalPath.LoaderPath);
        }

        var blocked = loader == WorkspaceObservedPathState.Blocked
            || entry == WorkspaceObservedPathState.Blocked;
        return new WorkspaceEntryObservation(
            blocked ? OperationalViewState.Blocked : OperationalViewState.Incomplete,
            blocked ? OperationalInstallationState.Blocked : OperationalInstallationState.Incomplete,
            entry == WorkspaceObservedPathState.Present ? SourceLogicalPath.WorkspaceEntryPath : null,
            loader == WorkspaceObservedPathState.Present ? SourceLogicalPath.LoaderPath : null);
    }

    private static WorkspaceEntryObservation ReadDoctorObservation(
        WorkspacePathObservation agents,
        WorkspacePathObservation entry,
        WorkspacePathObservation loader,
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

        var agentsState = WorkspacePathObserver.ReadDirectoryState(agents);
        var entryState = WorkspacePathObserver.ReadFileState(entry);
        var loaderState = WorkspacePathObserver.ReadFileState(loader);
        if (agentsState == WorkspaceObservedPathState.Missing
            && entryState == WorkspaceObservedPathState.Missing
            && loaderState == WorkspaceObservedPathState.Missing)
        {
            return new WorkspaceEntryObservation(
                OperationalViewState.Complete,
                OperationalInstallationState.Uninstalled,
                null,
                null);
        }

        if (agentsState == WorkspaceObservedPathState.Present
            && entryState == WorkspaceObservedPathState.Present
            && loaderState == WorkspaceObservedPathState.Present)
        {
            return new WorkspaceEntryObservation(
                OperationalViewState.Complete,
                OperationalInstallationState.Installed,
                SourceLogicalPath.WorkspaceEntryPath,
                SourceLogicalPath.LoaderPath);
        }

        var blocked = agentsState == WorkspaceObservedPathState.Blocked
            || entryState == WorkspaceObservedPathState.Blocked
            || loaderState == WorkspaceObservedPathState.Blocked;
        return new WorkspaceEntryObservation(
            blocked ? OperationalViewState.Blocked : OperationalViewState.Incomplete,
            blocked ? OperationalInstallationState.Blocked : OperationalInstallationState.Incomplete,
            entryState == WorkspaceObservedPathState.Present ? SourceLogicalPath.WorkspaceEntryPath : null,
            loaderState == WorkspaceObservedPathState.Present ? SourceLogicalPath.LoaderPath : null);
    }

    private sealed record WorkspaceEntryObservation(
        OperationalViewState State,
        OperationalInstallationState Installation,
        string? EntryPath,
        string? LoaderPath);
}
