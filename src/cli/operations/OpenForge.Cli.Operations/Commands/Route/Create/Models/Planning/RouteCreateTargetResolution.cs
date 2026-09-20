using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;

internal enum RouteCreateTargetResolutionState
{
    Resolved,
    Invalid,
}

internal sealed record RouteCreateTargetResolution
{
    public required RouteCreateTargetResolutionState State { get; init; }

    public required RouteCreateTarget Target { get; init; }

    public SourceLogicalIdentity? Identity { get; init; }

    public string? Cause { get; init; }
}
