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

    // @OpenForgeText extension.list.wording.source-failure-headline
    internal static string SourceFailureHeadline(string path) => $"Available Extensions could not be listed from {path}.";

    // @OpenForgeText extension.list.wording.manifest-invalid
    internal static string ManifestInvalid(string path) => $"{path} is not a valid Extension manifest.";

    // @OpenForgeText extension.list.wording.manifest-encoding-invalid
    internal static string ManifestEncodingInvalid(string path) => $"{path} is not valid UTF-8.";

    // @OpenForgeText extension.list.wording.manifest-access-denied
    internal static string ManifestAccessDenied(string path) => $"{path} could not be read: permission was denied.";

    // @OpenForgeText extension.list.wording.manifest-in-use
    internal static string ManifestInUse(string path) => $"{path} could not be read because it is in use.";

    // @OpenForgeText extension.list.wording.manifest-io-failed
    internal static string ManifestIoFailed(string path) => $"{path} could not be read because a filesystem operation failed.";

    // @OpenForgeText extension.list.wording.manifest-invalid.next
    internal static string ManifestInvalidNext() => "Check the manifest's required fields and values, then retry.";

    // @OpenForgeText extension.list.wording.manifest-encoding-invalid.next
    internal static string ManifestEncodingInvalidNext() => "Save the manifest as UTF-8, then retry.";

    // @OpenForgeText extension.list.wording.manifest-access-denied.next
    internal static string ManifestAccessDeniedNext() => "Check read access to the named file, then retry.";

    // @OpenForgeText extension.list.wording.manifest-in-use.next
    internal static string ManifestInUseNext() => "Close the program holding the file, then retry.";

    // @OpenForgeText extension.list.wording.manifest-io-failed.next
    internal static string ManifestIoFailedNext() => "Check that the file is accessible, then retry.";

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
