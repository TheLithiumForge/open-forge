using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

public sealed class RouteMoveRecoveryAndPartialResultTests
{
    private const string RecoveryPath = "/recovery/route-move-final.zip";

    [Theory(DisplayName = "Route Move preserves exact ordered partial effects and recovery outcomes"),
        InlineData("partial-verification-failure", (int)CliSemanticStatus.Failed),
        InlineData("retained-after-verification", (int)CliSemanticStatus.Attention),
        InlineData("unknown-recovery", (int)CliSemanticStatus.Failed),
        InlineData("recovery-unavailable-before-effects", (int)CliSemanticStatus.Incomplete),
        InlineData("recovery-conflict-before-effects", (int)CliSemanticStatus.Blocked),
        InlineData("cancellation-with-stronger-failure", (int)CliSemanticStatus.Failed)]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitBehavior")]
    public void PartialApplicationAndRecoveryRemainFullyObservable(
        string scenario,
        int expectedStatusValue)
    {
        var formation = Formation(scenario);

        var result = new RouteMoveResultBuilder().Build(formation);

        Assert.Equal((CliSemanticStatus)expectedStatusValue, result.Status);
        Assert.Equal(formation.Effects, result.Effects);
        Assert.Equal(formation.Recovery, result.Recovery);
        Assert.Equal(formation.Verification, result.Verification);
        Assert.Equal(formation.Findings, result.Findings);
        Assert.Equal(
            [
                RouteMoveTestData.DestinationPath,
                "README.md",
                RouteMoveTestData.SourcePath,
            ],
            result.Effects.Select(effect => effect.Path));
        Assert.Equal(
            [
                RouteMoveEffectKind.MovedFile,
                RouteMoveEffectKind.ReferenceSource,
                RouteMoveEffectKind.MovedFile,
            ],
            result.Effects.Select(effect => effect.Kind));
    }

