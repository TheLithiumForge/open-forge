using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairRenameSelectionTests
{
    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void ProvenRelinksUseOriginalUtf8SpansAndOneFinalFileEffect()
    {
        const string body = "Å Read [A](old-a.md#kept) and [B](old-b.md#kept).\r\nKeep this authored line.\r\n";
        var inputs = new[] { "a", "b" }.Select(name =>
        {
            var old = $"old-{name}.md#kept";
            var destination = $"new-{name}.md#kept";
            var targetPath = $".agents/docs/new-{name}.md";
            var candidate = RepairTestData.FileState("# Kept\n\nAuthored content.\n", targetPath);
            var proof = new RenameEvidenceObservation
            {
                OldPath = $".agents/docs/old-{name}.md",
                NewPath = targetPath,
                Basis = RenameEvidenceBasis.CommittedContent,
                Head = null,
                OldBlob = null,
                OldHash = candidate.ContentHash,
                NewHash = candidate.ContentHash!,
                Similarity = null,
                SameContent = true,
                Metadata = null,
                CandidateSnapshot = candidate,
                MissingOldExpectation = FileExpectation.Missing(Path.Combine(RepairTestData.WorkspaceRoot, $".agents/docs/old-{name}.md")),
                RecordExpectation = null,
            };
            var offset = System.Text.Encoding.UTF8.GetByteCount(body[..body.IndexOf(old, StringComparison.Ordinal)]);
            var occurrence = RepairTestData.Occurrence(offset, System.Text.Encoding.UTF8.GetByteCount(old));
            var target = RepairTestData.Target(targetPath, "kept") with { RenameProof = proof };
            var proposal = new RepairProposal(RepairCatalogueMember.MissingTargetRelink, RepairTestData.SourcePath, occurrence, old, destination, target) { RenameProof = proof };
            return new RepairProposalInput(proposal, new(RepairTestData.SourcePath, occurrence, RepairTestData.FileState(body)), old);
        }).ToArray();
        var plan = RepairPlanner.Build(RepairTestData.Request(), inputs);
        Assert.False(plan.IsBlocked);
        Assert.Equal(2, plan.Steps.Count);
        var effect = Assert.Single(plan.Effects);
        Assert.Equal(2, effect.Changes.Count);
        Assert.Equal(body.Replace("old-a.md", "new-a.md", StringComparison.Ordinal).Replace("old-b.md", "new-b.md", StringComparison.Ordinal), System.Text.Encoding.UTF8.GetString(effect.IntendedState.Bytes.AsSpan()));
    }
    [Theory, InlineData(0), InlineData(1), InlineData(2)]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void UniqueIdentitySelectsWithoutCandidateAuthority(int basisValue)
    {
        var basis = (RenameEvidenceBasis)basisValue;
        var candidate = RepairTestData.FileState("# Destination\n\nKeep this content.\n", RepairTestData.TargetPath);
        var proof = new RenameEvidenceObservation
        {
            OldPath = ".agents/docs/old.md",
            NewPath = RepairTestData.TargetPath,
            Basis = basis,
            Head = null,
            OldBlob = null,
            OldHash = candidate.ContentHash,
            NewHash = candidate.ContentHash ?? throw new InvalidOperationException(),
            Similarity = basis == RenameEvidenceBasis.GitRename ? 90 : null,
            SameContent = basis == RenameEvidenceBasis.CommittedContent,
            Metadata = null,
            CandidateSnapshot = candidate,
            MissingOldExpectation = FileExpectation.Missing(Path.Combine(RepairTestData.WorkspaceRoot, ".agents/docs/old.md")),
            RecordExpectation = null,
        };
        var target = RepairTestData.Target(fragment: "kept") with { RenameProof = proof };
        var proposal = new RepairProposal(RepairCatalogueMember.MissingTargetRelink, RepairTestData.SourcePath,
            RepairTestData.Occurrence(byteLength: 11), "old.md#kept", "new.md#kept", target)
        { RenameProof = proof };
        var input = new RepairProposalInput(proposal, new(RepairTestData.SourcePath, proposal.Occurrence, RepairTestData.FileState("old.md#kept")), "old.md#kept");
        var plan = RepairPlanner.Build(RepairTestData.Request(), [input]);
        Assert.False(plan.IsBlocked);
        var selected = Assert.Single(plan.Selection.Selected);
        Assert.Equal(RepairSelectionOrigin.Automatic, Assert.Single(selected.Origins));
        Assert.Null(selected.Proposal.Candidates);
        Assert.Equal("kept", selected.Resolution.Target.TargetFragment);
        Assert.Equal("new.md#kept", System.Text.Encoding.UTF8.GetString(Assert.Single(plan.Effects).IntendedState.Bytes.AsSpan()));
    }

    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void UnprovenEmptyCandidateSetIsManualAndCannotBeAutomaticallySelected()
    {
        var proposal = new RepairProposal(RepairCatalogueMember.MissingTargetRelink, RepairTestData.SourcePath,
            RepairTestData.Occurrence(byteLength: 6), "old.md", new RepairCandidateSet([]));
        var input = new RepairProposalInput(proposal, new(RepairTestData.SourcePath, proposal.Occurrence, RepairTestData.FileState("old.md")), "old.md");
        var plan = RepairPlanner.Build(RepairTestData.Request(), [input]);
        Assert.False(proposal.IsGuided);
        Assert.False(proposal.IsSafeExact);
        Assert.Empty(plan.Selection.Selected);
        Assert.Empty(plan.Effects);
        Assert.Single(plan.Selection.Unselected);
    }
}
