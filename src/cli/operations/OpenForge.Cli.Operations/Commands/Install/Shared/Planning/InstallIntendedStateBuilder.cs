using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallIntendedStateBuilder(PhysicalPathResolver physicalPathResolver)
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly SourceCatalogueReader _catalogueReader = new();
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();
    private readonly GeneratedNavigationProjector _projector = new();
    private readonly InstallTargetReader _targetReader = new(physicalPathResolver);
    private readonly InstallContentIdentity _contentIdentity = new();
    private readonly InstallWorkspaceAdoptionBuilder _adoptionBuilder = new(physicalPathResolver);

    internal async ValueTask<InstallIntendedStateBuild> BuildAsync(
        InstallRequest request,
        FrameworkPayload payload,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }

        WorkspaceOwnershipRead ownership;
        try
        {
            ownership = await WorkspaceOwnershipReader.ReadAsync(
                    physicalPathResolver,
                    request.Workspace,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }

        return await BuildAsync(request, payload, ownership, cancellationToken)
            .ConfigureAwait(false);
    }

    internal async ValueTask<InstallIntendedStateBuild> BuildAsync(
        InstallRequest request,
        FrameworkPayload payload,
        WorkspaceOwnershipRead ownership,
        CancellationToken cancellationToken)
    {
        var settingsRead = await WorkspaceSettingsReader.ReadAsync(
                physicalPathResolver,
                request.Workspace,
                cancellationToken)
            .ConfigureAwait(false);
        if (settingsRead.State == WorkspaceSettingsReadState.Invalid)
        {
            return Blocked(
                settingsRead.Cause ?? "The authored workspace settings are invalid.",
                InstallFindingCode.InvalidInput);
        }

        if (settingsRead.State == WorkspaceSettingsReadState.Unavailable)
        {
            return Incomplete(
                settingsRead.Cause ?? "The authored workspace settings are unavailable.",
                InstallFindingCode.LifecycleUnavailable);
        }

        if (settingsRead.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete))
        {
            throw new ArgumentOutOfRangeException(
                nameof(settingsRead),
                settingsRead.State,
                "The workspace settings read state is not defined.");
        }

        var settings = settingsRead.Document;
        var selectedPayloadAssets = payload.Assets
            .Where(asset => asset.Path.StartsWith(".agents/", StringComparison.Ordinal)
                && FrameworkPayloadSelection.IncludesPath(asset.Path, settings))
            .ToArray();
        if (ReadMissingExcludedDirectoryAncestor(
                request.Workspace.PhysicalRoot,
                settings.RemovedFiles,
                selectedPayloadAssets.Select(asset => asset.Path))
            is { } missingAncestor)
        {
            return Blocked(
                $"The excluded Framework path '{missingAncestor}' is required as a directory by another payload destination.",
                InstallFindingCode.TargetUnsafe);
        }

        SourceCatalogue catalogue;
        try
        {
            catalogue = await _catalogueReader.ReadAsync(
                    new SourceCatalogueRequest(
                        request.Workspace,
                        [SourceLogicalPath.AgentsRoot]),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }

        if (catalogue.IsCancelled)
        {
            return Cancelled();
        }

        var catalogueBoundary = ReadCatalogueBoundary(catalogue);
        if (catalogueBoundary is not null)
        {
            return catalogueBoundary;
        }

        try
        {
            var originalPayloadSources = selectedPayloadAssets
                .Select(asset => FrameworkPayloadSourceProjection.Create(request.Workspace, asset))
                .ToArray();
            var topology = _adoptionBuilder.PlanTopology(
                catalogue.Sources,
                originalPayloadSources,
                settings,
                ownership);
            if (topology.Cause is { } topologyCause)
            {
                return Blocked(topologyCause, InstallFindingCode.TargetUnsafe);
            }

            var sourcePlan = _adoptionBuilder.CreateSources(
                new InstallWorkspaceAdoptionSourceInput
                {
                    Workspace = request.Workspace,
                    SelectedPayloadAssets = selectedPayloadAssets,
                    Catalogue = catalogue,
                    Topology = topology,
                    IsFirstInstall = ownership.Document.Framework is null,
                });
            if (sourcePlan.Cause is { } sourceCause)
            {
                return Blocked(sourceCause, InstallFindingCode.TargetUnsafe);
            }

            var payloadAssets = sourcePlan.PayloadAssets;
            var payloadSources = sourcePlan.PayloadSources
                .ToDictionary(
                    source => source.Identity.CanonicalBasePath,
                    StringComparer.Ordinal);
            var payloadSourcePaths = sourcePlan.PayloadSourcePaths;
            var intendedSources = sourcePlan.IntendedSources;

            var documents = new Dictionary<string, string>(StringComparer.Ordinal);
            var documentBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            foreach (var asset in payloadAssets)
            {
                documentBytes.Add(asset.Path, asset.Bytes.ToArray());
                documents.Add(asset.Path, StrictUtf8.GetString(asset.Bytes.AsSpan()));
            }

            var projectionInputs = new List<InstallProjectionInputObservation>();
            foreach (var source in intendedSources.Where(source =>
                         !payloadSourcePaths.Contains(source.Identity.CanonicalBasePath)
                         && !sourcePlan.CreatedEntrypointPaths.Contains(source.Identity.CanonicalBasePath)))
            {
                var baseRead = await ReadProjectionInputAsync(
                        request,
                        source,
                        source.Base,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (baseRead.Build is { } baseBoundary)
                {
                    return baseBoundary;
                }

                projectionInputs.Add(baseRead.Observation
                    ?? throw new InvalidOperationException(
                        "A complete projection-input read requires its exact observation."));
                documentBytes.Add(
                    source.Identity.CanonicalBasePath,
                    baseRead.Bytes.ToArray());
                documents.Add(
                    source.Identity.CanonicalBasePath,
                    StrictUtf8.GetString(baseRead.Bytes.Span));
                if (source.Overwrite is not { } overwrite)
                {
                    continue;
                }

                var overwriteRead = await ReadProjectionInputAsync(
                        request,
                        source,
                        overwrite,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (overwriteRead.Build is { } overwriteBoundary)
                {
                    return overwriteBoundary;
                }

                projectionInputs.Add(overwriteRead.Observation
                    ?? throw new InvalidOperationException(
                        "A complete overwrite-input read requires its exact observation."));
            }

            var preservedEntrypointBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            var existingOverwriteBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            foreach (var path in sourcePlan.PreservedEntrypointPaths.Order(StringComparer.Ordinal))
            {
                var overwritePath = SourceOverwritePath.ReadAdjacentPath(path);
                var overwriteKey = PortableWorkspacePath.CreatePortableKey(overwritePath);
                var claimedCompanion = ownership.Document.Extensions
                    .SelectMany(extension => extension.Paths.Concat(extension.Regions.Select(region => region.Path)))
                    .Concat(ownership.Document.Libraries.SelectMany(library => library.Paths))
                    .Any(claim => PortableWorkspacePath.CreatePortableKey(claim) == overwriteKey)
                    || ownership.Document.Libraries.Any(library => overwriteKey.StartsWith(
                        PortableWorkspacePath.CreatePortableKey(library.DestinationRoot) + "/", StringComparison.Ordinal));
                if (claimedCompanion)
                {
                    return Blocked($"The preservation companion '{overwritePath}' is owned by another manager.",
                        InstallFindingCode.OwnershipConflict);
                }

                if (!FrameworkPayloadSelection.IncludesPath(overwritePath, settings))
                {
                    return Blocked($"The preservation companion '{overwritePath}' is excluded by workspace settings.",
                        InstallFindingCode.TargetUnsafe);
                }

                var source = catalogue.FindByPath(path)
                    ?? throw new InvalidOperationException("A preserved entrypoint requires its observed source.");
                var baseRead = await ReadProjectionInputAsync(request, source, source.Base, cancellationToken)
                    .ConfigureAwait(false);
                if (baseRead.Build is { } baseBoundary)
                {
                    return baseBoundary;
                }

                projectionInputs.Add(baseRead.Observation
                    ?? throw new InvalidOperationException("A preserved entrypoint requires its exact observation."));
                preservedEntrypointBytes.Add(path, baseRead.Bytes.ToArray());
                if (source.Overwrite is { } overwrite)
                {
                    var overwriteRead = await ReadProjectionInputAsync(request, source, overwrite, cancellationToken)
                        .ConfigureAwait(false);
                    if (overwriteRead.Build is { } overwriteBoundary)
                    {
                        return overwriteBoundary;
                    }

                    projectionInputs.Add(overwriteRead.Observation
                        ?? throw new InvalidOperationException("An existing companion requires its exact observation."));
                    existingOverwriteBytes.Add(overwritePath, overwriteRead.Bytes.ToArray());
                }
            }

            var adoption = _adoptionBuilder.ApplyDocuments(
                new InstallWorkspaceAdoptionDocumentInput
                {
                    Workspace = request.Workspace,
                    Catalogue = catalogue,
                    Topology = topology,
                    IntendedSources = intendedSources,
                    PayloadSourcePaths = payloadSourcePaths,
                    CreatedEntrypointPaths = sourcePlan.CreatedEntrypointPaths,
                    ObservedDocuments = documents,
                    ObservedBytes = documentBytes,
                    PreservedEntrypointBytes = preservedEntrypointBytes,
                    ExistingOverwriteBytes = existingOverwriteBytes,
                });
            if (adoption.Cause is { } adoptionCause)
            {
                return Blocked(adoptionCause, InstallFindingCode.TargetUnsafe);
            }

            documents = adoption.Documents.ToDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.Ordinal);
            foreach (var target in adoption.UserTargetBytes)
            {
                documentBytes[target.Key] = target.Value;
            }

            var formation = _formationBuilder.Build(catalogue, intendedSources);
            if (formation.Ambiguities.Count > 0
                || formation.IntendedTargetCollisions.Count > 0)
            {
                return Blocked(
                    "The intended Framework topology contains an ambiguous route, alias, or target collision.");
            }

            if (FrameworkPayloadSelection.FindMissingRequiredAncestor(payload, settings, formation)
                is { } missingRouteAncestor)
            {
                return Blocked(
                    $"The excluded Framework entrypoint '{missingRouteAncestor}' is missing and required to reach another payload route.",
                    InstallFindingCode.TargetUnsafe);
            }

            var parsedDocuments = new Dictionary<string, MarkdownDocumentFacts>(StringComparer.Ordinal);
            var metadata = new List<GeneratedNavigationMetadata>();
            foreach (var source in intendedSources)
            {
                if (source.Base.Form == SourceDocumentForm.Loader)
                {
                    continue;
                }

                var canonicalPath = source.Identity.CanonicalBasePath;
                var document = _markdownParser.Parse(documents[canonicalPath]);
                parsedDocuments.Add(canonicalPath, document);
                metadata.Add(new GeneratedNavigationMetadata(
                    source,
                    _metadataParser.Parse(document, source.Base.Form)));
            }

            var userRegionPaths = adoption.UserOwnedPaths;
            var regionSources = payloadSources.Values
                .Where(source => source.Base.Form == SourceDocumentForm.Loader
                    || SourceFormClassifier.IsEntrypoint(source.Base.Form))
                .Concat(intendedSources.Where(source =>
                    userRegionPaths.Contains(source.Identity.CanonicalBasePath)
                    && SourceFormClassifier.IsEntrypoint(source.Base.Form)))
                .DistinctBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .ToArray();
            var projection = _projector.Project(new GeneratedNavigationProjectionRequest(
                formation,
                ReadRegionInputs(regionSources, documents, parsedDocuments),
                metadata));
            if (projection.Regions.Any(region =>
                    region.State != GeneratedNavigationRegionState.Available))
            {
                var cause = projection.Regions.First(region =>
                        region.State != GeneratedNavigationRegionState.Available)
                    .Cause;
                return Blocked(
                    cause ?? "The intended Framework generated navigation projection is unavailable.");
            }

            var targetBytes = payloadAssets.ToDictionary(
                asset => asset.Path,
                asset => asset.Bytes.ToArray(),
                StringComparer.Ordinal);
            foreach (var userTarget in adoption.UserTargetBytes)
            {
                targetBytes[userTarget.Key] = [.. userTarget.Value];
            }

            var migrationPlans = new InstallMigrationPlanAccumulator(adoption.Migrations);
            var hasScopedAdoption = topology.EligibleSourcePaths.Count > 0
                || topology.ReusedEntrypoints.Count > 0
                || topology.CreatedEntrypointPaths.Count > 0;
            foreach (var region in projection.Regions)
            {
                var change = region.Change
                    ?? throw new InvalidOperationException(
                        "An available intended generated region requires a bounded change.");
                var intendedBytes = change.ExpectedDocumentBytes.ToArray();
                if (!documentBytes.TryGetValue(region.CanonicalPath, out var prospectiveBytes))
                {
                    throw new InvalidOperationException(
                        "An intended generated region requires prospective document bytes.");
                }

                var generatedContentChanged = !prospectiveBytes.AsSpan().SequenceEqual(intendedBytes);
                targetBytes[region.CanonicalPath] = intendedBytes;
                var userHostChanged = userRegionPaths.Contains(region.CanonicalPath);
                var existingPayloadHostChanged = hasScopedAdoption
                    && sourcePlan.PayloadSourcePaths.Contains(region.CanonicalPath)
                    && catalogue.FindByPath(region.CanonicalPath) is not null;
                if (generatedContentChanged && (userHostChanged || existingPayloadHostChanged))
                {
                    migrationPlans.AddNavigationUpdated(region.CanonicalPath);
                }
            }

            var rootAgent = payload.Find(FrameworkPayloadAsset.RootAgentPath)
                ?? throw new InvalidDataException(
                    $"The embedded Framework payload is missing {FrameworkPayloadAsset.RootAgentPath}.");
            var rootClaude = payload.Find(FrameworkPayloadAsset.RootClaudePath)
                ?? throw new InvalidDataException(
                    $"The embedded Framework payload is missing {FrameworkPayloadAsset.RootClaudePath}.");
            var managedBlockBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            if (FrameworkPayloadSelection.IncludesPath(FrameworkPayloadAsset.RootAgentPath, settings))
            {
                managedBlockBytes.Add(FrameworkPayloadAsset.RootAgentPath, rootAgent.Bytes.ToArray());
            }

            if (FrameworkPayloadSelection.IncludesPath(FrameworkPayloadAsset.RootClaudePath, settings))
            {
                managedBlockBytes.Add(FrameworkPayloadAsset.RootClaudePath, rootClaude.Bytes.ToArray());
            }

            FileExpectation? adoptionOwnershipExpectation = null;
            if (adoption.UserOwnedPaths.Count > 0)
            {
                if (ownership.Snapshot is not { } ownershipSnapshot)
                {
                    return Blocked(
                        "Workspace ownership bytes are unavailable for exact adoption revalidation.",
                        InstallFindingCode.LifecycleUnavailable);
                }

                adoptionOwnershipExpectation = ownershipSnapshot.Expectation;
            }

            var intendedState = new InstallIntendedState
            {
                TargetBytes = targetBytes,
                UserOwnedPaths = adoption.UserOwnedPaths,
                PreservedEntrypointPaths = preservedEntrypointBytes.Keys.ToHashSet(StringComparer.Ordinal),
                Migrations = migrationPlans.Build(),
                AdoptionOwnershipExpectation = adoptionOwnershipExpectation,
                ManagedBlockBytes = managedBlockBytes,
                GeneratedRegionPaths = projection.Regions
                    .Select(region => region.CanonicalPath)
                    .ToHashSet(StringComparer.Ordinal),
                ProjectionInputs = projectionInputs
                    .OrderBy(input => input.CanonicalLayerPath, StringComparer.Ordinal)
                    .ToArray(),
            };
            intendedState = await RemoveNoOpNavigationMigrationsAsync(
                    request,
                    intendedState,
                    cancellationToken)
                .ConfigureAwait(false);

            return new InstallIntendedStateBuild
            {
                State = InstallIntendedStateBuildState.Complete,
                IntendedState = intendedState,
                Cause = null,
                FindingCode = null,
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (DecoderFallbackException exception)
        {
            return Blocked(
                $"The embedded Framework payload or current authored source is not valid UTF-8: {exception.Message}");
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or InvalidOperationException)
        {
            return Blocked($"The intended Framework topology is invalid: {exception.Message}");
        }
    }

    private async ValueTask<InstallIntendedState> RemoveNoOpNavigationMigrationsAsync(
        InstallRequest request,
        InstallIntendedState intended,
        CancellationToken cancellationToken)
    {
        var candidates = intended.Migrations
            .Where(migration => migration.Actions.Contains(WorkspaceAdoptionAction.NavigationUpdated))
            .ToArray();
        if (candidates.Length == 0)
        {
            return intended;
        }

        var noOpPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var migration in candidates)
        {
            var current = await _targetReader.ReadAsync(
                    request.Workspace,
                    migration.Path,
                    cancellationToken)
                .ConfigureAwait(false);
            if (current.State == InstallTargetReadState.Missing)
            {
                noOpPaths.Add(migration.Path);
                continue;
            }

            if (current.State != InstallTargetReadState.File
                || current.Snapshot is not { HasBytes: true } snapshot
                || !intended.TargetBytes.TryGetValue(migration.Path, out var expectedBytes))
            {
                continue;
            }

            try
            {
                if (string.Equals(
                    _contentIdentity.ReadGeneratedFingerprint(snapshot.Bytes.AsSpan()),
                    _contentIdentity.ReadGeneratedFingerprint(expectedBytes),
                    StringComparison.Ordinal))
                {
                    noOpPaths.Add(migration.Path);
                }
            }
            catch (Exception exception) when (exception is ArgumentException
                or InvalidDataException
                or InvalidOperationException)
            {
                // An adopted host with a newly appended Entries section has no
                // previous region fingerprint; preserve its planned migration.
            }
        }

        if (noOpPaths.Count == 0)
        {
            return intended;
        }

        return intended with
        {
            Migrations = intended.Migrations
                .Select(migration => noOpPaths.Contains(migration.Path)
                        && migration.Actions.Contains(WorkspaceAdoptionAction.NavigationUpdated)
                    ? migration with
                    {
                        Actions = migration.Actions
                            .Where(action => action != WorkspaceAdoptionAction.NavigationUpdated)
                            .ToArray(),
                    }
                    : migration)
                .Where(migration => migration.Actions.Count > 0)
                .ToArray(),
        };
    }

    private async ValueTask<InstallProjectionInputRead> ReadProjectionInputAsync(
        InstallRequest request,
        SourceLogicalSource source,
        SourceLayer layer,
        CancellationToken cancellationToken)
    {
        var read = await _targetReader.ReadAsync(
                request.Workspace,
                layer.CanonicalPath,
                cancellationToken)
            .ConfigureAwait(false);
        if (read.State == InstallTargetReadState.Cancelled)
        {
            return new InstallProjectionInputRead(
                Observation: null,
                Bytes: ReadOnlyMemory<byte>.Empty,
                Build: Cancelled());
        }

        if (read.State == InstallTargetReadState.Unavailable)
        {
            return new InstallProjectionInputRead(
                Observation: null,
                Bytes: ReadOnlyMemory<byte>.Empty,
                Build: Incomplete(
                    "A current authored source is unavailable for intended navigation projection."));
        }

        if (read.State != InstallTargetReadState.File
            || read.Snapshot is not { } snapshot)
        {
            return new InstallProjectionInputRead(
                Observation: null,
                Bytes: ReadOnlyMemory<byte>.Empty,
                Build: Blocked(
                    "A current authored source changed or became unsafe during intended navigation projection."));
        }

        return new InstallProjectionInputRead(
            Observation: new InstallProjectionInputObservation
            {
                AutomaticId = source.Identity.AutomaticId,
                CanonicalBasePath = source.Identity.CanonicalBasePath,
                CanonicalLayerPath = layer.CanonicalPath,
                Form = layer.Form,
                Kind = layer.Kind,
                Expectation = snapshot.Expectation,
            },
            Bytes: snapshot.Bytes.ToArray(),
            Build: null);
    }

    private IEnumerable<GeneratedNavigationRegionInput> ReadRegionInputs(
        IEnumerable<SourceLogicalSource> sources,
        IReadOnlyDictionary<string, string> documents,
        Dictionary<string, MarkdownDocumentFacts> parsedDocuments)
    {
        foreach (var source in sources)
        {
            var canonicalPath = source.Identity.CanonicalBasePath;
            if (!parsedDocuments.TryGetValue(canonicalPath, out var document))
            {
                document = _markdownParser.Parse(documents[canonicalPath]);
                parsedDocuments.Add(canonicalPath, document);
            }

            yield return new GeneratedNavigationRegionInput(source, document);
        }
    }

    private static string? ReadMissingExcludedDirectoryAncestor(
        string physicalWorkspaceRoot,
        IEnumerable<string> excludedPaths,
        IEnumerable<string> selectedTargetPaths)
    {
        var targets = selectedTargetPaths.ToArray();
        foreach (var excludedPath in excludedPaths)
        {
            if (!targets.Any(path => path.StartsWith($"{excludedPath}/", StringComparison.Ordinal)))
            {
                continue;
            }

            var physicalPath = Path.Combine(
                physicalWorkspaceRoot,
                excludedPath.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(physicalPath))
            {
                return excludedPath;
            }
        }

        return null;
    }

    private static InstallIntendedStateBuild? ReadCatalogueBoundary(
        SourceCatalogue catalogue)
    {
        var issue = catalogue.Issues.FirstOrDefault(value =>
            value.Code != SourceCatalogueIssueCode.RootMissing);
        if (issue is null)
        {
            return null;
        }

        return issue.Code is SourceCatalogueIssueCode.RootUnavailable
            or SourceCatalogueIssueCode.DirectoryUnavailable
            or SourceCatalogueIssueCode.CandidateUnavailable
            or SourceCatalogueIssueCode.IdentityUnavailable
            ? Incomplete(
                "Current authored source catalogue facts are unavailable for intended navigation projection.")
            : Blocked(
                "Current authored source catalogue facts are unsafe or ambiguous for intended navigation projection.");
    }

    private static InstallIntendedStateBuild Blocked(
        string cause,
        InstallFindingCode findingCode = InstallFindingCode.GeneratedRegionUnsafe)
        => new()
        {
            State = InstallIntendedStateBuildState.Blocked,
            IntendedState = null,
            Cause = cause,
            FindingCode = findingCode,
        };

    private static InstallIntendedStateBuild Incomplete(
        string cause,
        InstallFindingCode findingCode = InstallFindingCode.ProjectionUnavailable)
        => new()
        {
            State = InstallIntendedStateBuildState.Incomplete,
            IntendedState = null,
            Cause = cause,
            FindingCode = findingCode,
        };

    private static InstallIntendedStateBuild Cancelled()
        => new()
        {
            State = InstallIntendedStateBuildState.Cancelled,
            IntendedState = null,
            Cause = null,
            FindingCode = InstallFindingCode.Interrupted,
        };
}
