namespace OpenForge.Cli.OutputText.Export;

internal static class ExportText
{
    internal static string HelpSyntax() => "open-forge export <destination> [--strip <parts>]... [--flatten <folder>] [--remove-edited <path>]... [--dry-run]";
    internal static string HelpExamples() => """
        open-forge export ../plain-docs --dry-run
        open-forge export ../stage-4 --strip metadata,entrypoints
        open-forge export ../stage-3 --strip metadata,entrypoints,links --flatten docs
        """;
    internal static string HelpBehavior() => """
        Export plans exactly what uninstall would do and writes the result to a new
        or empty folder. The project itself never changes. Open Forge's own files and
        state are left out, and every other file is copied: Git's tracked and
        untracked files that are not ignored, or everything except .git without Git.
        Prefer a folder outside the project. Preview with --dry-run. Export asks no
        question and keeps no recovery data. When writing stops part way, delete the
        destination and run the command again.
        """;
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelated()
        => [("open-forge uninstall --help", "Apply the same plan to this workspace")];

    internal static string Preview(int files, string destination) => files == 1 ? $"Would write 1 file to {destination}." : $"Would write {files} files to {destination}.";
    internal static string Done(int files, string destination) => files == 1 ? $"Wrote 1 file to {destination}." : $"Wrote {files} files to {destination}.";
}
