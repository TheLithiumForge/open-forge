using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal interface IRouteOperationalContributor
{
    // Null preserves unavailable policy; absent locks supply an explicit empty sharing view.
    ValueTask<RouteStatusView> ReadStatusAsync(
        WorkspaceLayoutRead observation,
        SourceSharing? sharing,
        CancellationToken cancellationToken);

    ValueTask<RouteDoctorView> ReadDoctorAsync(
        WorkspaceLayoutRead observation,
        SourceSharing? sharing,
        CancellationToken cancellationToken);
}

internal sealed class RouteOperationalContributor(RouteObservationReader reader)
    : IRouteOperationalContributor
{
    private readonly RouteObservationReader _reader = reader;

    internal async ValueTask<RouteDoctorView> ReadDoctorAsync(CliWorkspace workspace, SourceSharing? sharing, CancellationToken cancellationToken)
    {
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        return await ReadDoctorAsync(observation, sharing, cancellationToken).ConfigureAwait(false);
    }

    internal async ValueTask<RouteStatusView> ReadStatusAsync(CliWorkspace workspace, SourceSharing? sharing, CancellationToken cancellationToken)
    {
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        return await ReadStatusAsync(observation, sharing, cancellationToken).ConfigureAwait(false);
    }

    internal ValueTask<RouteStatusView> ReadStatusAsync(
        WorkspaceLayoutRead observation,
        SourceSharing? sharing,
        CancellationToken cancellationToken)
        => _reader.ReadStatusAsync(observation, sharing, cancellationToken);

    internal ValueTask<RouteDoctorView> ReadDoctorAsync(
        WorkspaceLayoutRead observation,
        SourceSharing? sharing,
        CancellationToken cancellationToken)
        => _reader.ReadDoctorAsync(observation, sharing, cancellationToken);

    ValueTask<RouteStatusView> IRouteOperationalContributor.ReadStatusAsync(
        WorkspaceLayoutRead observation,
        SourceSharing? sharing,
        CancellationToken cancellationToken)
        => ReadStatusAsync(observation, sharing, cancellationToken);

    ValueTask<RouteDoctorView> IRouteOperationalContributor.ReadDoctorAsync(
        WorkspaceLayoutRead observation,
        SourceSharing? sharing,
        CancellationToken cancellationToken)
        => ReadDoctorAsync(observation, sharing, cancellationToken);
}
