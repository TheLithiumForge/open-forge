using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Inventory;

internal sealed class SourceCatalogueReader
{
    private readonly PhysicalPathResolver _physicalPathResolver = new();

    internal ValueTask<SourceCatalogue> ReadAsync(
        SourceCatalogueRequest request,
        CancellationToken cancellationToken)
    {
        var state = new ReadState(request.Workspace);
        if (cancellationToken.IsCancellationRequested)
        {
            return new ValueTask<SourceCatalogue>(state.FormCatalogue(isCancelled: true));
        }

        foreach (var logicalRoot in request.LogicalRoots)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new ValueTask<SourceCatalogue>(state.FormCatalogue(isCancelled: true));
            }

            var rootResolution = Resolve(request.Workspace, logicalRoot);
            if (rootResolution.State != PhysicalPathState.Contained)
            {
                state.AddRootIssue(ReadRootIssue(logicalRoot, rootResolution));
                continue;
            }

            var rootPhysicalPath = rootResolution.GetContainedPhysicalPath();
            var rootComponent = LinkTargetReader.Read(rootPhysicalPath);
            if (rootComponent.State != PathComponentState.Ordinary
                || rootComponent.Attributes is not { } rootAttributes
                || (rootAttributes & FileAttributes.Directory) == 0)
            {
                var failure = rootComponent.Failure;
                state.AddRootIssue(new SourceCatalogueIssue(
                    SourceCatalogueIssueCode.RootUnavailable,
                    logicalRoot,
                    [],
                    null,
                    failure));
                continue;
            }

