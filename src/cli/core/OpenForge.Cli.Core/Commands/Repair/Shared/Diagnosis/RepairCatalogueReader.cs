using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal sealed class RepairCatalogueReader(PhysicalPathResolver physicalPathResolver)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<RepairCatalogueRead> ReadAsync(
        CliWorkspace workspace,
        LocalReferenceDoctorView view,
        CancellationToken cancellationToken,
        IReadOnlyList<RepairRelinkRequest>? relinks = null)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(view);
        var findings = new List<RepairFinding>();
        var snapshots = await ReadSnapshotsAsync(workspace, view, findings, cancellationToken)
            .ConfigureAwait(false);
        var proposals = new List<RepairProposalInput>();
        foreach (var reference in view.References
            .OrderBy(value => value.SourcePath, StringComparer.Ordinal)
            .ThenBy(value => value.DestinationLocation?.ByteOffset))
        {
            if (reference.Kind != LocalReferenceKind.Link
                || reference.DestinationLocation is null
                || !snapshots.TryGetValue(reference.SourcePath, out var snapshot))
            {
                continue;
            }

            var proposal = ReadProposal(reference, view);
            if (proposal?.Candidates is not null)
            {
                proposal = await ResolveCandidatesAsync(workspace, proposal, relinks ?? [], cancellationToken).ConfigureAwait(false);
            }
            if (proposal is null)
            {
                continue;
            }

            proposals.Add(new RepairProposalInput(
                proposal,
                new RepairOccurrenceState(
                    reference.SourcePath,
                    reference.DestinationLocation,
                    snapshot),
                reference.Destination));
        }

        findings.AddRange(RepairRemainingFindingReader.Read(view).Where(finding =>
            !proposals.Any(input => input.Proposal.SourceCanonicalPath == finding.SourceCanonicalPath
                && input.Proposal.Occurrence == finding.Occurrence)));
        return new RepairCatalogueRead(proposals, findings);
    }

    private static async ValueTask<RepairProposal> ResolveCandidatesAsync(
        CliWorkspace workspace, RepairProposal proposal, IReadOnlyList<RepairRelinkRequest> relinks, CancellationToken cancellationToken)
    {
        var requested = relinks.SingleOrDefault(relink => relink.SourceCanonicalPath == proposal.SourceCanonicalPath
            && relink.Line == proposal.Occurrence.Line && relink.Column == proposal.Occurrence.Column);
        var candidates = new List<RepairCandidate>();
        var reader = new RepairTargetReader();
        foreach (var candidate in proposal.Candidates?.Items ?? [])
        {
            var target = candidate.Target;
            if (requested is not null
                && Uri.UnescapeDataString(requested.Target.CanonicalTargetPath) == Uri.UnescapeDataString(target.CanonicalTargetPath))
            {
                target = new RepairTargetSelection(requested.Target.CanonicalTargetPath, requested.Target.TargetFragment, candidate.Evidence);
            }

            var facts = await reader.ResolveAsync(workspace, proposal.SourceCanonicalPath, target, cancellationToken).ConfigureAwait(false);
            if (facts.Target.Resolution == SourceLinkTargetResolution.Complete)
            {
                candidates.Add(new RepairCandidate(target, candidate.Evidence, candidate.RecommendedForReview));
            }
        }

        return new RepairProposal(proposal.Member, proposal.SourceCanonicalPath, proposal.Occurrence,
            proposal.ExpectedDestination, new RepairCandidateSet(candidates));
    }

    private static RepairProposal? ReadProposal(
        LocalReferenceObservation reference,
        LocalReferenceDoctorView view)
    {
        if (reference.Canonicalizations.Count != 0
            && (reference.Facts.Target.Resolution == SourceLinkTargetResolution.Complete || reference.Facts.CanonicalFragment is not null)
            && reference.Facts.Target is
            {
                Kind: SourceLinkTargetKind.Local,
                Path: { } targetPath,
                Resolution: SourceLinkTargetResolution.Complete
                    or SourceLinkTargetResolution.FragmentMissing,
            })
        {
            var correction = ReadFinalCorrection(reference.Canonicalizations);
            var target = new RepairTargetSelection(
                EncodeTargetPath(targetPath),
                reference.Facts.CanonicalFragment?.Canonical ?? reference.Facts.Fragment);
            return new RepairProposal(
                ReadMember(correction.Kind),
                reference.SourcePath,
                correction.DestinationLocation,
                correction.Expected,
                correction.Intended,
                target);
        }

        if (reference.Facts.Target is
            {
                Kind: SourceLinkTargetKind.Local,
                Resolution: SourceLinkTargetResolution.Missing,
            })
        {
            return new RepairProposal(
                RepairCatalogueMember.MissingTargetRelink,
                reference.SourcePath,
                reference.DestinationLocation
                    ?? throw new InvalidOperationException("A Repair proposal requires its exact destination location."),
                reference.Destination,
                RepairCandidateMapper.Read(reference, view));
        }

        return null;
    }

    private async ValueTask<IReadOnlyDictionary<string, FileStateSnapshot>> ReadSnapshotsAsync(
        CliWorkspace workspace,
        LocalReferenceDoctorView view,
        List<RepairFinding> findings,
        CancellationToken cancellationToken)
    {
        var snapshots = new Dictionary<string, FileStateSnapshot>(StringComparer.Ordinal);
        foreach (var sourcePath in view.References
            .Select(reference => reference.SourcePath)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal))
        {
            var lexicalPath = Path.Combine(
                workspace.LexicalRoot,
                sourcePath.Replace('/', Path.DirectorySeparatorChar));
            var resolution = _physicalPathResolver.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                lexicalPath);
            if (resolution.State != PhysicalPathState.Contained)
            {
                findings.Add(Incomplete(sourcePath, "The source path is not a contained ordinary file."));
                continue;
            }

            try
            {
                var physicalPath = resolution.GetContainedPhysicalPath();
                var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken)
                    .ConfigureAwait(false);
                snapshots[sourcePath] = FileStateSnapshot.File(lexicalPath, physicalPath, bytes);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                findings.Add(Incomplete(sourcePath, "The complete source bytes are unavailable."));
            }
        }

        return snapshots;
    }

    private static LocalReferenceCanonicalization ReadFinalCorrection(
        IReadOnlyList<LocalReferenceCanonicalization> corrections)
        => corrections
            .OrderBy(correction => correction.Kind == LocalReferenceCanonicalizationKind.Fragment ? 1 : 0)
            .Last();

    private static RepairCatalogueMember ReadMember(LocalReferenceCanonicalizationKind kind)
        => kind switch
        {
            LocalReferenceCanonicalizationKind.Path => RepairCatalogueMember.SameTargetPath,
            LocalReferenceCanonicalizationKind.Case => RepairCatalogueMember.SameTargetCase,
            LocalReferenceCanonicalizationKind.Encoding => RepairCatalogueMember.SameTargetEncoding,
            LocalReferenceCanonicalizationKind.Fragment => RepairCatalogueMember.UniqueCanonicalFragment,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The correction kind is not defined."),
        };

    private static string EncodeTargetPath(string path)
        => path.Replace("%", "%25", StringComparison.Ordinal).Replace("#", "%23", StringComparison.Ordinal)
            .Replace("?", "%3F", StringComparison.Ordinal);

    private static RepairFinding Incomplete(string sourcePath, string cause)
        => new(RepairFindingCode.DiagnosisIncomplete, cause, sourcePath);
}
