using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitIntendedChainBuilder
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly RouteInitMetadataResolver _metadataResolver = new();
    private readonly RouteInitScaffoldComposer _scaffoldComposer = new();
    private readonly SourceDocumentSnapshotReader _snapshotReader = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly FrameworkDocumentMetadataParser _frameworkMetadataParser = new();

    internal async ValueTask<RouteInitIntendedChainResult> BuildAsync(
        RouteInitRequest request,
        RouteInitInspectionFacts inspection,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(inspection);

        try
        {
            return new RouteInitIntendedChainCompleted(
                await BuildCoreAsync(request, inspection, cancellationToken)
                    .ConfigureAwait(false));
        }
        catch (RouteInitPlanningException exception)
        {
            return new RouteInitIntendedChainStopped(new RouteInitPlanningBoundary(
                exception.Code,
                exception.Message,
                exception.Incomplete,
                inspection.Target));
        }
    }

    private async ValueTask<RouteInitIntendedChain> BuildCoreAsync(
        RouteInitRequest request,
        RouteInitInspectionFacts inspection,
        CancellationToken cancellationToken)
    {
        var current = inspection.Current;
        var framework = inspection.Framework;
        var entries = new List<RouteInitIntendedEntrypoint>(current.Chain.Count);
        for (var index = 0; index < current.Chain.Count; index++)
        {
            var chain = current.Chain[index];
            var aligned = framework?.Alignment.Segments[index];
            if (chain.Existing is { } existing)
            {
                var read = current.Reads.Single(item => ReferenceEquals(item.Source, existing));
                var before = await ReadSourceSnapshotAsync(request, read.Base, cancellationToken)
                    .ConfigureAwait(false);
                var sourceAssetPath = ReadTrustedSourceAssetPath(
                    existing,
                    aligned,
                    framework?.Trust);
                var content = new RouteInitProspectiveSourceContent(
                    existing,
                    before,
                    before.Bytes.AsSpan(),
                    sourceAssetPath,
                    IsManagedFrameworkSegment(aligned));
                entries.Add(new RouteInitIntendedEntrypoint(
                    chain,
                    content,
                    Metadata: null,
                    sourceAssetPath is not null
                        ? RouteInitEntrypointOwnership.Framework
                        : RouteInitEntrypointOwnership.User));
                continue;
            }

            if (aligned is not null
                && aligned.Role is RouteInitFrameworkSegmentRole.InstalledRoot or RouteInitFrameworkSegmentRole.Managed)
            {
                var sourceAssetPath = aligned.SourceAssetPath
                    ?? throw new RouteInitPlanningException(
                        RouteInitFindingCode.FrameworkPayloadInvalid,
                        "A managed Framework segment has no embedded source path.",
                        incomplete: false);
                var asset = framework?.Payload.Find(sourceAssetPath)
                    ?? throw new RouteInitPlanningException(
                        RouteInitFindingCode.FrameworkPayloadInvalid,
                        "A managed Framework segment has no embedded asset.",
                        incomplete: false);
                var document = _markdownParser.Parse(StrictUtf8.GetString(asset.Bytes.AsSpan()));
                var facts = _frameworkMetadataParser.Parse(document);
                if (facts.State != FrameworkDocumentMetadataState.Complete
                    || facts.Metadata is not { } authored)
                {
                    throw new RouteInitPlanningException(
                        RouteInitFindingCode.FrameworkPayloadInvalid,
                        "A managed Framework entrypoint has incomplete embedded metadata.",
                        incomplete: false);
                }

                var embeddedMetadata = new RouteInitMetadata(
                    authored.Description,
                    RouteInitDescriptionSource.Embedded,
                    authored.Responsibility,
                    RouteInitResponsibilitySource.Embedded,
                    authored.Tags,
                    RouteInitTagsSource.Embedded);
                var content = new RouteInitProspectiveSourceContent(
                    aligned.IntendedSource,
                    MissingSnapshot(request, aligned.IntendedSource.Identity.CanonicalBasePath),
                    asset.Bytes.AsSpan(),
                    sourceAssetPath,
                    ownsGeneratedEntries: true);
                entries.Add(new RouteInitIntendedEntrypoint(
                    chain,
                    content,
                    embeddedMetadata,
                    RouteInitEntrypointOwnership.Framework));
                continue;
            }

            var metadata = _metadataResolver.Resolve(
                chain.Id,
                index == current.Chain.Count - 1,
                request.Metadata);
            var title = chain.Id.Split('/')[^1];
            var scaffold = _scaffoldComposer.Compose(chain.Id, title, metadata);
            var source = aligned?.IntendedSource
                ?? CreateSource(request, chain.Id, chain.CanonicalMissingPath);
            var userContent = new RouteInitProspectiveSourceContent(
                source,
                MissingSnapshot(request, source.Identity.CanonicalBasePath),
                scaffold.Bytes.AsSpan(),
                sourceAssetPath: null,
                ownsGeneratedEntries: false);
            entries.Add(new RouteInitIntendedEntrypoint(
                chain,
                userContent,
                scaffold.Metadata,
                RouteInitEntrypointOwnership.User));
        }

        return new RouteInitIntendedChain(entries);
    }

    private async ValueTask<FileStateSnapshot> ReadSourceSnapshotAsync(
        RouteInitRequest request,
        SourceDocumentReadResult read,
        CancellationToken cancellationToken)
    {
        if (read.Verification.State != SourceLayerVerificationState.Verified
            || read.Verification.CurrentPhysicalPath is null)
        {
            throw new RouteInitPlanningException(
                RouteInitFindingCode.InspectionIncomplete,
                "The exact current source snapshot is unavailable.",
                incomplete: true);
        }

        try
        {
            return await _snapshotReader.ReadAsync(request.Workspace, read, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new RouteInitPlanningException(
                RouteInitFindingCode.Interrupted,
                "Route Init source snapshotting was cancelled.",
                incomplete: true);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
            or IOException
            or DecoderFallbackException)
        {
            throw new RouteInitPlanningException(
                RouteInitFindingCode.InspectionIncomplete,
                exception.Message,
                incomplete: true);
        }
    }

    private static bool IsManagedFrameworkSegment(RouteInitFrameworkAlignedSegment? aligned)
        => aligned?.Role is RouteInitFrameworkSegmentRole.InstalledRoot
            or RouteInitFrameworkSegmentRole.Managed;

    private static string? ReadTrustedSourceAssetPath(
        SourceLogicalSource existing,
        RouteInitFrameworkAlignedSegment? aligned,
        RouteInitFrameworkTrust? trust)
    {
        if (!IsManagedFrameworkSegment(aligned)
            || aligned?.SourceAssetPath is not { } sourceAssetPath
            || trust?.Ownership.Document.Framework is not { } ownership)
        {
            return null;
        }

        return ownership.Paths.Contains(existing.Identity.CanonicalBasePath, StringComparer.Ordinal)
            ? sourceAssetPath
            : null;
    }

    private static FileStateSnapshot MissingSnapshot(
        RouteInitRequest request,
        string canonicalPath)
        => FileStateSnapshot.Missing(
            SourceLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, canonicalPath));

    private static SourceLogicalSource CreateSource(
        RouteInitRequest request,
        string id,
        string canonicalPath)
    {
        if (!SourceFormClassifier.TryClassify(canonicalPath, out var form))
        {
            throw new RouteInitPlanningException(
                RouteInitFindingCode.InvalidTarget,
                "The intended route source form is invalid.",
                incomplete: false);
        }

        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, canonicalPath),
            new SourceLayer(
                canonicalPath,
                Path.GetFullPath(SourceLogicalPath.ToLexicalPath(
                    request.Workspace.PhysicalRoot,
                    canonicalPath)),
                form,
                SourceLayerKind.Base));
    }
}