            state.EnqueueDirectory(new DirectoryBoundary(
                logicalRoot,
                rootPhysicalPath,
                [rootPhysicalPath]));
            var interrupted = Traverse(state, request.Workspace, cancellationToken);
            if (interrupted)
            {
                return new ValueTask<SourceCatalogue>(state.FormCatalogue(isCancelled: true));
            }
        }

        return new ValueTask<SourceCatalogue>(state.FormCatalogue(isCancelled: false));
    }

    private bool Traverse(
        ReadState state,
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        while (state.TryDequeueDirectory(out var directory))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return true;
            }

            if (!state.MarkDirectoryProcessed(directory.CanonicalPath))
            {
                continue;
            }

            var enumeration = DirectoryEntryEnumerator.Enumerate(
                directory.PhysicalPath,
                directory.CanonicalPath,
                cancellationToken);
            if (enumeration.State == DirectoryEnumerationState.Cancelled)
            {
                return true;
            }

            if (enumeration.State != DirectoryEnumerationState.Complete)
            {
                state.AddIssue(new SourceCatalogueIssue(
                    SourceCatalogueIssueCode.DirectoryUnavailable,
                    directory.CanonicalPath,
                    [],
                    directory.PhysicalPath,
                    enumeration.Failure));
                continue;
            }

            if (enumeration.Entries is not { } enumeratedEntries)
            {
                throw new InvalidOperationException("A complete directory enumeration must carry entries.");
            }

            var entries = enumeratedEntries
                .OrderBy(entry => Path.GetFileName(entry), StringComparer.Ordinal)
                .ThenBy(entry => entry, StringComparer.Ordinal)
                .ToArray();
            foreach (var entry in entries)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return true;
                }

                var name = Path.GetFileName(entry);
                if (!SourceLogicalPath.IsCanonicalSegment(name))
                {
                    continue;
                }

                var canonicalPath = SourceLogicalPath.Combine(directory.CanonicalPath, name);
                var recognized = SourceFormClassifier.TryClassify(canonicalPath, out var form);
                var resolution = Resolve(workspace, canonicalPath);
                if (resolution.State != PhysicalPathState.Contained)
                {
                    state.AddUnsafeCandidate(
                        canonicalPath,
                        recognized ? form : null,
                        recognized ? SourceIdentity.DeriveId(canonicalPath) : null,
                        resolution.State,
                        directory.PhysicalPath,
                        resolution.Failure);
                    continue;
                }

                var resolvedPhysicalPath = resolution.GetContainedPhysicalPath();
                var component = LinkTargetReader.Read(resolvedPhysicalPath);
                if (component.State != PathComponentState.Ordinary
                    || component.Attributes is not { } attributes)
                {
                    state.AddUnavailableCandidate(
                        canonicalPath,
                        recognized ? form : null,
                        recognized ? SourceIdentity.DeriveId(canonicalPath) : null,
                        directory.PhysicalPath,
                        component.Failure);
                    continue;
                }

                if ((attributes & FileAttributes.Directory) != 0)
                {
                    if (directory.AncestorPhysicalPaths.Contains(
                            resolvedPhysicalPath,
                            PhysicalIdentityTracker.PathComparer))
                    {
                        state.AddUnsafeCandidate(
                            canonicalPath,
                            recognized ? form : null,
                            recognized ? SourceIdentity.DeriveId(canonicalPath) : null,
                            PhysicalPathState.Cycle,
                            directory.PhysicalPath,
                            null);
                        continue;
                    }

                    state.EnqueueDirectory(new DirectoryBoundary(
                        canonicalPath,
                        resolvedPhysicalPath,
                        directory.AncestorPhysicalPaths.Append(resolvedPhysicalPath).ToArray()));
                    continue;
                }

                if (!recognized)
                {
                    continue;
                }

                state.AddContainedCandidate(
                    canonicalPath,
                    form,
                    SourceIdentity.DeriveId(canonicalPath),
                    resolvedPhysicalPath,
                    directory.PhysicalPath);
            }
        }

        return cancellationToken.IsCancellationRequested;
    }

    private PhysicalPathResolution Resolve(CliWorkspace workspace, string logicalPath)
    {
        return _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, logicalPath));
    }

    private static SourceCatalogueIssue ReadRootIssue(
        string logicalRoot,
        PhysicalPathResolution resolution)
    {
        var code = resolution.State switch
        {
            PhysicalPathState.Missing => SourceCatalogueIssueCode.RootMissing,
            PhysicalPathState.Inaccessible
                or PhysicalPathState.InputOutputFailure => SourceCatalogueIssueCode.RootUnavailable,
            PhysicalPathState.Contained
                or PhysicalPathState.Dangling
                or PhysicalPathState.External
                or PhysicalPathState.Cycle
                or PhysicalPathState.Invalid
                or PhysicalPathState.Unsupported => SourceCatalogueIssueCode.RootUnsafe,
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution.State, "The physical path state is not defined."),
        };
        return new SourceCatalogueIssue(
            code,
            logicalRoot,
            [],
            null,
            resolution.Failure);
    }

    private sealed record DirectoryBoundary(
        string CanonicalPath,
        string PhysicalPath,
        IReadOnlyList<string> AncestorPhysicalPaths);

    private sealed class ReadState
    {
        private readonly Dictionary<string, SourceCandidate> _candidatesByPath = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _firstLogicalByPhysical =
            new(PhysicalIdentityTracker.PathComparer);
        private readonly HashSet<string> _reportedUnsafePaths = new(StringComparer.Ordinal);
        private readonly HashSet<string> _processedDirectories = new(StringComparer.Ordinal);
        private readonly Queue<DirectoryBoundary> _directories = new();

        internal ReadState(CliWorkspace workspace)
        {
            Workspace = workspace;
        }

        internal CliWorkspace Workspace { get; }

        internal List<SourceCatalogueIssue> Issues { get; } = [];

        internal List<SourceCatalogueIssue> RootIssues { get; } = [];

        internal bool MarkDirectoryProcessed(string canonicalPath)
        {
            return _processedDirectories.Add(canonicalPath);
        }

        internal void EnqueueDirectory(DirectoryBoundary directory)
        {
            _directories.Enqueue(directory);
        }

        internal bool TryDequeueDirectory(
            [NotNullWhen(true)] out DirectoryBoundary? directory)
        {
            return _directories.TryDequeue(out directory);
        }

        internal void AddRootIssue(SourceCatalogueIssue issue)
        {
            RootIssues.Add(issue);
        }

        internal void AddIssue(SourceCatalogueIssue issue)
        {
            Issues.Add(issue);
        }

        internal void AddContainedCandidate(
            string canonicalPath,
            SourceDocumentForm form,
            string? automaticId,
            string physicalPath,
            string physicalParentPath)
        {
            if (!_candidatesByPath.TryAdd(
                    canonicalPath,
                    new SourceCandidate(
                        canonicalPath,
                        form,
                        automaticId,
                        PhysicalPathState.Contained,
                        physicalPath,
                        physicalParentPath)))
            {
                return;
            }

            if (automaticId is null)
            {
                AddIssue(new SourceCatalogueIssue(
                    SourceCatalogueIssueCode.IdentityUnavailable,
                    canonicalPath,
                    [],
                    physicalParentPath,
                    null));
            }

            if (_firstLogicalByPhysical.TryGetValue(physicalPath, out var firstPath))
            {
                AddIssue(new SourceCatalogueIssue(
                    SourceCatalogueIssueCode.PhysicalAlias,
                    canonicalPath,
                    [firstPath, canonicalPath],
                    physicalParentPath,
                    null));
            }
            else
            {
                _firstLogicalByPhysical.Add(physicalPath, canonicalPath);
            }
        }

        internal void AddUnsafeCandidate(
            string canonicalPath,
            SourceDocumentForm? form,
            string? automaticId,
            PhysicalPathState state,
            string physicalParentPath,
            FilesystemFailure? failure)
        {
            if (!_reportedUnsafePaths.Add(canonicalPath))
            {
                return;
            }

            if (form is not null)
            {
                _candidatesByPath.Add(
                    canonicalPath,
                    new SourceCandidate(
                        canonicalPath,
                        form,
                        automaticId,
                        state,
                        null,
                        physicalParentPath));
                if (automaticId is null)
                {
                    AddIssue(new SourceCatalogueIssue(
                        SourceCatalogueIssueCode.IdentityUnavailable,
                        canonicalPath,
                        [],
                        physicalParentPath,
                        null));
                }
            }

            AddIssue(new SourceCatalogueIssue(
                SourceCatalogueIssueCode.CandidateUnsafe,
                canonicalPath,
                [],
                physicalParentPath,
                failure));
        }

        internal void AddUnavailableCandidate(
            string canonicalPath,
            SourceDocumentForm? form,
            string? automaticId,
            string physicalParentPath,
            FilesystemFailure? failure)
        {
            if (form is null || !_reportedUnsafePaths.Add(canonicalPath))
            {
                return;
            }

            _candidatesByPath.Add(
                canonicalPath,
                new SourceCandidate(
                    canonicalPath,
                    form,
                    automaticId,
                    PhysicalPathState.InputOutputFailure,
                    null,
                    physicalParentPath));
            if (automaticId is null)
            {
                AddIssue(new SourceCatalogueIssue(
                    SourceCatalogueIssueCode.IdentityUnavailable,
                    canonicalPath,
                    [],
                    physicalParentPath,
                    null));
            }

            AddIssue(new SourceCatalogueIssue(
                SourceCatalogueIssueCode.CandidateUnavailable,
                canonicalPath,
                [],
                physicalParentPath,
                failure));
        }

        internal SourceCatalogue FormCatalogue(bool isCancelled)
        {
            var candidates = _candidatesByPath.Values.ToArray();
            var sources = FormSources(candidates);
            AddIdentityCollisions(sources, candidates);
            AddOrphanOverwrites(candidates, sources);
            return new SourceCatalogue(
                Workspace,
                candidates,
                sources,
                Issues.Concat(RootIssues),
                isCancelled);
        }

        private List<SourceLogicalSource> FormSources(IReadOnlyList<SourceCandidate> candidates)
        {
            var byPath = candidates
                .Where(candidate => candidate.PhysicalState == PhysicalPathState.Contained)
                .ToDictionary(candidate => candidate.CanonicalPath, StringComparer.Ordinal);
            var sources = new List<SourceLogicalSource>();
            foreach (var candidate in byPath.Values
                         .Where(candidate => candidate.Form is not null
                             && candidate.Form != SourceDocumentForm.OverwriteCompanion
                             && candidate.AutomaticId is not null))
            {
                var form = candidate.Form
                    ?? throw new InvalidOperationException("A source candidate with a form must retain that form.");
                var automaticId = candidate.AutomaticId
                    ?? throw new InvalidOperationException("A source candidate with an automatic ID must retain that ID.");
                var physicalPath = candidate.PhysicalPath
                    ?? throw new InvalidOperationException("A contained source candidate must carry a physical path.");
                var baseLayer = new SourceLayer(
                    candidate.CanonicalPath,
                    physicalPath,
                    form,
                    SourceLayerKind.Base);
                var overwritePath = SourceOverwritePath.ReadAdjacentPath(candidate.CanonicalPath);
                var overwrite = byPath.TryGetValue(overwritePath, out var overwriteCandidate)
                    && overwriteCandidate.Form == SourceDocumentForm.OverwriteCompanion
                    ? new SourceLayer(
                        overwriteCandidate.CanonicalPath,
                        overwriteCandidate.PhysicalPath
                            ?? throw new InvalidOperationException("A contained overwrite candidate must carry a physical path."),
                        SourceDocumentForm.OverwriteCompanion,
                        SourceLayerKind.Overwrite)
                    : null;
                sources.Add(new SourceLogicalSource(
                    new SourceLogicalIdentity(automaticId, candidate.CanonicalPath),
                    baseLayer,
                    overwrite));
            }

            return sources;
        }

        private void AddIdentityCollisions(
            IReadOnlyList<SourceLogicalSource> sources,
            IReadOnlyList<SourceCandidate> candidates)
        {
            var candidatesByPath = candidates.ToDictionary(candidate => candidate.CanonicalPath, StringComparer.Ordinal);
            foreach (var group in sources
                         .GroupBy(source => source.Identity.AutomaticId, StringComparer.Ordinal)
                         .Where(group => group.Count() > 1))
            {
                var paths = group
                    .Select(source => source.Identity.CanonicalBasePath)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray();
                var first = candidatesByPath[paths[0]];
                var forms = paths
                    .Select(path => candidatesByPath[path].Form
                        ?? throw new InvalidOperationException("A retained source candidate requires a source form."))
                    .ToArray();
                var code = SourceCatalogueIssueCode.IdentityCollision;
                if (forms.All(SourceFormClassifier.IsEntrypoint))
                {
                    code = forms.Any(form => form == SourceDocumentForm.CanonicalEntrypoint)
                        && forms.Any(form => form != SourceDocumentForm.CanonicalEntrypoint)
                            ? SourceCatalogueIssueCode.EntrypointCompatibilityCollision
                            : SourceCatalogueIssueCode.EntrypointAmbiguous;
                }

                AddIssue(new SourceCatalogueIssue(
                    code,
                    paths[0],
                    paths,
                    first.PhysicalParentPath,
                    null));
            }
        }

        private void AddOrphanOverwrites(
            IReadOnlyList<SourceCandidate> candidates,
            IReadOnlyList<SourceLogicalSource> sources)
        {
            var pairedPaths = sources
                .Where(source => source.Overwrite is not null)
                .Select(source =>
                    (source.Overwrite
                        ?? throw new InvalidOperationException("A source with an overwrite path must retain its overwrite layer.")).CanonicalPath)
                .ToHashSet(StringComparer.Ordinal);
            foreach (var overwrite in candidates.Where(candidate =>
                         candidate.Form == SourceDocumentForm.OverwriteCompanion
                         && candidate.PhysicalState == PhysicalPathState.Contained
                         && !pairedPaths.Contains(candidate.CanonicalPath)))
            {
                AddIssue(new SourceCatalogueIssue(
                    SourceCatalogueIssueCode.OrphanOverwrite,
                    overwrite.CanonicalPath,
                    [],
                    overwrite.PhysicalParentPath,
                    null));
            }
        }
    }
}
