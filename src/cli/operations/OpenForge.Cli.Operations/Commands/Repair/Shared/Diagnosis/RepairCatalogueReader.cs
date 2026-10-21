using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Shared.Acquisition;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal sealed class RepairCatalogueReader(PhysicalPathResolver physicalPathResolver, RepairRenameRead? renameRead = null,
    RepairRenameReferenceRead? renameReferenceRead = null, bool retainDiagnosisEvidence = false, RepairTargetReader? targets = null)
{
    internal const int MaximumConcurrentProposals = 16;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<RepairCatalogueRead> ReadAsync(
        CliWorkspace workspace,
        LocalReferenceDoctorView view,
        CancellationToken cancellationToken,
        IReadOnlyList<RepairRelinkRequest>? relinks = null,
        DoctorMaintenanceObservation? maintenance = null)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(view);
        var findings = new List<RepairFinding>();
        var protection = await new RepairProtectionReader(_physicalPathResolver).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        if (protection.Policy is not { } policy)
            return new([], [new(RepairFindingCode.DiagnosisIncomplete, "Repair source settings or ownership could not be established.")]);
        var retainedEvidence = retainDiagnosisEvidence ? maintenance?.Renames : null;
        var snapshots = await ReadSnapshotsAsync(workspace, view, findings, cancellationToken)
            .ConfigureAwait(false);
        var documents = ReadSnapshotDocuments(snapshots, view.Documents, findings);
        var scope = RepairAuthoredReferenceBoundary.Filter(view.References, documents);
        RenameEvidenceResult? evidence = retainedEvidence;
        if (!retainDiagnosisEvidence || maintenance is null)
        {
            if (renameReferenceRead is not null)
                evidence = await renameReferenceRead(workspace, view, cancellationToken).ConfigureAwait(false);
            else if (renameRead is not null)
                evidence = await renameRead(workspace, cancellationToken).ConfigureAwait(false);
        }
        var catalogue = view.Catalogue
            ?? (await new SourceReadSessionReader(_physicalPathResolver).ReadAsync(workspace, cancellationToken).ConfigureAwait(false)).Catalogue;
        var targetAcquisition = new SourceFileAcquisition();
        foreach (var document in view.Documents.Values) targetAcquisition.RetainDocument(document);
        var targetReader = targets ?? new RepairTargetReader(catalogue, acquisition: targetAcquisition);
        var candidateFilter = evidence is null ? null : new RenameWeakCandidateFilter(evidence.Facts.Candidates, evidence.Facts.Candidates);
        var renameReader = new RepairRenameProposalReader(_physicalPathResolver, targetReader, candidateFilter);
        var candidatesByOccurrence = view.Candidates.ToLookup(candidate => (candidate.SourcePath, candidate.Destination));
        AddBoundaryFindings(findings, scope.IncompleteSources);
        var references = scope.References
            .OrderBy(value => value.SourcePath, StringComparer.Ordinal)
            .ThenBy(value => value.DestinationLocation?.ByteOffset).ToArray();
        var proposalReads = new RepairProposalInput?[references.Length];
        var proposalFindings = new RepairFinding?[references.Length];
        await Parallel.ForEachAsync(Enumerable.Range(0, references.Length), new ParallelOptions
        {
            MaxDegreeOfParallelism = MaximumConcurrentProposals,
            CancellationToken = cancellationToken,
        }, async (index, token) =>
        {
            var reference = references[index];
            if (reference.Kind != LocalReferenceKind.Link
                || reference.DestinationLocation is null
                || reference.Facts.Target.Resolution == SourceLinkTargetResolution.Complete && reference.Canonicalizations.Count == 0)
            {
                return;
            }

            if (!RepairProtectionReader.Allows(reference.SourcePath, policy))
            {
                if (RepairRemainingFindingReader.Read([reference]).Any())
                    proposalFindings[index] = RepairProtectionReader.Finding(reference.SourcePath, reference.DestinationLocation, reference.SourcePath, policy);
                return;
            }
            if (reference.Facts.IsFolder || !snapshots.TryGetValue(reference.SourcePath, out var snapshot)) return;
            var referenceView = view with { Candidates = candidatesByOccurrence[(reference.SourcePath, reference.Destination)].ToArray() };
            if (reference.Facts.Target.Path is { } originalTarget && !RepairProtectionReader.Allows(originalTarget, policy))
            {
                if (RepairRemainingFindingReader.Read([reference]).Any())
                    proposalFindings[index] = RepairProtectionReader.Finding(reference.SourcePath, reference.DestinationLocation, originalTarget, policy);
                return;
            }

            var proposal = evidence is null ? null : await renameReader.ReadAsync(
                workspace, reference, referenceView, evidence, token).ConfigureAwait(false);
            proposal ??= ReadProposal(reference, RepairRenameProposalReader.FilterWeak(referenceView, reference, evidence, candidateFilter));
            if (proposal?.Candidates is not null)
            {
                var protectedTarget = proposal.Candidates.Items.FirstOrDefault(candidate =>
                    !RepairProtectionReader.Allows(Uri.UnescapeDataString(candidate.Target.CanonicalTargetPath), policy));
                proposal = await targetReader.ResolveCandidatesAsync(workspace, proposal, relinks ?? [], policy, token).ConfigureAwait(false);
                if (proposal.Candidates is { Items.Count: 0 } && protectedTarget is not null)
                {
                    proposalFindings[index] = RepairProtectionReader.Finding(reference.SourcePath, reference.DestinationLocation,
                        Uri.UnescapeDataString(protectedTarget.Target.CanonicalTargetPath), policy);
                    return;
                }
            }
            if (proposal is null)
            {
                return;
            }
            if (proposal.Target is { } target && !RepairProtectionReader.Allows(Uri.UnescapeDataString(target.CanonicalTargetPath), policy))
            {
                proposalFindings[index] = RepairProtectionReader.Finding(reference.SourcePath, reference.DestinationLocation,
                    Uri.UnescapeDataString(target.CanonicalTargetPath), policy);
                return;
            }

            proposalReads[index] = new RepairProposalInput(
                proposal,
                new RepairOccurrenceState(
                    reference.SourcePath,
                    reference.DestinationLocation,
                    snapshot),
                reference.Destination);
        }).ConfigureAwait(false);
        var proposals = proposalReads.OfType<RepairProposalInput>().ToArray();
        findings.AddRange(proposalFindings.OfType<RepairFinding>());

        findings.AddRange(RepairRemainingFindingReader.Read(scope.References).Where(finding =>
            !findings.Any(existing => existing.Protection && existing.SourceCanonicalPath == finding.SourceCanonicalPath && existing.Occurrence == finding.Occurrence)
            &&
            !proposals.Any(input => input.Proposal.SourceCanonicalPath == finding.SourceCanonicalPath
                && input.Proposal.Occurrence == finding.Occurrence)));
        return new RepairCatalogueRead(proposals, findings)
        {
            Catalogue = catalogue,
            RenameEvidence = evidence,
            Documents = documents.Concat(evidence?.Facts.Candidates.Select(candidate => KeyValuePair.Create(candidate.Path, candidate.Content.Document)) ?? [])
                .DistinctBy(item => item.Key).ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal),
            MissingTargetReferences = scope.References.Where(reference => !reference.Facts.IsFolder && reference.Kind == LocalReferenceKind.Link
                && reference.DestinationLocation is not null && reference.Facts.Target is { Resolution: SourceLinkTargetResolution.Missing, Path: not null }).ToArray(),
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
        var paths = _physicalPathResolver.CreateReadSession();
        foreach (var sourcePath in view.References
            .Select(reference => reference.SourcePath)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal))
        {
            var lexicalPath = Path.Combine(
                workspace.LexicalRoot,
                sourcePath.Replace('/', Path.DirectorySeparatorChar));
            var resolution = paths.ResolveCandidate(
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
        IReadOnlyDictionary<string, MarkdownDocumentFacts> retained,
        List<RepairFinding> findings)
    {
        var parser = new MarkdownDocumentParser();
        var documents = new Dictionary<string, MarkdownDocumentFacts>(StringComparer.Ordinal);
        foreach (var (sourcePath, snapshot) in snapshots.OrderBy(value => value.Key, StringComparer.Ordinal))
        {
            try
            {
                var text = StrictUtf8.GetString(snapshot.Bytes.AsSpan());
                var document = retained.TryGetValue(sourcePath, out var parsed) && string.Equals(parsed.Source, text, StringComparison.Ordinal)
                    ? parsed : parser.Parse(text);
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
