using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Detach.Shared.Planning;

public sealed class LibraryDetachPlannerTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void DryRunAndApplyHaveIdenticalEffects()
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        var preview = LibraryDetachPlanner.Plan(input with { Request = input.Request with { Mode = LibraryMode.DryRun } }, TestContext.Current.CancellationToken);
        var apply = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(apply.State, preview.State);
        Assert.Equal(apply.Links.ToArray(), preview.Links.ToArray());
        Assert.Equal(apply.RecordChange?.Kind, preview.RecordChange?.Kind);
        Assert.Equal(apply.RecordChange?.LogicalPath, preview.RecordChange?.LogicalPath);
        Assert.Equal(apply.RecordChange?.IntendedBytes.ToArray(), preview.RecordChange?.IntendedBytes.ToArray());
        Assert.Equal(apply.GeneratedRegions.Select(change => change.LogicalPath), preview.GeneratedRegions.Select(change => change.LogicalPath));
        Assert.Equal(apply.Directories.ToArray(), preview.Directories.ToArray());
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)LibraryMappingObservationState.Changed, (int)LibraryPlanState.Blocked)]
    [InlineData((int)LibraryMappingObservationState.Blocked, (int)LibraryPlanState.Blocked)]
    [InlineData((int)LibraryMappingObservationState.Unavailable, (int)LibraryPlanState.Incomplete)]
    public void UnsafeOrUnavailableMappingRefusesWholePlan(int stateValue, int expectedValue)
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        var state = (LibraryMappingObservationState)stateValue;
        input = input with { Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, state)] };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal((LibraryPlanState)expectedValue, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)LifecycleOwnershipManager.Framework)]
    [InlineData((int)LifecycleOwnershipManager.Extension)]
    public void SeparateOwnershipBlocksEveryEffect(int managerValue)
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        input = input with
        {
            Ownership = LibraryMutationPlanningData.Ownership(new LifecycleOwnershipClaim(
            LibraryMutationPlanningData.Leaf, (LifecycleOwnershipManager)managerValue, "another-owner"))
        };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void UnavailableRecordIsNeverTreatedAsEmpty()
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        input = input with { Record = new LibrariesRecordRead { State = LibrariesRecordReadState.Unavailable, Record = null, Snapshot = null, Cause = "Record cannot be read." } };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Incomplete, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void ExactLinksAreDeletedAndLastRecordIsRemoved()
    {
        var plan = LibraryDetachPlanner.Plan(LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf), TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Complete, plan.State);
        var link = Assert.Single(plan.Links);
        Assert.Equal(RelativeFileLinkEffectKind.Delete, link.Kind);
        Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md", link.RawRelativeTarget);
        Assert.Equal(PlannedFileChangeKind.Delete, Assert.IsType<PlannedFileChange>(plan.RecordChange).Kind);
        Assert.Empty(plan.Directories);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void MissingDestinationBlocksWholeDetach()
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf) with
        { Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Missing)] };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void MalformedRecordPreservesItsCommandSpecificStatusAndHasNoEffect()
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        input = input with
        {
            Record = new LibrariesRecordRead
            {
                State = LibrariesRecordReadState.Malformed,
                Record = null,
                Snapshot = FileStateSnapshot.File(LibraryMutationPlanningData.Absolute(LibraryMutationPlanningData.RecordPath),
                LibraryMutationPlanningData.Absolute(LibraryMutationPlanningData.RecordPath), "{"u8),
                Cause = "Malformed record."
            }
        };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Contains(plan.Findings, finding => finding.Status == CliSemanticStatus.Blocked);
        Assert.NotEmpty(plan.Findings);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void KnownBlockedMappingOutranksUnavailableRecordFacts()
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        input = input with
        {
            Record = new LibrariesRecordRead { State = LibrariesRecordReadState.Unavailable, Record = null, Snapshot = null, Cause = "Record unavailable." },
            Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Blocked)],
        };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData(true), InlineData(false)]
    public void UnknownIdAndMissingRecordAreInvalidWithoutAnyEffect(bool completeRecord)
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        input = completeRecord
            ? input with
            {
                Request = input.Request with { LibraryId = LibraryId.Create("other-library") },
            }
            : input with { Record = LibraryMutationPlanningData.MissingRecord() };

        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);

        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        var finding = Assert.Single(plan.Findings);
        Assert.Equal(LibraryDetachFindingCode.UnknownId, finding.Code);
        Assert.Equal(CliSemanticStatus.Invalid, finding.Status);
        Assert.Empty(plan.Directories);
        Assert.Empty(plan.Links);
        Assert.Empty(plan.GeneratedRegions);
        Assert.Null(plan.RecordChange);
    }
}
