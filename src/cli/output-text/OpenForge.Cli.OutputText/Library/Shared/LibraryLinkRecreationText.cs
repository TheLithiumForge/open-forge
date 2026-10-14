namespace OpenForge.Cli.OutputText.Library.Shared;

internal static class LibraryLinkRecreationText
{
    // @OpenForgeText library.shared.message.link-unreadable-on-windows
    internal static string Unreadable(string path)
        => $"Library link {path} is unreadable on Windows because its target uses forward slashes.";

    // @OpenForgeText library.shared.title.links-unreadable-on-windows
    internal static string Headline(string id)
        => $"Library {id} has unreadable links on Windows.";

    // @OpenForgeText library.shared.title.link-unreadable-on-windows
    internal static string Title() => "Library link unreadable on Windows";

    // @OpenForgeText library.shared.message.detach-and-attach-again
    internal static string Reason()
        => "Detach, clear the saved Library exclusion, then attach again with the same options. Source files are kept.";

    // @OpenForgeText library.shared.message.recreation-steps
    internal static string Steps(string detach, string id, string settings, string attach)
        => $"Run {detach}. Remove Library ID '{id}' from removedLibraries in {settings}. Then run {attach}.";
}
