namespace OpenForge.Cli.OutputText.Tool.Update;

internal static class ToolUpdateText
{
    internal static string Syntax() => "open-forge tool update [<id[@version]>] [--channel <channel>] [--yes] [--allow-path <path>...] [--dry-run]";
    internal static string Examples() => "open-forge tool update codegraph --dry-run\nopen-forge tool update codegraph --yes\nopen-forge tool update --dry-run";
    internal static string Behavior() => """
        Without a name, updates declared tools to newest support. A named tool must be declared or locked.
        Use ID@major.minor or ID@exact stable SemVer. --channel selects an advertised channel for a named tool.
        --dry-run checks the plan. Latest lookup can use the network. A verified current version needs no reinstall.
        --yes accepts the plan and downloads. PATH edits and shared deletion need separate live choices.
        An account-wide version replacement can affect other projects.
        """;
}
