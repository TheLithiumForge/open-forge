using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Presentation.Route.Shared.Inclusion.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Inclusion;

public sealed class RouteInclusionWireVocabularyTests
{
    [Fact(DisplayName = "Include and Exclude map every effect state and reject undefined values"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Unit")]
    public void EffectStatesAreClosed()
    {
        var expected = new Dictionary<RouteInclusionEffectState, CliEffectOutcome>
        {
            [RouteInclusionEffectState.Planned] = CliEffectOutcome.Planned,
            [RouteInclusionEffectState.NotStarted] = CliEffectOutcome.NotStarted,
            [RouteInclusionEffectState.Verified] = CliEffectOutcome.Done,
            [RouteInclusionEffectState.Unknown] = CliEffectOutcome.Unknown,
        };
        Assert.Equal(Enum.GetValues<RouteInclusionEffectState>(), expected.Keys.Order());
        foreach (var (state, outcome) in expected)
            Assert.Equal(outcome, RouteInclusionWireVocabulary.Effect(state));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInclusionWireVocabulary.Effect((RouteInclusionEffectState)int.MaxValue));
    }

    [Fact(DisplayName = "Include and Exclude finding codes keep their stable wire names"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Unit")]
    public void FindingCodesAreClosed()
    {
        var expected = new Dictionary<RouteInclusionFindingCode, string>
        {
            [RouteInclusionFindingCode.InvalidInput] = "invalid-input",
            [RouteInclusionFindingCode.WorkspaceUnsafe] = "workspace-unsafe",
            [RouteInclusionFindingCode.Interrupted] = "interrupted",
            [RouteInclusionFindingCode.WorkspaceLockUnavailable] = "workspace-lock-unavailable",
            [RouteInclusionFindingCode.TargetChanged] = "target-changed",
            [RouteInclusionFindingCode.RecoveryUnavailable] = "recovery-unavailable",
            [RouteInclusionFindingCode.RecoveryConflict] = "recovery-conflict",
            [RouteInclusionFindingCode.WriteFailed] = "write-failed",
            [RouteInclusionFindingCode.VerificationFailed] = "verification-failed",
            [RouteInclusionFindingCode.RecoveryFailed] = "recovery-failed",
            [RouteInclusionFindingCode.RecoveryArtifactRetained] = "recovery-artifact-retained",
        };
        Assert.Equal(Enum.GetValues<RouteInclusionFindingCode>(), expected.Keys.Order());
        foreach (var (code, name) in expected)
            Assert.Equal(name, RouteInclusionWireVocabulary.Finding(code));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInclusionWireVocabulary.Finding((RouteInclusionFindingCode)int.MaxValue));
    }

    [Fact(DisplayName = "Include and Exclude recovery states keep their shared dispositions"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Unit")]
    public void RecoveryStatesAreClosed()
    {
        var expected = new Dictionary<RouteInclusionRecoveryState, CliRecoveryDisposition>
        {
            [RouteInclusionRecoveryState.NotRequired] = CliRecoveryDisposition.NotRequired,
            [RouteInclusionRecoveryState.NotCreated] = CliRecoveryDisposition.NotRequired,
            [RouteInclusionRecoveryState.Removed] = CliRecoveryDisposition.Removed,
            [RouteInclusionRecoveryState.Retained] = CliRecoveryDisposition.Retained,
            [RouteInclusionRecoveryState.Unknown] = CliRecoveryDisposition.Unknown,
        };
        Assert.Equal(Enum.GetValues<RouteInclusionRecoveryState>(), expected.Keys.Order());
        foreach (var (state, disposition) in expected)
            Assert.Equal(disposition, RouteInclusionWireVocabulary.Recovery(state));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInclusionWireVocabulary.Recovery((RouteInclusionRecoveryState)int.MaxValue));
    }
}
