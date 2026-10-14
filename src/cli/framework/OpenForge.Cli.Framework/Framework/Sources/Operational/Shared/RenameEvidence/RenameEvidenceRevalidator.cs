using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal static class RenameEvidenceRevalidator
{
    // Call ReadAsync again under the command's lease, with fresh inventory, policy and record snapshots.
    internal static bool IsCurrent(RenameEvidenceResult previous, RenameEvidenceResult fresh, string oldPath)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(fresh);
        if (previous.Find(oldPath)?.Proven is not { } before || fresh.Find(oldPath)?.Proven is not { } after) return false;
        return before.NewPath == after.NewPath && before.Basis == after.Basis && before.Head == after.Head
            && before.OldBlob == after.OldBlob && before.OldHash == after.OldHash && before.NewHash == after.NewHash
            && before.Similarity == after.Similarity && before.CandidateSnapshot.Expectation == after.CandidateSnapshot.Expectation
            && before.MissingOldExpectation == after.MissingOldExpectation && before.RecordExpectation == after.RecordExpectation
            && MetadataMatches(before.Metadata, after.Metadata)
            && previous.Facts.PolicyExpectations.SequenceEqual(fresh.Facts.PolicyExpectations);
    }

    private static bool MetadataMatches(RenameMetadataSignature? left, RenameMetadataSignature? right)
        => left is null && right is null || RenameMetadataIdentity.Matches(left, right);
}
