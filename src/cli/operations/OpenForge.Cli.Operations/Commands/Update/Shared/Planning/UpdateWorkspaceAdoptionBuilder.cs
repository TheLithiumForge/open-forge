using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Shared;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Planning;

internal sealed class UpdateWorkspaceAdoptionBuilder(PhysicalPathResolver physicalPathResolver)
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly WorkspaceAdoptionDocumentPlanner _documentPlanner = new();

    internal UpdateWorkspaceAdoptionSourcePlan PlanSources(
        CliWorkspace workspace,
        SourceCatalogue catalogue,
        IReadOnlyList<FrameworkPayloadAsset> selectedAssets,
        WorkspaceSettingsDocument settings,
        IReadOnlySet<string> retiredTargetPaths,
        WorkspaceOwnershipRead? ownership)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(selectedAssets);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(retiredTargetPaths);

        var allPayloadAssets = selectedAssets
            .Where(asset => asset.Path.StartsWith(".agents/", StringComparison.Ordinal))
            .Where(asset => FrameworkPayloadSelection.IncludesPath(asset.Path, settings))
            .OrderBy(asset => asset.Path, StringComparer.Ordinal)
            .ToArray();
        var allPayloadSources = allPayloadAssets
            .Select(asset => FrameworkPayloadSourceProjection.Create(workspace, asset))
            .ToArray();

        var topology = ownership is { State: WorkspaceOwnershipReadState.Complete }
            && ownership.Document.Framework is not null
                ? WorkspaceAdoptionTopologyPlanner.Plan(
                    catalogue.Sources
                        .Where(source => FrameworkPayloadSelection.IncludesPath(
                            source.Identity.CanonicalBasePath,
                            settings))
                        .Where(source => !retiredTargetPaths.Contains(
                            source.Identity.CanonicalBasePath))
                        .ToArray(),
                    allPayloadSources,
                    settings.RemovedFiles,
                    ownership)
                : EmptyTopology();
        if (topology.Cause is { } topologyCause)
        {
            return Failed(topology, topologyCause);
        }

        var excludedDestination = topology.ReusedEntrypoints.Keys
            .Concat(topology.ReusedEntrypoints.Values)
            .Concat(topology.CreatedEntrypointPaths)
            .FirstOrDefault(path => !FrameworkPayloadSelection.IncludesPath(path, settings));
        if (excludedDestination is not null)
        {
            return Failed(
                WorkspaceAdoptionTopologyPlan.Blocked(
                    $"Required local catalogue destination '{excludedDestination}' is excluded by workspace settings."),
                $"Required local catalogue destination '{excludedDestination}' is excluded by workspace settings.");
        }

        var replacedPayloadPaths = topology.ReusedEntrypoints.Keys.ToHashSet(StringComparer.Ordinal);
        var payloadAssets = allPayloadAssets
            .Where(asset => !replacedPayloadPaths.Contains(asset.Path))
            .ToArray();
        var payloadSources = payloadAssets
            .Select(asset => FrameworkPayloadSourceProjection.Create(workspace, asset))
            .ToArray();
        var payloadSourcePaths = payloadSources
            .Select(source => source.Identity.CanonicalBasePath)
            .ToHashSet(StringComparer.Ordinal);

        var createdSources = new List<SourceLogicalSource>(topology.CreatedEntrypointPaths.Count);
        foreach (var path in topology.CreatedEntrypointPaths.Order(StringComparer.Ordinal))
        {
            if (catalogue.FindByPath(path) is not null)
            {
                return Failed(
                    topology,
                    $"The planned local entrypoint '{path}' is already present in the observed source catalogue.");
            }

            var created = CreateLocalEntrypointSource(workspace, path);
            if (created.Source is null)
            {
                return Failed(topology, created.Cause
                    ?? $"The planned local entrypoint '{path}' has no safe prospective source.");
            }

            createdSources.Add(created.Source);
        }

        var intendedSources = catalogue.Sources
            .Where(source => !payloadSourcePaths.Contains(source.Identity.CanonicalBasePath))
            .Concat(payloadSources)
            .Concat(createdSources)
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (intendedSources.Select(source => source.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != intendedSources.Length)
        {
            return Failed(topology, "The prospective Update topology contains duplicate canonical source paths.");
        }

        var adoptionSourcePaths = topology.EligibleSourcePaths
            .Concat(topology.ReusedEntrypoints.Values)
            .Concat(topology.CreatedEntrypointPaths)
            .Where(path => FrameworkPayloadSelection.IncludesPath(path, settings))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        return new UpdateWorkspaceAdoptionSourcePlan(
            payloadAssets,
            payloadSources,
            intendedSources,
            payloadSourcePaths,
            adoptionSourcePaths,
            topology.CreatedEntrypointPaths.ToHashSet(StringComparer.Ordinal),
            topology,
            Cause: null);
    }

    internal UpdateWorkspaceAdoptionDocumentBuild PlanDocuments(
        UpdateWorkspaceAdoptionSourcePlan sourcePlan,
        IReadOnlyDictionary<string, string> observedDocuments,
        IReadOnlyDictionary<string, byte[]> observedBytes,
        IReadOnlyDictionary<string, FileStateSnapshot> observedSnapshots,
        IReadOnlyDictionary<string, FileStateSnapshot> createdSnapshots)
    {
        ArgumentNullException.ThrowIfNull(sourcePlan);
        ArgumentNullException.ThrowIfNull(observedDocuments);
        ArgumentNullException.ThrowIfNull(observedBytes);
        ArgumentNullException.ThrowIfNull(observedSnapshots);
        ArgumentNullException.ThrowIfNull(createdSnapshots);

        var documents = observedDocuments.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);
        var targetBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var originalSnapshots = new Dictionary<string, FileStateSnapshot>(StringComparer.Ordinal);
        var migrationPlans = new UpdateMigrationPlanAccumulator();
        var sourcesByPath = sourcePlan.IntendedSources.ToDictionary(
            source => source.Identity.CanonicalBasePath,
            StringComparer.Ordinal);

        foreach (var path in sourcePlan.AdoptionSourcePaths.Order(StringComparer.Ordinal))
        {
            if (sourcePlan.CreatedEntrypointPaths.Contains(path))
            {
                if (!createdSnapshots.TryGetValue(path, out var missingSnapshot)
                    || missingSnapshot.Kind != FileExpectationKind.Missing)
                {
                    return Failed(
                        documents,
                        $"The planned local entrypoint '{path}' has no exact missing-target snapshot.");
                }

                if (!sourcesByPath.TryGetValue(path, out var createdSource))
                {
                    return Failed(
                        documents,
                        $"The planned local entrypoint '{path}' is absent from prospective Update topology.");
                }

                var create = _documentPlanner.CreateEntrypoint(
                    path,
                    ReadNewline(
                        path,
                        sourcePlan.IntendedSources,
                        sourcePlan.PayloadSourcePaths,
                        observedDocuments));
                if (create.Cause is { } createCause)
                {
                    return Failed(documents, createCause);
                }

                var createBytes = create.IntendedBytes
                    ?? throw new InvalidOperationException(
                        "An unblocked Update entrypoint creation requires intended bytes.");
                documents[path] = StrictUtf8.GetString(createBytes);
                targetBytes[path] = createBytes.ToArray();
                originalSnapshots[path] = missingSnapshot;
                if (create.Actions.Count > 0)
                {
                    migrationPlans.Add(path, create.Actions, create.Fields, create.Derivation);
                }

                continue;
            }

            if (sourcePlan.PayloadSourcePaths.Contains(path))
            {
                continue;
            }

            if (!sourcesByPath.TryGetValue(path, out var source))
            {
                return Failed(
                    documents,
                    $"The eligible adoption source '{path}' is missing from prospective Update topology.");
            }

            if (source.Base.Form is SourceDocumentForm.Loader or SourceDocumentForm.OverwriteCompanion)
            {
                continue;
            }

            if (!observedBytes.TryGetValue(path, out var originalBytes)
                || !observedDocuments.ContainsKey(path)
                || !observedSnapshots.TryGetValue(path, out var originalSnapshot))
            {
                return Failed(
                    documents,
                    $"The eligible adoption source '{path}' has no exact observed document bytes and snapshot.");
            }

            originalSnapshots[path] = originalSnapshot;

            var plan = _documentPlanner.Plan(
                path,
                source.Base.Form,
                originalBytes,
                ensureEntries: SourceFormClassifier.IsEntrypoint(source.Base.Form));
            if (plan.Cause is { } planCause)
            {
                return Failed(documents, planCause);
            }

            var intendedBytes = plan.IntendedBytes
                ?? throw new InvalidOperationException(
                    "An unblocked Update adoption document plan requires intended bytes.");
            if (originalBytes.AsSpan().SequenceEqual(intendedBytes))
            {
                continue;
            }

            targetBytes[path] = intendedBytes.ToArray();
            documents[path] = StrictUtf8.GetString(intendedBytes);
            if (plan.Actions.Count > 0)
            {
                migrationPlans.Add(path, plan.Actions, plan.Fields, plan.Derivation);
            }
        }

        return new UpdateWorkspaceAdoptionDocumentBuild(
            documents,
            targetBytes,
            originalSnapshots,
            migrationPlans.Build(),
            Cause: null);
    }

    private UpdateWorkspaceAdoptionLocalSource CreateLocalEntrypointSource(
        CliWorkspace workspace,
        string canonicalPath)
    {
        if (!SourceFormClassifier.TryClassify(canonicalPath, out var form)
            || !SourceFormClassifier.IsEntrypoint(form))
        {
            return new UpdateWorkspaceAdoptionLocalSource(
                Source: null,
                Cause: $"The planned local route '{canonicalPath}' is not a recognized entrypoint.");
        }

        var automaticId = SourceIdentity.DeriveId(canonicalPath);
        if (automaticId is null)
        {
            return new UpdateWorkspaceAdoptionLocalSource(
                Source: null,
                Cause: $"The planned local entrypoint '{canonicalPath}' has no canonical source identity.");
        }

        var logicalPath = Path.GetFullPath(Path.Combine(
            workspace.LexicalRoot,
            canonicalPath.Replace('/', Path.DirectorySeparatorChar)));
        var resolution = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            logicalPath);
        if (resolution.State != PhysicalPathState.Missing)
        {
            var cause = resolution.Failure?.DirectCause
                ?? $"The planned local entrypoint '{canonicalPath}' is not physically missing and safely contained.";
            return new UpdateWorkspaceAdoptionLocalSource(Source: null, Cause: cause);
        }

        var physicalPath = Path.GetFullPath(Path.Combine(
            workspace.PhysicalRoot,
            canonicalPath.Replace('/', Path.DirectorySeparatorChar)));
        return new UpdateWorkspaceAdoptionLocalSource(
            new SourceLogicalSource(
                new SourceLogicalIdentity(automaticId, canonicalPath),
                new SourceLayer(
                    canonicalPath,
                    physicalPath,
                    form,
                    SourceLayerKind.Base)),
            Cause: null);
    }

    private static string ReadNewline(
        string createdPath,
        IReadOnlyList<SourceLogicalSource> localSources,
        IReadOnlySet<string> payloadSourcePaths,
        IReadOnlyDictionary<string, string> observedDocuments)
    {
        var targetDirectory = SourceLogicalPath.ReadParent(createdPath);
        var ancestors = localSources
            .Where(source => !payloadSourcePaths.Contains(source.Identity.CanonicalBasePath)
                && source.Base.Form is not (SourceDocumentForm.Loader or SourceDocumentForm.OverwriteCompanion))
            .Select(source => (
                Source: source,
                Directory: SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath)))
            .Where(candidate => IsDirectoryAncestor(candidate.Directory, targetDirectory))
            .OrderByDescending(candidate => candidate.Directory.Count(character => character == '/'))
            .ThenBy(candidate => candidate.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        foreach (var candidate in ancestors)
        {
            if (observedDocuments.TryGetValue(candidate.Source.Identity.CanonicalBasePath, out var text)
                && FindNewline(text) is { } newline)
            {
                return newline;
            }
        }

        foreach (var source in localSources
                     .Where(source => !payloadSourcePaths.Contains(source.Identity.CanonicalBasePath)
                         && source.Base.Form is not (SourceDocumentForm.Loader or SourceDocumentForm.OverwriteCompanion))
                     .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal))
        {
            if (observedDocuments.TryGetValue(source.Identity.CanonicalBasePath, out var text)
                && FindNewline(text) is { } newline)
            {
                return newline;
            }
        }

        return "\n";
    }

    private static bool IsDirectoryAncestor(string ancestor, string directory)
        => string.Equals(ancestor, directory, StringComparison.Ordinal)
            || directory.StartsWith($"{ancestor}/", StringComparison.Ordinal);

    private static string? FindNewline(string text)
    {
        var lineFeed = text.IndexOf('\n');
        var carriageReturn = text.IndexOf('\r');
        if (lineFeed < 0 && carriageReturn < 0)
        {
            return null;
        }

        if (carriageReturn >= 0 && (lineFeed < 0 || carriageReturn < lineFeed))
        {
            return carriageReturn + 1 < text.Length && text[carriageReturn + 1] == '\n'
                ? "\r\n"
                : "\r";
        }

        return "\n";
    }

    private static WorkspaceAdoptionTopologyPlan EmptyTopology()
        => WorkspaceAdoptionTopologyPlan.Complete(
            new Dictionary<string, string>(StringComparer.Ordinal),
            [],
            []);

    private static UpdateWorkspaceAdoptionSourcePlan Failed(
        WorkspaceAdoptionTopologyPlan topology,
        string cause)
        => new([], [], [], new HashSet<string>(StringComparer.Ordinal),
            new HashSet<string>(StringComparer.Ordinal), new HashSet<string>(StringComparer.Ordinal),
            topology, cause);

    private static UpdateWorkspaceAdoptionDocumentBuild Failed(
        IReadOnlyDictionary<string, string> documents,
        string cause)
        => new(
            documents,
            new Dictionary<string, byte[]>(StringComparer.Ordinal),
            new Dictionary<string, FileStateSnapshot>(StringComparer.Ordinal),
            [],
            cause);
}

