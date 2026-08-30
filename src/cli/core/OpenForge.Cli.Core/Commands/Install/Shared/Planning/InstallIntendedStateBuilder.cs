using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
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

    internal async ValueTask<InstallIntendedStateBuild> BuildAsync(
        InstallRequest request,
        FrameworkPayload payload,
        CancellationToken cancellationToken)
    {
        var payloadAssets = payload.Assets
            .Where(asset => asset.Path.StartsWith(".agents/", StringComparison.Ordinal))
            .ToDictionary(asset => asset.Path, StringComparer.Ordinal);
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
            var payloadSources = payloadAssets.Values
                .Select(asset => CreatePayloadSource(request, asset))
                .ToDictionary(
                    source => source.Identity.CanonicalBasePath,
                    StringComparer.Ordinal);
            var intendedSources = catalogue.Sources
                .Where(source => !payloadSources.ContainsKey(
                    source.Identity.CanonicalBasePath))
                .Concat(payloadSources.Values)
                .ToArray();
            var formation = _formationBuilder.Build(catalogue, intendedSources);
            if (formation.Ambiguities.Count > 0
                || formation.IntendedTargetCollisions.Count > 0)
            {
                return Blocked(
                    "The intended Framework topology contains an ambiguous route, alias, or target collision.");
            }

            var documents = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var asset in payloadAssets.Values)
            {
                documents.Add(asset.Path, StrictUtf8.GetString(asset.Bytes.AsSpan()));
            }

            var projectionInputs = new List<InstallProjectionInputObservation>();
            foreach (var source in intendedSources.Where(source =>
                         !payloadSources.ContainsKey(source.Identity.CanonicalBasePath)))
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

            var metadata = intendedSources
                .Where(source => source.Base.Form != SourceDocumentForm.Loader)
                .Select(source => new GeneratedNavigationMetadata(
                    source,
                    _metadataParser.Parse(
                        _markdownParser.Parse(documents[source.Identity.CanonicalBasePath]),
                        source.Base.Form)))
                .ToArray();
            var regionSources = payloadSources.Values
                .Where(source => source.Base.Form == SourceDocumentForm.Loader
                    || SourceFormClassifier.IsEntrypoint(source.Base.Form))
                .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .ToArray();
            var projection = _projector.Project(new GeneratedNavigationProjectionRequest(
                formation,
                regionSources.Select(source => new GeneratedNavigationRegionInput(
                    source,
                    _markdownParser.Parse(documents[source.Identity.CanonicalBasePath]))),
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

            var targetBytes = payloadAssets.Values.ToDictionary(
                asset => asset.Path,
                asset => asset.Bytes.ToArray(),
                StringComparer.Ordinal);
            foreach (var region in projection.Regions)
            {
                var change = region.Change
                    ?? throw new InvalidOperationException(
                        "An available intended generated region requires a bounded change.");
                targetBytes[region.CanonicalPath] = change.ExpectedDocumentBytes.ToArray();
            }

            var rootAgent = payload.Find(FrameworkPayloadAsset.RootAgentPath)
                ?? throw new InvalidDataException("The embedded Framework payload is missing AGENTS.md.");
            var rootClaude = payload.Find(FrameworkPayloadAsset.RootClaudePath)
                ?? throw new InvalidDataException("The embedded Framework payload is missing CLAUDE.md.");
            return new InstallIntendedStateBuild
            {
                State = InstallIntendedStateBuildState.Complete,
                IntendedState = new InstallIntendedState
                {
                    TargetBytes = targetBytes,
                    ManagedBlockBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal)
                    {
                        [FrameworkPayloadAsset.RootAgentPath] = rootAgent.Bytes.ToArray(),
                        [FrameworkPayloadAsset.RootClaudePath] = rootClaude.Bytes.ToArray(),
                    },
                    GeneratedRegionPaths = projection.Regions
                        .Select(region => region.CanonicalPath)
                        .ToHashSet(StringComparer.Ordinal),
                    ProjectionInputs = projectionInputs
                        .OrderBy(input => input.CanonicalLayerPath, StringComparer.Ordinal)
                        .ToArray(),
                },
                Cause = null,
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

    private static SourceLogicalSource CreatePayloadSource(
        InstallRequest request,
        FrameworkPayloadAsset asset)
    {
        if (!SourceFormClassifier.TryClassify(asset.Path, out var form))
        {
            throw new InvalidDataException(
                $"The embedded Framework asset '{asset.Path}' is not a recognized authored source.");
        }

        var id = SourceIdentity.DeriveId(asset.Path)
            ?? throw new InvalidDataException(
                $"The embedded Framework asset '{asset.Path}' has no canonical source identity.");
        var physicalPath = Path.GetFullPath(Path.Combine(
            request.Workspace.PhysicalRoot,
            asset.Path.Replace('/', Path.DirectorySeparatorChar)));
        return new SourceLogicalSource(
            new SourceLogicalIdentity(
                automaticId: id,
                canonicalBasePath: asset.Path),
            new SourceLayer(
                canonicalPath: asset.Path,
                physicalPath: physicalPath,
                form: form,
                kind: SourceLayerKind.Base));
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

    private static InstallIntendedStateBuild Blocked(string cause)
        => new()
        {
            State = InstallIntendedStateBuildState.Blocked,
            IntendedState = null,
            Cause = cause,
        };

    private static InstallIntendedStateBuild Incomplete(string cause)
        => new()
        {
            State = InstallIntendedStateBuildState.Incomplete,
            IntendedState = null,
            Cause = cause,
        };

    private static InstallIntendedStateBuild Cancelled()
        => new()
        {
            State = InstallIntendedStateBuildState.Cancelled,
            IntendedState = null,
            Cause = null,
        };
}

internal sealed record InstallProjectionInputRead(
    InstallProjectionInputObservation? Observation,
    ReadOnlyMemory<byte> Bytes,
    InstallIntendedStateBuild? Build);
