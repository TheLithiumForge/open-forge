namespace OpenForge.Cli.OutputText.Route.Exclude;

internal static class RouteExcludeText
{
    internal static string HelpSyntax() => "open-forge route exclude <workspace-path> [--dry-run]";
    internal static string HelpExamples() => """
        open-forge route exclude CONTRIBUTING.md
        open-forge route exclude docs/team --dry-run
        open-forge route exclude .agents/patterns/_patterns.md
        """;
    internal static string HelpBehavior() => """
        Remove every loader row for a file and its roots registration.
        For a folder, remove only its outsideRoutes registration and catalogue rows.
        Folder contents and independent file registrations stay in place.
        The target may be missing. An unlisted, unregistered target changes nothing.
        Preview with --dry-run. Otherwise apply after checks, without a prompt.
        Existing target files keep their bytes.
        """;
    internal static string HelpBoundary() => """
        Use a workspace-relative file or folder, with / or \ and an optional leading ./.
        Globs, URLs and fragments are invalid.
        Paths must stay inside the workspace, including through links.
        Git metadata, the loader, settings and lock files are not valid targets.
        Inside the main root, use a direct child folder's catalogue.
        Exclude removes its loader row without deleting the catalogue.
        """;
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelated() => [("open-forge route include --help", "List a file or register an outside folder"), ("open-forge index --help", "Refresh generated navigation")];
    internal static string NothingChanged(string path) => $"Nothing changed for '{path}'.";
    internal static string Preview(string path) => $"Would exclude '{path}'.";
    internal static string Done(string path) => $"Excluded '{path}'.";
    internal static string Failure(string cause) => cause;
    internal static string Catalogue(string path) => $"Catalogue: {path}";
    internal static string Before(string row) => $"Before: {row}";
    internal static string After(string row) => $"After: {row}";
    internal static string NoRow() => "(no row)";
    internal static string DryRun() => "No files were changed.";
}
