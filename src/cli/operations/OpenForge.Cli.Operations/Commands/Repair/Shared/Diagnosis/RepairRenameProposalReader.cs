using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal sealed class RepairRenameProposalReader(PhysicalPathResolver paths, RepairTargetReader? targets = null, RenameWeakCandidateFilter? candidateFilter = null)
{
    private readonly RepairTargetReader _targets = targets ?? new();
    internal async ValueTask<RepairProposal?> ReadAsync(CliWorkspace workspace, LocalReferenceObservation reference,
        LocalReferenceDoctorView view, RenameEvidenceResult evidence, CancellationToken token)
    {
        if (reference.Facts.Target.Resolution != SourceLinkTargetResolution.Missing || reference.DestinationLocation is null) return null;
        var resolution = reference.Facts.Target.Path is { } oldPath ? evidence.Find(oldPath) : null;
        var proof = resolution?.Proven;
        RenameSourceMoveTarget? move = null;
        if (proof is null)
            move = await new RenameSourceMoveResolver(paths).ResolveAsync(new(workspace, reference), evidence, token).ConfigureAwait(false);
        var currentPath = proof?.NewPath ?? move?.NewTargetPath;
        if (currentPath is not null)
        {
            var currentTarget = new RepairTargetSelection(Encode(currentPath), reference.Facts.Fragment);
            var facts = await _targets.ResolveAsync(workspace, reference.SourcePath, currentTarget, token).ConfigureAwait(false);
            var canonical = RenameCanonicalEntrypointEvidence.Read(evidence).FirstOrDefault(item => item.Proof.NewPath == currentPath);
            var finalPath = currentPath;
            if (canonical is not null && !evidence.Facts.Candidates.Any(candidate => SourceLogicalPath.ReadParent(candidate.Path) == SourceLogicalPath.ReadParent(currentPath)
                && SourceFormClassifier.TryClassify(candidate.Path, out var form) && SourceFormClassifier.IsEntrypoint(form))) finalPath = canonical.CanonicalPath;
            if (facts.Target.Resolution != SourceLinkTargetResolution.Complete && facts.CanonicalFragment is null)
            {
                if (facts.Target.Resolution != SourceLinkTargetResolution.FragmentMissing) return null;
                var reason = new RepairCandidateEvidence(RepairCandidateEvidenceKind.LiteralContent, "recorded identity", null);
                var candidate = new RepairTargetSelection(Encode(finalPath), null, [reason]) { RenameProof = proof ?? move?.TargetMove ?? move?.SourceMove };
                return new RepairProposal(RepairCatalogueMember.MissingTargetRelink, reference.SourcePath, reference.DestinationLocation,
                    reference.Destination, new RepairCandidateSet([new RepairCandidate(candidate, [reason])
                    { Identity = Identity(proof ?? move?.TargetMove ?? move?.SourceMove) }]))
                {
                    MissingPath = proof?.OldPath ?? move?.OldTargetPath,
                    RenameResolution = RenameResolution.WeakOnly,
                    RenameProof = proof ?? move?.TargetMove,
                    SourceMoveProof = move?.SourceMove,
                };
            }
            var target = new RepairTargetSelection(Encode(finalPath), facts.CanonicalFragment?.Canonical ?? reference.Facts.Fragment)
            { RenameProof = proof ?? move?.TargetMove ?? move?.SourceMove };
            return new RepairProposal(RepairCatalogueMember.MissingTargetRelink, reference.SourcePath, reference.DestinationLocation,
                reference.Destination, RepairDestinationFormatter.Format(reference.SourcePath, target), target)
            {
                RenameProof = proof ?? move?.TargetMove,
                SourceMoveProof = move?.SourceMove,
                MissingPath = proof?.OldPath ?? move?.OldTargetPath,
                RenameResolution = RenameResolution.Proven,
            };
        }
        var candidates = RepairCandidateMapper.Read(reference, FilterWeak(view, reference, evidence, candidateFilter)).Items.ToList();
        foreach (var path in (resolution?.GuidedCandidatePaths ?? [])
            .Concat(resolution?.StrongEvidence.Select(item => item.NewPath) ?? []).Distinct(StringComparer.Ordinal))
        {
            var identity = Identity(resolution?.StrongEvidence.FirstOrDefault(item => item.NewPath == path));
            var existing = candidates.FindIndex(candidate => Uri.UnescapeDataString(candidate.Target.CanonicalTargetPath) == path);
            if (existing >= 0)
            {
                candidates[existing] = candidates[existing] with { Identity = identity };
                continue;
            }
            var reason = new RepairCandidateEvidence(RepairCandidateEvidenceKind.LiteralContent, "recorded identity", null);
            candidates.Add(new(new(Encode(path), reference.Facts.Fragment, [reason]), [reason]) { Identity = identity });
        }
        return new RepairProposal(RepairCatalogueMember.MissingTargetRelink, reference.SourcePath, reference.DestinationLocation,
            reference.Destination, new RepairCandidateSet(candidates.OrderBy(candidate => candidate.Target.CanonicalTargetPath, StringComparer.Ordinal)))
        {
            MissingPath = reference.Facts.Target.Path,
            RenameResolution = resolution?.Resolution ?? RenameResolution.WeakOnly,
            RenameEvidenceTimedOut = evidence.Facts.Git.Availability == RenameGitAvailability.TimedOut
        };
    }

    internal static LocalReferenceDoctorView FilterWeak(LocalReferenceDoctorView view, LocalReferenceObservation reference, RenameEvidenceResult? evidence,
        RenameWeakCandidateFilter? candidateFilter = null)
    {
        if (evidence is not null && candidateFilter is null)
            candidateFilter = new(evidence.Facts.Candidates, evidence.Facts.Candidates);
        return view with
        {
            Candidates = view.Candidates.Where(candidate => candidate.SourcePath != reference.SourcePath || candidate.Destination != reference.Destination
                || candidate.Candidate.Path != reference.SourcePath
                    && candidate.Bases.Any(basis => basis.Kind != LocalReferenceCandidateBasisKind.RouteNeighborhood)
                    && (evidence is null || candidateFilter?.ContainsCandidate(candidate.Candidate.Path) == true)).ToArray(),
        };
    }

    private static string Encode(string path)
        => path.Replace("%", "%25", StringComparison.Ordinal).Replace("#", "%23", StringComparison.Ordinal).Replace("?", "%3F", StringComparison.Ordinal);

    private static RepairCandidateIdentity? Identity(RenameEvidenceObservation? proof)
        => proof is null ? null : new(proof.SameContent, proof.Similarity);
}
