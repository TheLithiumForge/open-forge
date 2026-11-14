namespace OpenForge.Cli.OutputText.Tool.Shared.Help;

internal static class ToolHelpText
{
    internal static string GroupSyntax() => "open-forge tool <command> [options]";
    internal static string GroupExamples() => "open-forge tool list\nopen-forge tool inspect codegraph\nopen-forge tool install codegraph@1.6 --dry-run";
    internal static string GroupBehavior() => """
        List the catalogue, then inspect a tool's channels, native effects and requirements.
        Install and Update run the tool's native setup and add the declared Open Forge changes.
        Status checks this machine and project. Remove cleans up the recorded project additions.
        """;
    internal static string ListSyntax() => "open-forge tool list [options]";
    internal static string ListExamples() => "open-forge tool list\nopen-forge tool list --format json";
    internal static string ListBehavior() => """
        Lists catalogue entries and any project declarations. Changes no files.
        Use Inspect for channels, requirements and native effects before installation.
        """;
    internal static string InspectSyntax() => "open-forge tool inspect <id> [options]";
    internal static string InspectExamples() => "open-forge tool inspect codegraph\nopen-forge tool inspect codebase-memory --format json";
    internal static string InspectBehavior() => """
        Shows requirements, setup commands, possible native outputs and Open Forge additions.
        Reports installed versions, PATH readiness and stored native settings separately from inherited overrides.
        Changes no files. An unavailable installation can produce a warning.
        """;
    internal static string StatusSyntax() => "open-forge tool status [<id>] [options]";
    internal static string StatusExamples() => "open-forge tool status\nopen-forge tool status codegraph --format json";
    internal static string StatusBehavior() => """
        Checks declared or locked tools, or one named catalogue entry. Changes no files.
        Compares the lock with this machine and checks recorded project additions and native settings.
        Restart terminals and agent apps after PATH changes, then check again.
        """;
}
