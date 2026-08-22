using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal static class RouteInspectCatalogueSourceFactory
{
    internal static RouteSource Create(
        RouteInspectInventoryFile file,
        RouteInspectInventoryFile? overwrite,
        bool isRouteAmbiguous,
        RouteMetadataParser metadataParser,
        ICollection<RouteOverwriteFact> overwriteFacts)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(metadataParser);
        ArgumentNullException.ThrowIfNull(overwriteFacts);
        var hasOverwrite = overwrite is not null;
        var metadata = ReadMetadata(file, hasOverwrite, metadataParser);
        var source = new RouteSource(
            ToDocument(file),
            metadata,
            ReadSourceKind(file.Form),
            hasOverwrite ? ToDocument(overwrite!) : null,
            isRouteAmbiguous);
        if (hasOverwrite)
        {
            overwriteFacts.Add(new RouteOverwriteFact(
                RouteOverwriteState.Paired,
                ToDocument(overwrite!),
                [source.CanonicalPath]));
        }

        return source;
    }

    internal static RouteSourceDocument ToDocument(RouteInspectInventoryFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        return new RouteSourceDocument(
            file.CanonicalPath,
            file.PhysicalPath,
            file.Form,
            file.ReadState,
            file.Body);
    }

    private static RouteSourceMetadata ReadMetadata(
        RouteInspectInventoryFile file,
        bool isOverwritePresent,
        RouteMetadataParser parser)
    {
        var compatibility = RouteInspectSourcePolicy.IsCompatibilityEntrypoint(file.Form);
        if (file.ReadState != FileReadState.Complete)
        {
            return RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.ReadUnavailable,
                compatibility,
                isOverwritePresent);
        }

        if (file.Form == RouteSourceForm.Loader)
        {
            return RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.NotApplicable,
                isCompatibilityEntrypoint: false,
                isOverwritePresent);
        }

        return file.Form == RouteSourceForm.Skill
            ? parser.ParseSkill(file.Body!, isOverwritePresent)
            : parser.ParseOpenForge(file.Body!, compatibility, isOverwritePresent);
    }

    private static RouteSourceKind ReadSourceKind(RouteSourceForm form)
    {
        return form switch
        {
            RouteSourceForm.CanonicalEntrypoint
                or RouteSourceForm.IndexEntrypoint
                or RouteSourceForm.UnderscoreIndexEntrypoint
                or RouteSourceForm.ReferencesEntrypoint
                or RouteSourceForm.UnderscoreReferencesEntrypoint => RouteSourceKind.Entrypoint,
            RouteSourceForm.Loader => RouteSourceKind.Loader,
            RouteSourceForm.Markdown => RouteSourceKind.Markdown,
            RouteSourceForm.Skill => RouteSourceKind.Native,
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The source form is not a logical source base."),
        };
    }
}
