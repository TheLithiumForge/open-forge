using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal static class RouteInspectSourcePolicy
{
    internal static RouteInspectSourceKind ReadInspectKind(SourceDocumentForm form)
    {
        return form switch
        {
            SourceDocumentForm.CanonicalEntrypoint
                or SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint => RouteInspectSourceKind.Entrypoint,
            SourceDocumentForm.Markdown => RouteInspectSourceKind.Markdown,
            SourceDocumentForm.Skill => RouteInspectSourceKind.Native,
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The source form cannot be inspected."),
        };
    }

    internal static RouteInspectSourceForm ReadInspectForm(SourceDocumentForm form)
    {
        return form switch
        {
            SourceDocumentForm.CanonicalEntrypoint => RouteInspectSourceForm.CanonicalEntrypoint,
            SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint => RouteInspectSourceForm.CompatibilityEntrypoint,
            SourceDocumentForm.Markdown => RouteInspectSourceForm.Markdown,
            SourceDocumentForm.Skill => RouteInspectSourceForm.Native,
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The source form cannot be inspected."),
        };
    }
}
