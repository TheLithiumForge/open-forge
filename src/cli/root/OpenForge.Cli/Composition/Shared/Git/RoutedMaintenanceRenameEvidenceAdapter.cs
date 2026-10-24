using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Rename;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Input;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Composition.Shared.Git;

internal sealed class RoutedMaintenanceRenameEvidenceAdapter(RenameEvidenceReader reader) : IRoutedMaintenanceRenameEvidence
{
    private readonly ConditionalWeakTable<RouteScaffoldRenameProof, RenameEvidenceResult> _observations = new();

    internal ValueTask<RenameEvidenceResult> ReadEvidenceAsync(CliWorkspace workspace, CancellationToken token)
        => ReadEvidenceAsync(workspace, references: null, token);

    internal async ValueTask<RenameEvidenceResult> ReadEvidenceAsync(CliWorkspace workspace, LocalReferenceDoctorView? references, CancellationToken token)
    {
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, token).ConfigureAwait(false);
        return await ReadEvidenceAsync(observation, references, token).ConfigureAwait(false);
    }

    internal async ValueTask<RenameEvidenceResult> ReadEvidenceAsync(WorkspaceLayout layout, LocalReferenceDoctorView? references, CancellationToken token)
    {
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(layout.Workspace, token).ConfigureAwait(false);
        if (observation.Layout?.MainRoot != layout.MainRoot)
            return Unavailable(observation, "The selected layout changed before rename evidence was acquired.");
        observation = new WorkspaceLayoutRead(observation.State, observation.Settings, observation.Ownership, layout, observation.Issues, observation.Expectations);
        return await ReadEvidenceAsync(observation, references, token).ConfigureAwait(false);
    }

    internal async ValueTask<RenameEvidenceResult> ReadEvidenceAsync(WorkspaceLayoutRead observation, LocalReferenceDoctorView? references, CancellationToken token)
    {
        if (observation.Layout is not { } layout) return Unavailable(observation, observation.Issues.FirstOrDefault()?.Cause);
        var workspace = layout.Workspace;
        var paths = new PhysicalPathResolver().CreateReadSession();
        var catalogue = references?.Catalogue
            ?? await new SourceCatalogueReader().ReadAsync(new(layout, layout.SourceRoots), token).ConfigureAwait(false);
        var ownership = observation.Ownership;
        var settings = observation.Settings;
        var policy = new RenameEvidencePolicy
        {
            Layout = layout,
            ManagedPaths = [.. ownership.Document.ManagedPaths().Select(claim => claim.Path)],
            ExcludedPaths = [.. catalogue.Candidates.Where(candidate => WorkspaceRemovals.IsPathRemoved(layout, candidate.CanonicalPath, settings.Document))
                .Select(candidate => candidate.CanonicalPath), .. settings.Document.RemovedFiles, .. settings.Document.RemovedDirectories],
            PrivatePaths = [.. (ownership.Document.Framework?.GitIgnoredRoutes ?? []).Select(route => route.Directory)],
            ProjectionPaths = [.. ownership.Document.Libraries.SelectMany(library => library.Paths)],
        };
        var identities = ImmutableArray.CreateBuilder<RenameRecordedIdentity>();
        // Managed navigation may record an unmanaged child's identity. Admission still filters each recorded child.
        var recordPolicy = policy with { ManagedPaths = [] };
        var parser = new MarkdownDocumentParser();
        foreach (var source in catalogue.Sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form) || source.Base.Form == SourceDocumentForm.Loader))
        {
            var path = source.Identity.CanonicalBasePath;
            if (!RenameCandidateAdmission.Allows(path, recordPolicy) && path != layout.LoaderPath) continue;
            if (NoFollowLeafObserver.Observe(paths, workspace, SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path), token).State != NoFollowLeafState.OrdinaryFile) continue;
            try
            {
                var bytes = await File.ReadAllBytesAsync(source.Base.PhysicalPath, token).ConfigureAwait(false);
                var snapshot = FileStateSnapshot.File(SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path), source.Base.PhysicalPath, bytes);
                var text = Encoding.UTF8.GetString(bytes);
                var document = references?.Documents.TryGetValue(path, out var retained) == true && string.Equals(retained.Source, text, StringComparison.Ordinal)
                    ? retained : parser.Parse(text);
                identities.AddRange(RenameRecordedEntryReader.Read(layout, path, snapshot, document));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            {
                // Unavailable optional records do not invent old identities.
            }
        }
        return await reader.ReadAsync(new()
        {
            Workspace = workspace,
            CandidatePaths = [.. catalogue.Candidates.Select(candidate => candidate.CanonicalPath)],
            MissingPaths = [],
            CandidateUniverseComplete = !catalogue.IsCancelled && catalogue.Issues.Count == 0
                && ownership.State is WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete
                && settings.State is WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete,
            Policy = policy,
            RecordedIdentities = identities.ToImmutable(),
            Documents = references?.Documents ?? ImmutableDictionary<string, OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownDocumentFacts>.Empty,
            PolicyExpectations = [ownership.Snapshot?.Expectation ?? FileExpectation.Missing(ownership.LogicalPath),
                settings.Snapshot?.Expectation ?? FileExpectation.Missing(settings.LogicalPath)],
        }, token).ConfigureAwait(false);
    }

    private static RenameEvidenceResult Unavailable(WorkspaceLayoutRead observation, string? cause)
        => new(new RenameEvidenceFacts
        {
            Layout = null,
            UnavailableCause = cause,
            CandidateUniverseComplete = false,
            Candidates = [],
            MissingPaths = [],
            OldIdentities = [],
            Git = RenameGitObservation.Unavailable(RenameGitAvailability.Unavailable),
            PolicyExpectations = observation.Expectations,
        }, []);

    public async ValueTask<RouteScaffoldRenameProof?> ReadAsync(WorkspaceLayout layout, RouteScaffoldRenameCandidate candidate, CancellationToken token)
    {
        if (candidate.OldSource is not { } source) return null;
        var evidence = await ReadEvidenceAsync(layout, references: null, token).ConfigureAwait(false);
        var move = RenameCanonicalEntrypointEvidence.Read(layout, evidence).SingleOrDefault(move => move.Proof.NewPath == source.Identity.CanonicalBasePath
            && move.CanonicalPath == candidate.Destination.Identity.CanonicalBasePath);
        if (move is null) return null;
        var proof = new RouteScaffoldRenameProof
        {
            OldCanonicalPath = move.Proof.OldPath,
            CurrentCanonicalPath = move.Proof.NewPath,
            CurrentContentHash = move.Proof.NewHash,
            Basis = move.Proof.Basis == RenameEvidenceBasis.CommittedContent ? RouteScaffoldRenameBasis.CommittedContent : RouteScaffoldRenameBasis.GitRemoval,
            Similarity = move.Proof.Similarity,
        };
        _observations.Add(proof, evidence);
        return proof;
    }

    public async ValueTask<bool> VerifyAsync(WorkspaceLayout layout, RouteScaffoldRenameProof proof, CancellationToken token)
        => _observations.TryGetValue(proof, out var previous)
            && RenameEvidenceRevalidator.IsCurrent(previous, await ReadEvidenceAsync(layout, references: null, token).ConfigureAwait(false), proof.OldCanonicalPath);
}
