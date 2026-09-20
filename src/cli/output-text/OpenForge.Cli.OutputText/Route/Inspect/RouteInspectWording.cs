using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Inspect;

internal static class RouteInspectWording
{
    internal static string UnknownSource(string reference)
        => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.UnknownSource(reference);

    internal static string InvalidSourceReference(string operand)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FilterNotASource(operand);

    // @OpenForgeText route.inspect.wording.does-not-exist
    internal static string MissingSourceFile(string path)
        => $"{path} does not exist.";

    // @OpenForgeText route.inspect.wording.is-not-a-markdown-source-open-forge-routes
    internal static string UnsupportedSource(string path)
        => $"{path} is not a Markdown source Open Forge routes.";

    // @OpenForgeText route.inspect.wording.the-id-also-matches-use-the-exact-path-to-be-sure
    internal static string AutomaticIdNotUnique(string id, string other)
        => $"The ID {id} also matches {other}. Use the exact path to be sure.";

    // @OpenForgeText route.inspect.wording.uses-the-compatibility-name-the-canonical-name-is-md
    internal static string CompatibilityEntrypoint(string path, string name, string folder)
        => $"{path} uses the compatibility name {name}; the canonical name is _{folder}.md.";

    // @OpenForgeText route.inspect.wording.is-not-reachable-from-any-route-so-agents-never-load-it-automatically
    internal static string NotRouted(string path)
        => $"{path} is not reachable from any route, so agents never load it automatically.";

    // @OpenForgeText route.inspect.wording.belongs-to-a-route-that-no-loader-entry-reaches
    internal static string Detached(string path)
        => $"{path} belongs to a route that no Loader entry reaches.";

    // @OpenForgeText route.inspect.wording.is-read-together-with
    internal static string ValidOverwrite(string path, string basePath)
        => $"{path} is read together with {basePath}.";

    internal static string InspectionIncomplete(string path)
        => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.InspectionIncomplete(path);
}
