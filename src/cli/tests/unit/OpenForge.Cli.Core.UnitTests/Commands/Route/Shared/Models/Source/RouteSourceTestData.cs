using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models.Source;

internal static class RouteSourceTestData
{
    internal static SourceLogicalSource LogicalSource(
        string path,
        string? id = null,
        SourceDocumentForm form = SourceDocumentForm.Markdown,
        bool withOverwrite = false)
    {
        var automaticId = id ?? DeriveId(path);
        var baseLayer = new SourceLayer(
            path,
            PhysicalPath(path),
            form,
            SourceLayerKind.Base);
        var overwritePath = path[..^".md".Length] + ".overwrite.md";
        var overwrite = withOverwrite
            ? new SourceLayer(
                overwritePath,
                PhysicalPath(overwritePath),
                SourceDocumentForm.OverwriteCompanion,
                SourceLayerKind.Overwrite)
            : null;
        return new SourceLogicalSource(
            new SourceLogicalIdentity(automaticId, path),
            baseLayer,
            overwrite);
    }

    internal static SourceDocumentReadResult Read(
        SourceLogicalSource logicalSource,
        SourceLayer layer,
        string body = "body")
    {
        var verification = new SourceLayerVerification(
            layer,
            SourceLayerVerificationState.Verified,
            layer.PhysicalPath,
            null);
        return new SourceDocumentReadResult(
            layer,
            verification,
            FileReadResult<string>.Complete(layer.CanonicalPath, body));
    }

    internal static RouteSourceProjection Projection(
        SourceLogicalSource logicalSource,
        bool includeRouteSource = true,
        RouteSourceLayerProjection mode = RouteSourceLayerProjection.ExactLayers)
    {
        var baseRead = Read(logicalSource, logicalSource.Base);
        SourceDocumentReadResult? overwriteRead = null;
        if (mode == RouteSourceLayerProjection.ExactLayers && logicalSource.Overwrite is not null)
        {
            overwriteRead = Read(logicalSource, logicalSource.Overwrite);
        }

        var routeSource = includeRouteSource
            ? Source(logicalSource, overwriteRead is not null)
            : null;
        return new RouteSourceProjection(logicalSource, routeSource, baseRead, overwriteRead);
    }

