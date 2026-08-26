using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Source;

internal sealed class RouteSourceProjector
{
    internal async ValueTask<RouteSourceProjection> ReadAsync(
        SourceLogicalSource logicalSource,
        RouteSourceLayerProjection layerProjection,
        SourceDocumentReader reader,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(layerProjection))
        {
            throw new ArgumentOutOfRangeException(nameof(layerProjection), layerProjection, "The Route source layer projection is not defined.");
        }

        var baseRead = await reader
            .ReadAsync(logicalSource.Base, cancellationToken)
            .ConfigureAwait(false);
        SourceDocumentReadResult? overwriteRead = null;
        if (layerProjection == RouteSourceLayerProjection.ExactLayers
            && logicalSource.Overwrite is not null)
        {
            overwriteRead = await reader
                .ReadAsync(logicalSource.Overwrite, cancellationToken)
                .ConfigureAwait(false);
        }

        var source = BuildRouteSource(logicalSource, layerProjection, baseRead, overwriteRead);
        return new RouteSourceProjection(logicalSource, source, baseRead, overwriteRead);
    }

    private static RouteSource? BuildRouteSource(
        SourceLogicalSource logicalSource,
        RouteSourceLayerProjection layerProjection,
        SourceDocumentReadResult baseRead,
        SourceDocumentReadResult? overwriteRead)
    {
        if (!CanFormRouteDocument(baseRead)
            || overwriteRead is not null && !CanFormRouteDocument(overwriteRead))
        {
            return null;
        }

        var baseDocument = ToRouteDocument(baseRead);
        var overwriteDocument = overwriteRead is null ? null : ToRouteDocument(overwriteRead);
        var hasOverwrite = layerProjection == RouteSourceLayerProjection.ExactLayers
            && overwriteDocument is not null;
        var metadata = ReadMetadata(
            baseRead,
            baseDocument.Form,
            hasOverwrite);
        return new RouteSource(
            baseDocument,
            metadata,
            ReadSourceKind(baseDocument.Form),
            hasOverwrite ? overwriteDocument : null);
    }

    private static bool CanFormRouteDocument(SourceDocumentReadResult read)
    {
        return (read.Verification.State is SourceLayerVerificationState.Verified
                or SourceLayerVerificationState.Missing)
            && read.Read is not null;
    }

    private static RouteSourceDocument ToRouteDocument(SourceDocumentReadResult read)
    {
        var file = read.Read
            ?? throw new InvalidOperationException("A Route document requires an associated file read.");
        return new RouteSourceDocument(
            read.Layer.CanonicalPath,
            read.Layer.PhysicalPath,
            read.Layer.Form,
            file.State,
            file.Value);
    }

    private static RouteSourceMetadata ReadMetadata(
        SourceDocumentReadResult baseRead,
        SourceDocumentForm form,
        bool hasOverwrite)
    {
        var compatibility = form is SourceDocumentForm.IndexEntrypoint
            or SourceDocumentForm.UnderscoreIndexEntrypoint
            or SourceDocumentForm.ReferencesEntrypoint
            or SourceDocumentForm.UnderscoreReferencesEntrypoint;
        var file = baseRead.Read
            ?? throw new InvalidOperationException("A Route document requires an associated file read.");
        if (file.State != FileReadState.Complete)
        {
            return RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.ReadUnavailable,
                compatibility,
                hasOverwrite);
        }

        var body = file.Value
            ?? throw new InvalidOperationException("A complete Route document read requires a value.");
        var document = new MarkdownDocumentParser().Parse(body);
        var facts = new SourceAuthoredMetadataParser().Parse(document, form);
        return new RouteMetadataParser().Parse(facts, compatibility, hasOverwrite);
    }

    private static RouteSourceKind ReadSourceKind(SourceDocumentForm form)
    {
        return form switch
        {
            SourceDocumentForm.Loader => RouteSourceKind.Loader,
            SourceDocumentForm.Skill => RouteSourceKind.Native,
            SourceDocumentForm.Markdown => RouteSourceKind.Markdown,
            SourceDocumentForm.CanonicalEntrypoint
                or SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint => RouteSourceKind.Entrypoint,
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The Route source form is not a logical base form."),
        };
    }

}
