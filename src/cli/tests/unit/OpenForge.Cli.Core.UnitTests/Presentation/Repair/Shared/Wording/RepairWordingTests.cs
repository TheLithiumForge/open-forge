using OpenForge.Cli.Core.Presentation.Repair.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Repair.Shared.Wording;

public sealed class RepairWordingTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair guided finding names the exact broken destination"),
     Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void GuidedFindingNamesBrokenDestination()
    {
        Assert.Equal("target.md: Broken link; 4 possible targets",
            RepairWording.GuidedFinding("target.md", 4));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair remaining-problem wording agrees verbs with the problem count"),
     Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void RemainingProblemVerbsAgreeWithCount()
    {
        Assert.Equal(
            "Repaired 1 link. 1 problem still needs a choice.",
            RepairWording.RepairedWithRemaining(1, 1, includesManual: false));
        Assert.Equal(
            "Repaired 2 links. 2 problems still need a choice.",
            RepairWording.RepairedWithRemaining(2, 2, includesManual: false));
        Assert.Equal(
            "Would repair 1 link. 1 problem still needs a choice.",
            RepairWording.WouldRepairWithRemaining(1, 1, includesManual: false));
        Assert.Equal(
            "Nothing could be repaired automatically. 1 problem needs a choice.",
            RepairWording.NothingAutomatic(1, includesManual: false));
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
