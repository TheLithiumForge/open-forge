using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.Core.UnitTests.Commands.Repair;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class LibraryDoctorClassificationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("RecordMalformed", "BlockedRepair"), InlineData("RecordUnavailable", "Informational")]
    [InlineData("SourceRootInvalid", "BlockedRepair"), InlineData("SourceRootAliased", "BlockedRepair")]
    [InlineData("InventoryIncomplete", "Informational"), InlineData("ProjectionMissing", "ManualDecision")]
    [InlineData("ProjectionDangling", "BlockedRepair"), InlineData("ProjectionRetargeted", "BlockedRepair")]
    [InlineData("PathCollision", "ManualDecision"), InlineData("LinkCapabilityUnsupported", "BlockedRepair")]
    [InlineData("ExtensionCollision", "ManualDecision"), InlineData("RecoverySafeExact", "SafeExact")]
    public void ConsumerClassifiesEveryAcceptedLibraryKindFromTypedProducerFacts(string suffix, string resolution)
    {
        var view = View(suffix);
        ImmutableArray<LibraryResidualEvidence> residuals = suffix == "RecoverySafeExact" ? [LibraryRepairData.Evidence()] : [];
        var recovery = new RecoveryResidualDoctorView(OperationalViewState.Complete,
            [.. residuals.Select(value => RecoveryDoctorCandidateObservation.Create(value.Residual.Candidate, null, value.Residual))], null);
        var report = LibraryDoctorInspector.Inspect(LibraryMutationPlanningData.Workspace, EmptyWorkspace(), view, recovery, residuals);
        Assert.Equal(DoctorDomainKind.WorkspaceEntry, report.Domain);
        var finding = Assert.Single(report.Findings, item => item.Kind == Enum.Parse<DoctorFindingKind>("Library" + suffix));
        Assert.Equal(Enum.Parse<DoctorResolutionLane>(resolution), finding.Resolution);
        Assert.NotEmpty(finding.Evidence);
        if (suffix == "RecoverySafeExact")
        {
            Assert.NotNull(finding.Proposal);
            Assert.Equal(DoctorProposalKind.LibraryResidualRecovery, finding.Proposal.Kind);
            Assert.Null(finding.Proposal.Reference);
            Assert.Same(residuals[0], finding.Proposal.LibraryRecovery);
            Assert.Equal(DoctorProposalVerificationKind.NoFollowPriorState, finding.Proposal.Verification);
            Assert.Equal(DoctorProposalRecoveryKind.VerifiedLibraryResidual, finding.Proposal.Recovery);
        }
    }

    private static LibraryDoctorView View(string scenario)
    {
        var record = LibraryMutationPlanningData.Record(LibraryMutationPlanningData.Leaf);
        var inventory = LibraryMutationPlanningData.Inventory(LibraryMutationPlanningData.Leaf);
        var mapping = LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Current);
        var ownership = LibraryMutationPlanningData.Ownership();
        LibraryLinkCapabilityFact? capability = null;
        var state = OperationalViewState.Complete;
        switch (scenario)
        {
            case "RecordMalformed":
                record = record with { State = LibrariesRecordReadState.Malformed, Record = null, Cause = "Malformed current-v1 record." };
                break;
            case "RecordUnavailable":
                record = record with { State = LibrariesRecordReadState.Unavailable, Record = null, Snapshot = null, Cause = "Record unreadable." };
                state = OperationalViewState.Incomplete;
                break;
            case "SourceRootInvalid":
                inventory = inventory with { Source = inventory.Source with { State = LibrarySourceRootState.Invalid, Cause = "Not an ordinary root." }, Inventory = null };
                break;
            case "SourceRootAliased":
                inventory = inventory with
                {
                    Source = inventory.Source with { State = LibrarySourceRootState.Blocked, PhysicallyDisjoint = false, Cause = "Physical source alias." },
                    Inventory = null,
                };
                break;
            case "InventoryIncomplete":
                inventory = inventory with
                {
                    Inventory = null,
                    UnavailablePaths = [new LibraryInventoryUnavailablePath { Path = ".agents/directives/unreadable.md", Cause = "Inventory read unavailable." }],
                };
                state = OperationalViewState.Incomplete;
                break;
            case "ProjectionMissing":
                mapping = LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, LibraryMappingObservationState.Missing);
                break;
            case "ProjectionDangling": inventory = LibraryMutationPlanningData.Inventory(); break;
            case "ProjectionRetargeted":
                mapping = LibraryMappingObservation.Create(mapping.Mapping,
                    NoFollowLeafObservation.CreateRelativeFileLink(mapping.Leaf.LogicalPath,
                        RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../../shared/other.md")),
                    LibraryMappingObservationState.Changed, null);
                break;
            case "PathCollision":
                ownership = LibraryMutationPlanningData.Ownership(new LifecycleOwnershipClaim(LibraryMutationPlanningData.Leaf, LifecycleOwnershipManager.Framework, "open-forge"));
                break;
            case "ExtensionCollision":
                ownership = LibraryMutationPlanningData.Ownership(new LifecycleOwnershipClaim(LibraryMutationPlanningData.Leaf, LifecycleOwnershipManager.Extension, "toolkit"));
                break;
            case "LinkCapabilityUnsupported": capability = new LibraryLinkCapabilityFact(LibraryLinkCapabilityState.Unsupported, "Previously established platform capability."); break;
        }

        return new LibraryDoctorView
        {
            State = state,
            Ownership = ownership,
            LinkCapability = capability,
            Record = record,
            Inventories = record.Record is null ? [] : [inventory],
            Mappings = record.Record is null ? [] : [mapping],
        };
    }

    private static DoctorDomainReport EmptyWorkspace()
        => new()
        {
            Domain = DoctorDomainKind.WorkspaceEntry,
            Boundary = new DoctorBoundary { Kind = DoctorBoundaryKind.Workspace, Path = LibraryMutationPlanningData.Root },
            Coverage = DoctorCoverageState.Complete,
            Lifecycle = null,
            SourceAvailability = null,
            Limitations = [],
            Findings = [],
            Actions = [],
            Counts = new DoctorFindingCounts
            {
                Resolution = new DoctorResolutionCounts
                {
                    SafeExact = Zero(),
                    GuidedChoice = Zero(),
                    TargetedOperation = Zero(),
                    ManualDecision = Zero(),
                    BlockedRepair = Zero(),
                    Informational = Zero(),
                },
                Severity = new DoctorSeverityCounts { Information = Zero(), Warning = Zero(), Error = Zero() },
            },
        };

    private static DoctorCount Zero() => new() { State = OperationalValueState.Available, Value = 0 };
}
