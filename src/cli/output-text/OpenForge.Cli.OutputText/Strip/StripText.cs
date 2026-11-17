namespace OpenForge.Cli.OutputText.Strip;

internal static class StripText
{
    internal static string HelpSyntax() => "open-forge strip [<part>...] [--flatten <folder>] [--dry-run] [--automatic]";
    internal static string HelpExamples() => """
        open-forge strip links --dry-run
        open-forge strip metadata,entrypoints --automatic
        open-forge strip --flatten docs --dry-run --format json
        """;
    internal static string HelpBehavior() => """
        Strip edits the Markdown files below the main root, the native Skills folder
        and each registered outside route. Files Open Forge manages stay unchanged,
        so the Framework keeps working. Every other byte stays exactly as it was.
        Preview with --dry-run. Applying asks first unless you add --automatic.
        Redirected or JSON requests that would write need --automatic.
        Recovery data is kept until every change verifies, and an interrupted
        run reports it.
        """;
    internal static string HelpParts() => "metadata removes Open Forge frontmatter keys and keeps other keys and the body. frontmatter removes the whole frontmatter block and wins over metadata. entrypoints deletes the entrypoint files, with their Entries lists and any text you wrote in them. links turns links to documents into their text and removes the whole comment around an @open-forge or @rune comment link. External links, images, links to other files and code stay. Source code is never read.";
    internal static string HelpFlatten() => "Use it on a category, subcategory or custom route. A folder that holds any file Open Forge manages is refused, unless entrypoints deletes that file in the same run. SKILL.md files stay where they are, and so do entrypoints that are not deleted. Kept links are rewritten so they still work. A name two files would share stops the run before any write.";
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelated()
        => [("open-forge uninstall --help", "Remove Open Forge and strip what stays"), ("open-forge index --help", "Refresh generated navigation")];

    internal static string Preview(int changes) => changes == 1 ? "Would change 1 file." : $"Would change {changes} files.";
    internal static string Done(int changes) => changes == 1 ? "Changed 1 file." : $"Changed {changes} files.";
    internal static string NothingToDo() => "Nothing to do.";
    internal static string LabelFilesDeleted(decimal count) => count == 1 ? "file deleted" : "files deleted";
    internal static string LabelFilesRewritten(decimal count) => count == 1 ? "file rewritten" : "files rewritten";
    internal static string LabelFilesMoved(decimal count) => count == 1 ? "file moved" : "files moved";
    internal static string LabelFoldersDeleted(decimal count) => count == 1 ? "folder deleted" : "folders deleted";
    internal static string LabelFilesKept(decimal count) => count == 1 ? "edited file kept" : "edited files kept";
    internal static string Failure(string cause) => cause;
    internal static string FindingTitle(string code) => code switch
    {
        "invalid-input" => "Input is invalid",
        "workspace-unavailable" => "Workspace cannot be read",
        "document-unsupported" => "Document cannot be edited safely",
        "flatten-collision" => "Flattened names collide",
        "linked-file-skipped" => "Link left unchanged",
        "target-changed" => "File changed after the preview",
        "flatten-managed" => "Flatten folder holds managed files",
        "target-changed-during-apply" => "File changed while writing",
        "confirmation-required" => "Confirmation is required",
        "permission-declined" => "Plan was not confirmed",
        "recovery-unavailable" => "Recovery data is unavailable",
        "write-failed" => "A planned change failed",
        "recovery-artifact-retained" => "Recovery bundle was kept",
        "interrupted" => "Run was cancelled",
        "ownership-unavailable" => "Ownership lock cannot be read",
        "library-attached" => "A Library is attached",
        "target-unavailable" => "Managed file cannot be compared",
        "edited-kept" => "Edited file stays",
        "runtime-retained" => "Runtime folder stays",
        _ => "Strip finding",
    };
    internal static string KeptHeading() => "Kept:";
    internal static string CollisionsHeading() => "Collisions:";
    internal static string LinkedHeading() => "Skipped links:";
    internal static string NoFilesChanged() => "No files were changed.";
    internal static string PlanLine(string action, string path, string? to, string detail)
        => to is null ? $"  {action,-16} {path}  ({detail})" : $"  {action,-16} {path} -> {to}  ({detail})";
    internal static string HashLine(string? before, string? after)
        => $"      before {before ?? "-"}  after {after ?? "-"}";
    internal static string KeptLine(string path, string reason) => $"  {path}  ({reason})";
    internal static string CollisionLine(string path, string sources) => $"  {path} <- {sources}";
}
