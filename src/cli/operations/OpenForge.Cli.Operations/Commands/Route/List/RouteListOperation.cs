using OpenForge.Cli.Core.Commands.Route.List.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal delegate ValueTask<RouteListResult> RouteListOperation(
    RouteListRequest request,
    CancellationToken cancellationToken);
