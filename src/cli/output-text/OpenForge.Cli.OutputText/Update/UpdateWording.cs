using System.Globalization;

namespace OpenForge.Cli.OutputText.Update;

internal static class UpdateWording
{
    // @OpenForgeText update.wording.update-stopped-after-of-changes
    internal static string Failed(int completed, int total)
        => string.Create(CultureInfo.InvariantCulture, $"Update stopped after {completed} of {total} changes.");

    // @OpenForgeText update.wording.update-was-cancelled-stopped-after-of-changes
    internal static string CancelledAfter(int completed, int total)
        => string.Create(CultureInfo.InvariantCulture, $"Update was cancelled. Stopped after {completed} of {total} changes.");

    // @OpenForgeText update.wording.could-not-be-read-so-it-was-not-updated
    internal static string TargetUnavailable(string path) => $"{path} could not be read, so it was not updated.";

    // @OpenForgeText update.wording.the-ownership-record-names-an-unknown-source-for
    internal static string SourceProvenanceInvalid(string path)
        => $"The ownership record names an unknown source for {path}.";

    // @OpenForgeText update.wording.could-not-be-compared-with-the-version-this-cli-ships
    internal static string FingerprintUnsupported(string path)
        => $"{path} could not be compared with the version this CLI ships.";

    // @OpenForgeText update.wording.is-no-longer-part-of-this-release-so-it-was-kept
    internal static string RetiredPreserved(string path)
        => $"{path} is no longer part of this release, so it was kept.";

    // @OpenForgeText update.wording.current-sha-256
    internal static string CurrentHash(string path, string hash) => $"{path} current SHA-256: {hash}";

    // @OpenForgeText update.wording.shipped-sha-256
    internal static string ShippedHash(string path, string hash) => $"{path} shipped SHA-256: {hash}";

    // @OpenForgeText update.wording.source-asset
    internal static string SourceAsset(string path, string source) => $"{path} source asset: {source}";

    internal static string RecoveryData(string path) => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FormatRecoveryData(path);
}
