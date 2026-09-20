using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;

internal delegate ValueTask<RouteInspectResult> RouteInspectOperation(
    RouteInspectRequest request,
    CancellationToken cancellationToken);
