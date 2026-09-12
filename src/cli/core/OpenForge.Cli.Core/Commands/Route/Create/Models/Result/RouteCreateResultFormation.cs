using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Result;

internal sealed record RouteCreateResultFormation
{
    public CliWorkspace? Workspace { get; init; }

    public required RouteCreateMode Mode { get; init; }

    public required RouteCreateTarget Target { get; init; }

    public RouteCreateParent? Parent { get; init; }

    public required RouteCreateMetadata Metadata { get; init; }

    public RouteCreateTemplate? Template { get; init; }

    public required RouteCreatePlanFacts Plan { get; init; }

    public required ImmutableArray<RouteCreateEffect> Effects { get; init; }

    public required ImmutableArray<string> UnchangedPaths { get; init; }

    public required RouteCreateRecovery Recovery { get; init; }

    public required RouteCreateVerificationState Verification { get; init; }

    public required ImmutableArray<RouteCreateFinding> Findings { get; init; }
}