    private static RouteMoveResultFormation Formation(string scenario)
    {
        ImmutableArray<RouteMoveEffect> effects = scenario switch
        {
            "partial-verification-failure" or "cancellation-with-stronger-failure" =>
            [
                Effect(
                    RouteMoveTestData.DestinationPath,
                    RouteMoveEffectAction.Create,
                    RouteMoveEffectOutcome.Verified,
                    RouteMoveEffectResidual.Retained),
                Effect(
                    "README.md",
                    RouteMoveEffectAction.Replace,
                    RouteMoveEffectOutcome.VerificationFailed,
                    RouteMoveEffectResidual.Unknown),
                Effect(
                    RouteMoveTestData.SourcePath,
                    RouteMoveEffectAction.Delete,
                    RouteMoveEffectOutcome.NotStarted,
                    RouteMoveEffectResidual.None),
            ],
            "retained-after-verification" or "unknown-recovery" =>
            [
                Effect(
                    RouteMoveTestData.DestinationPath,
                    RouteMoveEffectAction.Create,
                    RouteMoveEffectOutcome.Verified,
                    RouteMoveEffectResidual.None),
                Effect(
                    "README.md",
                    RouteMoveEffectAction.Replace,
                    RouteMoveEffectOutcome.Verified,
                    RouteMoveEffectResidual.None),
                Effect(
                    RouteMoveTestData.SourcePath,
                    RouteMoveEffectAction.Delete,
                    RouteMoveEffectOutcome.Verified,
                    RouteMoveEffectResidual.None),
            ],
            "recovery-unavailable-before-effects" or "recovery-conflict-before-effects" =>
            [
                Effect(
                    RouteMoveTestData.DestinationPath,
                    RouteMoveEffectAction.Create,
                    RouteMoveEffectOutcome.NotStarted,
                    RouteMoveEffectResidual.None),
                Effect(
                    "README.md",
                    RouteMoveEffectAction.Replace,
                    RouteMoveEffectOutcome.NotStarted,
                    RouteMoveEffectResidual.None),
                Effect(
                    RouteMoveTestData.SourcePath,
                    RouteMoveEffectAction.Delete,
                    RouteMoveEffectOutcome.NotStarted,
                    RouteMoveEffectResidual.None),
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown recovery scenario."),
        };
        var recovery = scenario switch
        {
            "partial-verification-failure" or "cancellation-with-stronger-failure" or "retained-after-verification" =>
                new RouteMoveRecovery
                {
                    State = RouteMoveRecoveryState.Retained,
                    ProtectedPaths = ["README.md", RouteMoveTestData.SourcePath],
                    ResidualPath = RecoveryPath,
                },
            "unknown-recovery" or "recovery-unavailable-before-effects" => new RouteMoveRecovery
            {
                State = RouteMoveRecoveryState.Unknown,
                ProtectedPaths = ["README.md", RouteMoveTestData.SourcePath],
                ResidualPath = RecoveryPath,
            },
            "recovery-conflict-before-effects" => new RouteMoveRecovery
            {
                State = RouteMoveRecoveryState.NotCreated,
                ProtectedPaths = ["README.md", RouteMoveTestData.SourcePath],
                ResidualPath = null,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown recovery scenario."),
        };
        var verification = scenario is "retained-after-verification" or "unknown-recovery"
            ? RouteMoveVerificationState.Verified
            : scenario is "recovery-unavailable-before-effects" or "recovery-conflict-before-effects"
                ? RouteMoveVerificationState.NotRequested
                : RouteMoveVerificationState.Failed;
        ImmutableArray<RouteMoveFinding> findings = scenario switch
        {
            "partial-verification-failure" =>
            [RouteMoveTestData.Finding(RouteMoveFindingCode.VerificationFailed, CliSemanticStatus.Failed)],
            "retained-after-verification" =>
            [RouteMoveTestData.Finding(RouteMoveFindingCode.RecoveryArtifactRetained, CliSemanticStatus.Attention)],
            "unknown-recovery" =>
            [RouteMoveTestData.Finding(RouteMoveFindingCode.RecoveryFailed, CliSemanticStatus.Failed)],
            "recovery-unavailable-before-effects" =>
            [RouteMoveTestData.Finding(RouteMoveFindingCode.RecoveryUnavailable, CliSemanticStatus.Incomplete)],
            "recovery-conflict-before-effects" =>
            [RouteMoveTestData.Finding(RouteMoveFindingCode.RecoveryConflict, CliSemanticStatus.Blocked)],
            "cancellation-with-stronger-failure" =>
            [
                RouteMoveTestData.Finding(RouteMoveFindingCode.VerificationFailed, CliSemanticStatus.Failed),
                RouteMoveTestData.Finding(RouteMoveFindingCode.Interrupted, CliSemanticStatus.Interrupted),
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown recovery scenario."),
        };

        return RouteMoveTestData.Formation() with
        {
            Effects = effects,
            Recovery = recovery,
            Verification = verification,
            Findings = findings,
        };
    }

    private static RouteMoveEffect Effect(
        string path,
        RouteMoveEffectAction action,
        RouteMoveEffectOutcome outcome,
        RouteMoveEffectResidual residual)
        => new()
        {
            Path = path,
            Kind = path == "README.md"
                ? RouteMoveEffectKind.ReferenceSource
                : RouteMoveEffectKind.MovedFile,
            Action = action,
            Before = action == RouteMoveEffectAction.Create
                ? new RouteMovePathState(RouteMovePathStateKind.Missing, null)
                : new RouteMovePathState(RouteMovePathStateKind.File, new string('a', 64)),
            Expected = action == RouteMoveEffectAction.Delete
                ? new RouteMovePathState(RouteMovePathStateKind.Missing, null)
                : new RouteMovePathState(RouteMovePathStateKind.File, new string('b', 64)),
            Outcome = outcome,
            Residual = residual,
        };
}
