using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

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
        ArgumentNullException.ThrowIfNull(spec);
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
                    Form = RouteSourceForm.OverwriteCompanion,
                    ReadState = spec.OverwriteReadState,
                    Body = spec.OverwriteBody,
                    PhysicalPath = spec.OverwritePhysicalPath ?? PhysicalPath(spec.OverwritePath),
                });

        return new RouteSource(sourceDocument, metadata, spec.Kind, overwrite, spec.IsRouteAmbiguous);
    }

    internal static RouteSourceDocument Document(RouteInspectDocumentSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
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

internal sealed record RouteInspectSourceSpec
{
    internal required string Path { get; init; }

    internal required RouteSourceKind Kind { get; init; }

    internal string Body { get; init; } = "body";

    internal RouteSourceForm? Form { get; init; }

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

    internal required RouteSourceForm Form { get; init; }

    internal FileReadState ReadState { get; init; } = FileReadState.Complete;

    internal string Body { get; init; } = "body";

    internal string? PhysicalPath { get; init; }
}
