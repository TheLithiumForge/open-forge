using System.Text;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairPlanningModelTests
{
    [Fact(DisplayName = "Repair changes retain exact destination transition, target, catalogue member, and distinct origins"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void ChangeRetainsExactSelectionFacts()
    {
        var target = RepairTestData.Target(".agents/docs/new.md", "New");
        var change = new RepairChange(
            new SourceLocation(4, 9, 24, 7),
            "../old.md",
            "new.md#New",
            RepairCatalogueMember.UniqueCanonicalFragment,
            target,
            [
                RepairSelectionOrigin.ExplicitRelink,
                RepairSelectionOrigin.Automatic,
                RepairSelectionOrigin.ExplicitRelink,
            ]);

        Assert.Equal("../old.md", change.ExpectedDestination);
        Assert.Equal("new.md#New", change.IntendedDestination);
        Assert.Equal(RepairCatalogueMember.UniqueCanonicalFragment, change.CatalogueMember);
        Assert.Same(target, change.Target);
        Assert.Equal(
            [RepairSelectionOrigin.ExplicitRelink, RepairSelectionOrigin.Automatic],
            change.Origins);
        Assert.Throws<ArgumentException>(() => new RepairChange(
            new SourceLocation(1, 1, 0, 3),
            "old",
            "old",
            RepairCatalogueMember.SameTargetPath,
            RepairTestData.Target(),
            [RepairSelectionOrigin.Automatic]));
        Assert.Throws<ArgumentException>(() => new RepairChange(
            new SourceLocation(1, 1, 0, 3),
            "old",
            "new",
            RepairCatalogueMember.SameTargetPath,
            RepairTestData.Target(),
            []));
    }

    [Fact(DisplayName = "Repair effects project sorted non-overlapping UTF-8 destination replacements into one complete file state"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void EffectProjectsCompleteUtf8State()
    {
        var expected = RepairTestData.FileState("abcde");
        var intended = RepairTestData.FileState("AbcdE");
        var later = new RepairChange(
            new SourceLocation(1, 5, 4, 1),
            "e",
            "E",
            RepairCatalogueMember.SameTargetCase,
            RepairTestData.Target(),
            [RepairSelectionOrigin.Wizard]);
        var earlier = new RepairChange(
            new SourceLocation(1, 1, 0, 1),
            "a",
            "A",
            RepairCatalogueMember.SameTargetPath,
            RepairTestData.Target(),
            [RepairSelectionOrigin.Automatic]);
        var effect = new RepairEffect(
            RepairTestData.SourcePath,
            expected,
            intended,
            [later, earlier],
            RepairTestData.Attribution());

        Assert.Equal([earlier, later], effect.Changes);
        Assert.Same(expected, effect.ExpectedState);
        Assert.Same(intended, effect.IntendedState);
        Assert.Equal(PlannedFileChangeKind.Replace, effect.FileChange.Kind);
        Assert.Equal(intended.Bytes, effect.FileChange.IntendedBytes);
    }

    [Fact(DisplayName = "Repair effects use UTF-8 byte spans and support unequal replacement byte lengths"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void EffectUsesUtf8ByteCoordinates()
    {
        var expected = RepairTestData.FileState(
            "aéz"u8);
        var intended = RepairTestData.FileState("aez");
        var change = new RepairChange(
            new SourceLocation(1, 2, 1, 2),
            "é",
            "e",
            RepairCatalogueMember.SameTargetEncoding,
            RepairTestData.Target(),
            [RepairSelectionOrigin.Automatic]);

        var effect = new RepairEffect(
            RepairTestData.SourcePath,
            expected,
            intended,
            [change],
            RepairTestData.Attribution());

        Assert.Equal(2, effect.Changes[0].Occurrence.ByteLength);
        Assert.Equal(intended.Bytes, effect.IntendedState.Bytes);
    }

    [Fact(DisplayName = "Repair effects reject empty, invalid, overlapping, mismatched, unchanged, and non-UTF-8 transitions"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void EffectRejectsUnsafeTransitions()
    {
        var expected = RepairTestData.FileState("abcd");
        var changed = RepairTestData.FileState("abcD");

        Assert.Throws<ArgumentException>(() => new RepairEffect(
            RepairTestData.SourcePath,
            expected,
            expected,
            [Change(0, "a", "A")],
            RepairTestData.Attribution()));
        Assert.Throws<ArgumentException>(() => new RepairEffect(
            RepairTestData.SourcePath,
            expected,
            changed,
            [],
            RepairTestData.Attribution()));
        Assert.Throws<ArgumentException>(() => new RepairEffect(
            RepairTestData.SourcePath,
            expected,
            changed,
            [Change(3, "d", "D", byteLength: 2)],
            RepairTestData.Attribution()));
        Assert.Throws<ArgumentException>(() => new RepairEffect(
            RepairTestData.SourcePath,
            expected,
            changed,
            [
                Change(0, "ab", "AB", byteLength: 2),
                Change(1, "bc", "BC", byteLength: 2),
            ],
            RepairTestData.Attribution()));
        Assert.Throws<ArgumentException>(() => new RepairEffect(
            RepairTestData.SourcePath,
            expected,
            changed,
            [Change(1, "x", "B")],
            RepairTestData.Attribution()));
        Assert.Throws<ArgumentException>(() => new RepairEffect(
            RepairTestData.SourcePath,
            expected,
            FileStateSnapshotBuilder.File("aB!"),
            [Change(1, "b", "B")],
            RepairTestData.Attribution()));
        Assert.Throws<System.Text.EncoderFallbackException>(() => new RepairEffect(
            RepairTestData.SourcePath,
            expected,
            changed,
            [Change(1, "b", "\uD800")],
            RepairTestData.Attribution()));
    }

    [Fact(DisplayName = "Repair steps keep effect, no-op, recovery, verification, and lifecycle outcomes coherent"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void StepCoherenceIsExplicit()
    {
        var selected = RepairTestData.Selected();
        var effect = RepairTestData.Effect();
        var applied = RepairTestData.Step(
            selected,
            effect,
            outcome: RepairStepOutcome.Applied);
        Assert.Equal(RepairStepOutcome.Applied, applied.Outcome);
        Assert.Same(effect, applied.Effect);
        Assert.Null(applied.NoOp);
        Assert.Equal(RepairRecoveryRequirementKind.Required, applied.Recovery.Kind);
        Assert.Equal(
            [
                RepairDependencyDomain.WorkspaceContainment,
                RepairDependencyDomain.RouteAndHeading,
                RepairDependencyDomain.LocalReference,
            ],
            applied.Dependency.Domains);
        Assert.Equal(
            [
                RepairVerificationKind.DestinationLiteral,
                RepairVerificationKind.SameTargetIdentity,
                RepairVerificationKind.ResultingBytes,
            ],
            applied.Verification.Kinds);

        var noOpSelected = RepairTestData.Selected();
        var noOp = RepairTestData.NoOp();
        var noOpStep = RepairTestData.Step(
            noOpSelected,
            noOp: noOp,
            outcome: RepairStepOutcome.NoOp);
        Assert.Equal(RepairStepOutcome.NoOp, noOpStep.Outcome);
        Assert.Same(noOp, noOpStep.NoOp);
        Assert.Equal(RepairRecoveryRequirementKind.NotRequired, noOpStep.Recovery.Kind);

        var blocked = RepairTestData.Step(
            RepairTestData.Selected(),
            outcome: RepairStepOutcome.Blocked);
        Assert.Null(blocked.Effect);
        Assert.Null(blocked.NoOp);

        var failedWithoutEffect = RepairTestData.Step(
            RepairTestData.Selected(),
            outcome: RepairStepOutcome.Failed);
        Assert.Equal(RepairStepOutcome.Failed, failedWithoutEffect.Outcome);
        Assert.Null(failedWithoutEffect.Effect);

        Assert.Throws<ArgumentException>(() => RepairTestData.Step(
            selected,
            effect,
            noOp,
            RepairStepOutcome.Applied));
    }

    [Fact(DisplayName = "Repair plans bind exactly one step to each selected proposal and expose conflicts and no-ops"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void PlanBindsSelectedStepsAndConflictState()
    {
        var selected = RepairTestData.Selected();
        var effect = RepairTestData.Effect();
        var plan = RepairTestData.Plan(
            selected,
            RepairTestData.Step(selected, effect));

        Assert.Same(selected, Assert.Single(plan.Selection.Selected));
        Assert.Same(effect, Assert.Single(plan.Effects));
        Assert.Empty(plan.NoOps);
        Assert.False(plan.IsBlocked);
        Assert.False(plan.IsNoOp);

        var conflict = new RepairConflict(
            RepairConflictKind.OverlappingChanges,
            RepairTestData.SourcePath,
            new SourceLocation(2, 3, 12, 4),
            "The selected destination spans overlap.");
        var blocked = RepairTestData.Plan(
            selected,
            RepairTestData.Step(
                selected,
                outcome: RepairStepOutcome.Blocked),
            conflicts: [conflict]);
        Assert.True(blocked.IsBlocked);
        Assert.False(blocked.IsNoOp);
        Assert.Equal(RepairConflictKind.OverlappingChanges, Assert.Single(blocked.Conflicts).Kind);
        Assert.Equal(12, blocked.Conflicts[0].Occurrence!.ByteOffset);

        var noOpSelected = RepairTestData.Selected();
        var noOpPlan = RepairTestData.Plan(
            noOpSelected,
            RepairTestData.Step(
                noOpSelected,
                noOp: RepairTestData.NoOp(),
                outcome: RepairStepOutcome.NoOp));
        Assert.True(noOpPlan.IsNoOp);
        Assert.False(noOpPlan.IsBlocked);

        var emptySelection = new RepairSelection(
            RepairSelectionMode.Automatic,
            [],
            [],
            RepairLibrarySelection.Empty);
        var emptyPlan = new RepairPlan(
            RepairTestData.Request(),
            emptySelection,
            [],
            [],
            []);
        Assert.True(emptyPlan.IsNoOp);
    }

    [Fact(DisplayName = "Repair recovery states preserve residual path and attribution lifecycle semantics"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void RecoveryStateAndResidualCoherenceIsEnforced()
    {
        var attribution = RepairTestData.Attribution();
        var residualPath = Path.GetFullPath("/tmp/open-forge-repair/recovery.zip");
        var prepared = new RepairRecovery(
            RepairRecoveryState.Prepared,
            RepairResidualState.Retained,
            residualPath,
            attribution);
        Assert.Equal(RepairRecoveryState.Prepared, prepared.State);
        Assert.Equal(RepairResidualState.Retained, prepared.Residual);
        Assert.Equal(residualPath, prepared.ResidualPath);
        Assert.Same(attribution, prepared.Attribution);

        var removed = new RepairRecovery(
            RepairRecoveryState.Removed,
            RepairResidualState.None,
            residualPath: null,
            attribution);
        Assert.Equal(RepairRecoveryState.Removed, removed.State);
        Assert.Equal(RepairResidualState.None, removed.Residual);
        Assert.Same(attribution, removed.Attribution);

        Assert.Throws<ArgumentException>(() => new RepairRecovery(
            RepairRecoveryState.Prepared,
            RepairResidualState.None,
            residualPath: null,
            attribution));
        Assert.Throws<ArgumentException>(() => new RepairRecovery(
            RepairRecoveryState.NotRequired,
            RepairResidualState.None,
            residualPath: null,
            attribution));
        Assert.Throws<ArgumentException>(() => new RepairRecovery(
            RepairRecoveryState.Retained,
            RepairResidualState.Retained,
            "relative/recovery.zip",
            attribution));
        Assert.Throws<ArgumentException>(() => new RepairRecovery(
            RepairRecoveryState.Unknown,
            RepairResidualState.None,
            residualPath: null,
            attribution));

        var foreign = RecoveryBundleAttribution.Read(
            RecoveryBundleProducer.Framework,
            RecoveryBundleOperation.Install,
            RecoveryBundleSubject.Workspace(new string('b', 64)));
        Assert.Throws<ArgumentException>(() => RepairRecoveryRequirement.Required(foreign));
    }

    private static RepairChange Change(
        long byteOffset,
        string expected,
        string intended,
        int? byteLength = null)
        => new(
            new SourceLocation(
                line: 1,
                column: checked((int)byteOffset + 1),
                byteOffset,
                byteLength ?? Encoding.UTF8.GetByteCount(expected)),
            expected,
            intended,
            RepairCatalogueMember.SameTargetPath,
            RepairTestData.Target(),
            [RepairSelectionOrigin.Automatic]);

    private static class FileStateSnapshotBuilder
    {
        internal static FileStateSnapshot File(string text)
            => RepairTestData.FileState(text);
    }
}
