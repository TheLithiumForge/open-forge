using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal sealed class RepairCatalogueReader(PhysicalPathResolver physicalPathResolver, RepairRenameRead? renameRead = null)
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
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
        var protection = await new RepairProtectionReader(_physicalPathResolver).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        if (protection.Policy is not { } policy)
            return new([], [new(RepairFindingCode.DiagnosisIncomplete, "Repair source settings or ownership could not be established.")]);
        var snapshots = await ReadSnapshotsAsync(workspace, view, findings, cancellationToken)
            .ConfigureAwait(false);
        var documents = ReadSnapshotDocuments(snapshots, findings);
        var scope = RepairAuthoredReferenceBoundary.Filter(view.References, documents);
        var evidence = renameRead is null ? null : await renameRead(workspace, cancellationToken).ConfigureAwait(false);
        AddBoundaryFindings(findings, scope.IncompleteSources);
        var proposals = new List<RepairProposalInput>();
        foreach (var reference in scope.References
            .OrderBy(value => value.SourcePath, StringComparer.Ordinal)
            .ThenBy(value => value.DestinationLocation?.ByteOffset))
        {
            if (reference.Kind != LocalReferenceKind.Link
                || reference.DestinationLocation is null)
            {
                continue;
            }

            if (!RepairProtectionReader.Allows(reference.SourcePath, policy))
            {
                if (RepairRemainingFindingReader.Read([reference]).Any())
                    findings.Add(RepairProtectionReader.Finding(reference.SourcePath, reference.DestinationLocation, reference.SourcePath, policy));
                continue;
            }
            if (!snapshots.TryGetValue(reference.SourcePath, out var snapshot)) continue;
            if (reference.Facts.Target.Path is { } originalTarget && !RepairProtectionReader.Allows(originalTarget, policy))
            {
                if (RepairRemainingFindingReader.Read([reference]).Any())
                    findings.Add(RepairProtectionReader.Finding(reference.SourcePath, reference.DestinationLocation, originalTarget, policy));
                continue;
            }

            var proposal = evidence is null ? null : await new RepairRenameProposalReader(_physicalPathResolver).ReadAsync(
                workspace, reference, view, evidence, cancellationToken).ConfigureAwait(false);
            proposal ??= ReadProposal(reference, RepairRenameProposalReader.FilterWeak(view, reference, evidence));
            if (proposal?.Candidates is not null)
            {
                var protectedTarget = proposal.Candidates.Items.FirstOrDefault(candidate =>
                    !RepairProtectionReader.Allows(Uri.UnescapeDataString(candidate.Target.CanonicalTargetPath), policy));
                proposal = await ResolveCandidatesAsync(workspace, proposal, relinks ?? [], policy, cancellationToken).ConfigureAwait(false);
                if (proposal.Candidates is { Items.Count: 0 } && protectedTarget is not null)
                {
                    findings.Add(RepairProtectionReader.Finding(reference.SourcePath, reference.DestinationLocation,
                        Uri.UnescapeDataString(protectedTarget.Target.CanonicalTargetPath), policy));
                    continue;
                }
            }
            if (proposal is null)
            {
                continue;
            }
            if (proposal.Target is { } target && !RepairProtectionReader.Allows(Uri.UnescapeDataString(target.CanonicalTargetPath), policy))
            {
                findings.Add(RepairProtectionReader.Finding(reference.SourcePath, reference.DestinationLocation,
                    Uri.UnescapeDataString(target.CanonicalTargetPath), policy));
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

        findings.AddRange(RepairRemainingFindingReader.Read(scope.References).Where(finding =>
            !findings.Any(existing => existing.Protection && existing.SourceCanonicalPath == finding.SourceCanonicalPath && existing.Occurrence == finding.Occurrence)
            &&
            !proposals.Any(input => input.Proposal.SourceCanonicalPath == finding.SourceCanonicalPath
                && input.Proposal.Occurrence == finding.Occurrence)));
        return new RepairCatalogueRead(proposals, findings)
        {
            RenameEvidence = evidence,
            MissingTargetReferences = scope.References.Where(reference => reference.Kind == LocalReferenceKind.Link
                && reference.DestinationLocation is not null && reference.Facts.Target is { Resolution: SourceLinkTargetResolution.Missing, Path: not null }).ToArray(),
        };
    }

    private static async ValueTask<RepairProposal> ResolveCandidatesAsync(
        CliWorkspace workspace, RepairProposal proposal, IReadOnlyList<RepairRelinkRequest> relinks, RoutedMetadataPolicyContext policy, CancellationToken cancellationToken)
    {
        var requested = relinks.SingleOrDefault(relink => relink.SourceCanonicalPath == proposal.SourceCanonicalPath
            && relink.Line == proposal.Occurrence.Line && relink.Column == proposal.Occurrence.Column);
        var candidates = new List<RepairCandidate>();
        var reader = new RepairTargetReader();
        foreach (var candidate in proposal.Candidates?.Items ?? [])
        {
            var target = candidate.Target;
            if (!RepairProtectionReader.Allows(Uri.UnescapeDataString(target.CanonicalTargetPath), policy)) continue;
            if (requested is not null
                && Uri.UnescapeDataString(requested.Target.CanonicalTargetPath) == Uri.UnescapeDataString(target.CanonicalTargetPath))
            {
                target = new RepairTargetSelection(requested.Target.CanonicalTargetPath, requested.Target.TargetFragment, candidate.Evidence) { RenameProof = candidate.Target.RenameProof };
            }

            var readTarget = target.RenameProof is { } proof && proof.NewPath != Uri.UnescapeDataString(target.CanonicalTargetPath)
                && !File.Exists(Path.Combine(workspace.LexicalRoot, Uri.UnescapeDataString(target.CanonicalTargetPath).Replace('/', Path.DirectorySeparatorChar)))
                ? new RepairTargetSelection(proof.NewPath, target.TargetFragment) : target;
            var facts = await reader.ResolveAsync(workspace, proposal.SourceCanonicalPath, readTarget, cancellationToken).ConfigureAwait(false);
            if (facts.Target.Resolution == SourceLinkTargetResolution.Complete)
            {
                candidates.Add(new RepairCandidate(target, candidate.Evidence, candidate.RecommendedForReview) { Identity = candidate.Identity });
            }
        }

        return new RepairProposal(proposal.Member, proposal.SourceCanonicalPath, proposal.Occurrence,
            proposal.ExpectedDestination, new RepairCandidateSet(candidates))
        {
            MissingPath = proposal.MissingPath,
            RenameResolution = proposal.RenameResolution,
            RenameProof = proposal.RenameProof,
            SourceMoveProof = proposal.SourceMoveProof,
            RenameEvidenceTimedOut = proposal.RenameEvidenceTimedOut
        };
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

    private static IReadOnlyDictionary<string, MarkdownDocumentFacts> ReadSnapshotDocuments(
        IReadOnlyDictionary<string, FileStateSnapshot> snapshots,
        List<RepairFinding> findings)
    {
        var parser = new MarkdownDocumentParser();
        var documents = new Dictionary<string, MarkdownDocumentFacts>(StringComparer.Ordinal);
        foreach (var (sourcePath, snapshot) in snapshots.OrderBy(value => value.Key, StringComparer.Ordinal))
        {
            try
            {
                var document = parser.Parse(StrictUtf8.GetString(snapshot.Bytes.AsSpan()));
                documents.Add(sourcePath, document);
            }
            catch (DecoderFallbackException)
            {
                findings.Add(Incomplete(sourcePath, "The source bytes are not valid strict UTF-8 Markdown."));
            }
            catch (ArgumentException)
            {
                findings.Add(Incomplete(sourcePath, "The current Markdown source boundary is unavailable."));
            }
        }

        return documents;
    }

    private static void AddBoundaryFindings(
        List<RepairFinding> findings,
        IEnumerable<string> sourcePaths)
    {
        foreach (var finding in RepairAuthoredReferenceBoundary.IncompleteFindings(sourcePaths))
        {
            if (!findings.Any(existing =>
                    existing.Code == finding.Code
                    && existing.SourceCanonicalPath == finding.SourceCanonicalPath))
            {
                findings.Add(finding);
            }
        }
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
