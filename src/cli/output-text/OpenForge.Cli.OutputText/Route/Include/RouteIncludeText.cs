namespace OpenForge.Cli.OutputText.Route.Include;

internal static class RouteIncludeText
{
    internal static string HelpSyntax() => "open-forge route include <workspace-file> [--description <text>] [--tag <tag>]... [--apply-to <glob>]... [--dry-run]";
    internal static string HelpExamples() => """
        open-forge route include CONTRIBUTING.md --description "How to contribute"
        open-forge route include docs/team/_team.md --description "Team context" --dry-run
        open-forge route include .agents/patterns/_patterns.md
        """;
    internal static string HelpBehavior() => """
        List an existing text file where it lives. The loader row controls when it loads.
        An omitted field keeps its current value. An identical request changes nothing.
        Preview with --dry-run. Otherwise apply after checks, without a prompt.
        The target file keeps its bytes.
        """;
    internal static string HelpBoundary() => """
        Use a workspace-relative file, with / or \ and an optional leading ./.
        Use a catalogue instead of a directory. Globs, URLs and fragments are invalid.
        Paths must stay inside the workspace, including through links.
        Git metadata, the loader, settings and lock files are not valid targets.
        Inside .agents, use a direct child folder's catalogue.
        Change that catalogue's frontmatter to change its description, tags or conditions.
        """;
    internal static string HelpMetadata() => """
        Supply --description for a new row outside .agents.
        Repeat --tag in the order to keep, with or without #. No tags means on demand.
        Repeat --apply-to for conditions using workspace-relative globs.
        A supplied option replaces only its own field. Omitted fields stay as written.
        To clear tags or conditions, edit the row or exclude and include again.
        A file's own frontmatter never sets its loading.
        """;
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelated() => [("open-forge route exclude --help", "Stop listing a file"), ("open-forge index --help", "Refresh generated navigation")];
    internal static string NothingChanged(string path) => $"Nothing changed for '{path}'.";
    internal static string Preview(string path) => $"Would include '{path}'.";
    internal static string Done(string path) => $"Included '{path}'.";
    internal static string Failure(string cause) => cause;
    internal static string Before(string row) => $"Before: {row}";
    internal static string After(string row) => $"After: {row}";
    internal static string NoRow() => "(no row)";
    internal static string DryRun() => "No files were changed.";
}
