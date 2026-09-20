using System.Text;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
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
    private readonly UpdateComparisonReader _targetReader = new(physicalPathResolver);

    internal async ValueTask<UpdateGeneratedNavigationBuild> BuildAsync(
        UpdateRequest request,
        IReadOnlyList<FrameworkPayloadAsset> selectedAssets,
        IReadOnlySet<string> retiredTargetPaths,
        IReadOnlySet<string> generatedHosts,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(retiredTargetPaths);
        var payloadAssets = selectedAssets
            .Where(asset => asset.Path.StartsWith(".agents/", StringComparison.Ordinal))
            .ToDictionary(asset => asset.Path, StringComparer.Ordinal);
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
            var payloadSources = payloadAssets.Values
                .Select(asset => FrameworkPayloadSourceProjection.Create(request.Workspace, asset))
                .ToDictionary(
                    source => source.Identity.CanonicalBasePath,
                    StringComparer.Ordinal);
            var intendedSources = catalogue.Sources
                .Where(source => !payloadSources.ContainsKey(
                        source.Identity.CanonicalBasePath)
                    && !retiredTargetPaths.Contains(
                        source.Identity.CanonicalBasePath))
                .Concat(payloadSources.Values)
                .ToArray();
            var formation = _formationBuilder.Build(catalogue, intendedSources);
            if (formation.Ambiguities.Count > 0
                || formation.IntendedTargetCollisions.Count > 0)
            {
                return Blocked(
                    "The intended Update topology contains an ambiguous route, alias, or target collision.");
            }

            var documents = payloadAssets.Values.ToDictionary(
                asset => asset.Path,
                asset => StrictUtf8.GetString(asset.Bytes.AsSpan()),
                StringComparer.Ordinal);
            var projectionInputs = new List<FileStateSnapshot>();
            foreach (var source in intendedSources.Where(source =>
                         !payloadSources.ContainsKey(source.Identity.CanonicalBasePath)))
            {
                var baseRead = await ReadProjectionInputAsync(
                        request,
                        source.Base.CanonicalPath,
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
                documents.Add(
                    source.Identity.CanonicalBasePath,
                    StrictUtf8.GetString(baseSnapshot.Bytes.AsSpan()));
                if (source.Overwrite is not { } overwrite)
                {
                    continue;
                }

                var overwriteRead = await ReadProjectionInputAsync(
                        request,
                        overwrite.CanonicalPath,
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
                .Concat(intendedSources.Where(source => generatedHosts.Contains(source.Identity.CanonicalBasePath)))
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

            var targetBytes = payloadAssets.Values.ToDictionary(
                asset => asset.Path,
                asset => asset.Bytes.ToArray(),
                StringComparer.Ordinal);
            foreach (var region in projection.Regions)
            {
                var change = region.Change
                    ?? throw new InvalidOperationException(
                        "An available Update generated region requires a bounded change.");
                targetBytes[region.CanonicalPath] = change.ExpectedDocumentBytes.ToArray();
            }

            return new UpdateGeneratedNavigationBuild(
                targetBytes,
                projectionInputs
                    .OrderBy(snapshot => snapshot.LogicalPath, StringComparer.Ordinal)
                    .ToArray(),
                Finding: null);
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
        string path,
        CancellationToken cancellationToken)
    {
        var read = await _targetReader
            .ReadAsync(request.Workspace, path, cancellationToken)
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
                Incomplete(read.RelativePath, read.Cause
                    ?? "A current authored source is unavailable for intended navigation projection.")),
            _ => new UpdateProjectionInputRead(
                Snapshot: null,
                Blocked(read.Cause
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
        => new(
            TargetBytes: new Dictionary<string, byte[]>(StringComparer.Ordinal),
            ProjectionInputs: [],
            new UpdateFinding(
                UpdateFindingCode.GeneratedRegionUnsafe,
                target: null,
                cause));

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
