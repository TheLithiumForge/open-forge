using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using System.Text;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Result;

internal static class RepairMaintenanceResultReader
{
    internal static RepairMaintenanceResults Read(RepairResult result)
    {
        if (result.Plan is not { } plan) return RepairMaintenanceResults.Empty;
        var proofs = plan.Steps.Where(step => result.Mode == RepairMode.DryRun || step.Outcome == Models.Planning.RepairStepOutcome.Verified)
            .SelectMany(step => new[] { step.Proposal.RenameProof, step.Proposal.SourceMoveProof })
            .OfType<RenameEvidenceObservation>().DistinctBy(proof => (proof.OldPath, proof.NewPath)).OrderBy(proof => proof.OldPath, StringComparer.Ordinal);
        string FinalPath(string path) => plan.Maintenance?.Intent.Entrypoints.FirstOrDefault(entrypoint =>
            entrypoint.DeleteBefore?.LogicalPath == SourceLogicalPath.ToLexicalPath(plan.Request.Workspace.LexicalRoot, path))?.Source.Identity.CanonicalBasePath ?? path;
        var renames = proofs.Select(proof => new RepairRename(proof.OldPath, FinalPath(proof.NewPath), proof.Basis switch
        {
            RenameEvidenceBasis.GitRename => "git-rename",
            RenameEvidenceBasis.CommittedContent => "committed-content",
            RenameEvidenceBasis.RecordedMetadata => "recorded-metadata",
            _ => throw new ArgumentOutOfRangeException(nameof(proof)),
        }, proof.Similarity,
            new(proof.Head, proof.OldBlob, proof.OldHash, proof.NewHash,
                proof.Metadata is { } metadata ? new(metadata.Description, metadata.Tags, metadata.ApplyTo) : null),
            proof.SameContent, SourceLogicalPath.ReadParent(proof.OldPath) != SourceLogicalPath.ReadParent(FinalPath(proof.NewPath)))).ToArray();
        var linkRenames = plan.Steps.ToDictionary(step => (step.Proposal.SourceCanonicalPath, step.Proposal.OccurrenceView.Line, step.Proposal.OccurrenceView.Column),
            step => (IReadOnlyList<RepairRename>)renames.Where(rename => rename.OldPath == step.Proposal.RenameProof?.OldPath
                || rename.OldPath == step.Proposal.SourceMoveProof?.OldPath).ToArray());
        if (plan.Maintenance is not { } maintenance) return new(renames, [], [], []) { LinkRenames = linkRenames };
        string Outcome(string path)
        {
            if (result.Mode == RepairMode.DryRun) return "planned";
            var logical = SourceLogicalPath.ToLexicalPath(plan.Request.Workspace.LexicalRoot, path);
            var receipt = result.Verification.FileReceipts.SingleOrDefault(receipt => receipt.Change.LogicalPath == logical);
            if (receipt is null) return "not-started";
            if (result.Verification.FileStates.TryGetValue(logical, out var state) && state is RepairVerificationState.Failed or RepairVerificationState.Unknown) return "unknown";
            return receipt.VerificationState == FilesystemVerificationState.Verified ? "done" : "unknown";
        }
        var navigation = maintenance.Projection.Regions.Where(region => region.Change?.RequiresUpdate == true).Select(region => new RepairNavigation(
            region.CanonicalPath, SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(
                Encoding.UTF8.GetString(region.Change?.BeforeDocumentBytes.AsSpan().ToArray() ?? []))).Entries.Count,
            region.Entries.Count, Outcome(region.CanonicalPath))).ToArray();
        var completed = maintenance.Intent.Metadata.Select(item => new RepairCompletedMetadata(item.Path,
            item.Completion.Fields.Select(field => field switch
            {
                FrameworkMetadataField.Description => "description",
                FrameworkMetadataField.Tags => "tags",
                FrameworkMetadataField.Name => "name",
                _ => throw new ArgumentOutOfRangeException(nameof(field)),
            }).ToArray(), item.Completion.Description, item.Completion.Tags, Outcome(item.Path))).ToArray();
        var entrypoints = maintenance.Intent.Entrypoints.Select(item => new RepairCompletedEntrypoint(item.Source.Identity.CanonicalBasePath,
            Outcome(item.Source.Identity.CanonicalBasePath))).ToArray();
        var effects = plan.FileChanges.Where(change => change.Kind == PlannedFileChangeKind.Delete
                || !plan.Effects.Any(effect => effect.FileChange.LogicalPath == change.LogicalPath))
            .Select(change => new RepairMaintenanceEffect(Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, change.LogicalPath).Replace('\\', '/'),
                change.Kind == PlannedFileChangeKind.ReplaceGeneratedRegion ? "section" : "file", change.Kind switch
                {
                    PlannedFileChangeKind.Create => "created",
                    PlannedFileChangeKind.Delete => "deleted",
                    PlannedFileChangeKind.Replace or PlannedFileChangeKind.ReplaceGeneratedRegion => "rewritten",
                    _ => throw new ArgumentOutOfRangeException(nameof(change)),
                }, Outcome(Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, change.LogicalPath).Replace('\\', '/')))).ToArray();
        var finalHashes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var effect in plan.Effects)
        {
            var relocation = maintenance.Intent.Entrypoints.FirstOrDefault(entrypoint => entrypoint.DeleteBefore?.LogicalPath == effect.FileChange.LogicalPath);
            var final = plan.FileChanges.Single(change => change.LogicalPath == (relocation?.Before.LogicalPath ?? effect.FileChange.LogicalPath));
            finalHashes.Add(effect.SourceCanonicalPath, FileExpectation.Hash(final.IntendedBytes.AsSpan()));
        }
        var navigationRenames = new List<RepairNavigationRename>();
        foreach (var item in maintenance.Intent.Entrypoints)
        {
            if (item.DeleteBefore is not { } before) continue;
            var outcome = Outcome(item.Source.Identity.CanonicalBasePath);
            var oldOutcome = Outcome(Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, before.LogicalPath).Replace('\\', '/'));
            navigationRenames.Add(new(item.Source.Identity.CanonicalBasePath, Path.GetFileName(before.LogicalPath), outcome == oldOutcome ? outcome : "unknown"));
        }
        return new(renames, navigation, completed, entrypoints)
        {
            FilesSkipped = maintenance.Intent.Limitations.Count(item => item.Malformed),
            Effects = effects,
            LinkRenames = linkRenames,
            NavigationRenames = navigationRenames,
            FinalHashes = finalHashes,
            FailurePath = plan.FileChanges.Where(change => result.Verification.FileStates.TryGetValue(change.LogicalPath, out var state)
                && state is RepairVerificationState.Failed or RepairVerificationState.Unknown)
                .Select(change => Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, change.LogicalPath).Replace('\\', '/')).FirstOrDefault(),
        };
    }
}
