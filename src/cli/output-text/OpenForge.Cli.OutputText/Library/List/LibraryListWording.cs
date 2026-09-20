using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.List;

internal static class LibraryListWording
{
    // @OpenForgeText library.list.wording.the-source-folder-of-is-not-a-folder-inside-the-workspace
    internal static string SourceRootInvalid(string id, string path)
        => $"The source folder of {id}, {path}, is not a folder inside the workspace.";

    // @OpenForgeText library.list.wording.the-source-folder-of-cannot-be-read
    internal static string SourceRootUnavailable(string id, string path)
        => $"The source folder of {id}, {path}, cannot be read.";

    // @OpenForgeText library.list.wording.the-source-folder-of-resolves-to-an-unsafe-location
    internal static string SourceRootBlocked(string id, string path)
        => $"The source folder of {id}, {path}, resolves to an unsafe location.";

    internal static string LinkMissing(string path) => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.LinkMissing(path);

    internal static string LinkChanged(string path, string id) => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.LinkChanged(path, id);

    // @OpenForgeText library.list.wording.could-not-be-checked
    internal static string LinkUnavailable(string path) => $"{path}  could not be checked";
}
