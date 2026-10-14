using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Retry;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Result;

internal static class RouteRemoveRetryCommand
{
    internal static string From(RouteRemoveRequest request)
    {
        var arguments = new List<string> { "route", "remove", request.SourceReference };
        if (request.Automatic)
        {
            arguments.Add("--automatic");
        }
        return request.RequestCommand ?? RouteRetryCommand.Build(arguments, request.Workspace, request.IsDryRun);
    }
}
