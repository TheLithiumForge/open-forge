using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Source;

internal static class RouteInspectSourceTestData
{
    private const string GeneratedIndexStart = "<!-- open-forge:generated-index:start -->";
    private const string GeneratedIndexEnd = "<!-- open-forge:generated-index:end -->";
    private const string EmptyEntriesSentinel = "- none - No entries - #Empty";

    internal static RouteSource Source(string path, RouteSourceKind kind, string body = "body")
    {
        return Source(new RouteInspectSourceSpec
        {
            Path = path,
            Kind = kind,
            Body = body,
        });
    }

    internal static RouteSource Source(RouteInspectSourceSpec spec)
    {
        var sourceForm = spec.Form ?? ReadForm(spec.Path, spec.Kind);
        var compatibility = IsCompatibility(sourceForm);
        var tags = spec.Tags ?? (spec.Kind == RouteSourceKind.Native ? [] : ["Route"]);
        var metadata = spec.Kind == RouteSourceKind.Loader
            ? RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.NotApplicable,
                isCompatibilityEntrypoint: false,
                isOverwritePresent: spec.OverwritePath is not null)
            : spec.MetadataState == RouteSourceMetadataState.Complete
                ? RouteSourceMetadata.Complete(
                    $"Description for {spec.Path}",
                    tags,
                    compatibility,
                    spec.OverwritePath is not null)
                : RouteSourceMetadata.WithoutValues(
                    spec.MetadataState,
                    compatibility,
                    spec.OverwritePath is not null);

        var sourceDocument = Document(
            new RouteInspectDocumentSpec
            {
                Path = spec.Path,
                Form = sourceForm,
                ReadState = spec.DocumentReadState,
                Body = spec.Body,
                PhysicalPath = spec.PhysicalPath ?? PhysicalPath(spec.Path),
            });
        var overwrite = spec.OverwritePath is null
            ? null
            : Document(
                new RouteInspectDocumentSpec
                {
                    Path = spec.OverwritePath,
                    Form = SourceDocumentForm.OverwriteCompanion,
                    ReadState = spec.OverwriteReadState,
                    Body = spec.OverwriteBody,
                    PhysicalPath = spec.OverwritePhysicalPath ?? PhysicalPath(spec.OverwritePath),
                });

        return new RouteSource(sourceDocument, metadata, spec.Kind, overwrite, spec.IsRouteAmbiguous);
    }

    internal static RouteSourceDocument Document(RouteInspectDocumentSpec spec)
    {
        return new RouteSourceDocument(
            spec.Path,
            spec.PhysicalPath ?? PhysicalPath(spec.Path),
            spec.Form,
            spec.ReadState,
            spec.ReadState == FileReadState.Complete ? spec.Body : null);
    }

    internal static string PhysicalPath(string key)
    {
        return Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "route-inspect-profile-unit",
            key.Replace('/', Path.DirectorySeparatorChar)));
    }

    internal static string BodyWithEntries(
        string? authoredBody = null,
        IEnumerable<string>? declarations = null)
    {
        var materializedDeclarations = (declarations ?? []).ToArray();
        var generatedBody = materializedDeclarations.Length == 0
            ? EmptyEntriesSentinel
            : string.Join("\n", materializedDeclarations);
        var authored = authoredBody?.TrimEnd();
        var prefix = string.IsNullOrEmpty(authored)
            ? string.Empty
            : $"{authored}\n\n";
        return $"{prefix}## Entries\n\n{GeneratedIndexStart}\n{generatedBody}\n{GeneratedIndexEnd}";
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
}

internal sealed record RouteInspectSourceSpec
{
    internal required string Path { get; init; }

    internal required RouteSourceKind Kind { get; init; }

    internal string Body { get; init; } = "body";

    internal SourceDocumentForm? Form { get; init; }

    internal RouteSourceMetadataState MetadataState { get; init; } = RouteSourceMetadataState.Complete;

    internal string? OverwritePath { get; init; }

    internal string? PhysicalPath { get; init; }

    internal string OverwriteBody { get; init; } = "overwrite";

    internal string? OverwritePhysicalPath { get; init; }

    internal FileReadState DocumentReadState { get; init; } = FileReadState.Complete;

    internal FileReadState OverwriteReadState { get; init; } = FileReadState.Complete;

    internal bool IsRouteAmbiguous { get; init; }

    internal IEnumerable<string>? Tags { get; init; }
}

internal sealed record RouteInspectDocumentSpec
{
    internal required string Path { get; init; }

    internal required SourceDocumentForm Form { get; init; }

    internal FileReadState ReadState { get; init; } = FileReadState.Complete;

    internal string Body { get; init; } = "body";

    internal string? PhysicalPath { get; init; }
}
