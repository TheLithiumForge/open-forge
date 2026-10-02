using System.Text;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Planning;

internal sealed class UpdateGeneratedNavigationPlanner(PhysicalPathResolver physicalPathResolver)
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly SourceCatalogueReader _catalogueReader = new();
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();
    private readonly GeneratedNavigationProjector _projector = new();
    private readonly UpdateWorkspaceAdoptionBuilder _adoptionBuilder = new(physicalPathResolver);
    private readonly UpdateComparisonReader _targetReader = new(physicalPathResolver);

    internal ValueTask<UpdateGeneratedNavigationBuild> BuildAsync(
        UpdateRequest request,
        FrameworkPayload payload,
        WorkspaceOwnershipDocument ownershipDocument,
        WorkspaceSettingsDocument settings,
        IReadOnlyList<FrameworkPayloadAsset> selectedAssets,
        IReadOnlySet<string> retiredTargetPaths,
        IReadOnlySet<string> generatedHosts,
        CancellationToken cancellationToken)
        => BuildCoreAsync(
            request,
            payload,
            ownershipDocument,
            ownershipRead: null,
            settings,
            selectedAssets,
            retiredTargetPaths,
            generatedHosts,
            cancellationToken);

    internal ValueTask<UpdateGeneratedNavigationBuild> BuildAsync(
        UpdateRequest request,
        FrameworkPayload payload,
        WorkspaceOwnershipRead ownership,
        WorkspaceSettingsDocument settings,
        IReadOnlyList<FrameworkPayloadAsset> selectedAssets,
        IReadOnlySet<string> retiredTargetPaths,
        IReadOnlySet<string> generatedHosts,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ownership);
        return BuildCoreAsync(
            request,
            payload,
            ownership.Document,
            ownership,
            settings,
            selectedAssets,
            retiredTargetPaths,
            generatedHosts,
            cancellationToken);
    }

    private async ValueTask<UpdateGeneratedNavigationBuild> BuildCoreAsync(
        UpdateRequest request,
        FrameworkPayload payload,
        WorkspaceOwnershipDocument ownershipDocument,
        WorkspaceOwnershipRead? ownershipRead,
        WorkspaceSettingsDocument settings,
        IReadOnlyList<FrameworkPayloadAsset> selectedAssets,
        IReadOnlySet<string> retiredTargetPaths,
        IReadOnlySet<string> generatedHosts,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ownershipDocument);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(selectedAssets);
        ArgumentNullException.ThrowIfNull(retiredTargetPaths);
        SourceCatalogue catalogue;
        try
        {
            catalogue = await _catalogueReader
                .ReadAsync(
                    new SourceCatalogueRequest(
                        request.Workspace,
                        [SourceLogicalPath.AgentsRoot]),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Interrupted();
        }

        if (catalogue.IsCancelled)
        {
            return Interrupted();
        }

        if (ReadCatalogueBoundary(catalogue) is { } catalogueBoundary)
        {
            return catalogueBoundary;
        }

        try
        {
            var sourcePlan = _adoptionBuilder.PlanSources(
                request.Workspace,
                catalogue,
                selectedAssets,
                settings,
                retiredTargetPaths,
                ownershipRead);
            if (sourcePlan.Cause is { } sourceCause)
            {
                return Blocked(UpdateFindingCode.TargetUnsafe, target: null, sourceCause);
            }

            var payloadAssets = sourcePlan.PayloadAssets.ToDictionary(
                asset => asset.Path,
                StringComparer.Ordinal);

            var payloadSources = sourcePlan.PayloadSources
                .ToDictionary(
                    source => source.Identity.CanonicalBasePath,
                    StringComparer.Ordinal);
            var intendedSources = sourcePlan.IntendedSources
                .Where(source => !retiredTargetPaths.Contains(source.Identity.CanonicalBasePath))
                .ToArray();
            var formation = _formationBuilder.Build(catalogue, intendedSources);
            if (formation.Ambiguities.Count > 0
                || formation.IntendedTargetCollisions.Count > 0)
            {
                return Blocked(
                    "The intended Update topology contains an ambiguous route, alias, or target collision.");
            }

            if (FrameworkPayloadSelection.FindMissingRequiredAncestor(payload, settings, formation)
                is { } missingRouteAncestor)
            {
                return Blocked(
                    UpdateFindingCode.TargetUnsafe,
                    missingRouteAncestor,
                    $"The excluded Framework entrypoint '{missingRouteAncestor}' is missing and required to reach another payload route.");
            }

            var documents = payloadAssets.Values.ToDictionary(
                asset => asset.Path,
                asset => StrictUtf8.GetString(asset.Bytes.AsSpan()),
                StringComparer.Ordinal);
            var projectionInputReader = new UpdateProjectionInputReader(
                physicalPathResolver,
                ownershipDocument);
            var projectionInputs = new List<FileStateSnapshot>();
            var observedBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            var observedSnapshots = new Dictionary<string, FileStateSnapshot>(StringComparer.Ordinal);
            foreach (var source in intendedSources.Where(source =>
                         !payloadSources.ContainsKey(source.Identity.CanonicalBasePath)
                         && !sourcePlan.CreatedEntrypointPaths.Contains(source.Identity.CanonicalBasePath)))
            {
                var baseRead = await ReadProjectionInputAsync(
                        request,
                        source.Base,
                        projectionInputReader,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (baseRead.Build is { } baseBoundary)
                {
                    return baseBoundary;
                }

                var baseSnapshot = baseRead.Snapshot
                    ?? throw new InvalidOperationException(
                        "A complete Update projection input requires its exact snapshot.");
                projectionInputs.Add(baseSnapshot);
                observedSnapshots.Add(source.Identity.CanonicalBasePath, baseSnapshot);
                observedBytes.Add(source.Identity.CanonicalBasePath, baseSnapshot.Bytes.ToArray());
                documents.Add(
                    source.Identity.CanonicalBasePath,
                    StrictUtf8.GetString(baseSnapshot.Bytes.AsSpan()));
                if (source.Overwrite is not { } overwrite)
                {
                    continue;
                }

                var overwriteRead = await ReadProjectionInputAsync(
                        request,
                        overwrite,
                        projectionInputReader,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (overwriteRead.Build is { } overwriteBoundary)
                {
                    return overwriteBoundary;
                }

                projectionInputs.Add(overwriteRead.Snapshot
                    ?? throw new InvalidOperationException(
                        "A complete Update overwrite input requires its exact snapshot."));
            }

            var createdSnapshots = new Dictionary<string, FileStateSnapshot>(StringComparer.Ordinal);
            foreach (var path in sourcePlan.CreatedEntrypointPaths.Order(StringComparer.Ordinal))
            {
                var read = await _targetReader.ReadAsync(
                        request.Workspace,
                        path,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (read.State == UpdateTargetReadState.Cancelled)
                {
                    return Interrupted();
                }

                if (read.State == UpdateTargetReadState.Unavailable)
                {
                    return Incomplete(path, read.Cause
                        ?? "The planned local entrypoint target is unavailable for exact observation.");
                }

                if (read.State != UpdateTargetReadState.Missing
                    || read.Snapshot is not { Kind: FileExpectationKind.Missing } missing)
                {
                    return Blocked(
                        UpdateFindingCode.TargetUnsafe,
                        path,
                        read.Cause
                            ?? "The planned local entrypoint is no longer physically missing and safe.");
                }

                createdSnapshots.Add(path, missing);
            }

            var adoptionDocuments = _adoptionBuilder.PlanDocuments(
                sourcePlan,
                documents,
                observedBytes,
                observedSnapshots,
                createdSnapshots);
            if (adoptionDocuments.Cause is { } adoptionCause)
            {
                return Blocked(UpdateFindingCode.GeneratedRegionUnsafe, target: null, adoptionCause);
            }

            documents = adoptionDocuments.Documents.ToDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.Ordinal);

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

            var regionSources = payloadSources.Values
                .Concat(intendedSources.Where(source => generatedHosts.Contains(source.Identity.CanonicalBasePath)
                    || sourcePlan.AdoptionSourcePaths.Contains(source.Identity.CanonicalBasePath)))
                .DistinctBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .Where(source => source.Base.Form == SourceDocumentForm.Loader
                    || SourceFormClassifier.IsEntrypoint(source.Base.Form))
                .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .ToArray();
            var projection = _projector.Project(new GeneratedNavigationProjectionRequest(
                formation,
                ReadRegionInputs(regionSources, documents, parsedDocuments),
                metadata));
            var unavailable = projection.Regions.FirstOrDefault(region =>
                region.State != GeneratedNavigationRegionState.Available);
            if (unavailable is not null)
            {
                return Blocked(
                    unavailable.Cause
                        ?? "The intended Update generated-navigation projection is unavailable.");
            }
            var projectedEntryPaths = projection.Regions
                .Select(region => region.CanonicalPath)
                .ToHashSet(StringComparer.Ordinal);

            var targetBytes = payloadAssets.Values.ToDictionary(
                asset => asset.Path,
                asset => asset.Bytes.ToArray(),
                StringComparer.Ordinal);
            foreach (var target in adoptionDocuments.TargetBytes)
            {
                targetBytes[target.Key] = target.Value.ToArray();
            }

            var migrationPlans = new UpdateMigrationPlanAccumulator();
            foreach (var migration in adoptionDocuments.Migrations)
            {
                migrationPlans.Add(
                    migration.Path,
                    migration.Actions,
                    migration.Fields,
                    migration.Derivation);
            }

            var changedPaths = new HashSet<string>(StringComparer.Ordinal);
            var hasScopedAdoption = sourcePlan.Topology.EligibleSourcePaths.Count > 0
                || sourcePlan.Topology.ReusedEntrypoints.Count > 0
                || sourcePlan.Topology.CreatedEntrypointPaths.Count > 0;
            foreach (var region in projection.Regions)
            {
                var change = region.Change
                    ?? throw new InvalidOperationException(
                        "An available Update generated region requires a bounded change.");
                var finalBytes = change.ExpectedDocumentBytes.ToArray();
                var priorBytes = StrictUtf8.GetBytes(documents[region.CanonicalPath]);
                if (!priorBytes.AsSpan().SequenceEqual(finalBytes))
                {
                    changedPaths.Add(region.CanonicalPath);
                    if (hasScopedAdoption
                        && (generatedHosts.Contains(region.CanonicalPath)
                            || sourcePlan.AdoptionSourcePaths.Contains(region.CanonicalPath)))
                    {
                        migrationPlans.AddNavigationUpdated(region.CanonicalPath);
                    }
                }

                targetBytes[region.CanonicalPath] = finalBytes;
            }

            var sourceByPath = sourcePlan.IntendedSources.ToDictionary(
                source => source.Identity.CanonicalBasePath,
                StringComparer.Ordinal);
            var wholeFileFrameworkPaths = ownershipDocument.Framework?.Paths
                .ToHashSet(StringComparer.Ordinal)
                ?? new HashSet<string>(StringComparer.Ordinal);
            var userOwnedSourceMigrationPaths = sourcePlan.AdoptionSourcePaths
                .Where(path => !sourcePlan.PayloadSourcePaths.Contains(path)
                    && !wholeFileFrameworkPaths.Contains(path))
                .ToHashSet(StringComparer.Ordinal);
            var adoptionTargets = new List<UpdateAdoptionTarget>();
            foreach (var path in sourcePlan.AdoptionSourcePaths.Order(StringComparer.Ordinal))
            {
                if (!targetBytes.TryGetValue(path, out var finalBytes)
                    || !adoptionDocuments.OriginalSnapshots.TryGetValue(path, out var snapshot))
                {
                    continue;
                }

                if (snapshot.Kind == FileExpectationKind.Directory)
                {
                    return Blocked(
                        UpdateFindingCode.TargetUnsafe,
                        path,
                        "A planned local adoption target is an ordinary directory, not a file.");
                }

                var changed = snapshot.Kind == FileExpectationKind.Missing
                    || !snapshot.Bytes.AsSpan().SequenceEqual(finalBytes);
                if (!changed)
                {
                    continue;
                }

                if (!sourceByPath.TryGetValue(path, out var source))
                {
                    return Blocked(
                        UpdateFindingCode.TargetUnsafe,
                        path,
                        "A changed local adoption target is absent from prospective Update topology.");
                }

                var ownsGeneratedEntries = SourceFormClassifier.IsEntrypoint(source.Base.Form)
                    && projectedEntryPaths.Contains(path)
                    && _markdownParser.Parse(StrictUtf8.GetString(finalBytes)).GeneratedRegion.State
                        == MarkdownGeneratedRegionState.Complete;
                adoptionTargets.Add(new UpdateAdoptionTarget(
                    path,
                    snapshot,
                    finalBytes.ToArray(),
                    ownsGeneratedEntries));
                changedPaths.Add(path);
            }

            return new UpdateGeneratedNavigationBuild(
                targetBytes,
                projectionInputs
                    .OrderBy(snapshot => snapshot.LogicalPath, StringComparer.Ordinal)
                    .ToArray(),
                Finding: null)
            {
                AdoptionTargets = adoptionTargets,
                Migrations = migrationPlans.Build(changedPaths)
                    .Select(migration => migration with
                    {
                        IsUserOwnedSource = userOwnedSourceMigrationPaths.Contains(migration.Path),
                    })
                    .ToArray(),
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Interrupted();
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
            return Blocked($"The intended Update topology is invalid: {exception.Message}");
        }
    }

    private async ValueTask<UpdateProjectionInputRead> ReadProjectionInputAsync(
        UpdateRequest request,
        SourceLayer layer,
        UpdateProjectionInputReader projectionInputReader,
        CancellationToken cancellationToken)
    {
        var read = await projectionInputReader
            .ReadAsync(request.Workspace, layer, cancellationToken)
            .ConfigureAwait(false);
        return read.State switch
        {
            UpdateTargetReadState.Available when read.Snapshot is { Kind: FileExpectationKind.File } snapshot =>
                new UpdateProjectionInputRead(snapshot, Build: null),
            UpdateTargetReadState.Cancelled => new UpdateProjectionInputRead(
                Snapshot: null,
                Interrupted()),
            UpdateTargetReadState.Unavailable => new UpdateProjectionInputRead(
                Snapshot: null,
                Incomplete(layer.CanonicalPath, read.Cause
                    ?? "A current authored source is unavailable for intended navigation projection.")),
            _ => new UpdateProjectionInputRead(
                Snapshot: null,
                Blocked(
                    UpdateFindingCode.TargetUnsafe,
                    layer.CanonicalPath,
                    read.Cause
                        ?? "A current authored source changed or became unsafe during intended navigation projection.")),
        };
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

    private static UpdateGeneratedNavigationBuild? ReadCatalogueBoundary(
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
                issue.AttemptedCanonicalPath,
                "Current authored source catalogue facts are unavailable for intended navigation projection.")
            : Blocked(
                "Current authored source catalogue facts are unsafe or ambiguous for intended navigation projection.");
    }

    private static UpdateGeneratedNavigationBuild Blocked(string cause)
        => Blocked(UpdateFindingCode.GeneratedRegionUnsafe, target: null, cause);

    private static UpdateGeneratedNavigationBuild Blocked(
        UpdateFindingCode code,
        string? target,
        string cause)
        => new(
            TargetBytes: new Dictionary<string, byte[]>(StringComparer.Ordinal),
            ProjectionInputs: [],
            new UpdateFinding(code, target, cause));

    private static UpdateGeneratedNavigationBuild Incomplete(string? target, string cause)
        => new(
            TargetBytes: new Dictionary<string, byte[]>(StringComparer.Ordinal),
            ProjectionInputs: [],
            new UpdateFinding(UpdateFindingCode.ProjectionUnavailable, target, cause));

    private static UpdateGeneratedNavigationBuild Interrupted()
        => new(
            TargetBytes: new Dictionary<string, byte[]>(StringComparer.Ordinal),
            ProjectionInputs: [],
            new UpdateFinding(
                UpdateFindingCode.Interrupted,
                target: null,
                "Update generated-navigation planning was interrupted."));
}
