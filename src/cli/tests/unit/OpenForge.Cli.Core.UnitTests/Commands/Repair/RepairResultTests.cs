using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairResultTests
{
    [Fact(DisplayName = "Repair result applies failed invalid blocked incomplete interrupted and attention precedence without parsing causes"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void ResultStatusPrecedenceIsTyped()
    {
        var all = RepairTestData.Result();
        Assert.Equal(CliSemanticStatus.Complete, all.Status);

        var failed = RepairTestData.Result(
            findings:
            [
                Finding(RepairFindingCode.Interrupted),
                Finding(RepairFindingCode.DiagnosisIncomplete),
                Finding(RepairFindingCode.DiagnosisBlocked),
                Finding(RepairFindingCode.OperationFailed),
            ]);
        Assert.Equal(CliSemanticStatus.Failed, failed.Status);
        Assert.Equal("open-forge repair --verbose", failed.Next!.Command);

        var blocked = RepairTestData.Result(
            findings:
            [
                Finding(RepairFindingCode.Interrupted),
                Finding(RepairFindingCode.DiagnosisIncomplete),
                Finding(RepairFindingCode.DiagnosisBlocked),
            ]);
        Assert.Equal(CliSemanticStatus.Blocked, blocked.Status);
        Assert.Equal("open-forge doctor", blocked.Next!.Command);

        var incomplete = RepairTestData.Result(
            findings:
            [
                Finding(RepairFindingCode.Interrupted),
                Finding(RepairFindingCode.DiagnosisIncomplete),
            ]);
        Assert.Equal(CliSemanticStatus.Incomplete, incomplete.Status);
        Assert.Equal("open-forge doctor", incomplete.Next!.Command);

        var interrupted = RepairTestData.Result(
            findings: [Finding(RepairFindingCode.Interrupted)]);
        Assert.Equal(CliSemanticStatus.Interrupted, interrupted.Status);
        Assert.Equal("open-forge repair", interrupted.Next!.Command);
    }

    [Fact(DisplayName = "Repair result maps every semantic finding lane to one exact next action"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void ResultNextActionsCoverEverySemanticLane()
    {
        var cases = new[]
        {
            (Result: RepairTestData.Result(), Status: CliSemanticStatus.Complete, Command: (string?)null),
            (Result: RepairTestData.Result([Finding(RepairFindingCode.InvalidInput)]), Status: CliSemanticStatus.Invalid, Command: "open-forge repair --help"),
            (Result: RepairTestData.Result([Finding(RepairFindingCode.SelectionRequired)]), Status: CliSemanticStatus.Blocked, Command: "open-forge repair --automatic"),
            (Result: RepairTestData.Result([Finding(RepairFindingCode.DiagnosisBlocked)]), Status: CliSemanticStatus.Blocked, Command: "open-forge doctor"),
            (Result: RepairTestData.Result([Finding(RepairFindingCode.DiagnosisIncomplete)]), Status: CliSemanticStatus.Incomplete, Command: "open-forge doctor"),
            (Result: RepairTestData.Result([Finding(RepairFindingCode.ManualFindingRemaining)]), Status: CliSemanticStatus.Attention, Command: "open-forge repair"),
            (Result: RepairTestData.Result([Finding(RepairFindingCode.OperationFailed)]), Status: CliSemanticStatus.Failed, Command: "open-forge repair --verbose"),
            (Result: RepairTestData.Result([Finding(RepairFindingCode.Interrupted)]), Status: CliSemanticStatus.Interrupted, Command: "open-forge repair"),
        };

        foreach (var testCase in cases)
        {
            Assert.Equal(testCase.Status, testCase.Result.Status);
            Assert.Equal(testCase.Command, testCase.Result.Next?.Command);
        }
    }

    [Fact(DisplayName = "Repair retained recovery owns cleanup next action and preserves exact residual attribution"),
        Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void RetainedRecoveryProducesAttentionAndCleanup()
    {
        var recovery = new RepairRecovery(
            RepairRecoveryState.Retained,
            RepairResidualState.Retained,
            Path.GetFullPath("/tmp/open-forge-repair/recovery.zip"),
            RepairTestData.Attribution());
        var result = RepairTestData.Result(
            facts: RepairTestData.CompleteFacts(
                recovery: recovery,
                affectedPaths: [RepairTestData.SourcePath],
                counts: new RepairCounts(
                    selectedFindings: 1,
                    unselectedFindings: 1,
                    repaired: 1,
                    remaining: 1,
                    newFindings: 0,
                    manual: 0,
                    guided: 1,
                    blocked: 0,
                    selectedEffects: 1,
                    appliedEffects: 1,
                    verifiedEffects: 1,
                    noOps: 0,
                    conflicts: 0)),
            mode: RepairMode.Apply);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal("open-forge cleanup", result.Next!.Command);
        Assert.Equal(recovery.ResidualPath, result.Recovery.ResidualPath);
        Assert.Same(recovery.Attribution, result.Recovery.Attribution);
        Assert.Equal([RepairTestData.SourcePath], result.AffectedPaths);
        Assert.Equal(1, result.Counts.Repaired);
    }

    [Fact(DisplayName = "Repair application and verification lifecycle facts derive failure and incomplete states without losing typed evidence"),
        Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void TypedLifecycleFactsDeriveStatus()
    {
        var failedApplication = RepairTestData.Result(
            facts: RepairTestData.CompleteFacts(
                application: new RepairApplication(
                    RepairApplicationState.Failed,
                    appliedEffects: 1,
                    cause: "The target write failed.")),
            mode: RepairMode.Apply);
        Assert.Equal(CliSemanticStatus.Failed, failedApplication.Status);
        Assert.Equal("open-forge repair --verbose", failedApplication.Next!.Command);
        Assert.Equal(1, failedApplication.Application.AppliedEffects);

        var selected = RepairTestData.Selected();
        var effect = RepairTestData.Effect();
        var plan = RepairTestData.Plan(
            selected,
            RepairTestData.Step(
                selected,
                effect,
                outcome: RepairStepOutcome.Planned));
        var retained = new RepairRecovery(
            RepairRecoveryState.Prepared,
            RepairResidualState.Retained,
            Path.GetFullPath("/tmp/open-forge-repair/recovery.zip"),
            RepairTestData.Attribution());
        var incomplete = RepairTestData.Result(
            facts: RepairTestData.CompleteFacts(
                selection: plan.Selection,
                plan: plan,
                preflight: new RepairPreflight(
                    RepairPreflightState.Ready,
                    cause: null,
                    RepairRecoveryState.Prepared),
                application: new RepairApplication(
                    RepairApplicationState.NotStarted,
                    appliedEffects: 0,
                    cause: null),
                verification: new RepairVerification(
                    RepairVerificationState.Planned,
                    RepairVerificationState.Planned,
                    RepairVerificationState.Planned),
                recovery: retained),
            mode: RepairMode.Apply);
        Assert.Equal(CliSemanticStatus.Incomplete, incomplete.Status);
        Assert.Equal("open-forge doctor", incomplete.Next!.Command);
        Assert.Equal(RepairRecoveryState.Prepared, incomplete.Recovery.State);
        Assert.Equal(RepairApplicationState.NotStarted, incomplete.Application.State);

        var interruptedBlocked = RepairTestData.Result(
            facts: RepairTestData.CompleteFacts(
                application: new RepairApplication(
                    RepairApplicationState.Interrupted,
                    appliedEffects: 0,
                    cause: "The repair was interrupted."),
                postDiagnosis: new RepairPostDiagnosis(
                    RepairPostDiagnosisState.Blocked,
                    RepairTestData.CompleteCoverage(),
                    [])),
            mode: RepairMode.Apply);
        Assert.Equal(CliSemanticStatus.Blocked, interruptedBlocked.Status);

        var interruptedIncomplete = RepairTestData.Result(
            facts: RepairTestData.CompleteFacts(
                application: new RepairApplication(
                    RepairApplicationState.Interrupted,
                    appliedEffects: 0,
                    cause: "The repair was interrupted."),
                postDiagnosis: new RepairPostDiagnosis(
                    RepairPostDiagnosisState.Incomplete,
                    RepairTestData.CompleteCoverage(),
                    [])),
            mode: RepairMode.Apply);
        Assert.Equal(CliSemanticStatus.Incomplete, interruptedIncomplete.Status);
    }

    [Fact(DisplayName = "Repair result orders findings and affected paths deterministically while retaining post-diagnosis evidence"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void ResultOrderingAndPostDiagnosisAreDeterministic()
    {
        var postFinding = Finding(
            RepairFindingCode.GuidedFindingRemaining,
            sourcePath: ".agents/docs/z.md");
        var first = Finding(
            RepairFindingCode.ManualFindingRemaining,
            sourcePath: ".agents/docs/b.md");
        var second = Finding(
            RepairFindingCode.ManualFindingRemaining,
            sourcePath: ".agents/docs/a.md");
        var post = new RepairPostDiagnosis(
            RepairPostDiagnosisState.Complete,
            RepairTestData.CompleteCoverage(),
            [postFinding]);
        var result = RepairTestData.Result(
            findings: [first, second],
            facts: RepairTestData.CompleteFacts(
                postDiagnosis: post,
                affectedPaths:
                [
                    ".agents/docs/z.md",
                    ".agents/docs/a.md",
                    ".agents/docs/z.md",
                ]));

        Assert.Equal(
            [
                ".agents/docs/a.md",
                ".agents/docs/b.md",
            ],
            result.Findings.Select(finding => finding.SourceCanonicalPath));
        Assert.Equal(
            [".agents/docs/a.md", ".agents/docs/z.md"],
            result.AffectedPaths);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        var remainingFinding = Assert.Single(result.PostDiagnosis.Findings);
        Assert.Equal(RepairFindingCode.GuidedFindingRemaining, remainingFinding.Code);
    }

    private static RepairFinding Finding(
        RepairFindingCode code,
        string? sourcePath = null)
        => new(
            code,
            $"typed cause for {code}",
            sourcePath,
            sourcePath is null ? null : RepairTestData.Occurrence());
}
