using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Inspect;

internal static class LibraryInspectWording
{
    // @OpenForgeText library.inspect.wording.is-current-the-source-folder-has-no-eligible-files-and-no-links-are-registered
    internal static string Empty(string id, string source)
        => $"{id} is current. The source folder {source} has no eligible files and no links are registered.";

    // @OpenForgeText library.inspect.wording.no-ownership-record-exists-so-cannot-be-inspected
    internal static string NoOwnership(string id)
        => $"No ownership record exists, so {id} cannot be inspected.";

    internal static string SourceRootInvalid(string path)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.SourceRootInvalid(path);

    // @OpenForgeText library.inspect.wording.the-source-folder-cannot-be-read-so-the-comparison-could-not-finish
    internal static string SourceRootUnavailable(string path)
        => $"The source folder {path} cannot be read, so the comparison could not finish.";

    internal static string SourceRootBlocked(string path)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.SourceRootBlocked(path);

    // @OpenForgeText library.inspect.wording.some-files-under-could-not-be-listed-so-the-comparison-could-not-finish
    internal static string InventoryIncomplete(string source)
        => $"Some files under {source} could not be listed, so the comparison could not finish.";

    // @OpenForgeText library.inspect.wording.not-linked-yet-new-in-the-source-folder
    internal static string PathAdded(string path)
        => $"{path}  not linked yet; new in the source folder";

    // @OpenForgeText library.inspect.wording.linked-but-its-source-file-is-gone
    internal static string PathRetired(string path)
        => $"{path}  linked, but its source file is gone";

    internal static string LinkMissing(string path)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.LinkMissing(path);

    internal static string LinkChanged(string path, string id)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.LinkChanged(path, id);
}
