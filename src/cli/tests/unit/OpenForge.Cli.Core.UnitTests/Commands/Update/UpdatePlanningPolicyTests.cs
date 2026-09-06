using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Commands.Update;

public sealed class UpdatePlanningPolicyTests
{
    [Fact(DisplayName = "Update planning returns no-op for baseline-equivalent unchanged target"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanReturnsNoOpForBaselineEquivalentUnchangedTarget()
    {
        var plan = Plan([Unchanged()], force: false, prune: false);

        Assert.Equal(UpdatePlanningDisposition.NoOp, Assert.Single(plan.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update planning replaces baseline-equivalent current content when source changed"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanReplacesBaselineEquivalentCurrentWhenIntendedSourceChanged()
    {
        var plan = Plan([SourceChanged()], force: false, prune: false);

        Assert.Equal(UpdatePlanningDisposition.Replace, Assert.Single(plan.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update planning creates a genuinely new current-source target"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanCreatesGenuinelyNewCurrentSourceTarget()
    {
        var plan = Plan([NewTarget()], force: false, prune: false);

        Assert.Equal(UpdatePlanningDisposition.Create, Assert.Single(plan.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update planning preserves changed current content without force"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanPreservesChangedCurrentTargetWithoutForce()
    {
        var plan = Plan([Changed()], force: false, prune: false);

        Assert.Equal(UpdatePlanningDisposition.Preserve, Assert.Single(plan.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update planning preserves a missing current target without force"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanPreservesMissingCurrentTargetWithoutForce()
    {
        var plan = Plan([Missing()], force: false, prune: false);

        Assert.Equal(UpdatePlanningDisposition.Preserve, Assert.Single(plan.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update planning preserves retired content without prune"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanPreservesRetiredTargetWithoutPrune()
    {
        var plan = Plan([Retired()], force: false, prune: false);

        Assert.Equal(UpdatePlanningDisposition.Preserve, Assert.Single(plan.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update force replaces changed current content only"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanForceReplacesChangedCurrentTargetOnly()
    {
        var plan = Plan([Changed()], force: true, prune: false);

        Assert.Equal(UpdatePlanningDisposition.Replace, Assert.Single(plan.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update force restores missing current content only"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanForceRestoresMissingCurrentTargetOnly()
    {
        var plan = Plan([Missing()], force: true, prune: false);

        Assert.Equal(UpdatePlanningDisposition.Restore, Assert.Single(plan.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update force does not delete retired content"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanForceDoesNotDeleteRetiredTarget()
    {
        var plan = Plan([Retired()], force: true, prune: false);

        Assert.Equal(UpdatePlanningDisposition.Preserve, Assert.Single(plan.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update prune deletes eligible retired content only"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanPruneDeletesEligibleRetiredTargetOnly()
    {
        var plan = Plan([Retired()], force: false, prune: true);

        Assert.Equal(UpdatePlanningDisposition.Delete, Assert.Single(plan.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update prune blocks ineligible retired content"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanPruneBlocksIneligibleRetiredTarget()
    {
        var plan = Plan([Retired(eligible: false)], force: false, prune: true);

        Assert.Equal(UpdatePlanningDisposition.Blocked, Assert.Single(plan.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update force and prune compose their two independent exact effects"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanForceAndPruneComposeIndependentExactEffects()
    {
        var plan = Plan([Changed(), Retired()], force: true, prune: true);

        Assert.Equal(
            [UpdatePlanningDisposition.Replace, UpdatePlanningDisposition.Delete],
            plan.Decisions.Select(decision => decision.Disposition));
    }

    [Fact(DisplayName = "Update planning treats format-only semantic equality as no-op"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanTreatsFormatOnlySemanticEqualityAsNoOp()
    {
        var plan = Plan([FormatOnly()], force: false, prune: false);

        Assert.Equal(UpdatePlanningDisposition.NoOp, Assert.Single(plan.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update planning marks preserve and no-op combinations effect-free"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanMarksPreserveAndNoOpCombinationEffectFree()
    {
        var plan = Plan([Unchanged(), Changed()], force: false, prune: false);

        Assert.True(plan.IsEffectFree);
        Assert.False(plan.IsNoOp);
    }

    [Fact(DisplayName = "Update planning keeps force and prune authority independent"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PlanKeepsForceAndPruneAuthorityIndependent()
    {
        var forceOnly = Plan([Changed(), Retired()], force: true, prune: false);
        var pruneOnly = Plan([Changed(), Retired()], force: false, prune: true);

        Assert.Equal(
            [UpdatePlanningDisposition.Replace, UpdatePlanningDisposition.Preserve],
            forceOnly.Decisions.Select(decision => decision.Disposition));
        Assert.Equal(
            [UpdatePlanningDisposition.Preserve, UpdatePlanningDisposition.Delete],
            pruneOnly.Decisions.Select(decision => decision.Disposition));
    }

    private static UpdatePlanningPlan Plan(
        IReadOnlyList<UpdateComparison> comparisons,
        bool force,
        bool prune)
        => UpdatePlanningPolicy.Plan(
            comparisons,
            new UpdatePlanningAuthority(force, prune));

    private static UpdateComparison Unchanged()
        => Comparison(
            baseline: HashA,
            current: HashA,
            intended: HashA,
            currentState: UpdateComparisonCurrentState.BaselineEquivalent,
            intendedState: UpdateComparisonIntendedState.Same,
            retirementEligibility: UpdateRetirementEligibility.NotApplicable,
            currentBytes: BytesA,
            intendedBytes: BytesA);

    private static UpdateComparison SourceChanged()
        => Comparison(
            baseline: HashA,
            current: HashA,
            intended: HashB,
            currentState: UpdateComparisonCurrentState.BaselineEquivalent,
            intendedState: UpdateComparisonIntendedState.Changed,
            retirementEligibility: UpdateRetirementEligibility.NotApplicable,
            currentBytes: BytesA,
            intendedBytes: BytesB);

    private static UpdateComparison NewTarget()
        => Comparison(
            baseline: null,
            current: null,
            intended: HashB,
            currentState: UpdateComparisonCurrentState.Missing,
            intendedState: UpdateComparisonIntendedState.New,
            retirementEligibility: UpdateRetirementEligibility.NotApplicable,
            currentBytes: EmptyBytes,
            intendedBytes: BytesB);

    private static UpdateComparison Changed()
        => Comparison(
            baseline: HashA,
            current: HashB,
            intended: HashC,
            currentState: UpdateComparisonCurrentState.Changed,
            intendedState: UpdateComparisonIntendedState.Changed,
            retirementEligibility: UpdateRetirementEligibility.NotApplicable,
            currentBytes: BytesB,
            intendedBytes: BytesC);

    private static UpdateComparison Missing()
        => Comparison(
            baseline: HashA,
            current: null,
            intended: HashC,
            currentState: UpdateComparisonCurrentState.Missing,
            intendedState: UpdateComparisonIntendedState.Changed,
            retirementEligibility: UpdateRetirementEligibility.NotApplicable,
            currentBytes: EmptyBytes,
            intendedBytes: BytesC);

    private static UpdateComparison Retired(bool eligible = true)
        => Comparison(
            baseline: HashA,
            current: HashA,
            intended: null,
            currentState: UpdateComparisonCurrentState.BaselineEquivalent,
            intendedState: UpdateComparisonIntendedState.Retired,
            retirementEligibility: eligible
                ? UpdateRetirementEligibility.Eligible
                : UpdateRetirementEligibility.Ineligible,
            currentBytes: BytesA,
            intendedBytes: EmptyBytes,
            sourcePresent: false);

    private static UpdateComparison FormatOnly()
        => Comparison(
            baseline: HashA,
            current: HashA,
            intended: HashA,
            currentState: UpdateComparisonCurrentState.FormatOnly,
            intendedState: UpdateComparisonIntendedState.Same,
            retirementEligibility: UpdateRetirementEligibility.NotApplicable,
            currentBytes: BytesA,
            intendedBytes: BytesA);

    private static UpdateComparison Comparison(
        string? baseline,
        string? current,
        string? intended,
        UpdateComparisonCurrentState currentState,
        UpdateComparisonIntendedState intendedState,
        UpdateRetirementEligibility retirementEligibility,
        UpdateComparisonByteFacts currentBytes,
        UpdateComparisonByteFacts intendedBytes,
        bool sourcePresent = true)
        => new()
        {
            RelativePath = "docs/framework.md",
            Kind = UpdateComparisonTargetKind.File,
            RegionIdentity = null,
            SourceAssetPath = "framework/docs/framework.md",
            SourceAssetPresentInCurrentInventory = sourcePresent,
            FingerprintKind = UpdateComparisonFingerprintKind.OpenForgeMarkdownV1,
            BaselineFingerprint = baseline,
            CurrentFingerprint = current,
            IntendedFingerprint = intended,
            CurrentState = currentState,
            IntendedState = intendedState,
            RetirementEligibility = retirementEligibility,
            CurrentBytes = currentBytes,
            IntendedBytes = intendedBytes,
        };

    private static UpdateComparisonByteFacts BytesA => new()
    {
        ExactBytes = ImmutableArray.Create<byte>(1, 2, 3),
        Sha256 = ByteHashA,
    };

    private static UpdateComparisonByteFacts BytesB => new()
    {
        ExactBytes = ImmutableArray.Create<byte>(4, 5, 6),
        Sha256 = ByteHashB,
    };

    private static UpdateComparisonByteFacts BytesC => new()
    {
        ExactBytes = ImmutableArray.Create<byte>(7, 8, 9),
        Sha256 = ByteHashC,
    };

    private static UpdateComparisonByteFacts EmptyBytes => new()
    {
        ExactBytes = null,
        Sha256 = null,
    };

    private const string HashA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string HashB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string HashC = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";
    private const string ByteHashA = "039058c6f2c0cb492c533b0a4d14ef77cc0f78abccced5287d84a1a2011cfb81";
    private const string ByteHashB = "787c798e39a5bc1910355bae6d0cd87a36b2e10fd0202a83e3bb6b005da83472";
    private const string ByteHashC = "66a6757151f8ee55db127716c7e3dce0be8074b64e20eda542e5c1e46ca9c41e";
}
