using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.Framework.Workspace.Operational;

internal interface IWorkspaceEntryOperationalContributor
{
    ValueTask<WorkspaceEntryStatusView> ReadStatusAsync(
        CliWorkspace workspace, WorkspaceLayoutRead observation,
        CancellationToken cancellationToken);

    ValueTask<WorkspaceEntryDoctorView> ReadDoctorAsync(
        CliWorkspace workspace, WorkspaceLayoutRead observation,
        CancellationToken cancellationToken);
}

internal sealed class WorkspaceEntryOperationalContributor(
    WorkspacePathObserver pathObserver) : IWorkspaceEntryOperationalContributor
{
    internal async ValueTask<WorkspaceEntryDoctorView> ReadDoctorAsync(CliWorkspace workspace, CancellationToken cancellationToken)
    {
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        return await ReadDoctorAsync(workspace, observation, cancellationToken).ConfigureAwait(false);
    }

    internal async ValueTask<WorkspaceEntryStatusView> ReadStatusAsync(CliWorkspace workspace, CancellationToken cancellationToken)
    {
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        return await ReadStatusAsync(workspace, observation, cancellationToken).ConfigureAwait(false);
    }

    internal ValueTask<WorkspaceEntryStatusView> ReadStatusAsync(CliWorkspace workspace, WorkspaceLayoutRead observation, CancellationToken token)
        => observation.Layout is { } layout ? ReadStatusAsync(layout, token)
            : ValueTask.FromResult(new WorkspaceEntryStatusView(OperationalViewState.Incomplete, OperationalInstallationState.Incomplete,
                SourceLogicalPath.WorkspaceEntryPath, null));

    internal ValueTask<WorkspaceEntryDoctorView> ReadDoctorAsync(CliWorkspace workspace, WorkspaceLayoutRead observation, CancellationToken token)
        => observation.Layout is { } layout ? ReadDoctorAsync(layout, token)
            : ValueTask.FromResult(new WorkspaceEntryDoctorView(
                new(OperationalViewState.Incomplete, OperationalInstallationState.Incomplete, SourceLogicalPath.WorkspaceEntryPath, null),
                new(pathObserver.ObserveRoot(workspace), null, pathObserver.Observe(workspace, SourceLogicalPath.WorkspaceEntryPath), null))
            { UnavailableCause = observation.Issues.FirstOrDefault()?.Cause });

    internal ValueTask<WorkspaceEntryStatusView> ReadStatusAsync(
        WorkspaceLayout layout,
        CancellationToken cancellationToken)
    {
        var observation = Read(layout, cancellationToken);
        return ValueTask.FromResult(new WorkspaceEntryStatusView(
            observation.State,
            observation.Installation,
            observation.EntryPath,
            observation.LoaderPath));
    }

    internal ValueTask<WorkspaceEntryDoctorView> ReadDoctorAsync(
        WorkspaceLayout layout,
        CancellationToken cancellationToken)
    {
        var root = pathObserver.ObserveRoot(layout.Workspace);
        var agents = pathObserver.Observe(layout.Workspace, layout.MainRoot);
        var entry = pathObserver.Observe(layout.Workspace, SourceLogicalPath.WorkspaceEntryPath);
        var loader = pathObserver.Observe(layout.Workspace, layout.LoaderPath);
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
        CliWorkspace workspace, WorkspaceLayoutRead observation,
        CancellationToken cancellationToken)
        => ReadStatusAsync(workspace, observation, cancellationToken);

    ValueTask<WorkspaceEntryDoctorView> IWorkspaceEntryOperationalContributor.ReadDoctorAsync(
        CliWorkspace workspace, WorkspaceLayoutRead observation,
        CancellationToken cancellationToken)
        => ReadDoctorAsync(workspace, observation, cancellationToken);

    private WorkspaceEntryObservation Read(
        WorkspaceLayout layout,
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
            pathObserver.Observe(layout.Workspace, layout.LoaderPath));
        if (loader == WorkspaceObservedPathState.Missing)
        {
            return new WorkspaceEntryObservation(
                OperationalViewState.Complete,
                OperationalInstallationState.Uninstalled,
                null,
                null);
        }

        var entry = WorkspacePathObserver.ReadFileState(
            pathObserver.Observe(layout.Workspace, SourceLogicalPath.WorkspaceEntryPath));
        if (loader == WorkspaceObservedPathState.Present
            && entry == WorkspaceObservedPathState.Present)
        {
            return new WorkspaceEntryObservation(
                OperationalViewState.Complete,
                OperationalInstallationState.Installed,
                SourceLogicalPath.WorkspaceEntryPath,
                layout.LoaderPath);
        }

        var blocked = loader == WorkspaceObservedPathState.Blocked
            || entry == WorkspaceObservedPathState.Blocked;
        return new WorkspaceEntryObservation(
            blocked ? OperationalViewState.Blocked : OperationalViewState.Incomplete,
            blocked ? OperationalInstallationState.Blocked : OperationalInstallationState.Incomplete,
            entry == WorkspaceObservedPathState.Present ? SourceLogicalPath.WorkspaceEntryPath : null,
            loader == WorkspaceObservedPathState.Present ? layout.LoaderPath : null);
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
                loader.Path);
        }

        var blocked = agentsState == WorkspaceObservedPathState.Blocked
            || entryState == WorkspaceObservedPathState.Blocked
            || loaderState == WorkspaceObservedPathState.Blocked;
        return new WorkspaceEntryObservation(
            blocked ? OperationalViewState.Blocked : OperationalViewState.Incomplete,
            blocked ? OperationalInstallationState.Blocked : OperationalInstallationState.Incomplete,
            entryState == WorkspaceObservedPathState.Present ? SourceLogicalPath.WorkspaceEntryPath : null,
            loaderState == WorkspaceObservedPathState.Present ? loader.Path : null);
    }

    private sealed record WorkspaceEntryObservation(
        OperationalViewState State,
        OperationalInstallationState Installation,
        string? EntryPath,
        string? LoaderPath);
}
