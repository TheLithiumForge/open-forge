using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal sealed class RenameEvidenceEvaluator(Action<string, string>? contentComparisonStarted = null)
{
    internal RenameEvidenceResult Evaluate(RenameEvidenceFacts facts, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(facts);
        var aliases = facts.Candidates;
        var physicalComparer = PhysicalIdentityTracker.PathComparer;
        var candidates = facts.Candidates.OrderBy(candidate => candidate.Path, StringComparer.Ordinal)
            .DistinctBy(candidate => candidate.Snapshot.PhysicalPath, physicalComparer).ToImmutableArray();
        var oldByPath = facts.OldIdentities.ToLookup(old => old.Path, StringComparer.Ordinal);
        candidates = candidates.Select(candidate => candidate with
        {
            IsUnchangedAtHead = facts.Git.Availability == RenameGitAvailability.Available
                && oldByPath[candidate.Path].Any(old => old.Head == facts.Git.Head && old.Blob is not null
                    && old.Content?.NormalizedHash == candidate.Content.NormalizedHash),
        }).ToImmutableArray();
        facts = facts with { Candidates = candidates };
        var weakFilter = new RenameWeakCandidateFilter(candidates, aliases);
        var resolutions = ImmutableArray.CreateBuilder<RenamePathResolution>();
        foreach (var missing in facts.MissingPaths.DistinctBy(missing => missing.Path).OrderBy(missing => missing.Path, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var strong = ImmutableArray.CreateBuilder<RenameEvidenceObservation>();
            foreach (var candidate in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var observations = oldByPath[missing.Path]
                    .SelectMany(old => ReadStrong(facts, missing, old, candidate, cancellationToken));
                var strongest = observations.OrderBy(observation => Priority(observation.Basis)).FirstOrDefault();
                if (strongest is not null) strong.Add(strongest);
            }

            var weak = weakFilter.Read(facts.WeakCandidates, missing.Path);
            var guidedPaths = weak.Select(observation => observation.Candidate.Path)
                .Concat(candidates.Where(candidate => oldByPath[missing.Path].Any(old =>
                    RenameMetadataIdentity.Matches(old.Metadata, candidate.Metadata) && RenameMetadataIdentity.IsDistinctLabel(old, candidate)))
                    .Select(candidate => candidate.Path))
                .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToImmutableArray();
            resolutions.Add(new RenamePathResolution(missing.Path, Resolve(strong.Count, guidedPaths.Length, facts.CandidateUniverseComplete), strong.ToImmutable(), weak, guidedPaths));
        }

        var complete = resolutions.ToImmutable();
        var checkedMappings = complete.Select(resolution => HasContradiction(resolution, complete, facts.OldIdentities)
            ? resolution with { Resolution = RenameResolution.Contradictory }
            : resolution).ToImmutableArray();
        return new RenameEvidenceResult(facts with { Candidates = candidates }, checkedMappings);
    }

    private IEnumerable<RenameEvidenceObservation> ReadStrong(
        RenameEvidenceFacts facts,
        RenameMissingPath missing,
        RenameRecordedIdentity old,
        RenameCurrentCandidate candidate,
        CancellationToken cancellationToken)
    {
        // A timeout leaves committed successor identities unknown, including identities that contradict stale metadata.
        if (facts.Git.Availability == RenameGitAvailability.TimedOut) yield break;

        if (!candidate.IsUnchangedAtHead && facts.Git.Availability == RenameGitAvailability.Available && old.Head == facts.Git.Head && old.Blob is not null
            && old.Content is { HasAuthoredContent: true } content && candidate.Content.HasAuthoredContent)
        {
            contentComparisonStarted?.Invoke(old.Path, candidate.Path);
            cancellationToken.ThrowIfCancellationRequested();
            if (content.NormalizedHash == candidate.Content.NormalizedHash)
            {
                yield return Observation(RenameEvidenceBasis.CommittedContent, similarity: null, sameContent: true);
            }
            else if (RenameContentComparer.CouldReachThreshold(content, candidate.Content, cancellationToken))
            {
                var comparison = RenameContentComparer.Compare(content, candidate.Content, cancellationToken);
                if (comparison.IsStrong)
                {
                    yield return Observation(RenameEvidenceBasis.GitRename, comparison.Similarity, comparison.SameContent);
                }
            }
        }

        if (RenameMetadataIdentity.Matches(old.Metadata, candidate.Metadata) && RenameMetadataIdentity.IsDistinctLabel(old, candidate)
            && facts.Candidates.Count(current => RenameMetadataIdentity.Matches(current.Metadata, old.Metadata)) == 1
            && facts.OldIdentities.Where(record => RenameMetadataIdentity.Matches(record.Metadata, old.Metadata)).Select(record => record.Path).Distinct().Count() == 1
            && !facts.OldIdentities.Any(record => record.Path == candidate.Path && record.Path != old.Path))
        {
            yield return Observation(RenameEvidenceBasis.RecordedMetadata, similarity: null, sameContent: false);
        }

        RenameEvidenceObservation Observation(RenameEvidenceBasis basis, int? similarity, bool sameContent)
            => new()
            {
                OldPath = missing.Path,
                NewPath = candidate.Path,
                Basis = basis,
                Head = old.Head,
                OldBlob = old.Blob,
                OldHash = old.Content?.Hash,
                NewHash = candidate.Content.Hash,
                Similarity = similarity,
                SameContent = sameContent,
                Metadata = old.Metadata,
                CandidateSnapshot = candidate.Snapshot,
                MissingOldExpectation = missing.Expectation,
                RecordExpectation = old.RecordExpectation,
            };
    }

    // A current Entries row is not a committed successor identity. Metadata-only proof still needs every record to agree.
    private static bool HasContradiction(RenamePathResolution resolution, ImmutableArray<RenamePathResolution> all, ImmutableArray<RenameRecordedIdentity> records)
        => resolution.Resolution == RenameResolution.Proven
            && resolution.Proven is { } proof
            && (all.Any(other => other.OldPath != resolution.OldPath && other.StrongEvidence.Any(evidence => evidence.NewPath == proof.NewPath))
                || records.Any(record => record.Path == proof.NewPath && record.Path != proof.OldPath
                    && (proof.Basis == RenameEvidenceBasis.RecordedMetadata || record.Blob is not null)));

    private static RenameResolution Resolve(int strong, int weak, bool complete)
    {
        if (!complete) return RenameResolution.IncompleteUniverse;
        if (strong > 1) return RenameResolution.SeveralStrongCandidates;
        if (strong == 1) return RenameResolution.Proven;
        return weak > 0 ? RenameResolution.WeakOnly : RenameResolution.NoCandidate;
    }

    private static int Priority(RenameEvidenceBasis basis) => basis switch
    {
        RenameEvidenceBasis.CommittedContent => 0,
        RenameEvidenceBasis.GitRename => 1,
        RenameEvidenceBasis.RecordedMetadata => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(basis), basis, "The rename evidence basis is not defined."),
    };
}
