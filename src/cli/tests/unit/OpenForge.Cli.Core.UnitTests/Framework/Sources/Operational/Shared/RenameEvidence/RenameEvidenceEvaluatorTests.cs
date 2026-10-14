using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RenameEvidenceEvaluatorTests
{
    [Theory(DisplayName = "Timed-out Git admits no strong proof even when stale metadata matches")]
    [InlineData("committed"), InlineData("edited"), InlineData("metadata")]
    public void GitTimeoutAdmitsNoStrongProof(string identity)
    {
        var signature = RenameMetadataIdentity.Create("Detailed selection guidance", ["Guidance", "Specific"], []);
        var old = RenameEvidenceTestFacts.Old(body: identity == "metadata" ? null : RenameEvidenceTestFacts.Body, metadata: signature);
        var candidate = RenameEvidenceTestFacts.Candidate(body: RenameEvidenceTestFacts.Body + (identity == "edited" ? "\n" : ""), metadata: signature);
        var facts = RenameEvidenceTestFacts.Facts([candidate], [old]) with
        { Git = RenameGitObservation.Unavailable(RenameGitAvailability.TimedOut) };
        var resolution = Assert.Single(Evaluate(facts).Resolutions);
        Assert.Null(resolution.Proven);
        Assert.Empty(resolution.StrongEvidence);
        Assert.Equal(RenameResolution.WeakOnly, resolution.Resolution);
    }

    [Theory(DisplayName = "Absent Git retains unique stale-metadata proof")]
    [InlineData(RenameGitAvailability.NotRequested), InlineData(RenameGitAvailability.NoRepository)]
    [InlineData(RenameGitAvailability.Unborn), InlineData(RenameGitAvailability.Unavailable)]
    internal void AbsentGitRetainsMetadataProof(RenameGitAvailability availability)
    {
        var signature = RenameMetadataIdentity.Create("Detailed selection guidance", ["Guidance", "Specific"], []);
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate(metadata: signature)], [RenameEvidenceTestFacts.Old(body: null, metadata: signature)]) with
        { Git = RenameGitObservation.Unavailable(availability) };
        Assert.Equal(RenameEvidenceBasis.RecordedMetadata, Assert.IsType<RenameEvidenceObservation>(Assert.Single(Evaluate(facts).Resolutions).Proven).Basis);
    }

    [Theory, InlineData(false, false), InlineData(true, false), InlineData(false, true), InlineData(true, true)]
    public void SuccessorRecordOriginControlsGitProofContradiction(bool edited, bool committedSuccessor)
    {
        var candidate = RenameEvidenceTestFacts.Candidate(body: RenameEvidenceTestFacts.Body + (edited ? "\n" : ""));
        var successor = RenameEvidenceTestFacts.Old(RenameEvidenceTestFacts.NewPath, body: committedSuccessor ? "Other committed identity\n" : null);
        var facts = RenameEvidenceTestFacts.Facts([candidate], [RenameEvidenceTestFacts.Old(), successor]) with
        { MissingPaths = [new(RenameEvidenceTestFacts.OldPath, FileExpectation.Missing(RenameEvidenceTestFacts.Absolute(RenameEvidenceTestFacts.OldPath)))] };
        var resolution = Evaluate(facts).Resolutions[0];
        Assert.Equal(committedSuccessor ? RenameResolution.Contradictory : RenameResolution.Proven, resolution.Resolution);
        Assert.Equal(edited ? RenameEvidenceBasis.GitRename : RenameEvidenceBasis.CommittedContent, Assert.Single(resolution.StrongEvidence).Basis);
    }

    [Theory, InlineData(false), InlineData(true)]
    public void AnySuccessorRecordStillBlocksMetadataOnlyProof(bool committedSuccessor)
    {
        var signature = RenameMetadataIdentity.Create("Detailed selection guidance", ["Guidance", "Specific"], []);
        var old = RenameEvidenceTestFacts.Old(body: null, metadata: signature);
        var successor = RenameEvidenceTestFacts.Old(RenameEvidenceTestFacts.NewPath, body: committedSuccessor ? "Other committed identity\n" : null);
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate(metadata: signature)], [old, successor]) with
        { MissingPaths = [new(old.Path, FileExpectation.Missing(RenameEvidenceTestFacts.Absolute(old.Path)))] };
        Assert.Null(Evaluate(facts).Resolutions[0].Proven);
    }
    [Theory(DisplayName = "Zero one and two strong candidates select manual proven and guided outcomes")]
    [InlineData(0, RenameResolution.NoCandidate), InlineData(1, RenameResolution.Proven), InlineData(2, RenameResolution.SeveralStrongCandidates)]
    internal void Cardinality(int count, RenameResolution expected)
    {
        var candidates = Enumerable.Range(0, count).Select(index => RenameEvidenceTestFacts.Candidate($".agents/guidance/new-{index}.md")).ToImmutableArray();
        var result = Evaluate(RenameEvidenceTestFacts.Facts(candidates, [RenameEvidenceTestFacts.Old()]));
        Assert.Equal(expected, result.Resolutions[0].Resolution);
        Assert.Equal(count, result.Resolutions[0].StrongEvidence.Length);
    }

    [Fact(DisplayName = "A unique strong candidate wins over weak matches and exposes complete expectations")]
    public void StrongWinsOverWeak()
    {
        var weak = Weak(".agents/guidance/weak.md", LocalReferenceCandidateBasisKind.Filename);
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate(), RenameEvidenceTestFacts.Candidate(weak.Candidate.Path, "Different body\n")], [RenameEvidenceTestFacts.Old()])
            with
        { WeakCandidates = [new(RenameEvidenceTestFacts.OldPath, weak)] };
        var proof = Assert.IsType<RenameEvidenceObservation>(Evaluate(facts).Resolutions[0].Proven);
        Assert.Equal(RenameEvidenceBasis.CommittedContent, proof.Basis);
        Assert.Null(proof.Similarity);
        Assert.True(proof.SameContent);
        Assert.Equal(RenameEvidenceTestFacts.Head, proof.Head);
        Assert.Equal(RenameEvidenceTestFacts.Blob, proof.OldBlob);
        Assert.Equal(proof.NewHash, proof.CandidateSnapshot.ContentHash);
        Assert.Equal(RenameEvidenceTestFacts.Absolute(RenameEvidenceTestFacts.OldPath), proof.MissingOldExpectation.LogicalPath);
    }

    [Fact(DisplayName = "Competing old paths cannot both claim the same successor")]
    public void CompetingOldPaths()
    {
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate()], [RenameEvidenceTestFacts.Old(), RenameEvidenceTestFacts.Old(".agents/guidance/other.md")]);
        Assert.All(Evaluate(facts).Resolutions, resolution => Assert.Equal(RenameResolution.Contradictory, resolution.Resolution));
    }

    [Fact(DisplayName = "An existing recorded destination identity contradicts an automatic move")]
    public void ExistingOldIdentity()
    {
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate()], [RenameEvidenceTestFacts.Old(), RenameEvidenceTestFacts.Old(RenameEvidenceTestFacts.NewPath)])
            with
        { MissingPaths = [new(RenameEvidenceTestFacts.OldPath, FileExpectation.Missing(RenameEvidenceTestFacts.Absolute(RenameEvidenceTestFacts.OldPath)))] };
        Assert.Equal(RenameResolution.Contradictory, Evaluate(facts).Resolutions[0].Resolution);
    }

    [Fact(DisplayName = "Physical aliases count once before selecting a successor")]
    public void PhysicalAliases()
    {
        var candidate = RenameEvidenceTestFacts.Candidate();
        var alias = candidate with { Path = ".agents/guidance/alias.md" };
        var result = Evaluate(RenameEvidenceTestFacts.Facts([candidate, alias], [RenameEvidenceTestFacts.Old()]));
        Assert.Equal(RenameResolution.Proven, result.Resolutions[0].Resolution);
        Assert.Single(result.Resolutions[0].StrongEvidence);
    }

    [Theory(DisplayName = "Only independent weak facts nominate and self occurrences never nominate")]
    [InlineData(LocalReferenceCandidateBasisKind.Filename, false, RenameResolution.WeakOnly)]
    [InlineData(LocalReferenceCandidateBasisKind.Title, false, RenameResolution.WeakOnly)]
    [InlineData(LocalReferenceCandidateBasisKind.LiteralContent, false, RenameResolution.WeakOnly)]
    [InlineData(LocalReferenceCandidateBasisKind.RouteNeighborhood, false, RenameResolution.NoCandidate)]
    [InlineData(LocalReferenceCandidateBasisKind.LiteralContent, true, RenameResolution.NoCandidate)]
    internal void WeakAdmission(LocalReferenceCandidateBasisKind basis, bool self, RenameResolution expected)
    {
        var candidate = RenameEvidenceTestFacts.Candidate(body: "Different body\n");
        var facts = RenameEvidenceTestFacts.Facts([candidate], [RenameEvidenceTestFacts.Old()])
            with
        { WeakCandidates = [new(RenameEvidenceTestFacts.OldPath, Weak(candidate.Path, basis, self))] };
        Assert.Equal(expected, Evaluate(facts).Resolutions[0].Resolution);
    }

    [Fact(DisplayName = "An incomplete candidate universe cannot authorize unique proof")]
    public void IncompleteUniverse()
    {
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate()], [RenameEvidenceTestFacts.Old()]) with { CandidateUniverseComplete = false };
        Assert.Equal(RenameResolution.IncompleteUniverse, Evaluate(facts).Resolutions[0].Resolution);
    }

    [Fact(DisplayName = "Unavailable Git never promotes supplied blob content into proof")]
    public void GitUnavailable()
    {
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate()], [RenameEvidenceTestFacts.Old()])
            with
        { Git = RenameGitObservation.Unavailable(RenameGitAvailability.Unavailable) };
        Assert.Equal(RenameResolution.NoCandidate, Evaluate(facts).Resolutions[0].Resolution);
    }

    private static RenameEvidenceResult Evaluate(RenameEvidenceFacts facts) => new RenameEvidenceEvaluator().Evaluate(facts, TestContext.Current.CancellationToken);

    private static LocalReferenceCandidateObservation Weak(string path, LocalReferenceCandidateBasisKind kind, bool self = false)
        => LocalReferenceCandidateObservation.Create(self ? path : ".agents/guidance/source.md", "old.md",
            new SourceLinkIdentity { Id = null, Path = path }, [LocalReferenceCandidateBasis.Create(kind, "matching fact", null)]);
}
