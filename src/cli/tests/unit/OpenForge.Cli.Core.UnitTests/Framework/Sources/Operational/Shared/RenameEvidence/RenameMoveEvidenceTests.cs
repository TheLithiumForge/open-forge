using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RenameMoveEvidenceTests
{
    [Theory(DisplayName = "Exact content remains provable beyond either scored-comparison bound")]
    [InlineData(false), InlineData(true)]
    public void LargeExactContent(bool bytes)
    {
        var body = bytes ? new string('x', 1024 * 1024 + 1) : string.Concat(Enumerable.Repeat("Authored line\n", 2001));
        var result = Evaluate(RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate(body: body)], [RenameEvidenceTestFacts.Old(body: body)]));
        Assert.Equal(RenameEvidenceBasis.CommittedContent, result.Resolutions[0].Proven?.Basis);
        Assert.True(result.Resolutions[0].Proven?.SameContent);
        Assert.Null(result.Resolutions[0].Proven?.Similarity);
    }

    [Theory(DisplayName = "Only per-file content proof proposes canonical entrypoint preservation for a folder rename")]
    [InlineData("_billing.md", false, true), InlineData("_billing.md", true, false), InlineData("index.md", false, false)]
    public void CanonicalEntrypoint(string leaf, bool metadataOnly, bool expected)
    {
        var signature = RenameMetadataIdentity.Create("Specific authored rules for invoices", ["Guidance"], []);
        var old = RenameEvidenceTestFacts.Old($".agents/guidance/billing/{leaf}", body: metadataOnly ? null : RenameEvidenceTestFacts.Body, metadata: signature);
        var current = RenameEvidenceTestFacts.Candidate($".agents/guidance/invoices/{leaf}", metadata: signature);
        var moves = RenameCanonicalEntrypointEvidence.Read(Evaluate(RenameEvidenceTestFacts.Facts([current], [old])));
        Assert.Equal(expected ? 1 : 0, moves.Length);
        if (expected) Assert.Equal(".agents/guidance/invoices/_invoices.md", moves[0].CanonicalPath);
    }

    [Theory(DisplayName = "Self evidence stays excluded through physical aliases and overwrite layers")]
    [InlineData(false), InlineData(true)]
    public void AliasedSelfEvidence(bool overwrite)
    {
        var candidate = RenameEvidenceTestFacts.Candidate(body: "Unrelated body\n");
        var alias = candidate with { Path = ".agents/guidance/alias.md" };
        var sourcePath = overwrite ? SourceOverwritePath.ReadAdjacentPath(candidate.Path) : alias.Path;
        var weak = LocalReferenceCandidateObservation.Create(sourcePath, "old.md", new SourceLinkIdentity { Id = null, Path = candidate.Path },
            [LocalReferenceCandidateBasis.Create(LocalReferenceCandidateBasisKind.LiteralContent, "Matching label", null)]);
        var facts = RenameEvidenceTestFacts.Facts([candidate, alias], [RenameEvidenceTestFacts.Old()])
            with
        { WeakCandidates = [new(RenameEvidenceTestFacts.OldPath, weak)] };
        Assert.Equal(RenameResolution.NoCandidate, Evaluate(facts).Resolutions[0].Resolution);
    }

    [Theory(DisplayName = "The score boundary determines Git proof without calling edited bytes the same content")]
    [InlineData(89, false), InlineData(90, true), InlineData(100, true)]
    public void GitScoreAdmission(int score, bool expected)
    {
        var body = string.Concat(Enumerable.Range(0, 100).Select(index => $"old-{index:000}\n"));
        var current = string.Concat(Enumerable.Range(0, 100).Select(index => index < score ? $"old-{index:000}\n" : $"new-{index:000}\n"));
        var result = Evaluate(RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate(body: current)], [RenameEvidenceTestFacts.Old(body: body)]));
        Assert.Equal(expected, result.Resolutions[0].Proven is not null);
        if (score == 90)
        {
            Assert.Equal(RenameEvidenceBasis.GitRename, result.Resolutions[0].Proven?.Basis);
            Assert.Equal(90, result.Resolutions[0].Proven?.Similarity);
            Assert.False(result.Resolutions[0].Proven?.SameContent);
        }
    }

    [Fact(DisplayName = "HEAD changes invalidate previously selected evidence even if blob bytes still match")]
    public void HeadExpectation()
    {
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate()], [RenameEvidenceTestFacts.Old()]);
        var before = Evaluate(facts);
        const string nextHead = "cccccccccccccccccccccccccccccccccccccccc";
        var fresh = Evaluate(facts with { Git = facts.Git with { Head = nextHead }, OldIdentities = [facts.OldIdentities[0] with { Head = nextHead }] });
        Assert.NotNull(fresh.Resolutions[0].Proven);
        Assert.False(RenameEvidenceRevalidator.IsCurrent(before, fresh, RenameEvidenceTestFacts.OldPath));
    }

    private static RenameEvidenceResult Evaluate(RenameEvidenceFacts facts)
        => new RenameEvidenceEvaluator().Evaluate(facts, TestContext.Current.CancellationToken);
}
