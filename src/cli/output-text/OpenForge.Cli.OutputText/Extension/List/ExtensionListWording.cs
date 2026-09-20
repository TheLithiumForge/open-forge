using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.List;

internal static class ExtensionListWording
{
    // @OpenForgeText extension.list.wording.available-from
    internal static string AvailableFrom(string path) => $"Available (from {path})";

    // @OpenForgeText extension.list.wording.recorded-source
    internal static string RecordedSource(string path) => $"Recorded source: {path}";

    internal static string Source(string path) => global::OpenForge.Cli.OutputText.Extension.Shared.CanonicalPhrases.Source(path);

    // @OpenForgeText extension.list.wording.installed
    internal static string InstalledVersion(string version) => $"installed: {version}";

    // @OpenForgeText extension.list.wording.could-not-be-read-so-available-packages-are-not-listed
    internal static string SourceUnavailable(string path) => $"{path} could not be read, so available packages are not listed.";

    // @OpenForgeText extension.list.wording.the-extension-s-source-is-missing
    internal static string InstalledSourceMissing(string id, string path)
        => $"The {id} Extension's source {path} is missing.";

    // @OpenForgeText extension.list.wording.the-extension-s-source-could-not-be-read
    internal static string InstalledSourceUnavailable(string id, string path)
        => $"The {id} Extension's source {path} could not be read.";

    // @OpenForgeText extension.list.wording.the-files-installed-by-could-not-be-compared
    internal static string InstalledFilesUnavailable(string id)
        => $"The files installed by {id} could not be compared.";

    // @OpenForgeText extension.list.wording.the-installed-file-could-not-be-checked-safely
    internal static string InstalledTargetBlocked(string path)
        => $"The installed file {path} could not be checked safely.";
}
