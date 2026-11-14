namespace OpenForge.Cli.OutputText.Tool.Install;

internal static class ToolInstallText
{
    internal static string Syntax() => "open-forge tool install [<id[@version]>] [--channel <channel>] [--yes] [--allow-path <path>...] [--dry-run]";
    internal static string Examples() => "open-forge tool install codegraph@1.6 --dry-run\nopen-forge tool install codegraph@1.6 --yes\nopen-forge tool install";
    internal static string Behavior() => """
        Without a name, restores exact lock versions for teammates. A named bare ID selects latest support.
        Use ID@major.minor or ID@exact stable SemVer. --channel selects an advertised channel for a named tool.
        Review native effects and Open Forge additions with --dry-run. Latest lookup can use the network.
        --yes accepts the plan and downloads. PATH edits and shared deletion need separate live choices.
        Native setup keeps its MCP entries, hooks, permissions, instruction blocks and Skills.
        CodeGraph telemetry is turned off. Codebase Memory uses default account discovery and watcher settings.
        """;
}
