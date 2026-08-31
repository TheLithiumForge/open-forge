using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitPlanningException(
    RouteInitFindingCode code,
    string message,
    bool incomplete) : Exception(message)
{
    internal RouteInitFindingCode Code { get; } = code;

    internal bool Incomplete { get; } = incomplete;
}
