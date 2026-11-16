namespace OpenForge.Cli.OutputText.Strip;

internal static class StripText
{
    internal static string HelpSyntax() => "open-forge strip [<part>...] [--flatten <folder>] [--dry-run] [--automatic]";
    internal static string HelpExamples() => """
        open-forge strip links --dry-run
        open-forge strip metadata,entries --automatic
        open-forge strip --flatten docs --dry-run --format json
        open-forge strip metadata entries links --flatten docs --automatic
        """;
    internal static string HelpBehavior() => """
        Strip edits the Markdown files below the main root, the native Skills folder
        and each registered outside route. The Framework stays installed.
        Every other byte stays exactly as it was.
        Preview with --dry-run. Applying asks first unless you add --automatic.
        """;
    internal static string HelpParts() => """
        metadata  Remove Open Forge frontmatter keys. Other keys and the body stay.
        entries   Remove the generated Entries section. Text you wrote there stays.
        links     Turn links to documents into their text. External links, images,
                  links to other files, code and comment links stay.
        """;
    internal static string HelpFlatten() => """
        --flatten moves each document below the folder's subfolders into the folder.
        Entrypoints and SKILL.md files stay. Kept links are rewritten so they still work.
        A name that two files would share stops the run before any write.
        """;
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelated()
        => [("open-forge uninstall --help", "Remove Open Forge and optionally strip what remains"), ("open-forge index --help", "Refresh generated navigation")];

    internal static string Preview(int changes) => changes == 1 ? "Would change 1 file." : $"Would change {changes} files.";
    internal static string Done(int changes) => changes == 1 ? "Changed 1 file." : $"Changed {changes} files.";
    internal static string NothingToDo() => "Nothing to strip.";
    internal static string Failure(string cause) => cause;
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
