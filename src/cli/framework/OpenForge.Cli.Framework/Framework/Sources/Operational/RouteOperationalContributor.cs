using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal interface IRouteOperationalContributor
{
    // Null preserves unavailable policy; absent locks supply an explicit empty sharing view.
    ValueTask<RouteStatusView> ReadStatusAsync(
        CliWorkspace workspace,
        SourceSharing? sharing,
        CancellationToken cancellationToken);

    ValueTask<RouteDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        SourceSharing? sharing,
        CancellationToken cancellationToken);
}

internal sealed class RouteOperationalContributor(RouteObservationReader reader)
    : IRouteOperationalContributor
{
    private readonly RouteObservationReader _reader = reader;

    internal ValueTask<RouteStatusView> ReadStatusAsync(
        CliWorkspace workspace,
        SourceSharing? sharing,
        CancellationToken cancellationToken)
        => _reader.ReadStatusAsync(workspace, sharing, cancellationToken);

    internal ValueTask<RouteDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        SourceSharing? sharing,
        CancellationToken cancellationToken)
        => _reader.ReadDoctorAsync(workspace, sharing, cancellationToken);

    ValueTask<RouteStatusView> IRouteOperationalContributor.ReadStatusAsync(
        CliWorkspace workspace,
        SourceSharing? sharing,
        CancellationToken cancellationToken)
        => ReadStatusAsync(workspace, sharing, cancellationToken);

    ValueTask<RouteDoctorView> IRouteOperationalContributor.ReadDoctorAsync(
        CliWorkspace workspace,
        SourceSharing? sharing,
        CancellationToken cancellationToken)
        => ReadDoctorAsync(workspace, sharing, cancellationToken);
}
