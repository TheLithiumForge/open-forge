using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Shared;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallWorkspaceAdoptionBuilder(
    PhysicalPathResolver physicalPathResolver)
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly WorkspaceAdoptionDocumentPlanner _documentPlanner = new();

    internal WorkspaceAdoptionTopologyPlan PlanTopology(
        IReadOnlyList<SourceLogicalSource> existingSources,
        IReadOnlyList<SourceLogicalSource> selectedPayloadSources,
        WorkspaceSettingsDocument settings,
        WorkspaceOwnershipRead ownership)
    {
        ArgumentNullException.ThrowIfNull(existingSources);
        ArgumentNullException.ThrowIfNull(selectedPayloadSources);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(ownership);

        var adoptionCandidates = existingSources
            .Where(source => FrameworkPayloadSelection.IncludesPath(
                source.Identity.CanonicalBasePath,
                settings))
            .ToArray();
        var topology = WorkspaceAdoptionTopologyPlanner.Plan(
            adoptionCandidates,
            selectedPayloadSources,
            settings.RemovedFiles,
            ownership);
        if (topology.Cause is not null)
        {
            return topology;
        }

        var excludedDestination = topology.ReusedEntrypoints.Values
            .Concat(topology.CreatedEntrypointPaths)
            .FirstOrDefault(path => !FrameworkPayloadSelection.IncludesPath(path, settings));
        return excludedDestination is null
            ? topology
            : WorkspaceAdoptionTopologyPlan.Blocked(
                $"Required local catalogue destination '{excludedDestination}' is excluded by workspace settings.");
    }

    internal InstallWorkspaceAdoptionSourcePlan CreateSources(
        InstallWorkspaceAdoptionSourceInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var workspace = input.Workspace;
        var selectedPayloadAssets = input.SelectedPayloadAssets;
        var catalogue = input.Catalogue;
        var topology = input.Topology;

        var preservedEntrypointPaths = topology.ReusedEntrypoints
            .Where(pair => input.IsFirstInstall && string.Equals(pair.Key, pair.Value, StringComparison.Ordinal))
            .Select(pair => pair.Key)
            .ToHashSet(StringComparer.Ordinal);
        var reusedPayloadPaths = topology.ReusedEntrypoints.Keys
            .Where(path => !preservedEntrypointPaths.Contains(path))
            .ToHashSet(StringComparer.Ordinal);
        var payloadAssets = selectedPayloadAssets
            .Where(asset => !reusedPayloadPaths.Contains(asset.Path))
            .Where(asset => !topology.EligibleSourcePaths.Contains(asset.Path)
                || !SourceFormClassifier.TryClassify(asset.Path, out var form)
                || form != SourceDocumentForm.Skill)
            .OrderBy(asset => asset.Path, StringComparer.Ordinal)
            .ToArray();
        var payloadSources = payloadAssets
            .Select(asset => FrameworkPayloadSourceProjection.Create(workspace, asset))
            .ToArray();
        var payloadSourcePaths = payloadSources
            .Select(source => source.Identity.CanonicalBasePath)
            .ToHashSet(StringComparer.Ordinal);

        var createdEntrypointPaths = topology.CreatedEntrypointPaths
            .Order(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);
        var createdSources = new List<SourceLogicalSource>(createdEntrypointPaths.Count);
        foreach (var path in createdEntrypointPaths.Order(StringComparer.Ordinal))
        {
            if (catalogue.FindByPath(path) is not null)
            {
                return new InstallWorkspaceAdoptionSourcePlan
                {
                    PayloadAssets = payloadAssets,
                    PayloadSources = payloadSources,
                    IntendedSources = [],
                    PayloadSourcePaths = payloadSourcePaths,
                    CreatedEntrypointPaths = createdEntrypointPaths,
                    Cause = $"The planned local entrypoint '{path}' is already present in the observed source catalogue.",
                };
            }

            var source = CreateLocalEntrypointSource(workspace, path);
            if (source.Cause is not null)
            {
                return new InstallWorkspaceAdoptionSourcePlan
                {
                    PayloadAssets = payloadAssets,
                    PayloadSources = payloadSources,
                    IntendedSources = [],
                    PayloadSourcePaths = payloadSourcePaths,
                    CreatedEntrypointPaths = createdEntrypointPaths,
                    Cause = source.Cause,
                };
            }

            createdSources.Add(source.Source
                ?? throw new InvalidOperationException(
                    "A safe planned local entrypoint requires its prospective source."));
        }

        var replacedPayloadPaths = payloadSourcePaths;
        var observedSources = catalogue.Sources
            .Where(source => !replacedPayloadPaths.Contains(source.Identity.CanonicalBasePath));
        var intendedSources = observedSources
            .Concat(payloadSources)
            .Concat(createdSources)
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (intendedSources.Select(source => source.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != intendedSources.Length)
        {
            return new InstallWorkspaceAdoptionSourcePlan
            {
                PayloadAssets = payloadAssets,
                PayloadSources = payloadSources,
                IntendedSources = [],
                PayloadSourcePaths = payloadSourcePaths,
                CreatedEntrypointPaths = createdEntrypointPaths,
                Cause = "The prospective Install topology contains duplicate canonical source paths.",
            };
        }

        return new InstallWorkspaceAdoptionSourcePlan
        {
            PreservedEntrypointPaths = preservedEntrypointPaths,
            PayloadAssets = payloadAssets,
            PayloadSources = payloadSources,
            IntendedSources = intendedSources,
            PayloadSourcePaths = payloadSourcePaths,
            CreatedEntrypointPaths = createdEntrypointPaths,
            Cause = null,
        };
    }

    internal InstallWorkspaceAdoptionDocumentPlan ApplyDocuments(
        InstallWorkspaceAdoptionDocumentInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var workspace = input.Workspace;
        var catalogue = input.Catalogue;
        var topology = input.Topology;
        var intendedSources = input.IntendedSources;
        var payloadSourcePaths = input.PayloadSourcePaths;
        var createdEntrypointPaths = input.CreatedEntrypointPaths;
        var observedDocuments = input.ObservedDocuments;
        var observedBytes = input.ObservedBytes;

        var documents = observedDocuments.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);
        var userTargetBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var userOwnedPaths = new HashSet<string>(StringComparer.Ordinal);
        var migrations = new InstallMigrationPlanAccumulator();

        var reusedHostPaths = topology.ReusedEntrypoints.Values
            .Where(path => !payloadSourcePaths.Contains(path))
            .ToHashSet(StringComparer.Ordinal);
        foreach (var path in reusedHostPaths)
        {
            userOwnedPaths.Add(path);
        }

        foreach (var path in topology.EligibleSourcePaths
                     .Where(path => !payloadSourcePaths.Contains(path))
                     .Where(path => SourceFormClassifier.TryClassify(path, out var form)
                         && SourceFormClassifier.IsEntrypoint(form)))
        {
            userOwnedPaths.Add(path);
        }

        foreach (var path in createdEntrypointPaths)
        {
            userOwnedPaths.Add(path);
        }

        var sourceByPath = intendedSources.ToDictionary(
            source => source.Identity.CanonicalBasePath,
            StringComparer.Ordinal);
        var eligiblePaths = topology.EligibleSourcePaths
            .Concat(reusedHostPaths)
            .Where(path => !payloadSourcePaths.Contains(path))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        foreach (var path in eligiblePaths)
        {
            if (!sourceByPath.TryGetValue(path, out var source))
            {
                return Failed(
                    documents,
                    userOwnedPaths,
                    $"The eligible adoption source '{path}' is missing from prospective Install topology.");
            }

            if (source.Base.Form is SourceDocumentForm.Loader or SourceDocumentForm.OverwriteCompanion)
            {
                continue;
            }

            if (!observedBytes.TryGetValue(path, out var originalBytes)
                || !observedDocuments.ContainsKey(path))
            {
                return Failed(
                    documents,
                    userOwnedPaths,
                    $"The eligible adoption source '{path}' has no exact observed document bytes.");
            }

            var plan = _documentPlanner.Plan(
                path,
                source.Base.Form,
                originalBytes,
                ensureEntries: SourceFormClassifier.IsEntrypoint(source.Base.Form));
            if (plan.Cause is { } cause)
            {
                return Failed(documents, userOwnedPaths, cause);
            }

            var intendedBytes = plan.IntendedBytes
                ?? throw new InvalidOperationException(
                    "An unblocked adoption document plan requires intended bytes.");
            if (originalBytes.AsSpan().SequenceEqual(intendedBytes))
            {
                continue;
            }

            userTargetBytes[path] = [.. intendedBytes];
            userOwnedPaths.Add(path);
            documents[path] = StrictUtf8.GetString(intendedBytes);
            if (plan.Actions.Count > 0)
            {
                migrations.Add(
                    path,
                    plan.Actions,
                    plan.Fields,
                    plan.Derivation);
            }
        }

        foreach (var path in createdEntrypointPaths.Order(StringComparer.Ordinal))
        {
            var newline = ReadNewline(path, intendedSources, documents);
            var plan = _documentPlanner.CreateEntrypoint(path, newline, input.Frontmatter);
            if (plan.Cause is { } cause)
            {
                return Failed(documents, userOwnedPaths, cause);
            }

            var intendedBytes = plan.IntendedBytes
                ?? throw new InvalidOperationException(
                    "An unblocked entrypoint creation plan requires intended bytes.");
            documents[path] = StrictUtf8.GetString(intendedBytes);
            userTargetBytes[path] = [.. intendedBytes];
            if (plan.Actions.Count > 0)
            {
                migrations.Add(
                    path,
                    plan.Actions,
                    plan.Fields,
                    plan.Derivation);
            }
        }

        foreach (var path in input.PreservedEntrypointBytes.Keys.Order(StringComparer.Ordinal))
        {
            var overwritePath = SourceOverwritePath.ReadAdjacentPath(path);
            input.ExistingOverwriteBytes.TryGetValue(overwritePath, out var existingOverwrite);
            userTargetBytes[overwritePath] = InstallEntrypointPreservation.Preserve(
                input.PreservedEntrypointBytes[path], existingOverwrite);
            userOwnedPaths.Add(overwritePath);
            migrations.Add(overwritePath, [WorkspaceAdoptionAction.ContentPreserved], [], []);
        }

        return new InstallWorkspaceAdoptionDocumentPlan
        {
            Documents = documents,
            UserTargetBytes = userTargetBytes,
            UserOwnedPaths = userOwnedPaths,
            Migrations = migrations.Build(),
            Cause = null,
        };
    }

    private InstallWorkspaceAdoptionLocalSource CreateLocalEntrypointSource(
        CliWorkspace workspace,
        string canonicalPath)
    {
        if (!SourceFormClassifier.TryClassify(canonicalPath, out var form)
            || !SourceFormClassifier.IsEntrypoint(form))
        {
            return new InstallWorkspaceAdoptionLocalSource
            {
                Source = null,
                Cause = $"The planned local route '{canonicalPath}' is not a recognized entrypoint.",
            };
        }

        var automaticId = SourceIdentity.DeriveId(canonicalPath);
        if (automaticId is null)
        {
            return new InstallWorkspaceAdoptionLocalSource
            {
                Source = null,
                Cause = $"The planned local entrypoint '{canonicalPath}' has no canonical source identity.",
            };
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
            return new InstallWorkspaceAdoptionLocalSource
            {
                Source = null,
                Cause = cause,
            };
        }

        var physicalPath = Path.GetFullPath(Path.Combine(
            workspace.PhysicalRoot,
            canonicalPath.Replace('/', Path.DirectorySeparatorChar)));
        return new InstallWorkspaceAdoptionLocalSource
        {
            Source = new SourceLogicalSource(
                new SourceLogicalIdentity(automaticId, canonicalPath),
                new SourceLayer(
                    canonicalPath,
                    physicalPath,
                    form,
                    SourceLayerKind.Base)),
            Cause = null,
        };
    }

    private static string ReadNewline(
        string createdPath,
        IReadOnlyList<SourceLogicalSource> localSources,
        IReadOnlyDictionary<string, string> documents)
    {
        var targetDirectory = SourceLogicalPath.ReadParent(createdPath);
        var ancestors = localSources
            .Where(source => source.Base.Form is not (SourceDocumentForm.Loader or SourceDocumentForm.OverwriteCompanion))
            .Select(source => (
                Source: source,
                Directory: SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath)))
            .Where(candidate => IsDirectoryAncestor(candidate.Directory, targetDirectory))
            .OrderByDescending(candidate => candidate.Directory.Count(character => character == '/'))
            .ThenBy(candidate => candidate.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        foreach (var candidate in ancestors)
        {
            if (documents.TryGetValue(candidate.Source.Identity.CanonicalBasePath, out var text)
                && FindNewline(text) is { } newline)
            {
                return newline;
            }
        }

        foreach (var source in localSources
                     .Where(source => source.Base.Form is not (SourceDocumentForm.Loader or SourceDocumentForm.OverwriteCompanion))
                     .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal))
        {
            if (documents.TryGetValue(source.Identity.CanonicalBasePath, out var text)
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

    private static InstallWorkspaceAdoptionDocumentPlan Failed(
        IReadOnlyDictionary<string, string> documents,
        IReadOnlySet<string> userOwnedPaths,
        string cause)
        => new()
        {
            Documents = documents,
            UserTargetBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal),
            UserOwnedPaths = userOwnedPaths,
            Migrations = [],
            Cause = cause,
        };
}

internal sealed class InstallMigrationPlanAccumulator
{
    private readonly Dictionary<string, MutableMigration> _rows = new(StringComparer.Ordinal);

    internal InstallMigrationPlanAccumulator()
    {
    }

    internal InstallMigrationPlanAccumulator(IEnumerable<InstallMigrationPlan> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        foreach (var row in rows)
        {
            Add(row.Path, row.Actions, row.Fields, row.Derivation);
        }
    }

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

    internal IReadOnlyList<InstallMigrationPlan> Build()
        => _rows
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new InstallMigrationPlan
            {
                Path = pair.Key,
                Actions = pair.Value.Actions
                    .OrderBy(ActionOrder)
                    .ToArray(),
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
