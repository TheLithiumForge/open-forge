namespace OpenForge.Cli.OutputText.Uninstall;

internal static class UninstallText
{
    internal static string HelpSyntax() => "open-forge uninstall [--strip <parts>]... [--flatten <folder>] [--remove-edited <path>]... [--dry-run] [--automatic]";
    internal static string HelpExamples() => """
        open-forge uninstall --dry-run
        open-forge uninstall --strip metadata,entries,links --automatic
        open-forge uninstall --strip metadata,links --flatten docs --automatic
        """;
    internal static string HelpBehavior() => """
        Uninstall deletes the Framework and Extension files that still match what
        Open Forge installed, its sections in AGENTS.md and CLAUDE.md, the route
        scaffolds it wrote, its settings and lock, and folders left empty.
        Files you wrote stay. Managed files you edited stay unless you name them.
        Preview with --dry-run. Applying asks first unless you add --automatic.
        Redirected or JSON requests that would write need --automatic.
        Recovery data is kept until every change verifies, and an interrupted
        run reports it.
        """;
    internal static string HelpKept() => "Edited managed files and sections are listed as kept. Name one with --remove-edited to remove it anyway. Your own text in AGENTS.md and CLAUDE.md stays, and Git-ignore rules stay. Detach Libraries first.";
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelated()
        => [("open-forge strip --help", "Strip documents without uninstalling"), ("open-forge status --help", "See which managed files you edited")];

    internal static string Preview(int changes) => changes == 1 ? "Would remove Open Forge with 1 change." : $"Would remove Open Forge with {changes} changes.";
    internal static string Done(int changes) => changes == 1 ? "Removed Open Forge with 1 change." : $"Removed Open Forge with {changes} changes.";
    internal static string NothingToDo() => "Nothing to do.";
}
