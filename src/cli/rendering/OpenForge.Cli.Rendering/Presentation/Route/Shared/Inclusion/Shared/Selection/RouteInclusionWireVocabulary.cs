using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Shared.Inclusion.Shared.Selection;

internal static class RouteInclusionWireVocabulary
{
    internal static string FindingCode(string prefix, RouteInclusionFindingCode code)
        => $"{prefix}.{Finding(code)}";

    internal static CliEffectOutcome Effect(RouteInclusionEffectState state) => state switch
    {
        RouteInclusionEffectState.Planned => CliEffectOutcome.Planned,
        RouteInclusionEffectState.Verified => CliEffectOutcome.Done,
        RouteInclusionEffectState.NotStarted => CliEffectOutcome.NotStarted,
        RouteInclusionEffectState.Unknown => CliEffectOutcome.Unknown,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The effect state is not defined."),
    };

    internal static string Finding(RouteInclusionFindingCode code) => code switch
    {
        RouteInclusionFindingCode.InvalidInput => "invalid-input",
        RouteInclusionFindingCode.WorkspaceUnsafe => "workspace-unsafe",
        RouteInclusionFindingCode.Interrupted => "interrupted",
        RouteInclusionFindingCode.WorkspaceLockUnavailable => "workspace-lock-unavailable",
        RouteInclusionFindingCode.TargetChanged => "target-changed",
        RouteInclusionFindingCode.RecoveryUnavailable => "recovery-unavailable",
        RouteInclusionFindingCode.RecoveryConflict => "recovery-conflict",
        RouteInclusionFindingCode.WriteFailed => "write-failed",
        RouteInclusionFindingCode.VerificationFailed => "verification-failed",
        RouteInclusionFindingCode.RecoveryFailed => "recovery-failed",
        RouteInclusionFindingCode.RecoveryArtifactRetained => "recovery-artifact-retained",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The finding code is not defined."),
    };

    internal static CliRecoveryDisposition Recovery(RouteInclusionRecoveryState state) => state switch
    {
        RouteInclusionRecoveryState.NotRequired or RouteInclusionRecoveryState.NotCreated => CliRecoveryDisposition.NotRequired,
        RouteInclusionRecoveryState.Removed => CliRecoveryDisposition.Removed,
        RouteInclusionRecoveryState.Retained => CliRecoveryDisposition.Retained,
        RouteInclusionRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The recovery state is not defined."),
    };
}
