using OpenForge.Cli.Core.Presentation.Repair.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Repair.Shared.Wording;

public sealed class RepairWordingTests
{
    [Theory, InlineData(true, null, "same content"), InlineData(false, 98, "content is 98% unchanged"),
        InlineData(false, null, "recorded description and tags match")]
    [Trait("Boundary", "Output"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void CandidateIdentityTakesPrecedenceOverWeakMatches(bool sameContent, int? similarity, string expected)
    {
        var candidate = new Core.Commands.Repair.Models.Selection.RepairCandidate(
            UnitTests.Commands.Repair.RepairTestData.Target(".agents/docs/new.md"),
            [new(Core.Commands.Repair.Models.Selection.RepairCandidateEvidenceKind.Title, "Rounding", null)])
        { Identity = new(sameContent, similarity) };
        Assert.Equal(expected, RepairWording.StrongestCandidateReason(".agents/docs/notes.md", candidate));
    }

    [Theory, InlineData(0, "same file name"), InlineData(1, "same title"), InlineData(2, "matching text")]
    [Trait("Boundary", "Output"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void WeakChoiceReasonsUsePlainWords(int kind, string expected)
    {
        var candidate = new Core.Commands.Repair.Models.Selection.RepairCandidate(
            UnitTests.Commands.Repair.RepairTestData.Target(".agents/docs/new.md"),
            [new((Core.Commands.Repair.Models.Selection.RepairCandidateEvidenceKind)kind, "Rounding", null)]);
        Assert.Equal(expected, RepairWording.StrongestCandidateReason(".agents/docs/notes.md", candidate));
    }

    [Theory(DisplayName = "Repair candidate reasons describe the verified route relationship")]
    [InlineData("parent", ".agents/docs/_docs.md", "The file that lists this source.")]
    [InlineData("sibling", ".agents/docs/new.md", "In the same folder as this link.")]
    [InlineData("sibling", ".agents/docs/nested/_nested.md", "Listed alongside this source in the same route.")]
    [InlineData("child", ".agents/docs/nested/_nested.md", "Listed by this source.")]
    [InlineData("unrecognized", ".agents/docs/new.md", "nearby route: unrecognized.")]
    [Trait("Boundary", "Output"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void CandidateReasonsUsePlainRelationships(string relationship, string target, string expected)
    {
        var candidate = new Core.Commands.Repair.Models.Selection.RepairCandidate(
            UnitTests.Commands.Repair.RepairTestData.Target(target),
            [new(Core.Commands.Repair.Models.Selection.RepairCandidateEvidenceKind.RouteNeighborhood, relationship, null)],
            recommendedForReview: false);
        var proposal = new Core.Commands.Repair.Models.Selection.RepairProposal(
            Core.Commands.Repair.Models.Selection.RepairCatalogueMember.MissingTargetRelink,
            ".agents/docs/guide.md", UnitTests.Commands.Repair.RepairTestData.Occurrence(), "old.md",
            new Core.Commands.Repair.Models.Selection.RepairCandidateSet([candidate]));
        Assert.Equal(expected, RepairWording.CandidateDescription(proposal, candidate));
        Assert.Equal(expected, Assert.Single(RepairWording.CandidateReasons(proposal.SourceCanonicalPath, candidate)));
    }
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair guided finding names the exact broken destination"),
     Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void GuidedFindingNamesBrokenDestination()
    {
        Assert.Equal("target.md: 4 possible targets.",
            RepairWording.GuidedFinding("target.md", 4));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair remaining-problem wording agrees verbs with the problem count"),
     Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void RemainingProblemVerbsAgreeWithCount()
    {
        Assert.Equal(
            "Repaired 1 link. 1 problem needs a choice.",
            RepairWording.RepairedWithRemaining(1, 1, edits: 0));
        Assert.Equal(
            "Repaired 2 links. 2 problems need a choice.",
            RepairWording.RepairedWithRemaining(2, 2, edits: 0));
        Assert.Equal(
            "Would repair 1 link. 1 problem needs a choice.",
            RepairWording.WouldRepairWithRemaining(1, 1, edits: 0));
        Assert.Equal(
            "Nothing could be repaired automatically. 1 problem needs a choice.",
            RepairWording.NothingAutomatic(1, edits: 0));
        Assert.Equal("Nothing could be repaired automatically. 2 problems need a choice and 1 problem needs an edit.", RepairWording.NothingAutomatic(2, 1));
        Assert.Equal("Repaired 1 link. 1 problem needs an edit.", RepairWording.RepairedWithRemaining(1, 0, 1));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair conflicting-facts wording names the disputed occurrence"),
     Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void ConflictingFactsNameTheOccurrence()
    {
        Assert.Equal(
            "The current Repair facts disagree about .agents/docs/guide.md:4:7.",
            RepairWording.FactsConflicting(".agents/docs/guide.md", 4, 7));
    }
}
