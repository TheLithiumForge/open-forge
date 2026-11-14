namespace OpenForge.Cli.OutputText.Tool.Install;

internal static class ToolInstallText
{
    internal static string Syntax() => "open-forge tool install [<id[@version]>] [--channel <channel>] [--yes] [--allow-path <path>...] [--dry-run]";
    internal static string Examples() => """
        open-forge tool install <id>@1.2
        open-forge tool install --dry-run
        """;
    internal static string Behavior() => """
        Review the complete plan before applying. --yes accepts downloads and declared native setup.
        PATH changes and shared file deletion require separate live choices.
        Without a name, restore exact locked versions. A named bare ID selects latest support.
        """;
}
