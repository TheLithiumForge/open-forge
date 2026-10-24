using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Shared.NavigationSharing.Models;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Input;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Maintenance;

// Reads immutable maintenance facts. It never forms a mutation plan or persists evidence.
internal sealed class DoctorMaintenanceReader(RenameEvidenceReader renames, Func<string, string>? readRouteParent = null,
    PhysicalPathResolver? physicalPaths = null)
{
    private static DoctorMaintenanceObservation Unavailable() => new()
    {
        Eligibility = ImmutableDictionary<string, RoutedMetadataEligibility>.Empty,
        IndexablePaths = [],
        ChainGapPaths = [],
        OldNavigationPaths = ImmutableDictionary<string, string>.Empty,
        Renames = null,
    };

    internal const int MaximumConcurrentSources = 8;
    private readonly Func<string, string> _readRouteParent = readRouteParent ?? SourceLogicalPath.ReadParent;
    private readonly RoutedMetadataCompletionPolicy _metadata = new();
    private readonly FrameworkMetadataCompleter _completion = new();

    internal async ValueTask<DoctorMaintenanceObservation> ReadAsync(
        RouteDoctorView routes, LocalReferenceDoctorView references, NavigationSharingRead sharing, CancellationToken token)
    {
        if (routes.Catalogue is not { } catalogue) return Unavailable();
        var observation = await new WorkspaceLayoutReader(physicalPaths ?? new PhysicalPathResolver()).ReadAsync(catalogue.Workspace, token).ConfigureAwait(false);
        return await ReadAsync(routes, Task.FromResult(references), sharing, observation, token).ConfigureAwait(false);
    }

    internal async ValueTask<DoctorMaintenanceObservation> ReadAsync(
        RouteDoctorView routes, Task<LocalReferenceDoctorView> referenceRead, NavigationSharingRead sharing, WorkspaceLayoutRead observation, CancellationToken token)
    {
        var paths = (physicalPaths ?? new PhysicalPathResolver()).CreateReadSession();
        if (routes.Catalogue is not { } catalogue) return Unavailable();
        var workspace = catalogue.Workspace;
        var protection = ReferenceProtectionReader.Read(observation);
        var settings = observation.Settings;
        var policyAvailable = settings.State is WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete
            && sharing.Ownership.State is WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete;
        var context = new RoutedMetadataPolicyContext
        {
            Layout = catalogue.Layout,
            Settings = settings.Document,
            Ownership = sharing.Ownership.Document,
        };
        var roots = routes.Routes.Topology.LoaderRootPaths.ToImmutableArray();
        var rootFolders = roots.Select(path => (Path: path, Folder: _readRouteParent(path))).ToArray();
        var malformedMetadata = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        var entrypoints = catalogue.Sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form))
            .Select(source => source.Identity.CanonicalBasePath)
            .Select(path => (Path: path, Folder: _readRouteParent(path))).ToArray();
        var eligibility = ImmutableDictionary.CreateBuilder<string, RoutedMetadataEligibility>(StringComparer.Ordinal);
        var states = new RoutedMetadataEligibility?[routes.Sources.Count];
        var malformed = new string?[routes.Sources.Count];
        await Parallel.ForEachAsync(Enumerable.Range(0, routes.Sources.Count), new ParallelOptions
        {
            MaxDegreeOfParallelism = MaximumConcurrentSources,
            CancellationToken = token,
        }, (index, cancellation) =>
        {
            var source = routes.Sources[index];
            if (source.Document is not { } document) return ValueTask.CompletedTask;
            var path = source.Source.Identity.CanonicalBasePath;
            var root = rootFolders.FirstOrDefault(root => Within(path, root.Folder)).Path;
            var facts = new RoutedMetadataFileFacts
            {
                CanonicalPath = path,
                LoaderRootEntrypointPath = root,
                RouteEntrypointPaths = [.. entrypoints.Where(entrypoint => Within(path, entrypoint.Folder)).Select(entrypoint => entrypoint.Path)],
                IsSelected = true,
                IsReadable = source.IsReadable,
                IsSafeOrdinaryLeaf = NoFollowLeafObserver.Observe(paths, workspace, SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path), cancellation).State == NoFollowLeafState.OrdinaryFile,
                IsGeneratedFile = false,
                HasManagedAuthoredRegions = document.ManagedHost.State != MarkdownManagedHostState.Absent,
            };
            var state = _metadata.Evaluate(facts, context);
            states[index] = state;
            if ((state is RoutedMetadataEligibility.Eligible or RoutedMetadataEligibility.OutsideRoot
                    || state == RoutedMetadataEligibility.GeneratedContent && document.Frontmatter.State == MarkdownFrontmatterState.Unavailable)
                && (source.FrameworkMetadata.State != FrameworkDocumentMetadataState.Complete
                    || FrameworkMetadataCompletionValidator.ReadMalformedCause(source.FrameworkMetadata) is not null)
                && _completion.ReadMalformedCause(document) is { } cause)
                malformed[index] = cause;
            return ValueTask.CompletedTask;
        }).ConfigureAwait(false);
        for (var index = 0; index < routes.Sources.Count; index++)
        {
            var path = routes.Sources[index].Source.Identity.CanonicalBasePath;
            if (states[index] is { } state) eligibility[path] = state;
            if (malformed[index] is { } cause) malformedMetadata[path] = cause;
        }
        var eligible = routes.Sources.Where((source, index) => policyAvailable && states[index] == RoutedMetadataEligibility.Eligible).ToArray();
        var discovery = new RouteScaffoldChainDiscovery().Discover(new()
        {
            Catalogue = catalogue,
            RecognizedRootPaths = roots,
            EligibleSources = [.. eligible.Select(source => source.Source)],
            Settings = context.Settings,
            Sharing = sharing.Sharing,
        });
        var unsafeFolders = routes.Sources.Where(source => !source.IsReadable
                || malformedMetadata.ContainsKey(source.Source.Identity.CanonicalBasePath)
                || source.WorkspaceIssues.Any(issue => issue.Kind == RouteWorkspaceSourceIssueKind.FrontmatterMalformed)
                || source.Document?.GeneratedRegion.InvalidKind is not null)
            .Where(source => SourceFormClassifier.IsEntrypoint(source.Source.Base.Form))
            .Select(source => SourceLogicalPath.ReadParent(source.Source.Identity.CanonicalBasePath))
            .Concat(discovery.Boundaries.Select(boundary => boundary.Path))
            .ToList();
        foreach (var gap in discovery.Gaps)
        {
            if (NoFollowLeafObserver.Observe(paths, workspace, SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, gap.CanonicalPath), token).State != NoFollowLeafState.Missing)
                unsafeFolders.Add(SourceLogicalPath.ReadParent(gap.CanonicalPath));
        }
        var indexable = eligible.Select(source => source.Source.Identity.CanonicalBasePath)
            .Where(path => !malformedMetadata.ContainsKey(path) && !unsafeFolders.Any(folder => Within(path, folder))).ToImmutableHashSet(StringComparer.Ordinal);
        var gapPaths = indexable.Where(path => discovery.Gaps.Any(gap => Within(path, SourceLogicalPath.ReadParent(gap.CanonicalPath))))
            .ToImmutableHashSet(StringComparer.Ordinal);
        RenameEvidenceResult? evidence = null;
        if (policyAvailable)
        {
            var policy = new RenameEvidencePolicy
            {
                Layout = catalogue.Layout,
                ManagedPaths = [.. context.Ownership.ManagedPaths().Select(claim => claim.Path)],
                ExcludedPaths = [.. catalogue.Candidates.Where(candidate => WorkspaceRemovals.IsPathRemoved(catalogue.Layout, candidate.CanonicalPath, context.Settings))
                    .Select(candidate => candidate.CanonicalPath), .. context.Settings.RemovedFiles, .. context.Settings.RemovedDirectories],
                PrivatePaths = [.. context.Ownership.Framework?.GitIgnoredRoutes.Select(route => route.Directory) ?? []],
                ProjectionPaths = [.. context.Ownership.Libraries.SelectMany(library => library.Paths)],
            };
            var records = ImmutableArray.CreateBuilder<RenameRecordedIdentity>();
            // Match Repair's evidence boundary, including current rows that contradict an apparent metadata rename.
            // Managed navigation can record an unmanaged child. The reader admits each recorded child separately.
            var recordPolicy = policy with { ManagedPaths = [] };
            var parser = new MarkdownDocumentParser();
            var documents = routes.Sources.Where(source => source.Document is not null)
                .ToDictionary(source => source.Source.Identity.CanonicalBasePath,
                    source => source.Document ?? throw new InvalidOperationException("An observed document must be present."), StringComparer.Ordinal);
            foreach (var source in catalogue.Sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form) || source.Base.Form == SourceDocumentForm.Loader))
            {
                var path = source.Identity.CanonicalBasePath;
                if (!RenameCandidateAdmission.Allows(path, recordPolicy) && path != catalogue.Layout.LoaderPath) continue;
                if (NoFollowLeafObserver.Observe(paths, workspace, SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path), token).State != NoFollowLeafState.OrdinaryFile) continue;
                try
                {
                    var bytes = await File.ReadAllBytesAsync(source.Base.PhysicalPath, token).ConfigureAwait(false);
                    var snapshot = FileStateSnapshot.File(SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path), source.Base.PhysicalPath, bytes);
                    var text = System.Text.Encoding.UTF8.GetString(bytes);
                    var document = documents.TryGetValue(path, out var retained) && retained is not null && string.Equals(retained.Source, text, StringComparison.Ordinal)
                        ? retained : parser.Parse(text);
                    records.AddRange(RenameRecordedEntryReader.Read(catalogue.Layout, path, snapshot, document));
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Text.DecoderFallbackException or ArgumentException or InvalidOperationException) { }
            }
            var requiresEvidence = ReferenceRenameDemand.RequiresEvidence(catalogue, paths, records, hasMissingTarget: false, token);
            if (!requiresEvidence)
            {
                var observedReferences = await referenceRead.ConfigureAwait(false);
                requiresEvidence = observedReferences.References.Any(reference => !reference.Facts.IsFolder && reference.Facts.Target.Resolution == SourceLinkTargetResolution.Missing);
            }
            if (requiresEvidence)
                evidence = await renames.ReadAsync(new()
                {
                    Workspace = workspace,
                    CandidatePaths = [.. catalogue.Candidates.Select(candidate => candidate.CanonicalPath)],
                    MissingPaths = [],
                    CandidateUniverseComplete = !catalogue.IsCancelled && catalogue.Issues.Count == 0
                    && sharing.Ownership.State is WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete
                    && settings.State is WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete,
                    RecordedIdentities = records.ToImmutable(),
                    Documents = documents,
                    Policy = policy,
                    PolicyExpectations = [sharing.Ownership.Snapshot?.Expectation ?? FileExpectation.Missing(sharing.Ownership.LogicalPath),
                    settings.Snapshot?.Expectation ?? FileExpectation.Missing(settings.LogicalPath)],
                }, token).ConfigureAwait(false);
        }
        var references = await referenceRead.ConfigureAwait(false);
        var provenNavigation = evidence is null ? [] : RenameCanonicalEntrypointEvidence.Read(evidence).Select(move => move.Proof.NewPath).ToArray();
        var sourceMoves = ImmutableArray.CreateBuilder<DoctorSourceMoveObservation>();
        if (evidence is not null)
        {
            var resolver = new RenameSourceMoveResolver(paths);
            foreach (var reference in references.References)
            {
                var moved = await resolver.ResolveAsync(new(workspace, reference), evidence, token).ConfigureAwait(false);
                if (moved is not null) sourceMoves.Add(new(reference, moved));
            }
        }
        var oldNavigation = RoutedMaintenanceOldNavigationReader.Read(catalogue)
            .Where(item => eligibility.TryGetValue(item.Path, out var state) && state == RoutedMetadataEligibility.Eligible
                && !provenNavigation.Contains(item.Path, StringComparer.Ordinal))
            .ToImmutableDictionary(item => item.Path, item => item.CanonicalFileName, StringComparer.Ordinal);
        var existingCanonicalNavigation = oldNavigation
            .Select(item => SourceLogicalPath.Combine(SourceLogicalPath.ReadParent(item.Key), item.Value))
            .Where(path => NoFollowLeafObserver.Observe(paths, workspace,
                SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path), token).State != NoFollowLeafState.Missing)
            .ToImmutableHashSet(StringComparer.Ordinal);
        return new()
        {
            Eligibility = eligibility.ToImmutable(),
            IndexablePaths = indexable,
            ChainGapPaths = gapPaths,
            OldNavigationPaths = oldNavigation,
            ExistingCanonicalNavigationPaths = existingCanonicalNavigation,
            Renames = evidence,
            ReferenceProtectionPolicy = protection.Policy,
            SourceMoves = sourceMoves.ToImmutable(),
            MalformedMetadata = malformedMetadata.ToImmutable(),
            GeneratedEntries = ReadGeneratedEntries(routes),
            ManagingUpdates = context.Ownership.ManagedPaths().GroupBy(claim => claim.Path, StringComparer.Ordinal)
                .Where(group => group.Count() == 1).ToImmutableDictionary(group => group.Key, group => ManagingUpdate(group.Single()), StringComparer.Ordinal),
        };
    }

    private static bool Within(string path, string folder) => path == folder || path.StartsWith(folder + "/", StringComparison.Ordinal);

    private static ImmutableDictionary<string, SourceLocation> ReadGeneratedEntries(RouteDoctorView routes)
    {
        var regions = ImmutableDictionary.CreateBuilder<string, SourceLocation>(StringComparer.Ordinal);
        foreach (var source in routes.Sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Source.Base.Form)))
        {
            if (source.Document is not { } document || document.GeneratedRegion.State != MarkdownGeneratedRegionState.Complete
                || document.GeneratedRegion.EntriesBlock is not { } block) continue;
            regions.Add(source.Source.Identity.CanonicalBasePath, new Utf8SourceMap(document.Source).Map(block.Span.Start, block.Span.Length));
        }
        return regions.ToImmutable();
    }

    internal static DoctorNextAction ManagingUpdate(OwnedPath claim) => claim.Manager switch
    {
        OwnedPathManager.Framework => DoctorMaintenanceGuidance.Command(DoctorNextOperation.Update, "open-forge update", "The Framework owns this metadata."),
        OwnedPathManager.Extension => DoctorMaintenanceGuidance.Command(DoctorNextOperation.ExtensionUpdate,
            $"open-forge extension update {CliCommandText.Quote(claim.Owner)}", "The Extension owns this metadata."),
        _ => throw new ArgumentOutOfRangeException(nameof(claim), claim.Manager, "The managing operation is not defined."),
    };
}
