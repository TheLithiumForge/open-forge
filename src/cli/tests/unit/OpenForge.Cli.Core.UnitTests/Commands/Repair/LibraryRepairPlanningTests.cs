using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class LibraryRepairPlanningTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("OrdinaryCreate"), InlineData("OrdinaryReplace"), InlineData("OrdinaryReplaceGeneratedRegion")]
    [InlineData("OrdinaryDelete"), InlineData("RelativeFileLinkCreate"), InlineData("RelativeFileLinkDelete")]
    public void AutomaticSelectionRetainsExactTypedEffectAndSelectedDependencyClosure(string entryKind)
    {
        var evidence = LibraryRepairData.Evidence(Enum.Parse<RecoveryEntryKind>(entryKind));
        var plan = RepairLibraryRecoveryPlanner.Build(LibraryRepairData.Input(evidence));
        Assert.Empty(plan.Steps);
        var step = Assert.Single(plan.LibrarySteps);
        Assert.Same(evidence, step.Selection.Proposal.Evidence);
        Assert.Equal([RepairSelectionOrigin.Automatic], step.Selection.Origins);
        Assert.NotNull(step.Effect);
        Assert.Equal(evidence.Entry.Input.Context.Entry, step.Effect.Entry);
        Assert.Contains(RepairDependencyDomain.LibraryRecord, step.Dependency.Domains);
        Assert.Contains(RepairDependencyDomain.LibraryResidual, step.Dependency.Domains);
        Assert.DoesNotContain(RepairDependencyDomain.LocalReference, step.Dependency.Domains);
        Assert.Contains(RepairVerificationKind.NoFollowIdentity, step.Verification.Kinds);
        Assert.Contains(RepairVerificationKind.PriorState, step.Verification.Kinds);
        Assert.Empty(plan.Conflicts);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("Third"), InlineData("Unavailable"), InlineData("Blocked")]
    public void UnsafeOrUnprovenSelectedCurrentStateCannotProduceAnEffect(string state)
    {
        var evidence = LibraryRepairData.Evidence(comparison: Enum.Parse<RecoveryBundleTargetComparisonState>(state));
        var plan = RepairLibraryRecoveryPlanner.Build(LibraryRepairData.Input(evidence));
        Assert.All(plan.LibrarySteps, step => Assert.Null(step.Effect));
        Assert.True(plan.IsBlocked);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void AlreadyPriorStateDoesNotProduceAnotherInverseEffect()
    {
        var plan = RepairLibraryRecoveryPlanner.Build(LibraryRepairData.Input(
            LibraryRepairData.Evidence(comparison: RecoveryBundleTargetComparisonState.Prior)));
        Assert.All(plan.LibrarySteps, step => Assert.Null(step.Effect));
        Assert.Empty(plan.Effects);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void NoSelectionAuthorityRetainsResidualUnselected()
    {
        var evidence = LibraryRepairData.Evidence();
        var plan = RepairLibraryRecoveryPlanner.Build(LibraryRepairData.Input(evidence, automatic: false));
        Assert.Empty(plan.LibrarySteps);
        Assert.Empty(plan.Selection.Libraries.Selected);
        Assert.Same(evidence, Assert.Single(plan.Selection.Libraries.Unselected).Evidence);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void ChangedIndependentComparisonRefusesRevalidation()
    {
        var plan = LibraryRepairData.Plan();
        var input = LibraryRepairData.Input(LibraryRepairData.Evidence(comparison: RecoveryBundleTargetComparisonState.Third));
        Assert.False(RepairLibraryRecoveryPlanner.Revalidate(plan, input));
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void NewAtomicAndReceiptKindsHaveAnExactClosedNamedSet()
    {
        Assert.Equal(["Reference", "LibraryRecovery"], Enum.GetNames<RepairAtomicEffectKind>());
        Assert.Equal(["Ordinary", "RelativeFileLink"], Enum.GetNames<RepairLibraryRecoveryKind>());
        Assert.Equal(["Lease", "Revalidation", "ForwardPreparation", "Effect", "Verification", "ForwardCleanup", "PostDiagnosis"],
            Enum.GetNames<RepairLibraryExecutionStage>());
        Assert.Throws<ArgumentOutOfRangeException>(() => new RepairAtomicEffect(0, (RepairAtomicEffectKind)int.MaxValue, null, null));
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void AtomicEffectRejectsMissingAndConflictingPayloads()
    {
        var reference = RepairTestData.Effect();
        var library = LibraryRepairData.Plan().LibrarySteps[0].Effect!;
        Assert.Throws<ArgumentException>(() => new RepairAtomicEffect(0, RepairAtomicEffectKind.Reference, null, null));
        Assert.Throws<ArgumentException>(() => new RepairAtomicEffect(0, RepairAtomicEffectKind.LibraryRecovery, null, null));
        Assert.Throws<ArgumentException>(() => new RepairAtomicEffect(0, RepairAtomicEffectKind.Reference, reference, library));
        Assert.Throws<ArgumentException>(() => new RepairAtomicEffect(0, RepairAtomicEffectKind.LibraryRecovery, reference, library));
        Assert.Same(reference, new RepairAtomicEffect(0, RepairAtomicEffectKind.Reference, reference, null).Reference);
        Assert.Same(library, new RepairAtomicEffect(1, RepairAtomicEffectKind.LibraryRecovery, null, library).LibraryRecovery);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("ExplicitRelink"), InlineData("Undefined")]
    public void LibrarySelectionRejectsReferenceOnlyAndUndefinedOrigins(string origin)
    {
        var value = origin == "Undefined" ? (RepairSelectionOrigin)int.MaxValue : RepairSelectionOrigin.ExplicitRelink;
        Assert.Throws<ArgumentException>(() => new RepairSelectedLibraryRecovery(
            new RepairLibraryRecoveryProposal(LibraryRepairData.Evidence()), [value]));
    }
}
