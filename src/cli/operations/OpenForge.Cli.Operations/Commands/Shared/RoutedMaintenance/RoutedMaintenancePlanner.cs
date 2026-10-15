using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Discovery;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Rename;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;

// Completion precedes intended membership and direct-child projection, per Forgiving Maintenance.
internal sealed class RoutedMaintenancePlanner(PhysicalPathResolver paths, IRoutedMaintenanceRenameEvidence? renameEvidence = null,
    RoutedMaintenanceSnapshotRead? readSnapshot = null)
{
    private readonly RoutedMetadataCompletionPolicy _policy = new();
    private readonly RouteScaffoldSourceBuilder _sources = new();
    private readonly MarkdownDocumentParser _parser = new();

    internal async ValueTask<RoutedMaintenanceIntent> PrepareAsync(RoutedMaintenanceRequest request, CancellationToken token)
    {
        var catalogue = request.Catalogue;
        var originals = new Dictionary<string, FileStateSnapshot>(StringComparer.Ordinal);
        var documents = new Dictionary<string, OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownDocumentFacts>(StringComparer.Ordinal);
        var completions = new List<RoutedMaintenanceMetadata>();
        var replacements = new Dictionary<string, ImmutableArray<byte>>(StringComparer.Ordinal);
        var limitations = new List<RoutedMaintenanceLimitation>();
        var eligible = new List<SourceLogicalSource>();
        var readPaths = paths.CreateReadSession();
        var reader = new SourceDocumentReader(catalogue.Workspace, readPaths);
        var roots = request.RootPaths.Select(path => (Path: path, Folder: SourceLogicalPath.ReadParent(path))).ToArray();
        var entrypointsByFolder = catalogue.Sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form))
            .Select(source => source.Identity.CanonicalBasePath).Select(path => (Path: path, Folder: SourceLogicalPath.ReadParent(path))).ToArray();
        var acquisitions = new List<(SourceLogicalSource Source, RoutedMetadataFileFacts Facts, SourceLayerVerification Verification)>();
        foreach (var source in request.SelectedSources.OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .DistinctBy(source => source.Base.PhysicalPath, PhysicalIdentityTracker.PathComparer))
        {
            var path = source.Identity.CanonicalBasePath;
            var facts = Facts(source);
            var leaf = NoFollowLeafObserver.Observe(readPaths, catalogue.Workspace, SourceLogicalPath.ToLexicalPath(catalogue.Workspace.LexicalRoot, path), token);
            facts = facts with { IsSafeOrdinaryLeaf = leaf.State == NoFollowLeafState.OrdinaryFile };
            if (_policy.Evaluate(facts, request.Policy) != RoutedMetadataEligibility.Eligible) continue;
            var verification = reader.Verify(source.Base, token);
            if (verification.State == SourceLayerVerificationState.Cancelled) token.ThrowIfCancellationRequested();
            if (verification.State != SourceLayerVerificationState.Verified) continue;
            acquisitions.Add((source, facts, verification));
        }
        var snapshots = await new RoutedMaintenanceSnapshotReader(readSnapshot)
            .ReadAsync(catalogue.Workspace, acquisitions.Select(item => item.Verification).ToArray(), token).ConfigureAwait(false);
        for (var index = 0; index < acquisitions.Count; index++)
        {
            token.ThrowIfCancellationRequested();
            if (snapshots[index] is not { } original) continue;
            var (source, facts, _) = acquisitions[index];
            var path = source.Identity.CanonicalBasePath;
            originals.Add(path, original);
            var bytes = request.IntendedBytes.TryGetValue(path, out var intended) ? intended : original.Bytes;
            request.ObservedDocuments.TryGetValue(path, out var observedDocument);
            var result = _policy.Complete(new() { File = facts, Bytes = bytes, Document = observedDocument }, request.Policy).Completion;
            documents.Add(path, result.Document ?? _parser.Parse(Encoding.UTF8.GetString(result.Bytes.AsSpan())));
            if (result.State is FrameworkMetadataCompletionState.Malformed or FrameworkMetadataCompletionState.Unsafe)
            {
                limitations.Add(new(path, result.Cause ?? "The authored metadata is invalid.", result.State == FrameworkMetadataCompletionState.Malformed));
                if (result.State == FrameworkMetadataCompletionState.Malformed && !SourceFormClassifier.IsEntrypoint(source.Base.Form))
                    eligible.Add(source);
                continue;
            }
            eligible.Add(source);
            if (!result.Bytes.AsSpan().SequenceEqual(original.Bytes.AsSpan())) replacements.Add(path, result.Bytes);
            if (result.State == FrameworkMetadataCompletionState.Completed) completions.Add(new(path, result));
        }

        foreach (var source in request.SelectedSources.Where(source => !documents.ContainsKey(source.Identity.CanonicalBasePath)))
        {
            var original = originals.FirstOrDefault(item => PhysicalIdentityTracker.PathComparer.Equals(item.Value.PhysicalPath, source.Base.PhysicalPath));
            if (original.Value is not null) documents[source.Identity.CanonicalBasePath] = documents[original.Key];
        }

        var malformedEntrypoints = limitations.Where(item => item.Malformed && catalogue.FindByPath(item.Path) is { } source
            && SourceFormClassifier.IsEntrypoint(source.Base.Form)).Select(item => SourceLogicalPath.ReadParent(item.Path)).ToArray();
        eligible.RemoveAll(source => malformedEntrypoints.Any(folder => source.Identity.CanonicalBasePath.StartsWith(folder + "/", StringComparison.Ordinal)));
        completions.RemoveAll(item => malformedEntrypoints.Any(folder => item.Path.StartsWith(folder + "/", StringComparison.Ordinal)));
        foreach (var path in replacements.Keys.Where(path => malformedEntrypoints.Any(folder => path.StartsWith(folder + "/", StringComparison.Ordinal))).ToArray())
            replacements.Remove(path);
        var discovery = new RouteScaffoldChainDiscovery().Discover(new()
        {
            Catalogue = catalogue,
            RecognizedRootPaths = request.RootPaths,
            EligibleSources = [.. eligible],
            Settings = request.Policy.Settings,
            Sharing = new SourceSharing(request.Policy.Ownership.Framework?.GitIgnoredRoutes ?? []),
        });
        foreach (var boundary in discovery.Boundaries)
            limitations.Add(new(boundary.Path, $"The selected entrypoint chain is unsafe: {boundary.Kind}.", false) { Topology = true });
        var intendedEntrypointPaths = discovery.Gaps.Select(gap => gap.CanonicalPath).ToArray();
        for (var index = 0; index < completions.Count; index++)
        {
            var metadata = completions[index];
            if (!metadata.Completion.Fields.Contains(FrameworkMetadataField.Tags)) continue;
            var source = catalogue.FindByPath(metadata.Path) ?? throw new InvalidOperationException();
            var facts = Facts(source, intendedEntrypointPaths);
            if (metadata.Completion.Tags is { } currentTags && currentTags.SequenceEqual(RoutedMetadataDefaults.Tags(facts), StringComparer.Ordinal)) continue;
            var original = originals[metadata.Path];
            var bytes = request.IntendedBytes.TryGetValue(metadata.Path, out var intended) ? intended : original.Bytes;
            var completion = _policy.Complete(new() { File = facts, Bytes = bytes }, request.Policy).Completion;
            completions[index] = new(metadata.Path, completion);
            replacements[metadata.Path] = completion.Bytes;
            documents[metadata.Path] = completion.Document ?? _parser.Parse(Encoding.UTF8.GetString(completion.Bytes.AsSpan()));
        }
        var entrypoints = new List<RouteScaffoldEntrypointIntent>();
        var removed = new List<string>();
        var oldNavigation = new List<RouteScaffoldOldNavigation>();
        var proofs = new List<RouteScaffoldRenameProof>();
        foreach (var gap in discovery.Gaps)
        {
            var source = _sources.Create(catalogue.Workspace, gap.CanonicalPath);
            var leaf = NoFollowLeafObserver.Observe(readPaths, catalogue.Workspace, SourceLogicalPath.ToLexicalPath(catalogue.Workspace.LexicalRoot, gap.CanonicalPath), token);
            if (leaf.State != NoFollowLeafState.Missing)
            {
                limitations.Add(new(gap.CanonicalPath, "The intended entrypoint path is occupied or unsafe.", false));
                continue;
            }
            var tags = RoutedMetadataDefaults.Tags(Facts(source, intendedEntrypointPaths));
            var scaffold = new RouteScaffoldComposer().Compose(RouteScaffoldComposer.FolderDocument(gap.FolderId,
                FrameworkMetadataDescriptionDeriver.SentenceCase(SourceLogicalPath.ReadFileName(SourceLogicalPath.ReadParent(gap.CanonicalPath))), tags, request.Policy.Settings.Frontmatter));
            var old = eligible.Where(item => SourceLogicalPath.ReadParent(item.Identity.CanonicalBasePath) == SourceLogicalPath.ReadParent(gap.CanonicalPath)
                && SourceLogicalPath.ReadFileName(item.Identity.CanonicalBasePath).StartsWith('_')).ToArray();
            var candidate = new RouteScaffoldRenameCandidate
            {
                Destination = source,
                DestinationBefore = FileStateSnapshot.Missing(SourceLogicalPath.ToLexicalPath(catalogue.Workspace.LexicalRoot, gap.CanonicalPath)),
                ScaffoldBytes = scaffold,
                OldSource = old.Length == 1 ? old[0] : null,
                OldSnapshot = old.Length == 1 ? originals[old[0].Identity.CanonicalBasePath] : null,
            };
            var oldMalformed = candidate.OldSource is { } selectedOld && limitations.Any(item => item.Path == selectedOld.Identity.CanonicalBasePath);
            var proof = renameEvidence is null || oldMalformed ? null : await renameEvidence.ReadAsync(catalogue.Workspace, candidate, token).ConfigureAwait(false);
            var intent = new RouteScaffoldRenamePlanner().Decide(candidate, proof);
            if (intent.IsRename && candidate.OldSource is { } movedSource
                && request.IntendedBytes.TryGetValue(movedSource.Identity.CanonicalBasePath, out var movedBytes))
                intent = intent with { IntendedBytes = movedBytes };
            if (intent.IsRename)
            {
                var completed = _policy.Complete(new() { File = Facts(source, intendedEntrypointPaths), Bytes = intent.IntendedBytes }, request.Policy).Completion;
                if (completed.State == FrameworkMetadataCompletionState.Completed)
                {
                    intent = intent with { IntendedBytes = completed.Bytes };
                    completions.Add(new(gap.CanonicalPath, completed));
                    originals.Add(gap.CanonicalPath, intent.Before);
                }
            }
            entrypoints.Add(intent);
            documents[gap.CanonicalPath] = _parser.Parse(Encoding.UTF8.GetString(intent.IntendedBytes.AsSpan()));
            if (intent.DeleteBefore is not null && candidate.OldSource is { } oldSource && proof is not null)
            {
                removed.Add(oldSource.Identity.CanonicalBasePath);
                completions.RemoveAll(item => item.Path == oldSource.Identity.CanonicalBasePath);
                replacements.Remove(oldSource.Identity.CanonicalBasePath);
                proofs.Add(proof);
            }
            if (intent.OldNavigation is { } fact) oldNavigation.Add(fact);
        }

        var malformedLeaves = limitations.Where(item => item.Malformed && catalogue.FindByPath(item.Path) is { } source
            && !SourceFormClassifier.IsEntrypoint(source.Base.Form)).Select(item => item.Path);
        removed.AddRange(catalogue.Sources.Where(source => WorkspaceRemovals.IsPathRemoved(source.Identity.CanonicalBasePath, request.Policy.Settings))
            .Select(source => source.Identity.CanonicalBasePath));
        var formation = _sources.Replace(catalogue, entrypoints.Select(item => item.Source), removed.Concat(malformedLeaves));
        return new()
        {
            Formation = formation,
            Documents = documents.ToImmutableDictionary(StringComparer.Ordinal),
            Originals = originals.ToImmutableDictionary(StringComparer.Ordinal),
            Metadata = [.. completions],
            Entrypoints = [.. entrypoints],
            Limitations = [.. limitations],
            OldNavigation = [.. oldNavigation],
            RemovedPaths = [.. removed],
            RenameProofs = [.. proofs],
            Replacements = replacements.ToImmutableDictionary(StringComparer.Ordinal),
        };

        RoutedMetadataFileFacts Facts(SourceLogicalSource source, IEnumerable<string>? intendedEntrypoints = null)
        {
            var path = source.Identity.CanonicalBasePath;
            return new()
            {
                CanonicalPath = path,
                LoaderRootEntrypointPath = roots.FirstOrDefault(root => RoutedMetadataDefaults.IsAncestor(path, root.Folder)).Path,
                RouteEntrypointPaths = [.. entrypointsByFolder.Where(item => RoutedMetadataDefaults.IsAncestor(path, item.Folder)).Select(item => item.Path)
                    .Concat((intendedEntrypoints ?? []).Where(entrypoint => RoutedMetadataDefaults.IsAncestor(path, SourceLogicalPath.ReadParent(entrypoint))))],
                IsSelected = true,
                IsReadable = true,
                IsSafeOrdinaryLeaf = true,
                IsGeneratedFile = false,
                HasManagedAuthoredRegions = false,
            };
        }
    }

    internal async ValueTask<bool> VerifyProofsAsync(RoutedMaintenanceIntent intent, CancellationToken token)
    {
        foreach (var proof in intent.RenameProofs)
            if (renameEvidence is null || !await renameEvidence.VerifyAsync(intent.Formation.Catalogue.Workspace, proof, token).ConfigureAwait(false)) return false;
        return true;
    }

}
