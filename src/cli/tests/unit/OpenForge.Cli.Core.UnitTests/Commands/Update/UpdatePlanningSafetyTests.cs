using System.Collections.Immutable;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Commands.Update;

public sealed class UpdatePlanningSafetyTests
{
    [Fact(DisplayName = "Update planning keeps safe source replacement independent of force and prune"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void KeepsSafeSourceReplacementIndependentOfForceAndPrune()
    {
        var normal = Plan([SourceChanged()], force: false, prune: false);
        var widened = Plan([SourceChanged()], force: true, prune: true);

        Assert.Equal(UpdatePlanningDisposition.Replace, Assert.Single(normal.Decisions).Disposition);
        Assert.Equal(UpdatePlanningDisposition.Replace, Assert.Single(widened.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update planning keeps genuine creation independent of force and prune"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void KeepsGenuineCreationIndependentOfForceAndPrune()
    {
        var normal = Plan([NewTarget()], force: false, prune: false);
        var widened = Plan([NewTarget()], force: true, prune: true);

        Assert.Equal(UpdatePlanningDisposition.Create, Assert.Single(normal.Decisions).Disposition);
        Assert.Equal(UpdatePlanningDisposition.Create, Assert.Single(widened.Decisions).Disposition);
    }

    [Fact(DisplayName = "Update prune leaves an already absent retired target effect-free"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PruneLeavesAlreadyAbsentRetiredTargetEffectFree()
    {
        var plan = Plan([AbsentRetired()], force: false, prune: true);

        Assert.Equal(UpdatePlanningDisposition.NoOp, Assert.Single(plan.Decisions).Disposition);
        Assert.True(plan.IsEffectFree);
    }

    [Fact(DisplayName = "Update one ineligible retirement blocks the complete force-prune plan"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void OneIneligibleRetirementBlocksCompleteForcePrunePlan()
    {
        var plan = Plan([Changed(), Retired(eligible: false)], force: true, prune: true);

        Assert.True(plan.IsBlocked);
        Assert.Equal(
            [UpdatePlanningDisposition.Replace, UpdatePlanningDisposition.Blocked],
            plan.Decisions.Select(decision => decision.Disposition));
    }

    private static UpdatePlanningPlan Plan(
        IReadOnlyList<UpdateComparison> comparisons,
        bool force,
        bool prune)
        => UpdatePlanningPolicy.Plan(
            comparisons,
            new UpdatePlanningAuthority(force, prune));

    private static UpdateComparison SourceChanged()
        => Comparison(
            baseline: FingerprintA,
            current: FingerprintA,
            intended: FingerprintB,
            currentState: UpdateComparisonCurrentState.BaselineEquivalent,
            intendedState: UpdateComparisonIntendedState.Changed,
            retirementEligibility: UpdateRetirementEligibility.NotApplicable,
            currentBytes: Bytes(1),
            intendedBytes: Bytes(2));

    private static UpdateComparison NewTarget()
        => Comparison(
            baseline: null,
            current: null,
            intended: FingerprintB,
            currentState: UpdateComparisonCurrentState.Missing,
            intendedState: UpdateComparisonIntendedState.New,
            retirementEligibility: UpdateRetirementEligibility.NotApplicable,
            currentBytes: MissingBytes(),
            intendedBytes: Bytes(2));

    private static UpdateComparison Changed()
        => Comparison(
            baseline: FingerprintA,
            current: FingerprintB,
            intended: FingerprintC,
            currentState: UpdateComparisonCurrentState.Changed,
            intendedState: UpdateComparisonIntendedState.Changed,
            retirementEligibility: UpdateRetirementEligibility.NotApplicable,
            currentBytes: Bytes(2),
            intendedBytes: Bytes(3));

    private static UpdateComparison AbsentRetired()
        => Comparison(
            baseline: FingerprintA,
            current: null,
            intended: null,
            currentState: UpdateComparisonCurrentState.Missing,
            intendedState: UpdateComparisonIntendedState.Retired,
            retirementEligibility: UpdateRetirementEligibility.NotApplicable,
            currentBytes: MissingBytes(),
            intendedBytes: MissingBytes(),
            sourcePresent: false);

    private static UpdateComparison Retired(bool eligible)
        => Comparison(
            baseline: FingerprintA,
            current: FingerprintA,
            intended: null,
            currentState: UpdateComparisonCurrentState.BaselineEquivalent,
            intendedState: UpdateComparisonIntendedState.Retired,
            retirementEligibility: eligible
                ? UpdateRetirementEligibility.Eligible
                : UpdateRetirementEligibility.Ineligible,
            currentBytes: Bytes(1),
            intendedBytes: MissingBytes(),
            sourcePresent: false);

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

    private static UpdateComparisonByteFacts Bytes(byte value)
    {
        var bytes = ImmutableArray.Create(value);
        return new UpdateComparisonByteFacts
        {
            ExactBytes = bytes,
            Sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes.AsSpan())),
        };
    }

    private static UpdateComparisonByteFacts MissingBytes()
        => new()
        {
            ExactBytes = null,
            Sha256 = null,
        };

    private const string FingerprintA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string FingerprintB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string FingerprintC = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";
}
