using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup;

public sealed class CleanupResultContractTests
{
    [Fact(DisplayName = "Cleanup planned effect conditions retain only delete or preserve plan pairs"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void PlannedEffectConditionsAreCoherent()
    {
        var delete = CleanupEffectCondition.Create(
            CleanupEffectOutcome.Planned,
            CleanupEffectResidual.None);
        var preserve = CleanupEffectCondition.Create(
            CleanupEffectOutcome.NotStarted,
            CleanupEffectResidual.Retained);

        Assert.Equal(CleanupEffectOutcome.Planned, delete.Outcome);
        Assert.Equal(CleanupEffectResidual.None, delete.Residual);
        Assert.Equal(CleanupEffectOutcome.NotStarted, preserve.Outcome);
        Assert.Equal(CleanupEffectResidual.Retained, preserve.Residual);

        Assert.Throws<ArgumentException>(() => CleanupEffectCondition.Create(
            CleanupEffectOutcome.Verified,
            CleanupEffectResidual.None));
        Assert.Throws<ArgumentException>(() => CleanupEffectCondition.Create(
            CleanupEffectOutcome.Planned,
            CleanupEffectResidual.Retained));
        Assert.Throws<ArgumentException>(() => CleanupEffectCondition.Create(
            CleanupEffectOutcome.NotStarted,
            CleanupEffectResidual.Unknown));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupEffectCondition.Create(
            (CleanupEffectOutcome)int.MaxValue,
            CleanupEffectResidual.None));
    }

    [Fact(DisplayName = "Cleanup actual effect outcomes retain monotonic deletion and preserved residual states"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void ActualEffectOutcomesAreMonotonic()
    {
        var action = CleanupPlanAction.Delete;
        var accepted = new[]
        {
            (CleanupEffectOutcome.Planned, CleanupEffectResidual.None),
            (CleanupEffectOutcome.NotStarted, CleanupEffectResidual.Retained),
            (CleanupEffectOutcome.Verified, CleanupEffectResidual.None),
            (CleanupEffectOutcome.VerificationFailed, CleanupEffectResidual.Retained),
            (CleanupEffectOutcome.VerificationFailed, CleanupEffectResidual.Unknown),
            (CleanupEffectOutcome.CompletionUnknown, CleanupEffectResidual.Unknown),
        };

        foreach (var (outcome, residual) in accepted)
        {
            CleanupEffectFactsValidation.ValidateActual(action, outcome, residual);
        }

        CleanupEffectFactsValidation.ValidateActual(
            CleanupPlanAction.Preserve,
            CleanupEffectOutcome.NotStarted,
            CleanupEffectResidual.Retained);
        Assert.Throws<ArgumentException>(() => CleanupEffectFactsValidation.ValidateActual(
            action,
            CleanupEffectOutcome.Verified,
            CleanupEffectResidual.Retained));
        Assert.Throws<ArgumentException>(() => CleanupEffectFactsValidation.ValidateActual(
            action,
            CleanupEffectOutcome.CompletionUnknown,
            CleanupEffectResidual.Retained));
        Assert.Throws<ArgumentException>(() => CleanupEffectFactsValidation.ValidateActual(
            CleanupPlanAction.Preserve,
            CleanupEffectOutcome.Verified,
            CleanupEffectResidual.None));
    }

    [Fact(DisplayName = "Cleanup effects and residuals preserve plan identity and reject empty residual evidence"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void EffectsAndResidualsPreservePlanIdentity()
    {
        var plan = CleanupTestData.Plan();
        var entry = Assert.Single(plan.Entries);
        var effect = CleanupTestData.Effect(entry);
        var retainedEffect = CleanupTestData.Effect(
            entry,
            CleanupEffectOutcome.VerificationFailed,
            CleanupEffectResidual.Retained,
            "The synthetic deletion verification failed.");
        var residual = CleanupTestData.Residual(retainedEffect);

        Assert.Same(entry, effect.PlanEntry);
        Assert.Equal(entry.Path, effect.Path);
        Assert.Equal(CleanupEffectOutcome.Verified, effect.Outcome);
        Assert.Equal(CleanupEffectResidual.None, effect.Residual);
        Assert.Same(retainedEffect, residual.Effect);
        Assert.Same(entry, residual.PlanEntry);
        Assert.Equal(CleanupEffectResidual.Retained, residual.Residual);

        Assert.Throws<ArgumentException>(() => CleanupResidual.Create(effect));
        Assert.Throws<ArgumentNullException>(() => CleanupEffect.Create(
            null!,
            CleanupEffectOutcome.Verified,
            CleanupEffectResidual.None));
    }

    [Fact(DisplayName = "Cleanup result facts keep effects and residuals ordered and attached to one plan"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void ResultFactsKeepOrderedTypedGraph()
    {
        var workspace = CleanupTestData.Workspace("result-facts");
        var first = CleanupTestData.Candidate(
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-a.zip"));
        var second = CleanupTestData.Candidate(
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-b.zip"),
            operationId: Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
        var plan = CleanupTestData.Plan(
            CleanupTestData.Request(workspace),
            CleanupTestData.Catalogue(CleanupCatalogueCoverage.Complete, first, second));
        var firstEffect = CleanupTestData.Effect(plan.Entries[0]);
        var secondEffect = CleanupTestData.Effect(
            plan.Entries[1],
            CleanupEffectOutcome.VerificationFailed,
            CleanupEffectResidual.Unknown,
            "The second deletion result could not be observed.");
        var residual = CleanupTestData.Residual(
            secondEffect,
            "The second candidate remains unresolved.");
        var finding = CleanupFinding.Create(
            CleanupFindingCode.VerificationFailed,
            "The second deletion result could not be observed.",
            secondEffect.Path);
        var facts = CleanupTestData.Facts(
            plan,
            [firstEffect, secondEffect],
            [residual],
            [finding]);

        Assert.Same(plan, facts.Plan);
        Assert.Same(plan.Catalogue, facts.Catalogue);
        Assert.Equal(CleanupMode.Apply, facts.Mode);
        Assert.Equal([firstEffect, secondEffect], facts.Effects);
        Assert.Equal([residual], facts.Residuals);
        Assert.Same(finding, Assert.Single(facts.Findings));
        Assert.Equal(CleanupVerificationState.Verified, facts.Verification.State);
        Assert.Equal(CleanupCatalogueComparisonState.Matched, facts.Revalidation.State);
    }

    [Fact(DisplayName = "Cleanup result facts reject duplicate, out-of-order, foreign, and default effect collections"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void ResultFactsRejectInvalidEffectCollections()
    {
        var workspace = CleanupTestData.Workspace("result-validation");
        var first = CleanupTestData.Candidate(
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-a.zip"));
        var second = CleanupTestData.Candidate(
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-b.zip"),
            operationId: Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
        var plan = CleanupTestData.Plan(
            CleanupTestData.Request(workspace),
            CleanupTestData.Catalogue(CleanupCatalogueCoverage.Complete, first, second));
        var firstEffect = CleanupTestData.Effect(plan.Entries[0]);
        var secondEffect = CleanupTestData.Effect(plan.Entries[1]);

        Assert.Throws<ArgumentException>(() => CleanupTestData.Facts(
            plan,
            [firstEffect, firstEffect],
            [],
            []));
        Assert.Throws<ArgumentException>(() => CleanupTestData.Facts(
            plan,
            [secondEffect, firstEffect],
            [],
            []));

        var foreignPlan = CleanupTestData.Plan(
            CleanupTestData.Request(CleanupTestData.Workspace("foreign-plan")));
        var foreignEffect = CleanupTestData.Effect(foreignPlan.Entries[0]);
        Assert.Throws<ArgumentException>(() => CleanupTestData.Facts(
            plan,
            [foreignEffect],
            [],
            []));

        Assert.Throws<ArgumentException>(() => CleanupResultFacts.Create(
            plan,
            new CleanupPreflight { State = CleanupPreflightState.Complete },
            new CleanupLease { State = CleanupLeaseState.Acquired },
            new CleanupCatalogueComparison { State = CleanupCatalogueComparisonState.Matched },
            default,
            [],
            new CleanupVerification { State = CleanupVerificationState.Verified },
            []));
    }

    [Fact(DisplayName = "Cleanup findings preserve derived semantic status and require bounded subjects and causes"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void FindingsPreserveStatusAndCauseBoundary()
    {
        var finding = CleanupFinding.Create(
            CleanupFindingCode.RecoveryFinalMalformed,
            "The exact-name final is malformed.",
            "/tmp/cleanup-recovery/operation.zip");

        Assert.Equal(CleanupFindingCode.RecoveryFinalMalformed, finding.Code);
        Assert.Equal(CliSemanticStatus.Blocked, finding.Status);
        Assert.Equal("/tmp/cleanup-recovery/operation.zip", finding.Subject);
        Assert.Equal("The exact-name final is malformed.", finding.Cause);

        Assert.Throws<ArgumentException>(() => CleanupFinding.Create(
            CleanupFindingCode.InvalidInput,
            " "));
        Assert.Throws<ArgumentException>(() => CleanupFinding.Create(
            CleanupFindingCode.InvalidInput,
            "The input is invalid.",
            string.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupFinding.Create(
            (CleanupFindingCode)int.MaxValue,
            "The finding code is invalid."));
    }

    [Fact(DisplayName = "Cleanup result retains one typed fact graph and optional next action"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void ResultRetainsTypedGraphAndNextAction()
    {
        var facts = CleanupTestData.Facts();
        var request = facts.Plan.Request;
        Assert.NotNull(request);
        var next = new CliNextAction(
            CleanupDefinitions.CleanupCommandLine,
            "Rerun the same Cleanup request.");
        var result = CleanupTestData.Result(
            facts,
            CliSemanticStatus.Blocked,
            request.Workspace,
            next);

        Assert.Equal(CleanupDefinitions.CommandIdentity, result.Command);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Same(request.Workspace, result.Workspace);
        Assert.Same(next, result.Next);
        Assert.Same(facts, result.Facts);
        Assert.Same(facts.Catalogue, result.Catalogue);
        Assert.Same(facts.Plan, result.Plan);
        Assert.Same(facts.Preflight, result.Preflight);
        Assert.Same(facts.Lease, result.Lease);
        Assert.Same(facts.Revalidation, result.Revalidation);
        Assert.Equal(facts.Effects, result.Effects);
        Assert.Equal(facts.Residuals, result.Residuals);
        Assert.Same(facts.Verification, result.Verification);
        Assert.Equal(facts.Findings, result.Findings);
    }
}
