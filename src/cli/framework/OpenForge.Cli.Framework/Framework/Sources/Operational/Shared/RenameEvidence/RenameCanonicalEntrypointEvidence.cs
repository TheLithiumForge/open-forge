using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal static class RenameCanonicalEntrypointEvidence
{
    internal static ImmutableArray<RenameCanonicalEntrypointMove> Read(RenameEvidenceResult result)
        => result.Resolutions.Select(resolution => resolution.Proven).OfType<RenameEvidenceObservation>()
            .Where(proof => proof.Basis is RenameEvidenceBasis.CommittedContent or RenameEvidenceBasis.GitRename
                && SourceFormClassifier.Matches(proof.OldPath, SourceDocumentForm.CanonicalEntrypoint)
                && SourceLogicalPath.ReadParent(proof.OldPath) != SourceLogicalPath.ReadParent(proof.NewPath)
                && SourceLogicalPath.ReadFileName(proof.OldPath) == SourceLogicalPath.ReadFileName(proof.NewPath))
            .Select(proof => new RenameCanonicalEntrypointMove(proof,
                SourceLogicalPath.Combine(SourceLogicalPath.ReadParent(proof.NewPath), $"_{SourceLogicalPath.ReadFileName(SourceLogicalPath.ReadParent(proof.NewPath))}.md")))
            .Where(move => move.CanonicalPath != move.Proof.NewPath).ToImmutableArray();
}
