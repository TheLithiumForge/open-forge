using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal static class RouteInspectSourcePolicy
{
    internal static RouteInspectSourceKind ReadInspectKind(RouteSourceKind kind)
    {
        return kind switch
        {
            RouteSourceKind.Entrypoint => RouteInspectSourceKind.Entrypoint,
            RouteSourceKind.Markdown => RouteInspectSourceKind.Markdown,
            RouteSourceKind.Native => RouteInspectSourceKind.Native,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The source kind cannot be inspected."),
        };
    }

    internal static RouteInspectSourceForm ReadInspectForm(RouteSourceForm form)
    {
        return form switch
        {
            RouteSourceForm.CanonicalEntrypoint => RouteInspectSourceForm.CanonicalEntrypoint,
            RouteSourceForm.IndexEntrypoint
                or RouteSourceForm.UnderscoreIndexEntrypoint
                or RouteSourceForm.ReferencesEntrypoint
                or RouteSourceForm.UnderscoreReferencesEntrypoint => RouteInspectSourceForm.CompatibilityEntrypoint,
            RouteSourceForm.Markdown => RouteInspectSourceForm.Markdown,
            RouteSourceForm.Skill => RouteInspectSourceForm.Native,
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The source form cannot be inspected."),
        };
    }

    internal static bool IsEntrypoint(RouteSourceForm form)
    {
        return RouteSourceFormClassifier.IsEntrypoint(form);
    }

    internal static bool IsCompatibilityEntrypoint(RouteSourceForm form)
    {
        return RouteSourceFormClassifier.IsCompatibilityEntrypoint(form);
    }
}
