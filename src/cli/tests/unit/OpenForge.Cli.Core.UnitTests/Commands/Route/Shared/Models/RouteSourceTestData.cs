using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models;

internal static class RouteSourceTestData
{
    internal static RouteSourceDocument Document(
        string path,
        RouteSourceForm form,
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

    internal static RouteSource Source(
        string path,
        RouteSourceKind kind,
        RouteSourceForm? form = null,
        RouteSourceMetadataState metadataState = RouteSourceMetadataState.Complete,
        string? overwritePath = null,
        FileReadState documentReadState = FileReadState.Complete,
        bool isRouteAmbiguous = false)
    {
        var sourceForm = form ?? ReadForm(path, kind);
        var compatibility = IsCompatibility(sourceForm);
        var metadata = sourceForm == RouteSourceForm.Loader
            ? RouteSourceMetadata.WithoutValues(
                metadataState == RouteSourceMetadataState.Complete
                    ? RouteSourceMetadataState.NotApplicable
                    : metadataState,
                isCompatibilityEntrypoint: false,
                isOverwritePresent: overwritePath is not null)
            : metadataState == RouteSourceMetadataState.Complete
            ? RouteSourceMetadata.Complete(
                $"Description for {path}",
                sourceForm == RouteSourceForm.Skill ? [] : ["Route"],
                compatibility,
                overwritePath is not null)
            : RouteSourceMetadata.WithoutValues(
                metadataState,
                compatibility,
                overwritePath is not null);
        var sourceDocument = Document(path, sourceForm, documentReadState);
        var overwrite = overwritePath is null
            ? null
            : Document(overwritePath, RouteSourceForm.OverwriteCompanion);
        return new RouteSource(sourceDocument, metadata, kind, overwrite, isRouteAmbiguous);
    }

    internal static string PhysicalPath(string canonicalPath)
    {
        return Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "route-shared-model-unit",
            canonicalPath.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static RouteSourceForm ReadForm(string path, RouteSourceKind kind)
    {
        if (kind == RouteSourceKind.Loader)
        {
            return RouteSourceForm.Loader;
        }

        var fileName = path[(path.LastIndexOf('/') + 1)..];
        if (kind == RouteSourceKind.Native)
        {
            return RouteSourceForm.Skill;
        }

        if (kind == RouteSourceKind.Markdown)
        {
            return RouteSourceForm.Markdown;
        }

        return fileName switch
        {
            "index.md" => RouteSourceForm.IndexEntrypoint,
            "_index.md" => RouteSourceForm.UnderscoreIndexEntrypoint,
            "references.md" => RouteSourceForm.ReferencesEntrypoint,
            "_references.md" => RouteSourceForm.UnderscoreReferencesEntrypoint,
            _ => RouteSourceForm.CanonicalEntrypoint,
        };
    }

    private static bool IsCompatibility(RouteSourceForm form)
    {
        return form is RouteSourceForm.IndexEntrypoint
            or RouteSourceForm.UnderscoreIndexEntrypoint
            or RouteSourceForm.ReferencesEntrypoint
            or RouteSourceForm.UnderscoreReferencesEntrypoint;
    }
}
