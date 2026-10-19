using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Resolution;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Maintenance;

internal static class DoctorRenameFindingFactory
{
    internal static DoctorFinding Create(LocalReferenceObservation reference, RenameEvidenceResult evidence,
        IReadOnlyList<LocalReferenceCandidateObservation> observations, RenameWeakCandidateFilter? candidateFilter = null)
    {
        var oldPath = reference.Facts.Target.Path ?? throw new ArgumentException("The missing reference has no canonical target path.", nameof(reference));
        var resolution = evidence.Find(oldPath) ?? new RenamePathResolution(oldPath,
            evidence.Facts.CandidateUniverseComplete ? RenameResolution.NoCandidate : RenameResolution.IncompleteUniverse, [], [], []);
        if (resolution.Proven is not null) return Create(reference, resolution, evidence);
        // Repair's weak hints belong to one source occurrence. Global identity evidence remains shared.
        var weak = (candidateFilter ?? new RenameWeakCandidateFilter(evidence.Facts.Candidates, evidence.Facts.Candidates)).Read(
            [.. observations.Where(candidate => candidate.SourcePath == reference.SourcePath && candidate.Destination == reference.Destination)
                .Select(candidate => new RenameWeakCandidate(oldPath, candidate))],
            oldPath);
        resolution = resolution with
        {
            Resolution = resolution.Resolution == RenameResolution.NoCandidate && weak.Length > 0 ? RenameResolution.WeakOnly : resolution.Resolution,
            WeakCandidates = weak,
            GuidedCandidatePaths = [.. resolution.GuidedCandidatePaths.Concat(weak.Select(candidate => candidate.Candidate.Path)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
        };
        return Create(reference, resolution, evidence);
    }

    internal static DoctorFinding Create(LocalReferenceObservation reference, RenamePathResolution resolution, RenameEvidenceResult evidence)
    {
        var fragmentFailure = resolution.Proven is { } proven ? ReadFragmentFailure(reference, proven.NewPath, evidence) : null;
        if (resolution.Proven is { } proof && (fragmentFailure is null || fragmentFailure.CanonicalFragment is not null))
        {
            var finalPath = RenameCanonicalEntrypointEvidence.Read(evidence).FirstOrDefault(move => move.Proof == proof)?.CanonicalPath ?? proof.NewPath;
            return Proven(reference, proof, finalPath, fragmentFailure?.CanonicalFragment?.Canonical ?? reference.Facts.Fragment);
        }
        // Repair can offer the proven file without the lost fragment, but weak choices must preserve a valid authored fragment.
        var wholeFileChoice = resolution.Proven is not null && fragmentFailure?.Target.Resolution == SourceLinkTargetResolution.FragmentMissing;
        var candidates = Candidates(reference, resolution, evidence, wholeFileChoice);
        var condition = resolution.Resolution switch
        {
            RenameResolution.SeveralStrongCandidates => DoctorMaintenanceCondition.SeveralStrongCandidates,
            RenameResolution.Proven or RenameResolution.Contradictory or RenameResolution.WeakOnly => DoctorMaintenanceCondition.WeakOnly,
            RenameResolution.NoCandidate => DoctorMaintenanceCondition.NoCandidate,
            RenameResolution.IncompleteUniverse => DoctorMaintenanceCondition.UnsafeBoundary,
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution.Resolution, "The rename resolution is not defined."),
        };
        if (condition != DoctorMaintenanceCondition.UnsafeBoundary && candidates.Items.Count == 0)
            condition = DoctorMaintenanceCondition.NoCandidate;
        if (wholeFileChoice) condition = DoctorMaintenanceCondition.RenamedFragmentMissing;
        var message = condition switch
        {
            DoctorMaintenanceCondition.SeveralStrongCandidates => "Several strong candidates match the missing identity.",
            DoctorMaintenanceCondition.WeakOnly => "The replacement identity is not proven.",
            DoctorMaintenanceCondition.NoCandidate => "No admitted replacement was found.",
            DoctorMaintenanceCondition.UnsafeBoundary => "The candidate universe is incomplete.",
            DoctorMaintenanceCondition.RenamedFragmentMissing => "The file identity is proven, but the authored fragment is missing.",
            _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, "The missing-target condition is not defined."),
        };
        var lane = DoctorResolutionLane.GuidedChoice;
        var action = DoctorMaintenanceGuidance.Command(DoctorNextOperation.Repair, "open-forge repair", message);
        if (condition == DoctorMaintenanceCondition.NoCandidate)
        {
            lane = DoctorResolutionLane.ManualDecision;
            action = DoctorMaintenanceGuidance.Manual(reference.SourcePath);
        }
        if (condition == DoctorMaintenanceCondition.UnsafeBoundary)
        {
            lane = DoctorResolutionLane.BlockedRepair;
            action = DoctorMaintenanceGuidance.Manual(message);
        }
        return LocalReferenceDoctorFindingFactory.Create(reference,
            DoctorDomainSupport.Warning(DoctorFindingKind.ReferenceTargetMissing, message, lane, action), candidates)
            with
        {
            Maintenance = new()
            {
                Condition = condition,
                OldPath = resolution.OldPath,
                NewPath = resolution.Proven?.NewPath,
                Fragment = reference.Facts.Fragment,
                PreserveRenameIdentity = wholeFileChoice && resolution.Proven?.Basis == RenameEvidenceBasis.RecordedMetadata,
            },
        };
    }

