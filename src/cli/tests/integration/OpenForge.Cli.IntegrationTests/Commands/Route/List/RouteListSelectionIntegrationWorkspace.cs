using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

internal sealed class RouteListSelectionIntegrationWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _temporary;

    private RouteListSelectionIntegrationWorkspace(TemporaryWorkspace temporary)
    {
        _temporary = temporary;
        _temporary.CreateDirectory(".agents");
    }

    internal string Path => _temporary.Path;

    internal CliWorkspace Workspace => new(
        Path,
        Path,
        CliWorkspaceSelectionMethod.ExplicitWorkspace);

    internal static RouteListSelectionIntegrationWorkspace Create()
    {
        return new RouteListSelectionIntegrationWorkspace(
            TemporaryWorkspace.Create("route-list-selection-integration"));
    }

    internal void Write(string relativePath, string contents)
    {
        _temporary.WriteText(relativePath, contents);
    }

    internal void WriteLoader(string entries)
    {
        Write(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build(entries));
    }

    internal string Absolute(string relativePath)
    {
        return _temporary.Combine(relativePath);
    }

    internal RouteSource Source(
        string id,
        string canonicalPath,
        RouteListSourceKind kind = RouteListSourceKind.RoutedLeaf,
        string? physicalRelativePath = null,
        string? overwritePath = null,
        bool isRouteAmbiguous = false)
    {
        var physicalPath = _temporary.Combine(physicalRelativePath ?? canonicalPath);
        if (!string.Equals(id, RouteSourceIdentity.DeriveId(canonicalPath), StringComparison.Ordinal))
        {
            throw new ArgumentException("The source ID must match its canonical path identity.", nameof(id));
        }

        var form = ReadForm(canonicalPath, kind);
        var sourceKind = form switch
        {
            RouteSourceForm.Loader => RouteSourceKind.Loader,
            RouteSourceForm.CanonicalEntrypoint
                or RouteSourceForm.IndexEntrypoint
                or RouteSourceForm.UnderscoreIndexEntrypoint
                or RouteSourceForm.ReferencesEntrypoint
                or RouteSourceForm.UnderscoreReferencesEntrypoint => RouteSourceKind.Entrypoint,
            RouteSourceForm.Skill => RouteSourceKind.Native,
            RouteSourceForm.Markdown => RouteSourceKind.Markdown,
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The source form is not defined."),
        };
        var metadata = form == RouteSourceForm.Loader
            ? RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.NotApplicable,
                isCompatibilityEntrypoint: false,
                isOverwritePresent: overwritePath is not null)
            : kind == RouteListSourceKind.Unrouted
            ? RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.Missing,
                IsCompatibility(form),
                overwritePath is not null)
            : RouteSourceMetadata.Complete(
                $"Description for {id}",
                form == RouteSourceForm.Skill ? [] : ["Route"],
                IsCompatibility(form),
                overwritePath is not null);
        var baseDocument = new RouteSourceDocument(
            canonicalPath,
            physicalPath,
            form,
            FileReadState.Complete,
            "body");
        var overwrite = overwritePath is null
            ? null
            : new RouteSourceDocument(
                overwritePath,
                _temporary.Combine(overwritePath),
                RouteSourceForm.OverwriteCompanion,
                FileReadState.Complete,
                "overwrite");
        return new RouteSource(
            baseDocument,
            metadata,
            sourceKind,
            overwrite,
            isRouteAmbiguous);
    }

    internal RouteSourceCatalogue Catalogue(params RouteSource[] sources)
    {
        var overwriteFacts = sources
            .Where(source => source.Overwrite is not null)
            .Select(source => new RouteOverwriteFact(
                RouteOverwriteState.Paired,
                source.Overwrite!,
                [source.CanonicalPath]));
        return new RouteSourceCatalogue(sources, overwriteFacts);
    }

    private static RouteSourceForm ReadForm(string path, RouteListSourceKind kind)
    {
        if (kind == RouteListSourceKind.Loader)
        {
            return RouteSourceForm.Loader;
        }

        var fileName = path[(path.LastIndexOf('/') + 1)..];
        if (fileName == "SKILL.md")
        {
            return RouteSourceForm.Skill;
        }

        if (kind != RouteListSourceKind.Entrypoint)
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

    internal RouteListRequest Request(string? sourceReference = null)
    {
        return new RouteListRequest(
            Workspace,
            sourceReference,
            RouteListDepth.Default);
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
    {
        return _temporary.SnapshotHashes();
    }

    internal bool TryCreateDirectorySymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
    {
        return _temporary.TryCreateDirectorySymbolicLink(relativeLinkPath, targetPath, out linkPath);
    }

    internal bool TryCreateFileSymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
    {
        return _temporary.TryCreateFileSymbolicLink(relativeLinkPath, targetPath, out linkPath);
    }

    public void Dispose()
    {
        _temporary.Dispose();
    }
}
