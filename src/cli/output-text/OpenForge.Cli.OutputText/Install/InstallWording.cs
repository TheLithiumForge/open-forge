using System.Globalization;

namespace OpenForge.Cli.OutputText.Install;

internal static class InstallWording
{
    // @OpenForgeText install.wording.metadata-completed
    internal static string MetadataCompleted(bool preview)
        => preview
            ? "metadata would be completed, your content would be kept"
            : "metadata completed, your content was kept";

    // @OpenForgeText install.wording.configure-headline
    internal static string Configured(string workspace, bool preview)
        => preview
            ? $"Would change the Open Forge setup in {workspace}."
            : $"Changed the Open Forge setup in {workspace}.";

    // @OpenForgeText install.wording.summary-with-host-files
    internal static string WithHostFiles(string summary)
        => $"{summary.TrimEnd('.')}{InstallText.MessagePlusAgentsMdAndClaudeMd()}";

    // @OpenForgeText install.wording.change-summary
    internal static string ChangeSummary(string? creationSummary, int updatedFiles, int replacedFiles, bool preview)
    {
        var updatedNoun = updatedFiles == 1 ? "file" : "files";
        var replacedNoun = replacedFiles == 1 ? "file" : "files";
        var updateVerb = preview ? "update" : "updated";
        var replaceVerb = preview ? "replace" : "replaced";
        if (creationSummary is null && !preview)
        {
            updateVerb = "Updated";
            replaceVerb = "Replaced";
        }

        string changes;
        if (updatedFiles > 0 && replacedFiles > 0)
        {
            changes = string.Create(CultureInfo.InvariantCulture,
                $"{updateVerb} {updatedFiles} existing {updatedNoun} and {(preview ? "replace" : "replaced")} {replacedFiles} existing {replacedNoun}");
        }
        else if (replacedFiles > 0)
            changes = string.Create(CultureInfo.InvariantCulture, $"{replaceVerb} {replacedFiles} existing {replacedNoun}");
        else
            changes = string.Create(CultureInfo.InvariantCulture, $"{updateVerb} {updatedFiles} existing {updatedNoun}");

        if (creationSummary is not null)
            return $"{creationSummary.TrimEnd('.')}, and {changes}.";

        return preview ? $"Would {changes}." : $"{changes}.";
    }

    // @OpenForgeText install.wording.frontmatter-conversion
    internal static string FrontmatterConversion(string form, bool preview)
        => (form, preview) switch
        {
            ("root", true) => "metadata would move to root keys",
            ("root", false) => "metadata moved to root keys",
            ("scoped", true) => "metadata would move under open-forge:",
            ("scoped", false) => "metadata moved under open-forge:",
            _ => throw new ArgumentOutOfRangeException(nameof(form)),
        };

    // @OpenForgeText install.wording.git-ignore-rules
    internal static string GitIgnoreRules(bool added, bool preview)
        => (added, preview) switch
        {
            (true, true) => "Open Forge Git-ignore rules would be added",
            (true, false) => "Open Forge Git-ignore rules added",
            (false, true) => "Open Forge Git-ignore rules would be updated",
            (false, false) => "Open Forge Git-ignore rules updated",
        };

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
