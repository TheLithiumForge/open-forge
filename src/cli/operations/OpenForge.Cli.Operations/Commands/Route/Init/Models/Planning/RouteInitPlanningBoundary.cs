using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution.Models;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;

internal sealed record RouteInitPlanningBoundary(
    RouteInitFindingCode Code,
    string Cause,
    bool Incomplete,
    RouteInitTargetFacts? Target = null,
    string? Requested = null,
    RouteInitFrameworkAlignment? Alignment = null,
    FrameworkPayload? Payload = null)
{
    internal string? FindingTarget { get; init; }
}
