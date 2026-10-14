using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RenameMetadataIdentityTests
{
    private static RenameMetadataSignature Signature => RenameMetadataIdentity.Create("Specific authored rounding policy", ["Guidance", "Billing"], ["src/**", "tests/**"]);

    [Fact(DisplayName = "Unique recorded metadata proves an edited successor without Git")]
    public void UniqueSignature()
    {
        var signature = Signature;
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate(metadata: signature)], [RenameEvidenceTestFacts.Old(body: null, metadata: signature)])
            with
        { Git = RenameGitObservation.Unavailable(RenameGitAvailability.Unavailable) };
        var proof = Assert.IsType<RenameEvidenceObservation>(Evaluate(facts).Resolutions[0].Proven);
        Assert.Equal(RenameEvidenceBasis.RecordedMetadata, proof.Basis);
        Assert.Equal(signature, proof.Metadata);
        Assert.Null(proof.OldHash);
        Assert.Null(proof.OldBlob);
        Assert.False(proof.SameContent);
    }

    [Theory(DisplayName = "Metadata uniqueness includes every current candidate and available old identity")]
    [InlineData(false), InlineData(true)]
    public void DuplicateSignatures(bool old)
    {
        var signature = Signature;
        var facts = RenameEvidenceTestFacts.Facts(
            [RenameEvidenceTestFacts.Candidate(metadata: signature)], [RenameEvidenceTestFacts.Old(body: null, metadata: signature)]);
        facts = old
            ? facts with { OldIdentities = facts.OldIdentities.Add(RenameEvidenceTestFacts.Old(".agents/guidance/another.md", body: null, metadata: signature)) }
            : facts with { Candidates = facts.Candidates.Add(RenameEvidenceTestFacts.Candidate(".agents/guidance/another.md", metadata: signature)) };
        Assert.Null(Evaluate(facts).Resolutions[0].Proven);
    }

    [Fact(DisplayName = "applyTo set is normalized but tag order remains part of recorded identity")]
    public void PatternsAndTags()
    {
        var equivalent = RenameMetadataIdentity.Create(Signature.Description, Signature.Tags, ["tests/**", "src/**", "src/**"]);
        Assert.True(RenameMetadataIdentity.Matches(Signature, equivalent));
        var reordered = RenameMetadataIdentity.Create(Signature.Description, ["Billing", "Guidance"], Signature.ApplyTo);
        Assert.False(RenameMetadataIdentity.Matches(Signature, reordered));
    }

    [Theory(DisplayName = "Filename title and empty-tag labels never become recorded metadata identity")]
    [InlineData("old", false), InlineData("Old", false), InlineData("guidance/old", false)]
    [InlineData("Different title", false), InlineData("Specific authored rounding policy", true)]
    public void WeakLabels(string description, bool emptyTags)
    {
        var signature = RenameMetadataIdentity.Create(description, emptyTags ? [] : ["Guidance"], []);
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate(metadata: signature)], [RenameEvidenceTestFacts.Old(body: null, metadata: signature)]);
        Assert.Null(Evaluate(facts).Resolutions[0].Proven);
    }

    [Fact(DisplayName = "Repeated records for the same old path are occurrences rather than competing identities")]
    public void RepeatedOldOccurrences()
    {
        var old = RenameEvidenceTestFacts.Old(body: null, metadata: Signature);
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate(metadata: Signature)], [old, old]);
        Assert.Equal(RenameResolution.Proven, Evaluate(facts).Resolutions[0].Resolution);
    }

    [Fact(DisplayName = "All proof bases are combined before cardinality without preferring an exact-looking candidate")]
    public void CombinedBases()
    {
        var old = RenameEvidenceTestFacts.Old(metadata: Signature);
        var facts = RenameEvidenceTestFacts.Facts([
            RenameEvidenceTestFacts.Candidate(),
            RenameEvidenceTestFacts.Candidate(".agents/guidance/edited.md", body: "Entirely edited body\n", metadata: Signature)], [old]);
        Assert.Equal(RenameResolution.SeveralStrongCandidates, Evaluate(facts).Resolutions[0].Resolution);
        Assert.Equal([RenameEvidenceBasis.RecordedMetadata, RenameEvidenceBasis.CommittedContent],
            Evaluate(facts).Resolutions[0].StrongEvidence.Select(proof => proof.Basis));
    }

    private static RenameEvidenceResult Evaluate(RenameEvidenceFacts facts)
        => new RenameEvidenceEvaluator().Evaluate(facts, TestContext.Current.CancellationToken);
}
