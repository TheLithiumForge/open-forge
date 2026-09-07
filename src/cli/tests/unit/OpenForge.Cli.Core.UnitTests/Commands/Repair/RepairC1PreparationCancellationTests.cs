using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Commands.Repair.Shared.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairC1PreparationCancellationTests
{
    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void CancelledPreparationWithIncompleteResidualFactsFailsAndRetainsKnownPath()
    {
        var request = RepairTestData.Request(mode: RepairMode.Apply);
        var plan = RepairPlanner.Build(request, [RepairTestData.Reference()]);
        var attribution = RepairTestData.Attribution();
        var residualPath = Path.Combine(RepairTestData.WorkspaceRoot, "recovery.zip");
        var preparation = RecoveryBundlePreparationResult.Cancelled(residualPath);
        var outcome = RepairApplicationOutcomeFactory.Preparation(preparation, attribution);
        var coverage = RepairTestData.CompleteCoverage();
        var result = RepairResultBuilder.Build(new RepairResultInput
        {
            Request = request,
            Diagnosis = coverage,
            Plan = plan,
            Findings = outcome.Findings,
            InitialFindings = [],
            Preflight = outcome.Preflight,
            Application = outcome.Application,
            Verification = outcome.Verification,
            Recovery = outcome.Recovery,
            PostDiagnosis = new RepairPostDiagnosis(RepairPostDiagnosisState.Complete, coverage, []),
        });

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(RepairApplicationState.Interrupted, result.Application.State);
        Assert.Equal(0, result.Counts.AppliedEffects);
        Assert.Equal(0, result.Counts.VerifiedEffects);
        Assert.Equal(RepairRecoveryState.Incomplete, result.Recovery.State);
        Assert.Equal(RepairResidualState.Retained, result.Recovery.Residual);
        Assert.Equal(residualPath, result.Recovery.ResidualPath);
        Assert.Same(attribution, result.Recovery.Attribution);
    }
}
