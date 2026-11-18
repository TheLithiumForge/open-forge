namespace OpenForge.Cli.OutputText.Strip;

// Wording the plain-docs plan shares between uninstall and export.
internal static class StripText
{
    internal static string HelpParts() => "metadata removes Open Forge frontmatter keys and keeps other keys and the body. frontmatter removes the whole frontmatter block and wins over metadata. A SKILL.md keeps its Skill keys under both. entrypoints deletes the entrypoint files, with their Entries lists and any text you wrote in them. links turns links to documents into their text and removes the whole comment around an @open-forge or @rune comment link. External links, images, links to other files and code stay. Source code is never read.";
    internal static string HelpFlatten() => "Use it on a category, subcategory or custom route. SKILL.md files stay where they are, and so do entrypoints that are not deleted. Kept links are rewritten so they still work. A name two files would share stops the run before any write.";
    internal static string LabelFilesDeleted(decimal count) => count == 1 ? "file deleted" : "files deleted";
    internal static string LabelFilesRewritten(decimal count) => count == 1 ? "file rewritten" : "files rewritten";
    internal static string LabelFilesMoved(decimal count) => count == 1 ? "file moved" : "files moved";
    internal static string LabelFoldersDeleted(decimal count) => count == 1 ? "folder deleted" : "folders deleted";
    internal static string LabelFilesKept(decimal count) => count == 1 ? "edited file kept" : "edited files kept";
    internal static string LabelFilesLeftOut(decimal count) => count == 1 ? "file left out" : "files left out";
    internal static string LabelFilesCopied(decimal count) => count == 1 ? "file copied" : "files copied";
    internal static string Failure(string cause) => cause;
    internal static string FindingTitle(string code) => code switch
    {
        "invalid-input" => "Input is invalid",
        "workspace-unavailable" => "Workspace cannot be read",
        "document-unsupported" => "Document cannot be edited safely",
        "flatten-collision" => "Flattened names collide",
        "linked-file-skipped" => "Link left unchanged",
        "target-changed" => "File changed after the preview",
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
        _ => "Plan finding",
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
