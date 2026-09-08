using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.Core.UnitTests.Commands.Repair;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class LibraryDoctorProjectionTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void SubjectVocabularyRetainsEveryOriginalAndLibraryKind()
    {
        string[] expected = ["workspace", "path", "route", "generated-region", "source-occurrence", "target", "recovery-item",
            "managed-file", "extension", "dependency", "library", "library-source-root", "library-mapping", "library-projection", "library-residual"];
        Assert.Equal(expected, Enum.GetValues<DoctorSubjectKind>().Select(DoctorFindingWireVocabulary.Subject));
        Assert.Throws<ArgumentOutOfRangeException>(() => DoctorFindingWireVocabulary.Subject((DoctorSubjectKind)int.MaxValue));
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("Current", "current"), InlineData("Missing", "missing"), InlineData("Changed", "changed")]
    [InlineData("Blocked", "blocked"), InlineData("Unavailable", "unavailable")]
    public void JsonRetainsCompleteRegisteredInventoryAndRawProjectionFacts(string state, string wire)
    {
        var observation = Observation(Enum.Parse<LibraryMappingObservationState>(state));
        var json = DoctorLibraryPresentation.Project(observation);
        Assert.Equal(".agents/open-forge.libraries.json", json.RecordPath);
        var root = Assert.Single(json.Roots);
        Assert.NotNull(root.InventoryPaths);
        Assert.Equal([".agents/directives/new.md", ".agents/directives/review.md"], root.InventoryPaths);
        var mapping = Assert.Single(json.Mappings);
        Assert.Equal(wire, mapping.State);
        Assert.Equal(".agents/directives/review.md", mapping.DestinationPath);
        Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md", mapping.ExpectedRelativeLink);
        Assert.Null(json.LinkCapability);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void HumanRetainsRegisteredRootAndProjection()
    {
        var builder = new StringBuilder();
        DoctorLibraryPresentation.Append(builder, Observation(LibraryMappingObservationState.Missing));
        Assert.Contains("shared/team-knowledge", builder.ToString(), StringComparison.Ordinal);
        Assert.Contains(".agents/directives/review.md", builder.ToString(), StringComparison.Ordinal);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("Library"), InlineData("LibrarySourceRoot"), InlineData("LibraryMapping")]
    [InlineData("LibraryProjection"), InlineData("LibraryResidual")]
    public void TypedSubjectKeepsExactlyItsDiscriminatedPayload(string kindName)
    {
        var kind = Enum.Parse<DoctorSubjectKind>(kindName);
        var view = Observation(LibraryMappingObservationState.Current);
        var subject = new DoctorLibrarySubject(kind, LibraryId.Create("team-knowledge"),
            kind == DoctorSubjectKind.Library ? view.Record.Record!.Libraries[0] : null,
            kind == DoctorSubjectKind.LibrarySourceRoot ? view.Inventories[0].Source : null,
            kind == DoctorSubjectKind.LibraryMapping ? view.Mappings[0].Mapping : null,
            kind == DoctorSubjectKind.LibraryProjection ? view.Mappings[0] : null,
            kind == DoctorSubjectKind.LibraryResidual ? LibraryRepairData.Evidence() : null);
        var json = DoctorLibraryPresentation.Subject(subject);
        Assert.Equal("team-knowledge", json.LibraryId);
        if (kind == DoctorSubjectKind.LibraryResidual)
        {
            Assert.Equal(0, json.EntryOrdinal);
            Assert.NotNull(json.BundlePath);
            Assert.EndsWith("observed-residual.zip", json.BundlePath, StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal("shared/team-knowledge", json.SourceRoot);
            Assert.Null(json.BundlePath);
        }
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("Library"), InlineData("LibrarySourceRoot"), InlineData("LibraryMapping")]
    [InlineData("LibraryProjection"), InlineData("LibraryResidual")]
    public void TypedSubjectRejectsMissingPayload(string kind)
        => Assert.Throws<ArgumentException>(() => new DoctorLibrarySubject(Enum.Parse<DoctorSubjectKind>(kind),
            LibraryId.Create("team-knowledge"), null, null, null, null, null));

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void UndefinedSubjectAndProposalKindsFailClosed()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DoctorLibrarySubject((DoctorSubjectKind)int.MaxValue,
            LibraryId.Create("team-knowledge"), null, null, null, null, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => Proposal((DoctorProposalKind)int.MaxValue));
        Assert.Throws<ArgumentException>(() => Proposal(DoctorProposalKind.LibraryResidualRecovery));
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void ExactRecoveryProjectionRetainsEntryProofWithoutReferenceSurrogate()
    {
        var evidence = LibraryRepairData.Evidence();
        var proposal = DoctorLibraryRecoveryPresentation.Project(evidence);
        Assert.Equal("team-knowledge", proposal.LibraryId);
        Assert.Equal("relative-file-link-create", proposal.EntryKind);
        Assert.Equal("intended", proposal.Comparison);
        Assert.Equal(0, proposal.EntryOrdinal);
        Assert.Equal(evidence.Entry.Input.Context.LogicalPath, proposal.LogicalPath);
        Assert.Equal(evidence.Residual.Candidate.Path, proposal.BundlePath);
        Assert.False(proposal.VerifiedPriorRecord);
    }

    private static DoctorExactProposal Proposal(DoctorProposalKind kind)
        => new(kind, reference: null, libraryRecovery: null)
        {
            Subject = new DoctorSubject { Kind = DoctorSubjectKind.LibraryResidual, Path = null, Identifier = "team-knowledge", Location = null },
            Boundary = new DoctorBoundary { Kind = DoctorBoundaryKind.Workspace, Path = LibraryMutationPlanningData.Root },
            Verification = DoctorProposalVerificationKind.NoFollowPriorState,
            Recovery = DoctorProposalRecoveryKind.VerifiedLibraryResidual,
        };

    private static LibraryDoctorView Observation(LibraryMappingObservationState state)
        => new()
        {
            State = OperationalViewState.Complete,
            Ownership = LibraryMutationPlanningData.Ownership(),
            LinkCapability = null,
            Record = LibraryMutationPlanningData.Record(LibraryMutationPlanningData.Leaf),
            Inventories = [LibraryMutationPlanningData.Inventory(".agents/directives/new.md", LibraryMutationPlanningData.Leaf)],
            Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, state)],
        };
}
