namespace OpenForge.Cli.OutputText.Tool.Update;

internal static class ToolUpdateText
{
    internal static string Syntax() => "open-forge tool update [<id[@version]>] [--channel <channel>] [--yes] [--allow-path <path>...] [--dry-run]";
    internal static string Examples() => """
        open-forge tool update <id>@1.2
        open-forge tool update --dry-run
        """;
    internal static string Behavior() => """
        Review the complete plan before applying. --yes accepts downloads and declared native setup.
        PATH changes and shared file deletion require separate live choices.
        Without a name, update declared tools to newest support. A named tool must be declared or locked.
        """;
}