    internal static DoctorFinding? SourceMove(LocalReferenceObservation reference, RenameSourceMoveTarget target, RenameEvidenceResult evidence)
    {
        var failure = ReadFragmentFailure(reference, target.NewTargetPath, evidence);
        return failure is null || failure.CanonicalFragment is not null
            ? Proven(reference, target.SourceMove, target.NewTargetPath, failure?.CanonicalFragment?.Canonical ?? reference.Facts.Fragment) : null;
    }

    private static DoctorFinding Proven(LocalReferenceObservation reference, RenameEvidenceObservation proof, string targetPath, string? fragment)
    {
        var message = "One successor has a proven identity.";
        var intended = Destination(reference, targetPath, fragment);
        var finding = LocalReferenceDoctorFindingFactory.Create(reference,
            DoctorDomainSupport.Warning(DoctorFindingKind.ReferenceTargetRenamed, message, DoctorResolutionLane.SafeExact,
                DoctorMaintenanceGuidance.Command(DoctorNextOperation.Repair, "open-forge repair --automatic", "Apply the proven reference correction.")),
            additionalEvidence: [new DoctorComparisonEvidence(intended, reference.Destination),
                new DoctorAuthoredValueEvidence(proof.OldPath, null), new DoctorAuthoredValueEvidence(proof.NewPath, null),
                new DoctorAuthoredValueEvidence(proof.NewHash, null)]);
        return finding with
        {
            Maintenance = new()
            {
                Condition = DoctorMaintenanceCondition.ProvenRename,
                OldPath = proof.OldPath,
                NewPath = proof.NewPath,
                Moved = SourceLogicalPath.ReadParent(proof.OldPath) != SourceLogicalPath.ReadParent(proof.NewPath),
                SameContent = proof.SameContent,
                Similarity = proof.Similarity
            },
            Proposal = new(DoctorProposalKind.ReferenceCanonicalization, new(reference.Destination, intended), libraryRecovery: null)
            {
                Subject = finding.Subject,
                Boundary = new() { Kind = DoctorBoundaryKind.LocalReferenceUniverse, Path = reference.SourcePath },
                Verification = DoctorProposalVerificationKind.ResultingBytes,
                Recovery = DoctorProposalRecoveryKind.RepairReceiptRequired,
            },
        };
    }

    private static SourceLinkDestinationFacts? ReadFragmentFailure(LocalReferenceObservation reference, string targetPath, RenameEvidenceResult evidence)
    {
        var fragment = reference.Facts.Fragment;
        if (fragment is null) return null;
        var target = reference.Facts.Target with { Path = targetPath, Resolution = SourceLinkTargetResolution.Complete };
        if (!SourceDestinationDecoder.TryDecode(fragment, out var decoded, out _))
            return new() { Fragment = fragment, Target = target with { Resolution = SourceLinkTargetResolution.EncodingUnsupported }, Finding = null };
        var document = evidence.Facts.Candidates.FirstOrDefault(candidate => candidate.Path == targetPath)?.Content.Document;
        if (document is null)
            return new() { Fragment = fragment, Target = target with { Resolution = SourceLinkTargetResolution.Unreadable }, Finding = null };
        // This is the same heading and canonical-spelling owner used by Repair's destination resolver.
        return SourceLinkFragmentProjection.ReadFailure(document.Headings, target, fragment, decoded);
    }

    private static DoctorCandidateSet Candidates(LocalReferenceObservation reference, RenamePathResolution resolution, RenameEvidenceResult evidence, bool wholeFileChoice)
    {
        var weak = LocalReferenceDoctorCandidateProjector.Project(reference, resolution.WeakCandidates);
        var items = resolution.GuidedCandidatePaths.Concat(resolution.StrongEvidence.Select(proof => proof.NewPath))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
            .Where(path => wholeFileChoice && path == resolution.Proven?.NewPath || ReadFragmentFailure(reference, path, evidence) is null)
            .Select(path => weak.Items.FirstOrDefault(candidate => candidate.Subject.Path == path) ?? new DoctorCandidate
            {
                Subject = DoctorDomainSupport.Subject(DoctorSubjectKind.Target, path),
                Evidence = [],
                Provenance = DoctorDomainSupport.Provenance(DoctorDomainKind.LocalReferences, DoctorProvenanceSource.LocalReferences, path),
            }).ToArray();
        return new() { Cardinality = items.Length switch { 0 => DoctorCandidateCardinality.None, 1 => DoctorCandidateCardinality.One, _ => DoctorCandidateCardinality.Several }, Items = items };
    }

    private static string Destination(LocalReferenceObservation reference, string path, string? fragment)
    {
        var directory = Path.GetDirectoryName(reference.SourcePath.Replace('/', Path.DirectorySeparatorChar));
        if (string.IsNullOrEmpty(directory)) directory = ".";
        var relative = Path.GetRelativePath(directory, path.Replace('/', Path.DirectorySeparatorChar))
            .Replace(Path.DirectorySeparatorChar, '/');
        relative = SourceDestinationEncoder.EncodePath(relative);
        return fragment is not null ? $"{relative}#{fragment}" : relative;
    }

}