internal sealed record UpdateWorkspaceAdoptionSourcePlan(
    IReadOnlyList<FrameworkPayloadAsset> PayloadAssets,
    IReadOnlyList<SourceLogicalSource> PayloadSources,
    IReadOnlyList<SourceLogicalSource> IntendedSources,
    IReadOnlySet<string> PayloadSourcePaths,
    IReadOnlySet<string> AdoptionSourcePaths,
    IReadOnlySet<string> CreatedEntrypointPaths,
    WorkspaceAdoptionTopologyPlan Topology,
    string? Cause);

internal sealed record UpdateWorkspaceAdoptionDocumentBuild(
    IReadOnlyDictionary<string, string> Documents,
    IReadOnlyDictionary<string, byte[]> TargetBytes,
    IReadOnlyDictionary<string, FileStateSnapshot> OriginalSnapshots,
    IReadOnlyList<UpdateMigrationPlan> Migrations,
    string? Cause);

internal sealed record UpdateWorkspaceAdoptionLocalSource(
    SourceLogicalSource? Source,
    string? Cause);

internal sealed class UpdateMigrationPlanAccumulator
{
    private readonly Dictionary<string, MutableMigration> _rows = new(StringComparer.Ordinal);

    internal void Add(
        string path,
        IEnumerable<WorkspaceAdoptionAction> actions,
        IEnumerable<string> fields,
        IEnumerable<WorkspaceAdoptionDerivation> derivation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(derivation);

        if (!_rows.TryGetValue(path, out var row))
        {
            row = new MutableMigration();
            _rows.Add(path, row);
        }

        foreach (var action in actions)
        {
            if (!row.Actions.Contains(action))
            {
                row.Actions.Add(action);
            }
        }

        foreach (var field in fields)
        {
            if (!row.Fields.Contains(field, StringComparer.Ordinal))
            {
                row.Fields.Add(field);
            }
        }

        foreach (var item in derivation)
        {
            if (!row.Derivation.Contains(item))
            {
                row.Derivation.Add(item);
            }
        }
    }

