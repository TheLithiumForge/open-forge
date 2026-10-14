using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Retry;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Result;

internal static class RouteMoveRetryCommand
{
    internal static string From(RouteMoveRequest request)
        => request.RequestCommand ?? RouteRetryCommand.Build(["route", "move", request.SourceReference, request.DestinationTarget], request.Workspace, request.IsDryRun);
}
