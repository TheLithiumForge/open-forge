namespace OpenForge.Cli.OutputText.Tool.Remove;

internal static class ToolRemoveText
{
    internal static string Syntax() => "open-forge tool remove <id> [--yes] [--allow-path <path>...] [--dry-run]";
    internal static string Examples() => """
        open-forge tool remove <id>
        open-forge tool remove <id> --dry-run
        """;
    internal static string Behavior() => """
        Review the complete plan before applying. --yes accepts downloads and declared native setup.
        PATH changes and shared file deletion require separate live choices.
        Account-wide uninstall requires a live choice naming its binary and all agent entries. Otherwise project cleanup continues and the native command is printed. Account settings are kept.
        """;
}
