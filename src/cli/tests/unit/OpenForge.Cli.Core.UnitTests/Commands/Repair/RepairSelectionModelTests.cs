using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairSelectionModelTests
{
    [Fact(DisplayName = "Repair candidate cardinality preserves zero one and several bounded choices"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void CandidateCardinalityIsFiniteAndExact()
    {
        var none = new RepairCandidateSet([]);
        var one = new RepairCandidateSet([Candidate(".agents/docs/one.md", "one.md")]);
        var several = new RepairCandidateSet(
        [
            Candidate(".agents/docs/one.md", "one.md"),
            Candidate(".agents/docs/two.md", "two.md", recommended: true),
        ]);

        Assert.Equal(RepairCandidateCardinality.None, none.Cardinality);
        Assert.Empty(none.Items);
        Assert.Equal(RepairCandidateCardinality.One, one.Cardinality);
        Assert.Equal(RepairCandidateCardinality.Several, several.Cardinality);
        Assert.True(Assert.Single(several.Items, candidate => candidate.RecommendedForReview).RecommendedForReview);
        Assert.All(several.Items, candidate => Assert.NotEmpty(candidate.Evidence));
    }

    [Fact(DisplayName = "Repair candidate sets require unique targets and at most one recommendation"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void CandidateSetRejectsDuplicateTargetsAndRecommendations()
    {
        Assert.Throws<ArgumentException>(() => new RepairCandidateSet(
        [
            Candidate(".agents/docs/one.md", "first"),
            Candidate(".agents/docs/one.md", "second"),
        ]));
        Assert.Throws<ArgumentException>(() => new RepairCandidateSet(
        [
            Candidate(".agents/docs/one.md", "first", recommended: true),
            Candidate(".agents/docs/two.md", "second", recommended: true),
        ]));
        Assert.Throws<ArgumentException>(() => new RepairCandidate(
            RepairTestData.Target(".agents/docs/one.md"),
            [],
            recommendedForReview: false));
    }

    [Fact(DisplayName = "Repair catalogue separates safe-exact and guided proposals and preserves bounded evidence"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void CatalogueSeparatesFiniteProposalLanes()
    {
        var safe = RepairTestData.SafeProposal();
        var candidates = new RepairCandidateSet(
        [
            Candidate(".agents/docs/new.md", "new.md", recommended: true),
            Candidate(".agents/docs/other.md", "other.md"),
        ]);
        var guided = new RepairProposal(
            RepairCatalogueMember.MissingTargetRelink,
            ".agents/docs/missing.md",
            RepairTestData.Occurrence(byteLength: 7),
            "missing.md",
            candidates);

        var catalogue = new RepairCatalogue([guided, safe]);

        Assert.Equal([guided, safe], catalogue.Proposals);
        Assert.Equal([safe], catalogue.SafeExact);
        Assert.Equal([guided], catalogue.Guided);
        Assert.True(safe.IsSafeExact);
        Assert.False(safe.IsGuided);
        Assert.False(guided.IsSafeExact);
        Assert.True(guided.IsGuided);
        Assert.Equal(RepairCandidateCardinality.Several, guided.Candidates!.Cardinality);

        Assert.Throws<ArgumentException>(() => new RepairCatalogue(
            [
                RepairTestData.SafeProposal(sourcePath: ".agents/docs/duplicate.md"),
                RepairTestData.SafeProposal(sourcePath: ".agents/docs/duplicate.md"),
            ]));
    }

    [Fact(DisplayName = "Repair selected proposals preserve safe-exact resolution and reject automatic guided authority"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void SelectedProposalAuthorityIsBounded()
    {
        var safe = RepairTestData.SafeProposal();
        var selectedSafe = new RepairSelectedProposal(
            safe,
            safe.Resolution!,
            [RepairSelectionOrigin.ExplicitRelink]);
        Assert.Equal(RepairSelectionOrigin.ExplicitRelink, Assert.Single(selectedSafe.Origins));
        Assert.Same(safe.Resolution, selectedSafe.Resolution);

        var candidates = new RepairCandidateSet([Candidate(".agents/docs/new.md", "new.md")]);
        var guided = new RepairProposal(
            RepairCatalogueMember.MissingTargetRelink,
            ".agents/docs/missing.md",
            RepairTestData.Occurrence(byteLength: 10),
            "missing.md",
            candidates);
        var selectedGuided = new RepairSelectedProposal(
            guided,
            new RepairProposalResolution(
                "new.md",
                candidates.Items[0].Target),
            [RepairSelectionOrigin.Wizard]);
        Assert.Equal(RepairSelectionOrigin.Wizard, Assert.Single(selectedGuided.Origins));

        Assert.Throws<ArgumentException>(() => new RepairSelectedProposal(
            guided,
            new RepairProposalResolution("new.md", candidates.Items[0].Target),
            [RepairSelectionOrigin.Automatic]));
        Assert.Throws<ArgumentException>(() => new RepairSelectedProposal(
            guided,
            new RepairProposalResolution(
                "other.md",
                RepairTestData.Target(".agents/docs/other.md")),
            [RepairSelectionOrigin.ExplicitRelink]));
        Assert.Throws<ArgumentException>(() => new RepairSelectedProposal(
            safe,
            new RepairProposalResolution("wrong.md", RepairTestData.Target(".agents/docs/wrong.md")),
            [RepairSelectionOrigin.ExplicitRelink]));
    }

    [Fact(DisplayName = "Repair selection keeps selected and unselected proposal occurrences unique and non-overlapping"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void SelectionRejectsDuplicateAndOverlappingOccurrences()
    {
        var selected = RepairTestData.Selected();
        var unselected = new RepairProposal(
            RepairCatalogueMember.SameTargetCase,
            ".agents/docs/other.md",
            RepairTestData.Occurrence(byteOffset: 4, byteLength: 2),
            "ab",
            "AB",
            RepairTestData.Target(".agents/docs/target.md"));
        var valid = new RepairSelection(
            RepairSelectionMode.Automatic,
            [selected],
            [unselected], RepairLibrarySelection.Empty);
        Assert.Same(selected, Assert.Single(valid.Selected));
        Assert.Same(unselected, Assert.Single(valid.Unselected));

        var duplicate = RepairTestData.SafeProposal(
            sourcePath: RepairTestData.SourcePath,
            byteOffset: 0,
            byteLength: 3);
        Assert.Throws<ArgumentException>(() => new RepairSelection(
            RepairSelectionMode.Automatic,
            [selected],
            [duplicate], RepairLibrarySelection.Empty));

        var overlapping = RepairTestData.SafeProposal(
            sourcePath: RepairTestData.SourcePath,
            byteOffset: 2,
            byteLength: 3);
        Assert.Throws<ArgumentException>(() => new RepairSelection(
            RepairSelectionMode.Automatic,
            [selected],
            [overlapping], RepairLibrarySelection.Empty));
    }

    [Fact(DisplayName = "Repairable reference input requires guided provenance and forbids automatic candidate selection"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void RepairableInputPreservesCatalogueSafety()
    {
        var guidedTarget = RepairTestData.Target(
            ".agents/docs/new.md",
            provenance:
            [
                new RepairCandidateEvidence(
                    RepairCandidateEvidenceKind.Filename,
                    "new.md",
                    new SourceLocation(1, 1, 0, 6)),
            ]);
        var guided = new RepairableReferenceInput(
            new RepairOccurrenceState(
                RepairTestData.SourcePath,
                RepairTestData.Occurrence(),
                RepairTestData.FileState("old")),
            new RepairDestinationTransition("old", "old", "new"),
            RepairCatalogueMember.MissingTargetRelink,
            RepairSelectionOrigin.Wizard,
            guidedTarget);
        Assert.Equal(RepairCatalogueMember.MissingTargetRelink, guided.CatalogueMember);
        Assert.NotEmpty(guided.Target.CandidateProvenance);

        Assert.Throws<ArgumentException>(() => new RepairableReferenceInput(
            guided.OccurrenceState,
            guided.DestinationTransition,
            RepairCatalogueMember.MissingTargetRelink,
            RepairSelectionOrigin.Automatic,
            guidedTarget));
        Assert.Throws<ArgumentException>(() => new RepairableReferenceInput(
            guided.OccurrenceState,
            guided.DestinationTransition,
            RepairCatalogueMember.SameTargetPath,
            RepairSelectionOrigin.Wizard,
            guidedTarget));
    }

    private static RepairCandidate Candidate(
        string path,
        string evidenceValue,
        bool recommended = false)
        => new(
            RepairTestData.Target(path),
            [
                new RepairCandidateEvidence(
                    RepairCandidateEvidenceKind.Filename,
                    evidenceValue,
                    new SourceLocation(2, 1, 10, evidenceValue.Length)),
            ],
            recommended);
}
