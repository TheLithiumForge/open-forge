using System.Globalization;

namespace OpenForge.Cli.OutputText.Install;

internal static class InstallWording
{
    // @OpenForgeText install.wording.install-stopped-after-of-changes
    internal static string Failed(int completed, int total)
        => string.Create(CultureInfo.InvariantCulture,
            $"Install stopped after {completed} of {total} changes.");

    // @OpenForgeText install.wording.install-was-cancelled-stopped-after-of-changes
    internal static string CancelledAfter(int completed, int total)
        => string.Create(CultureInfo.InvariantCulture,
            $"Install was cancelled. Stopped after {completed} of {total} changes.");

    // @OpenForgeText install.wording.source
    internal static string SourceAsset(string path) => $"source: {path}";

    internal static string RecoveryData(string path) => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FormatRecoveryData(path);

    // @OpenForgeText install.wording.has-changed-since-it-was-installed-install-does-not-replace-changed-files
    internal static string ManagedDivergence(string path) => $"{path} has changed since it was installed. Install does not replace changed files.";

    internal static string RecoveryRetained(string path) => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheRecoveryBundleWasRetainedAt(path);

    // @OpenForgeText install.wording.verification
    internal static string Verification(string state) => $"Verification: {state}.";

    // @OpenForgeText install.wording.bundled-framework-fingerprint-assets
    internal static string Source(string fingerprint, int assets)
        => string.Create(CultureInfo.InvariantCulture,
            $"Bundled Framework fingerprint: {fingerprint} ({assets} assets).");
}
