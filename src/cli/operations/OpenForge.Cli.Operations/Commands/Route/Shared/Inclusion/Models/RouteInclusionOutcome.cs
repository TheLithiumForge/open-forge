using OpenForge.Cli.Core.Commands.Shared.Models.Mutation;
using OpenForge.Cli.Core.Shell.Definitions;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;

internal enum RouteInclusionEffectState { Planned, NotStarted, Verified, Unknown }
internal enum RouteInclusionEffectKind { LoaderEntries, Settings, Catalogue }

internal sealed record RouteInclusionEffect(string Path, string? Before, string After, RouteInclusionEffectState State, RouteInclusionEffectKind Kind);

internal sealed record RouteInclusionOutcome
{
    internal WorkspaceLockFailureInfo? LockFailure { get; init; }
    public CliSemanticStatus Status { get; init; } = CliSemanticStatus.Complete;
    public string? Path { get; init; }
    public RouteInclusionTargetKind? TargetKind { get; init; }
    public string? Catalogue { get; init; }
    public string? Before { get; init; }
    public string? After { get; init; }
    public bool RegisteredBefore { get; init; }
    public bool RegisteredAfter { get; init; }
    public bool NoOp { get; init; }
    public bool Verified { get; init; }
    public ImmutableArray<RouteInclusionEffect> Effects { get; init; } = [];
    public string? Cause { get; init; }
    public RouteInclusionFindingCode? Code { get; init; }
    public RouteInclusionRecoveryState Recovery { get; init; } = RouteInclusionRecoveryState.NotRequired;
    public string? RecoveryPath { get; init; }
}
