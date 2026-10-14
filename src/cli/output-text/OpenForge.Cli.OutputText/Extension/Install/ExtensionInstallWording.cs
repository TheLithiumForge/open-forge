using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Install;

internal static class ExtensionInstallWording
{
    // @OpenForgeText extension.install.wording.partially-excluded-files
    internal static string PartiallyExcludedFiles(string name, int count, IReadOnlyList<string> categories, bool preview)
    {
        var noun = count == 1 ? "file" : "files";
        var files = string.Create(CultureInfo.InvariantCulture, $"{count} package {noun}");
        if (categories.Count == 1 && categories[0] == "Templates") files = $"starter {noun}";
        var possessive = name.EndsWith('s') ? $"{name}'" : $"{name}'s";
        return $"{possessive} {files} {SkippedTense(count, preview)} skipped because {ExclusionReason(categories)}.";
    }

    // Pass no categories when the headline already names the exclusion, so the reason is not repeated.
    internal static string ExcludedFiles(string id, int count, IReadOnlyList<string>? categories, bool preview)
    {
        var files = count == 1
            ? $"The package file from {id}"
            : string.Create(CultureInfo.InvariantCulture, $"All {count} package files from {id}");
        var reason = categories is null ? string.Empty : $" because {ExclusionReason(categories)}";
        return $"{files} {SkippedTense(count, preview)} skipped{reason}.";
    }

    internal static string ExcludedCategories(string categories, int count)
        => $"{categories} {(count == 1 ? "is" : "are")} left out";

    private static string SkippedTense(int count, bool preview)
        => preview ? "would be" : count == 1 ? "was" : "were";

    private static string ExclusionReason(IReadOnlyList<string> categories)
        => categories.Count switch
        {
            0 => "workspace settings exclude those paths",
            1 => $"{categories[0]} is left out",
            _ => $"{string.Join(", ", categories)} are left out",
        };

    internal static string ExcludedPaths() => "workspace settings exclude the package content";

    internal static string RecordedExcludedPackage(string id, string reason, bool preview, bool alreadyRecorded = false)
        => RecordedExcludedPackages([id], reason, preview, alreadyRecorded);

    internal static string RecordedExcludedPackages(IReadOnlyList<string> ids, string reason, bool preview, bool alreadyRecorded)
    {
        var subject = ids.Count == 1
            ? $"the {ids[0]} Extension"
            : string.Create(CultureInfo.InvariantCulture, $"{ids.Count} Extensions ({string.Join(", ", ids)})");
        if (alreadyRecorded)
        {
            return $"Already recorded {subject} without {(ids.Count == 1 ? "its" : "their")} files, because {reason}.";
        }
        return $"{(preview ? "Would record" : "Recorded")} {subject} without {(ids.Count == 1 ? "its" : "their")} files, because {reason}.";
    }

    internal static string IncludeCategory(string label)
        => $"To add its files, restore {label} and update the package:";

    internal static string PreviewCommand(string command) => $"  {command}";

    internal static string RecordedWithoutFiles(string id)
        => $"{id} is already installed without its files. Update it to add them.";

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
