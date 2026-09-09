using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Attach.Shared.Planning;

public sealed class LibraryAttachPlannerTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void DryRunAndApplyHaveIdenticalEffects()
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf);
        var preview = LibraryAttachPlanner.Plan(input with { Request = input.Request with { Mode = LibraryMode.DryRun } }, TestContext.Current.CancellationToken);
        var apply = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
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
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf);
        var state = (LibraryMappingObservationState)stateValue;
        input = input with { Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, state)] };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal((LibraryPlanState)expectedValue, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)LifecycleOwnershipManager.Framework)]
    [InlineData((int)LifecycleOwnershipManager.Extension)]
    public void SeparateOwnershipBlocksEveryEffect(int managerValue)
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf);
        input = input with
        {
            Ownership = LibraryMutationPlanningData.Ownership(new LifecycleOwnershipClaim(
            LibraryMutationPlanningData.Leaf, (LifecycleOwnershipManager)managerValue, "another-owner"))
        };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void UnavailableRecordIsNeverTreatedAsEmpty()
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf);
        input = input with { Record = new LibrariesRecordRead { State = LibrariesRecordReadState.Unavailable, Record = null, Snapshot = null, Cause = "Record cannot be read." } };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Incomplete, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void CompleteEmptyInventoryStillCreatesRegistration()
    {
        var plan = LibraryAttachPlanner.Plan(LibraryMutationPlanningData.Attach(), TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Complete, plan.State);
        Assert.Empty(plan.Links);
        Assert.Empty(plan.Directories);
        Assert.Equal(PlannedFileChangeKind.Create, Assert.IsType<PlannedFileChange>(plan.RecordChange).Kind);
        Assert.Empty(Assert.Single(Assert.IsType<LibrariesRecord>(plan.IntendedRecord).Libraries).Paths);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void CompleteInventoryCreatesOnlyExactRelativeLinksAndRecord()
    {
        var plan = LibraryAttachPlanner.Plan(LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf), TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Complete, plan.State);
        var link = Assert.Single(plan.Links);
        Assert.Equal(RelativeFileLinkEffectKind.Create, link.Kind);
        Assert.Equal(".agents/directives/review.md", link.DestinationPath.Value);
        Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md", link.RawRelativeTarget);
        Assert.Equal(PlannedFileChangeKind.Create, Assert.IsType<PlannedFileChange>(plan.RecordChange).Kind);
        Assert.Empty(plan.GeneratedRegions);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void DuplicateIdBlocksEvenWhenRecordHasNoPaths()
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf) with { Record = LibraryMutationPlanningData.Record() };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void UnregisteredMatchingLinkIsACollision()
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf) with
        { Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Current)] };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void IncompleteInventoryNeverCreatesASafeSubsetOrRetirement()
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf);
        var inventory = Assert.IsType<LibraryInventory>(input.Source.Inventory);
        input = input with
        {
            Source = input.Source with
            {
                Inventory = LibraryInventory.Classified(
            inventory.SourceRoot, inventory.PhysicalSourceRoot, LibrarySourceRootState.Available, LibraryInventoryState.Incomplete,
            "A required subtree could not be enumerated.", inventory.Entries)
            }
        };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Incomplete, plan.State);
        Assert.Empty(plan.Links);
        Assert.Empty(plan.GeneratedRegions);
        Assert.Null(plan.RecordChange);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void MalformedRecordPreservesItsCommandSpecificStatusAndHasNoEffect()
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf);
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
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Contains(plan.Findings, finding => finding.Status == CliSemanticStatus.Invalid);
        Assert.NotEmpty(plan.Findings);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void KnownBlockedMappingOutranksUnavailableRecordFacts()
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf);
        input = input with
        {
            Record = new LibrariesRecordRead { State = LibrariesRecordReadState.Unavailable, Record = null, Snapshot = null, Cause = "Record unavailable." },
            Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Blocked)],
        };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }
}
