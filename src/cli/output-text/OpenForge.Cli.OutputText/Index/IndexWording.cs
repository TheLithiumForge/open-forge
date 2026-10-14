using System.Globalization;

namespace OpenForge.Cli.OutputText.Index;

internal static class IndexWording
{
    // Names only the parts that happened, entrypoints first because they come first in the plan.
    internal static string Completion(int metadata, int entrypoints, int renames, bool preview)
    {
        var parts = new List<string>(3);
        if (entrypoints > 0)
            parts.Add(string.Create(CultureInfo.InvariantCulture,
                $"{(preview ? "create" : "created")} {entrypoints} {(entrypoints == 1 ? "entrypoint" : "entrypoints")}"));
        if (renames > 0)
            parts.Add(string.Create(CultureInfo.InvariantCulture,
                $"{(preview ? "rename" : "renamed")} {renames} {(renames == 1 ? "navigation file" : "navigation files")}"));
        if (metadata > 0)
            parts.Add(string.Create(CultureInfo.InvariantCulture,
                $"{(preview ? "complete" : "completed")} metadata in {metadata} {(metadata == 1 ? "file" : "files")}"));
        if (parts.Count == 0) return preview ? "Would change nothing." : "Nothing was changed.";
        var sentence = string.Join(" and ", parts);
        return preview ? $"Would {sentence}." : $"{char.ToUpperInvariant(sentence[0])}{sentence[1..]}.";
    }
    internal static string NavigationFilesRenamed() => "Navigation files renamed";
    internal static string MetadataRow(bool preview) => preview
        ? "metadata would be completed, your content would be kept" : "metadata completed, your content was kept";
    internal static string EntrypointRow(bool preview) => preview ? "entrypoint would be created" : "entrypoint created";
    internal static string DeletedRow(bool preview) => preview ? "would be deleted" : "deleted";
    internal static string RenamedRow(string oldName, bool preview)
        => preview ? $"would be renamed from {oldName}, your rules would be kept" : $"renamed from {oldName}, your rules were kept";
    internal static string MetadataSkipped(string path, bool preview) => preview
        ? $"{path} has invalid frontmatter. It was left unchanged. Other files would be indexed."
        : $"{path} has invalid frontmatter. It was left unchanged. Other files were indexed.";
    internal static string EditFrontmatter(string path, string command) => $"Fix the frontmatter in {path}, then run {command}.";
    internal static string Verify() => "Check the workspace after indexing.";
    internal static string ApplyPreview() => "Apply the checked plan.";
    // @OpenForgeText index.wording.cannot-index
    internal static string CannotIndex(string operand, string reason) => $"Cannot index {operand}: {reason}.";

    // @OpenForgeText index.wording.cannot-rebuild-the-entries-section-of
    internal static string Blocked(string parent) => $"Cannot rebuild the Entries section of {parent}.";

    // @OpenForgeText index.wording.entries-section
    internal static string EntriesSection(string path) => $"--- {path}  (Entries section)";

    // @OpenForgeText index.wording.selection
    internal static string ExplicitSelection(string sources) => $"Selection: {sources}";

    internal static string RecoveryRetained(string path) => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheRecoveryBundleWasRetainedAt(path);

    internal static string UnknownSource(string operand) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.UnknownSource(operand);

    // @OpenForgeText index.wording.is-a-folder-not-a-source
    internal static string FolderSource(string operand) => $"{operand} is a folder, not a source.";

    // @OpenForgeText index.wording.open-forge-index
    internal static string CorrectedSource(string id) => $"open-forge index {id}";

    // @OpenForgeText index.wording.the-routes-under-could-not-be-resolved-to-one-structure
    internal static string TopologyAmbiguous(string source, string reason) => $"The routes under {source} could not be resolved to one structure: {reason}.";

    // @OpenForgeText index.wording.is-routed-but-no-parent-lists-it-so-its-entry-has-nowhere-to-go
    internal static string TargetUnexposed(string path) => $"{path} is routed but no parent lists it, so its entry has nowhere to go.";

    // @OpenForgeText index.wording.could-not-be-read-so-the-routes-below-it-are-unknown
    internal static string DiscoveryIncomplete(string path) => $"{path} could not be read, so the routes below it are unknown.";
}
