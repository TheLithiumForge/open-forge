using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Commands.Repair.Shared.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairC1SelectionTests
{
    [Trait("Boundary", "Processing")]
    [Theory, InlineData("stale", RepairTestData.TargetPath), InlineData("old", ".agents/docs/unadmitted.md")]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void AutomaticSelectionCannotReplaceAnInvalidExplicitTuple(string expected, string target)
    {
        var plan = RepairPlanner.Build(
            RepairTestData.Request(relinks: [RepairTestData.Relink(expected, target)]),
            [RepairTestData.Reference()]);

        Assert.True(plan.IsBlocked);
        Assert.Empty(plan.Effects);
        Assert.DoesNotContain(plan.Selection.Selected, value => value.Origins.Contains(RepairSelectionOrigin.Automatic));
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void MatchingAutomaticAndExplicitAuthorityShareOneEffect()
    {
        var plan = RepairPlanner.Build(
            RepairTestData.Request(relinks: [RepairTestData.Relink("old")]),
            [RepairTestData.Reference()]);

        Assert.False(plan.IsBlocked);
        Assert.Single(plan.Effects);
        var selected = Assert.Single(plan.Selection.Selected);
        Assert.Equal(RepairSelectionMode.AutomaticAndExplicit, plan.Selection.Mode);
        Assert.Contains(RepairSelectionOrigin.Automatic, selected.Origins);
        Assert.Contains(RepairSelectionOrigin.ExplicitRelink, selected.Origins);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void ExplicitAuthorityCanChooseAnAdmittedNonrecommendedCandidate()
    {
        var evidence = new RepairCandidateEvidence(RepairCandidateEvidenceKind.Title, "Alternate title", null);
        var alternate = RepairTestData.Target(".agents/docs/alternate.md", provenance: [evidence]);
        var proposal = new RepairProposal(RepairCatalogueMember.MissingTargetRelink,
            RepairTestData.SourcePath, RepairTestData.Occurrence(), "old", new RepairCandidateSet(
            [
                new RepairCandidate(RepairTestData.Target(), [evidence], recommendedForReview: true),
                new RepairCandidate(alternate, [evidence]),
            ]));
        var input = new RepairProposalInput(proposal,
            new RepairOccurrenceState(RepairTestData.SourcePath, proposal.Occurrence, RepairTestData.FileState("old")), "old");
        var plan = RepairPlanner.Build(
            RepairTestData.Request(relinks: [RepairTestData.Relink("old", alternate.CanonicalTargetPath)]),
            [input]);

        Assert.False(plan.IsBlocked);
        var selected = Assert.Single(plan.Selection.Selected);
        Assert.Equal(alternate.CanonicalTargetPath, selected.Resolution.Target.CanonicalTargetPath);
        Assert.Equal([RepairSelectionOrigin.ExplicitRelink], selected.Origins);
        Assert.Same(evidence, Assert.Single(selected.Resolution.Target.CandidateProvenance));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Conflicting current facts map to the facts-conflicting finding"),
     Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void ConflictingCurrentFactsMapToFinding()
    {
        var occurrenceState = new RepairOccurrenceState(
            RepairTestData.SourcePath,
            RepairTestData.Occurrence(),
            RepairTestData.FileState("old"));
        var first = new RepairProposalInput(
            RepairTestData.SafeProposal(intendedDestination: "new"),
            occurrenceState,
            "old");
        var second = new RepairProposalInput(
            RepairTestData.SafeProposal(intendedDestination: "other"),
            occurrenceState,
            "other");

        var plan = RepairPlanner.Build(RepairTestData.Request(), [first, second]);

        Assert.True(plan.IsBlocked);
        var conflict = Assert.Single(plan.Conflicts);
        Assert.Equal(RepairConflictKind.TargetIdentityMismatch, conflict.Kind);
        var finding = RepairResultBuilder.Conflict(conflict);
        Assert.Equal(RepairFindingCode.FactsConflicting, finding.Code);
        Assert.Equal(RepairTestData.SourcePath, finding.SourceCanonicalPath);
        Assert.Equal(RepairTestData.Occurrence(), finding.Occurrence);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void IndependentlyParsedIdenticalRelinksAreIdempotent()
    {
        var first = RepairRelinkRequest.Parse(".agents/docs/guide.md@1:1", "old", ".agents/docs/new.md#Heading");
        var second = RepairRelinkRequest.Parse(".agents/docs/guide.md@1:1", "old", ".agents/docs/new.md#Heading");
        var request = RepairTestData.Request(relinks: [first, second]);

        var relink = Assert.Single(request.Relinks);
        Assert.Equal("Heading", relink.SelectedTargetFragment);
    }

    [Trait("Boundary", "Processing")]
    [Theory, InlineData("other", ".agents/docs/new.md#Heading"), InlineData("old", ".agents/docs/new.md#Other")]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void IndependentlyParsedContradictoryRelinksRemainInvalid(string expected, string target)
    {
        var first = RepairRelinkRequest.Parse(".agents/docs/guide.md@1:1", "old", ".agents/docs/new.md#Heading");
        var second = RepairRelinkRequest.Parse(".agents/docs/guide.md@1:1", expected, target);

        Assert.Throws<ArgumentException>(() => RepairTestData.Request(relinks: [first, second]));
    }

    [Trait("Boundary", "Processing")]
    [Theory, InlineData("old", true), InlineData("other", false)]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void BinderNormalizesRepeatedTriplesAndRejectsContradictions(string secondExpected, bool valid)
    {
        var symbols = RepairBinding.CreateSymbols();
        var parse = CliCommandTree.Create(CliHelpContent.Empty,
            [new CliRootBranch(symbols.RepairCommand, CliHelpContent.Empty)], []).Parse(
            [
                "repair", "--relink", ".agents/docs/guide.md@1:1", "old", ".agents/docs/new.md#Heading",
                "--relink", ".agents/docs/guide.md@1:1", secondExpected, ".agents/docs/new.md#Heading",
            ]);
        Assert.Empty(parse.Result.Errors);
        var bound = new RepairRequestBinder(symbols).Bind(
            new CliBindingParse(parse.Result, parse.OriginalArguments), RepairTestData.Invocation());

        if (valid)
        {
            Assert.Null(bound.InvalidResult);
            Assert.Single(Assert.IsType<RepairRequest>(bound.Request).Relinks);
        }
        else
        {
            Assert.Null(bound.Request);
            Assert.NotNull(bound.InvalidResult);
        }
    }
}
