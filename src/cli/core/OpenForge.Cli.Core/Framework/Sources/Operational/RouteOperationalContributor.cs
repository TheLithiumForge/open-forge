using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal interface IRouteOperationalContributor
{
    ValueTask<RouteStatusView> ReadStatusAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken);

    ValueTask<RouteDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken);
}

internal sealed class RouteOperationalContributor(RouteObservationReader reader)
    : IRouteOperationalContributor
{
    private readonly RouteObservationReader _reader = reader;

    internal ValueTask<RouteStatusView> ReadStatusAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => _reader.ReadStatusAsync(workspace, cancellationToken);

    internal ValueTask<RouteDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => _reader.ReadDoctorAsync(workspace, cancellationToken);

    ValueTask<RouteStatusView> IRouteOperationalContributor.ReadStatusAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => ReadStatusAsync(workspace, cancellationToken);

    ValueTask<RouteDoctorView> IRouteOperationalContributor.ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => ReadDoctorAsync(workspace, cancellationToken);
}
