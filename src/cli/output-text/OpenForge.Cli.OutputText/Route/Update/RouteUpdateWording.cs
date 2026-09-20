using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Update;

internal static class RouteUpdateWording
{
    // @OpenForgeText route.update.wording.updated
    internal static string Updated(string id) => $"Updated {id}";

    // @OpenForgeText route.update.wording.already-has-these-values-nothing-to-do
    internal static string AlreadyMatching(string id)
        => $"{id} already has these values. Nothing to do.";

    // @OpenForgeText route.update.wording.would-update
    internal static string WouldUpdate(string id) => $"Would update {id}";

    // @OpenForgeText route.update.wording.updated-but-the-template-body-was-not-copied
    internal static string UpdatedProtected(string id)
        => $"Updated {id}, but the Template body was not copied.";

    // @OpenForgeText route.update.wording.route-update-stopped-after-of-changes
    internal static string Failed(int completed, int total)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"Route update stopped after {completed} of {total} changes.");

    // @OpenForgeText route.update.wording.path
    internal static string Path(string path) => $"Path: {path}";

    internal static string TemplatePath(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.TemplatePath(path);

    // @OpenForgeText route.update.wording.frontmatter-rewritten
    internal static string FrontmatterRewritten(string path) => $"{path}  frontmatter rewritten";

    internal static string EntryUpdated(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.EntryUpdated(path);

    internal static string BodyCopied(string reference) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.BodyCopied(reference);

    // @OpenForgeText route.update.wording.the-file-already-has-content-which-was-kept-the-template-was-not-copied
    internal static string ProtectedBody(string reference)
        => $"The file already has content, which was kept. The Template {reference} was not copied.";

    // @OpenForgeText route.update.wording.is-not-a-routed-source-that-can-be-updated
    internal static string InvalidTarget(string reference)
        => $"{reference} is not a routed source that can be updated.";

    internal static string InvalidTemplate(string reference)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.InvalidTemplate(reference);

    // @OpenForgeText route.update.wording.the-frontmatter-of-cannot-be-rewritten-safely
    internal static string FrontmatterUnsafe(string path, string reason)
        => $"The frontmatter of {path} cannot be rewritten safely: {reason}.";

    // @OpenForgeText route.update.wording.other-frontmatter-keys-in-could-not-be-preserved-so-nothing-was-changed
    internal static string MetadataPreservationUnsafe(string path)
        => $"Other frontmatter keys in {path} could not be preserved, so nothing was changed.";
}
