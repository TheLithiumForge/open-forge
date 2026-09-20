using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.List;

internal static class RouteListWording
{
    internal static string InvalidSourceReference(string operand)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FilterNotASource(operand);

    // @OpenForgeText route.list.wording.resolves-outside-the-workspace-and-was-not-listed
    internal static string PhysicalBoundary(string path)
        => $"{path} resolves outside the workspace and was not listed.";

    // @OpenForgeText route.list.wording.is-not-a-routed-markdown-source
    internal static string UnsupportedSource(string path)
        => $"{path} is not a routed Markdown source.";

    // @OpenForgeText route.list.wording.could-not-be-read-so-the-routes-below-it-are-not-listed
    internal static string ReadUnavailable(string path)
        => $"{path} could not be read, so the routes below it are not listed.";

    // @OpenForgeText route.list.wording.has-no-description-in-its-frontmatter
    internal static string MetadataMissing(string path)
        => $"{path} has no description in its frontmatter.";

    // @OpenForgeText route.list.wording.uses-the-compatibility-entrypoint-name
    internal static string AuthoredForm(string path, string name)
        => $"{path} uses the compatibility entrypoint name {name}.";

    // @OpenForgeText route.list.wording.rename-to-md
    internal static string RenameEntrypoint(string path, string name)
        => $"Rename {path} to _{name}.md.";

    // @OpenForgeText route.list.wording.open-forge-route-update-description
    internal static string RepairMetadata(string id)
        => $"open-forge route update {id} --description \"...\"";

    // @OpenForgeText route.list.wording.use-to-continue-the-route-listing
    internal static string NextReason(string action) => $"Use {action} to continue the route listing.";
}