    internal static RouteSource Source(
        SourceLogicalSource logicalSource,
        bool withOverwrite = false,
        bool isRouteAmbiguous = false)
    {
        var form = logicalSource.Base.Form;
        var compatibility = form is SourceDocumentForm.IndexEntrypoint
            or SourceDocumentForm.UnderscoreIndexEntrypoint
            or SourceDocumentForm.ReferencesEntrypoint
            or SourceDocumentForm.UnderscoreReferencesEntrypoint;
        var metadata = form == SourceDocumentForm.Loader
            ? RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.NotApplicable,
                false,
                withOverwrite)
            : RouteSourceMetadata.Complete(
                $"Description for {logicalSource.Identity.CanonicalBasePath}",
                form == SourceDocumentForm.Skill ? [] : ["Route"],
                compatibility,
                withOverwrite);
        var @base = new RouteSourceDocument(
            logicalSource.Base.CanonicalPath,
            logicalSource.Base.PhysicalPath,
            form,
            FileReadState.Complete,
            "body");
        RouteSourceDocument? overwrite;
        if (withOverwrite)
        {
            var overwriteLayer = logicalSource.Overwrite
                ?? throw new InvalidOperationException("The fixed logical source must retain its overwrite layer.");
            overwrite = new RouteSourceDocument(
                overwriteLayer.CanonicalPath,
                overwriteLayer.PhysicalPath,
                SourceDocumentForm.OverwriteCompanion,
                FileReadState.Complete,
                "overwrite");
        }
        else
        {
            overwrite = null;
        }
        return new RouteSource(@base, metadata, ToRouteKind(form), overwrite, isRouteAmbiguous);
    }

    internal static RouteSource Source(
        string path,
        RouteSourceKind kind,
        SourceDocumentForm? form = null,
        RouteSourceMetadataState metadataState = RouteSourceMetadataState.Complete,
        string? overwritePath = null,
        FileReadState documentReadState = FileReadState.Complete,
        bool isRouteAmbiguous = false)
    {
        var sourceForm = form ?? ReadForm(path, kind);
        var compatibility = IsCompatibility(sourceForm);
        var metadata = sourceForm == SourceDocumentForm.Loader
            ? RouteSourceMetadata.WithoutValues(
                metadataState == RouteSourceMetadataState.Complete
                    ? RouteSourceMetadataState.NotApplicable
                    : metadataState,
                isCompatibilityEntrypoint: false,
                isOverwritePresent: overwritePath is not null)
            : metadataState == RouteSourceMetadataState.Complete
                ? RouteSourceMetadata.Complete(
                    $"Description for {path}",
                    sourceForm == SourceDocumentForm.Skill ? [] : ["Route"],
                    compatibility,
                    overwritePath is not null)
                : RouteSourceMetadata.WithoutValues(
                    metadataState,
                    compatibility,
                    overwritePath is not null);
        var @base = Document(path, sourceForm, documentReadState);
        var overwrite = overwritePath is null
            ? null
            : Document(overwritePath, SourceDocumentForm.OverwriteCompanion);
        return new RouteSource(@base, metadata, kind, overwrite, isRouteAmbiguous);
    }

    internal static RouteSourceDocument Document(
        string path,
        SourceDocumentForm form,
        FileReadState readState = FileReadState.Complete,
        string? body = "body")
    {
        return new RouteSourceDocument(
            path,
            PhysicalPath(path),
            form,
            readState,
            readState == FileReadState.Complete ? body ?? "body" : null);
    }

    internal static string PhysicalPath(string canonicalPath)
    {
        return Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "route-projection-model-unit",
            canonicalPath.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static SourceDocumentForm ReadForm(string path, RouteSourceKind kind)
    {
        if (kind == RouteSourceKind.Loader)
        {
            return SourceDocumentForm.Loader;
        }

        var fileName = path[(path.LastIndexOf('/') + 1)..];
        if (kind == RouteSourceKind.Native)
        {
            return SourceDocumentForm.Skill;
        }

        if (kind == RouteSourceKind.Markdown)
        {
            return SourceDocumentForm.Markdown;
        }

        return fileName switch
        {
            "index.md" => SourceDocumentForm.IndexEntrypoint,
            "_index.md" => SourceDocumentForm.UnderscoreIndexEntrypoint,
            "references.md" => SourceDocumentForm.ReferencesEntrypoint,
            "_references.md" => SourceDocumentForm.UnderscoreReferencesEntrypoint,
            _ => SourceDocumentForm.CanonicalEntrypoint,
        };
    }

    private static bool IsCompatibility(SourceDocumentForm form)
    {
        return form is SourceDocumentForm.IndexEntrypoint
            or SourceDocumentForm.UnderscoreIndexEntrypoint
            or SourceDocumentForm.ReferencesEntrypoint
            or SourceDocumentForm.UnderscoreReferencesEntrypoint;
    }

    private static string DeriveId(string path)
    {
        var value = path[".agents/".Length..];
        return value switch
        {
            var item when item.EndsWith("/_" + Path.GetFileName(item)[1..], StringComparison.Ordinal)
                => item[..item.LastIndexOf("/_", StringComparison.Ordinal)],
            var item when item.EndsWith("/index.md", StringComparison.Ordinal)
                => item[..^"/index.md".Length],
            var item when item.EndsWith("/_index.md", StringComparison.Ordinal)
                => item[..^"/_index.md".Length],
            var item when item.EndsWith("/references.md", StringComparison.Ordinal)
                => item[..^"/references.md".Length],
            var item when item.EndsWith("/_references.md", StringComparison.Ordinal)
                => item[..^"/_references.md".Length],
            var item when item.EndsWith("/SKILL.md", StringComparison.Ordinal)
                => item[..^"/SKILL.md".Length],
            var item when item.EndsWith(".overwrite.md", StringComparison.Ordinal)
                => item[..^".overwrite.md".Length],
            var item when item.EndsWith(".md", StringComparison.Ordinal)
                => item[..^".md".Length],
            _ => value,
        };
    }

    private static RouteSourceKind ToRouteKind(SourceDocumentForm form)
    {
        return form switch
        {
            SourceDocumentForm.Loader => RouteSourceKind.Loader,
            SourceDocumentForm.Skill => RouteSourceKind.Native,
            SourceDocumentForm.Markdown => RouteSourceKind.Markdown,
            _ when form is SourceDocumentForm.CanonicalEntrypoint
                or SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint => RouteSourceKind.Entrypoint,
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The Route form is not a source base form."),
        };
    }
}
