using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Install;

internal static class ExtensionInstallWording
{
    // @OpenForgeText extension.install.wording.the-extension-is-already-installed-and-matches-the-package-nothing-to-do
    internal static string AlreadyInstalled(string id)
        => $"The {id} Extension is already installed and matches the package. Nothing to do.";

    // @OpenForgeText extension.install.wording.nothing-was-installed-from-the-package-has-no-content-directory
    internal static string NothingInstalled(string source)
        => $"Nothing was installed from {source}: the package has no content directory.";

    // @OpenForgeText extension.install.wording.package-files-belong-under-content-agents
    internal static string NoContentRow(string source)
        => $"Package files belong under {source}/content/.agents/.";

    // @OpenForgeText extension.install.wording.cannot-install-it-writes-outside-agents-and-no-grant-allows-that
    internal static string PermissionRequired(string id)
        => $"Cannot install {id}: it writes outside .agents and no grant allows that.";

    // @OpenForgeText extension.install.wording.open-forge-extension-install-allow-path
    internal static string PermissionNext(string id, string path)
        => $"open-forge extension install {id} --allow-path {path}";

    // @OpenForgeText extension.install.wording.open-forge-extension-install-force-dry-run
    internal static string ForcePreviewNext(string id)
        => $"open-forge extension install {id} --force --dry-run";

    // @OpenForgeText extension.install.wording.or-add-to-allow-install-paths-in-agents-open-forge-json
    internal static string PermissionAlternative(string path)
        => $"Or add \"{path}\" to allowInstallPaths in .agents/open-forge.json.";

    // @OpenForgeText extension.install.wording.extension-install-stopped-after-of-changes
    internal static string Failed(int completed, int total)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"Extension install stopped after {completed} of {total} changes.");

    // @OpenForgeText extension.install.wording.the-files-or-dependencies-recorded-for-do-not-match-the-selected-source
    internal static string PackageContentsChanged(string id)
        => $"The files or dependencies recorded for {id} do not match the selected source.";

    internal static string SourceUnavailable(string path) => global::OpenForge.Cli.OutputText.Extension.Shared.CanonicalPhrases.SourceUnreadable(path);
}
