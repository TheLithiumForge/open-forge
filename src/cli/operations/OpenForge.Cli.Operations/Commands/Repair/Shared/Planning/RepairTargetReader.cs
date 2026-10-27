using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Sources.Shared.Acquisition;
using System.Collections.Concurrent;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal sealed class RepairTargetReader(SourceCatalogue? catalogue = null, Action? targetResolutionStarted = null,
    SourceFileAcquisition? acquisition = null, Func<string, string, string>? formatPath = null, RoutedMetadataPolicyContext? policy = null)
{
    private readonly Func<string, string, string> _formatPath = formatPath ?? RepairDestinationFormatter.FormatPath;
    private readonly ConcurrentDictionary<(string Folder, string Target), Lazy<string>> _spellings = new();
    private readonly PhysicalPathResolver _physicalPaths = new();
    private SourceLinkDestinationResolver? _resolver;

    internal async ValueTask<SourceLinkDestinationFacts> ResolveAsync(
        CliWorkspace workspace, string sourcePath, RepairTargetSelection target, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var canonicalTarget = Uri.UnescapeDataString(target.CanonicalTargetPath);
        if (policy is not null && (!RepairOutsideAdmission.Allows(sourcePath, policy, cancellationToken)
            || !RepairOutsideAdmission.Allows(canonicalTarget, policy, cancellationToken)))
            return new()
            {
                Fragment = target.TargetFragment,
                Target = new()
                {
                    Kind = SourceLinkTargetKind.Local,
                    Id = null,
                    Path = canonicalTarget,
                    PhysicalPath = null,
                    Layer = null,
                    Resolution = SourceLinkTargetResolution.PhysicalEscape,
                    Network = null
                },
                Finding = new()
                {
                    Code = SourceLinkDestinationFindingCode.TargetUnsafe,
                    Cause = "The selected outside target has no safe retained maintenance boundary.",
                    Candidates = []
                },
            };
        targetResolutionStarted?.Invoke();
        var selectedCatalogue = catalogue ?? throw new InvalidOperationException("Repair target resolution requires its source catalogue.");
        SourceLinkDestinationResolver resolver;
        lock (_physicalPaths)
        {
            resolver = _resolver ??= new SourceLinkDestinationResolver(
                _physicalPaths,
                StrictUtf8FileReader.ReadAsync, new MarkdownDocumentParser().Parse).CreateSession(acquisition ?? new SourceFileAcquisition());
        }
        var facts = await resolver.ResolveAsync(new SourceLinkDestinationInput
        {
            Workspace = workspace,
            Catalogue = selectedCatalogue,
            SourceCanonicalPath = sourcePath,
            RawDestination = FormatDestination(sourcePath, target),
        }, cancellationToken).ConfigureAwait(false);
        return facts;
    }

    private string FormatDestination(string source, RepairTargetSelection target)
    {
        // Only relative spelling is shared. Resolution, containment and file acquisition always run afresh.
        if (!SourceLogicalPath.IsCanonicalSource(source) || !SourceLogicalPath.IsCanonicalSource(target.CanonicalTargetPath))
            return RepairDestinationFormatter.AppendFragment(_formatPath(source, target.CanonicalTargetPath), target.TargetFragment);
        var key = (Folder: SourceLogicalPath.ReadParent(source), Target: target.CanonicalTargetPath);
        var spelling = _spellings.GetOrAdd(key, static (_, input) => new(() => input.Reader._formatPath(input.Source, input.Target)),
            (Reader: this, Source: source, Target: target.CanonicalTargetPath));
        try { return RepairDestinationFormatter.AppendFragment(spelling.Value, target.TargetFragment); }
        catch
        {
            _spellings.TryRemove(key, out _);
            throw;
        }
    }

    internal async ValueTask<RepairProposal> ResolveCandidatesAsync(
        CliWorkspace workspace, RepairProposal proposal, IReadOnlyList<RepairRelinkRequest> relinks, RoutedMetadataPolicyContext policy, CancellationToken cancellationToken)
    {
        var requested = relinks.SingleOrDefault(relink => relink.SourceCanonicalPath == proposal.SourceCanonicalPath
            && relink.Line == proposal.Occurrence.Line && relink.Column == proposal.Occurrence.Column);
        var candidates = new List<RepairCandidate>();
        foreach (var candidate in proposal.Candidates?.Items ?? [])
        {
            var target = candidate.Target;
            if (!RepairProtectionReader.Allows(Uri.UnescapeDataString(target.CanonicalTargetPath), policy)) continue;
            if (!RepairOutsideAdmission.Allows(proposal.SourceCanonicalPath, policy, cancellationToken)
                || !RepairOutsideAdmission.Allows(Uri.UnescapeDataString(target.CanonicalTargetPath), policy, cancellationToken, target.RenameProof is not null)) continue;
            if (requested is not null
                && Uri.UnescapeDataString(requested.Target.CanonicalTargetPath) == Uri.UnescapeDataString(target.CanonicalTargetPath))
            {
                target = new RepairTargetSelection(requested.Target.CanonicalTargetPath, requested.Target.TargetFragment, candidate.Evidence) { RenameProof = candidate.Target.RenameProof };
            }

            var readTarget = target.RenameProof is { } proof && proof.NewPath != Uri.UnescapeDataString(target.CanonicalTargetPath)
                && !File.Exists(Path.Combine(workspace.LexicalRoot, Uri.UnescapeDataString(target.CanonicalTargetPath).Replace('/', Path.DirectorySeparatorChar)))
                ? new RepairTargetSelection(proof.NewPath, target.TargetFragment) : target;
            if (!RepairOutsideAdmission.Allows(Uri.UnescapeDataString(readTarget.CanonicalTargetPath), policy, cancellationToken)) continue;
            var facts = await ResolveAsync(workspace, proposal.SourceCanonicalPath, readTarget, cancellationToken).ConfigureAwait(false);
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

    internal async ValueTask<IReadOnlyList<FileExpectation>?> ReadAsync(RepairPlan plan, CancellationToken cancellationToken)
    {
        var expectations = new List<FileExpectation>();
        var selectedCatalogue = catalogue;
        if (plan.LayoutRead is not { } observation) return null;
        if (selectedCatalogue is null && plan.Selection.Selected.Count > 0 && observation.Layout is { } layout)
            selectedCatalogue = (await new SourceReadSessionReader(_physicalPaths).ReadIncludedAsync(layout, cancellationToken).ConfigureAwait(false)).Catalogue;
        var protection = RepairProtectionReader.Read(observation);
        if (protection.Policy is not { } policy) return null;
        var reader = this;
        if (catalogue is null && selectedCatalogue is not null)
            reader = new RepairTargetReader(selectedCatalogue, policy: policy);
        foreach (var selected in plan.Selection.Selected)
        {
            var target = selected.Resolution.Target;
            if (!RepairProtectionReader.Allows(selected.Proposal.SourceCanonicalPath, policy)
                || !RepairProtectionReader.Allows(Uri.UnescapeDataString(target.CanonicalTargetPath), policy)) return null;
            if (!RepairOutsideAdmission.Allows(selected.Proposal.SourceCanonicalPath, policy, cancellationToken)
                || !RepairOutsideAdmission.Allows(Uri.UnescapeDataString(target.CanonicalTargetPath), policy, cancellationToken, target.RenameProof is not null)) return null;
            var canonicalCreate = plan.Maintenance?.Effects.Changes.FirstOrDefault(change => change.Kind == PlannedFileChangeKind.Create
                && change.LogicalPath == Path.Combine(plan.Request.Workspace.LexicalRoot, Uri.UnescapeDataString(target.CanonicalTargetPath).Replace('/', Path.DirectorySeparatorChar)));
            if (canonicalCreate is not null && target.RenameProof is { } proof)
            {
                var current = new RepairTargetSelection(proof.NewPath, target.TargetFragment);
                if (!RepairOutsideAdmission.Allows(proof.NewPath, policy, cancellationToken)) return null;
                var currentFacts = await reader.ResolveAsync(plan.Request.Workspace, selected.Proposal.SourceCanonicalPath, current, cancellationToken).ConfigureAwait(false);
                if (currentFacts.Target.Resolution != SourceLinkTargetResolution.Complete) return null;
                expectations.Add(proof.CandidateSnapshot.Expectation);
                continue;
            }
            var facts = await reader.ResolveAsync(plan.Request.Workspace, selected.Proposal.SourceCanonicalPath, target, cancellationToken)
                .ConfigureAwait(false);
            if (facts.Target.Resolution != SourceLinkTargetResolution.Complete || facts.Target.PhysicalPath is not { } physicalPath
                || facts.Target.Path != Uri.UnescapeDataString(target.CanonicalTargetPath))
            {
                return null;
            }

            var logicalPath = Path.Combine(plan.Request.Workspace.LexicalRoot,
                Uri.UnescapeDataString(target.CanonicalTargetPath).Replace('/', Path.DirectorySeparatorChar));
            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
            expectations.Add(FileStateSnapshot.File(logicalPath, physicalPath, bytes).Expectation);
        }

        expectations.AddRange(protection.Expectations);
        return expectations.Distinct().ToArray();
    }

    internal async ValueTask<bool> ValidateAsync(
        RepairPlan plan, IReadOnlyList<FileExpectation> targets, CancellationToken cancellationToken)
    {
        var validator = new FileExpectationValidator(_physicalPaths);
        foreach (var target in targets)
        {
            var result = await validator.ValidateAsync(plan.Request.Workspace, target, cancellationToken).ConfigureAwait(false);
            if (result.State != FileExpectationValidationState.Matched)
            {
                return false;
            }
        }

        return true;
    }
}
