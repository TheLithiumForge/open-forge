using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Attach.Shared.Planning;

public sealed class LibraryAttachPlannerTests
{
    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void DryRunAndApplyHaveIdenticalEffects()
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf);
        var preview = LibraryAttachPlanner.Plan(input with { Request = input.Request with { Mode = LibraryMode.DryRun } }, TestContext.Current.CancellationToken);
        var apply = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(apply.State, preview.State);
        Assert.Equal(apply.Links.ToArray(), preview.Links.ToArray());
        Assert.Equal(apply.OwnershipChange?.Kind, preview.OwnershipChange?.Kind);
        Assert.Equal(apply.OwnershipChange?.LogicalPath, preview.OwnershipChange?.LogicalPath);
        Assert.Equal(apply.OwnershipChange?.IntendedBytes.ToArray(), preview.OwnershipChange?.IntendedBytes.ToArray());
        Assert.Equal(apply.GeneratedRegions.Select(change => change.LogicalPath), preview.GeneratedRegions.Select(change => change.LogicalPath));
        Assert.Equal(apply.Directories.ToArray(), preview.Directories.ToArray());
    }

    [Trait("Boundary", "Processing")]
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

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)OwnedPathManager.Framework)]
    [InlineData((int)OwnedPathManager.Extension)]
    public void SeparateOwnershipBlocksEveryEffect(int managerValue)
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf);
        input = input with
        {
            Ownership = LibraryMutationPlanningData.Ownership(new OwnedPath(
            LibraryMutationPlanningData.Leaf, (OwnedPathManager)managerValue, "another-owner"))
        };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void UnreadableOwnershipDoesNotGateVerifiedNewLinks()
    {
        var ownership = LibraryMutationPlanningData.WorkspaceOwnership() with
        {
            State = WorkspaceOwnershipReadState.Unavailable,
            Snapshot = null,
            Cause = "Lock cannot be read.",
        };
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf) with
        {
            Ownership = ownership,
            Record = LibraryRegistrationReader.Read(ownership),
        };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Complete, plan.State);
        Assert.Single(plan.Links);
        Assert.Null(plan.OwnershipChange);
        Assert.Contains(plan.Findings, finding => finding.Status == CliSemanticStatus.Complete);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void CompleteEmptyInventoryStillCreatesRegistration()
    {
        var plan = LibraryAttachPlanner.Plan(LibraryMutationPlanningData.Attach(), TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Complete, plan.State);
        Assert.Empty(plan.Links);
        Assert.Empty(plan.Directories);
        Assert.Equal(PlannedFileChangeKind.Create, Assert.IsType<PlannedFileChange>(plan.OwnershipChange).Kind);
        Assert.Empty(Assert.Single(Assert.IsType<LibraryRegistrationSet>(plan.IntendedRecord).Libraries).Paths);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void CompleteInventoryCreatesOnlyExactRelativeLinksAndRecord()
    {
        var plan = LibraryAttachPlanner.Plan(LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf), TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Complete, plan.State);
        var link = Assert.Single(plan.Links);
        Assert.Equal(RelativeFileLinkEffectKind.Create, link.Kind);
        Assert.Equal(".agents/directives/review.md", link.DestinationPath.Value);
        Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md", link.RawRelativeTarget);
        Assert.Equal(PlannedFileChangeKind.Create, Assert.IsType<PlannedFileChange>(plan.OwnershipChange).Kind);
        Assert.Empty(plan.GeneratedRegions);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void DuplicateIdBlocksEvenWhenRecordHasNoPaths()
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf) with { Record = LibraryMutationPlanningData.Record() };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void UnregisteredMatchingLinkIsACollision()
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf) with
        { Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Current)] };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Trait("Boundary", "Processing")]
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
        Assert.Null(plan.OwnershipChange);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void MalformedOwnershipWithReadableLockPlansVerifiedNewKnowledge()
    {
        var ownership = LibraryMutationPlanningData.WorkspaceOwnership() with
        {
            State = WorkspaceOwnershipReadState.Invalid,
            Snapshot = FileStateSnapshot.File(LibraryMutationPlanningData.Absolute(".agents/open-forge.lock.json"),
                LibraryMutationPlanningData.Absolute(".agents/open-forge.lock.json"), "{"u8),
            Cause = "Malformed lock.",
        };
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf) with
        {
            Ownership = ownership,
            Record = LibraryRegistrationReader.Read(ownership),
        };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Complete, plan.State);
        Assert.Single(plan.Links);
        Assert.Empty(plan.GeneratedRegions);
        Assert.Equal(PlannedFileChangeKind.Replace, Assert.IsType<PlannedFileChange>(plan.OwnershipChange).Kind);
        var registration = Assert.Single(Assert.IsType<LibraryRegistrationSet>(plan.IntendedRecord).Libraries);
        Assert.Equal(LibraryMutationPlanningData.Id, registration.Id.Value);
        Assert.Equal([LibraryMutationPlanningData.Leaf], registration.Paths.Select(path => path.Value));
        Assert.Contains(plan.Findings, finding =>
            finding.Code == LibraryAttachFindingCode.OwnershipObservation
            && finding.Status == CliSemanticStatus.Complete);
        Assert.DoesNotContain(plan.Findings, finding => finding.Code == LibraryAttachFindingCode.RecordInvalid);
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)LibraryMappingObservationState.Current)]
    [InlineData((int)LibraryMappingObservationState.Changed)]
    public void MalformedOwnershipStillRefusesOccupiedDestination(int mappingStateValue)
    {
        var ownership = LibraryMutationPlanningData.WorkspaceOwnership() with
        {
            State = WorkspaceOwnershipReadState.Invalid,
            Snapshot = FileStateSnapshot.File(LibraryMutationPlanningData.Absolute(".agents/open-forge.lock.json"),
                LibraryMutationPlanningData.Absolute(".agents/open-forge.lock.json"), "{"u8),
            Cause = "Malformed lock.",
        };
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf) with
        {
            Ownership = ownership,
            Record = LibraryRegistrationReader.Read(ownership),
            Mappings = [LibraryMutationPlanningData.Mapping(
                LibraryMutationPlanningData.Leaf,
                (LibraryMappingObservationState)mappingStateValue)],
        };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.Empty(plan.Links);
        Assert.Null(plan.OwnershipChange);
        Assert.Null(plan.IntendedRecord);
        Assert.Contains(plan.Findings, finding =>
            finding.Code == LibraryAttachFindingCode.DestinationCollision
            && finding.Status == CliSemanticStatus.Blocked);
        Assert.Contains(plan.Findings, finding =>
            finding.Code == LibraryAttachFindingCode.OwnershipObservation
            && finding.Status == CliSemanticStatus.Complete);
        Assert.DoesNotContain(plan.Findings, finding => finding.Code == LibraryAttachFindingCode.RecordInvalid);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void KnownBlockedMappingOutranksUnavailableRecordFacts()
    {
        var input = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf);
        input = input with
        {
            Record = new LibraryRegistrationRead { State = LibraryRegistrationReadState.Unavailable, Record = null, Snapshot = null, Cause = "Record unavailable." },
            Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Blocked)],
        };
        var plan = LibraryAttachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }
}
