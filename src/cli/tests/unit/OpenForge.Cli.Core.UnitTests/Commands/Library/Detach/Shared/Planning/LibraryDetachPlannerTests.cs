using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Detach.Shared.Planning;

public sealed class LibraryDetachPlannerTests
{
    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void DryRunAndApplyHaveIdenticalEffects()
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        var preview = LibraryDetachPlanner.Plan(input with { Request = input.Request with { Mode = LibraryMode.DryRun } }, TestContext.Current.CancellationToken);
        var apply = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
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

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void OrdinaryChangedDestinationAllowsRegistrationReleaseWithoutLinkEffect()
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf) with
        {
            Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Changed)],
        };

        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);

        Assert.Equal(LibraryPlanState.Complete, plan.State);
        Assert.Empty(plan.Links);
        var finding = Assert.Single(plan.Findings, finding => finding.Code == LibraryDetachFindingCode.MappingBlocked);
        Assert.Equal(CliSemanticStatus.Attention, finding.Status);
        Assert.Equal(LibraryDetachOccupantKind.OrdinaryFile, finding.OccupantKind);
        Assert.Null(plan.IntendedRecord);
        Assert.Equal(PlannedFileChangeKind.Replace, Assert.IsType<PlannedFileChange>(plan.OwnershipChange).Kind);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void RetargetedRelativeLinkRemainsBlocked()
    {
        var sourcePath = SourceRelativeEligiblePath.Create(LibraryMutationPlanningData.Leaf);
        var destinationRoot = LibraryDestinationRoot.Create(".");
        var mapping = LibraryMapping.Create(
            WorkspaceRelativeDirectory.Create(LibraryMutationPlanningData.SourceRoot),
            destinationRoot,
            sourcePath);
        var observation = LibraryMappingObservation.Create(
            mapping,
            NoFollowLeafObservation.CreateRelativeFileLink(
                LibraryMutationPlanningData.Absolute(LibraryMutationPlanningData.Leaf),
                RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../../another-source.md")),
            LibraryMappingObservationState.Changed);
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf) with
        {
            Mappings = [observation],
        };

        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);

        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        var finding = Assert.Single(plan.Findings, finding => finding.Code == LibraryDetachFindingCode.MappingBlocked);
        Assert.Equal(CliSemanticStatus.Blocked, finding.Status);
        Assert.Equal(LibraryDetachOccupantKind.DifferentLink, finding.OccupantKind);
        Assert.Empty(plan.Links);
        Assert.Null(plan.OwnershipChange);
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)OwnedPathManager.Framework)]
    [InlineData((int)OwnedPathManager.Extension)]
    public void SeparateOwnershipBlocksEveryEffect(int managerValue)
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        input = input with
        {
            Ownership = LibraryMutationPlanningData.Ownership(new OwnedPath(
            LibraryMutationPlanningData.Leaf, (OwnedPathManager)managerValue, "another-owner"))
        };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void UnavailableRecordIsNeverTreatedAsEmpty()
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        input = input with { Record = new LibraryRegistrationRead { State = LibraryRegistrationReadState.Unavailable, Record = null, Snapshot = null, Cause = "Record cannot be read." } };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Incomplete, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void ExactLinksAreDeletedAndLastRegistrationIsRemoved()
    {
        var plan = LibraryDetachPlanner.Plan(LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf), TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Complete, plan.State);
        var link = Assert.Single(plan.Links);
        Assert.Equal(RelativeFileLinkEffectKind.Delete, link.Kind);
        Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md", link.RawRelativeTarget);
        var publication = Assert.IsType<PlannedFileChange>(plan.OwnershipChange);
        Assert.Equal(PlannedFileChangeKind.Replace, publication.Kind);
        using var document = System.Text.Json.JsonDocument.Parse(publication.IntendedBytes.ToArray());
        Assert.Empty(document.RootElement.GetProperty("libraries").EnumerateArray());
        Assert.Empty(plan.Directories);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void MissingDestinationWarnsAndReleasesRegistration()
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf) with
        { Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Missing)] };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Complete, plan.State);
        Assert.Empty(plan.Links);
        var finding = Assert.Single(plan.Findings, finding => finding.Code == LibraryDetachFindingCode.RegisteredLinkMissing);
        Assert.Equal(CliSemanticStatus.Attention, finding.Status);
        Assert.Null(plan.IntendedRecord);
        Assert.Equal(PlannedFileChangeKind.Replace, Assert.IsType<PlannedFileChange>(plan.OwnershipChange).Kind);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void MissingDestinationStillDeletesOtherExactLink()
    {
        const string other = "other.md";
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf, other) with
        {
            Mappings =
            [
                LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Missing),
                LibraryMutationPlanningData.Mapping(other, LibraryMappingObservationState.Current),
            ],
        };

        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);

        Assert.Equal(LibraryPlanState.Complete, plan.State);
        var link = Assert.Single(plan.Links);
        Assert.Equal(other, link.DestinationPath.Value);
        Assert.Equal(RelativeFileLinkEffectKind.Delete, link.Kind);
        var finding = Assert.Single(plan.Findings, finding => finding.Code == LibraryDetachFindingCode.RegisteredLinkMissing);
        Assert.Equal(CliSemanticStatus.Attention, finding.Status);
        Assert.Null(plan.IntendedRecord);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void MalformedRecordPreservesItsCommandSpecificStatusAndHasNoEffect()
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        input = input with
        {
            Record = new LibraryRegistrationRead
            {
                State = LibraryRegistrationReadState.Malformed,
                Record = null,
                Snapshot = FileStateSnapshot.File(LibraryMutationPlanningData.Absolute(LibraryMutationPlanningData.RecordPath),
                LibraryMutationPlanningData.Absolute(LibraryMutationPlanningData.RecordPath), "{"u8),
                Cause = "Malformed record."
            }
        };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Contains(plan.Findings, finding => finding.Status == CliSemanticStatus.Incomplete);
        Assert.NotEmpty(plan.Findings);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void KnownBlockedMappingOutranksUnavailableRecordFacts()
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        input = input with
        {
            Record = new LibraryRegistrationRead { State = LibraryRegistrationReadState.Unavailable, Record = null, Snapshot = null, Cause = "Record unavailable." },
            Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Blocked)],
        };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void ProtectedDestinationUsesItsDedicatedFinding()
    {
        var sourcePath = SourceRelativeEligiblePath.Create("review.md");
        var destinationRoot = LibraryDestinationRoot.Create(LibraryMutationPlanningData.SourceRoot);
        var registration = LibraryRegistration.Create(
            LibraryId.Create(LibraryMutationPlanningData.Id),
            WorkspaceRelativeDirectory.Create(LibraryMutationPlanningData.SourceRoot),
            destinationRoot,
            [sourcePath]);
        var mapping = LibraryMapping.Create(registration.SourceRoot, destinationRoot, sourcePath);
        var observation = LibraryMappingObservation.Create(
            mapping,
            NoFollowLeafObservation.CreateRelativeFileLink(
                LibraryMutationPlanningData.Absolute("shared/team-knowledge/review.md"),
                RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, mapping.ExpectedRelativeLink.Value)),
            LibraryMappingObservationState.Current);
        var input = LibraryMutationPlanningData.Detach("review.md") with
        {
            Record = new LibraryRegistrationRead
            {
                State = LibraryRegistrationReadState.Complete,
                Record = LibraryRegistrationSet.Create([registration]),
                Snapshot = null,
                Cause = null,
            },
            Mappings = [observation],
        };

        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);

        var finding = Assert.Single(
            plan.Findings,
            finding => finding.Code == LibraryDetachFindingCode.DestinationProtected);
        Assert.Equal(CliSemanticStatus.Blocked, finding.Status);
        Assert.Equal("shared/team-knowledge/review.md", finding.Path);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void BlockedMappingWithUnclassifiedLeafCarriesUnclassifiedCause()
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf) with
        {
            Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Blocked)],
        };

        var finding = Assert.Single(
            LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken).Findings,
            finding => finding.Code == LibraryDetachFindingCode.MappingBlocked);

        Assert.Null(finding.OccupantKind);
        Assert.Equal(
            ".agents/directives/review.md could not be classified as a supported destination occupant.",
            finding.Cause);
    }

    [Trait("Boundary", "Processing")]
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
        Assert.Null(plan.OwnershipChange);
    }
}
