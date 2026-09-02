using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

internal sealed record RouteUpdateResultFormation
{
    public CliWorkspace? Workspace { get; init; }

    public required RouteUpdateMode Mode { get; init; }

    public required RouteUpdateTarget Target { get; init; }

    public required RouteUpdatePatch Patch { get; init; }

    public RouteUpdateTemplate? Template { get; init; }

    public required RouteUpdatePlanFacts Plan { get; init; }

    public required ImmutableArray<RouteUpdateEffect> Effects { get; init; }

    public required ImmutableArray<string> UnchangedPaths { get; init; }

    public required RouteUpdateRecovery Recovery { get; init; }

    public required RouteUpdateVerificationState Verification { get; init; }

    public required ImmutableArray<RouteUpdateFinding> Findings { get; init; }
}
