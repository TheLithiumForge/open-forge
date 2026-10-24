using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal static class RenameCanonicalEntrypointEvidence
{
    internal static ImmutableArray<RenameCanonicalEntrypointMove> Read(RenameEvidenceResult result)
        => Read(SourceContentRoots.Default, result);

    internal static ImmutableArray<RenameCanonicalEntrypointMove> Read(WorkspaceLayout layout, RenameEvidenceResult result)
        => Read(new SourceContentRoots(layout), result);

    private static ImmutableArray<RenameCanonicalEntrypointMove> Read(SourceContentRoots roots, RenameEvidenceResult result)
        => result.Resolutions.Select(resolution => resolution.Proven).OfType<RenameEvidenceObservation>()
            .Where(proof => proof.Basis is RenameEvidenceBasis.CommittedContent or RenameEvidenceBasis.GitRename
                && SourceFormClassifier.Matches(roots, proof.OldPath, SourceDocumentForm.CanonicalEntrypoint)
                && SourceLogicalPath.ReadParent(proof.OldPath) != SourceLogicalPath.ReadParent(proof.NewPath)
                && SourceLogicalPath.ReadFileName(proof.OldPath) == SourceLogicalPath.ReadFileName(proof.NewPath))
            .Select(proof => new RenameCanonicalEntrypointMove(proof,
                SourceLogicalPath.Combine(SourceLogicalPath.ReadParent(proof.NewPath), SourceFormClassifier.CanonicalEntrypointFileName(SourceLogicalPath.ReadFileName(SourceLogicalPath.ReadParent(proof.NewPath))))))
            .Where(move => move.CanonicalPath != move.Proof.NewPath).ToImmutableArray();
}
