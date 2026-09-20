using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;

internal enum RouteCreateTemplateResolutionState
{
    NotRequested,
    Resolved,
    Invalid,
    Blocked,
    Incomplete,
}

internal sealed record RouteCreateTemplateResolution
{
    public required RouteCreateTemplateResolutionState State { get; init; }

    public RouteCreateTemplate? Template { get; init; }

    public SourceLogicalSource? Source { get; init; }

    public required ImmutableArray<byte> BodyBytes { get; init; }

    public RouteCreateFinding? Finding { get; init; }
}