    internal void AddNavigationUpdated(string path)
        => Add(path, [WorkspaceAdoptionAction.NavigationUpdated], [], []);

    internal IReadOnlyList<UpdateMigrationPlan> Build(IReadOnlySet<string>? changedPaths = null)
        => _rows
            .Where(pair => changedPaths is null || changedPaths.Contains(pair.Key))
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new UpdateMigrationPlan
            {
                Path = pair.Key,
                Actions = pair.Value.Actions.OrderBy(ActionOrder).ToArray(),
                Fields = pair.Value.Fields.ToArray(),
                Derivation = pair.Value.Derivation.ToArray(),
            })
            .ToArray();

    private static int ActionOrder(WorkspaceAdoptionAction action)
        => action switch
        {
            WorkspaceAdoptionAction.MetadataCompleted => 0,
            WorkspaceAdoptionAction.EntrypointCreated => 1,
            WorkspaceAdoptionAction.EntriesSectionAdded => 2,
            WorkspaceAdoptionAction.NavigationUpdated => 3,
            WorkspaceAdoptionAction.ContentPreserved => 4,
            _ => throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "The workspace adoption action is not defined."),
        };

    private sealed class MutableMigration
    {
        internal List<WorkspaceAdoptionAction> Actions { get; } = [];

        internal List<string> Fields { get; } = [];

        internal List<WorkspaceAdoptionDerivation> Derivation { get; } = [];
    }
}
