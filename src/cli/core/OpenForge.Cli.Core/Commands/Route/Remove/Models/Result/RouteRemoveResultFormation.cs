using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

internal sealed record RouteRemoveResultFormation
{
    public CliWorkspace? Workspace { get; init; }

    public required RouteRemoveMode Mode { get; init; }

    public required RouteRemoveSource Source { get; init; }

    public required RouteRemoveSubject Subject { get; init; }

    public required RouteRemoveOwnership Ownership { get; init; }

    public required RouteRemoveReferences References { get; init; }

    public required RouteRemoveGeneratedNavigation GeneratedNavigation { get; init; }

    public required RouteRemovePlanFacts Plan { get; init; }

    public ImmutableArray<RouteRemoveEffect> Effects { get; init; } = [];

    public ImmutableArray<string> UnchangedPaths { get; init; } = [];

    public required RouteRemoveRecovery Recovery { get; init; }

    public required RouteRemoveVerificationState Verification { get; init; }

    public ImmutableArray<RouteRemoveFinding> Findings { get; init; } = [];
}
