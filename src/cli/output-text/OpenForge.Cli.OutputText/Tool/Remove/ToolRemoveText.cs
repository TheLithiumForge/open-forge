namespace OpenForge.Cli.OutputText.Tool.Remove;

internal static class ToolRemoveText
{
    internal static string Syntax() => "open-forge tool remove <id> [--yes] [--allow-path <path>...] [--dry-run]";
    internal static string Examples() => "open-forge tool remove codegraph --dry-run\nopen-forge tool remove codegraph --yes\nopen-forge tool remove codebase-memory --yes";
    internal static string Behavior() => """
        Removes recorded Open Forge project additions and offers the declared native uninstall.
        Codebase Memory uninstall removes the account binary and all agent entries. It needs a live confirmation naming those effects.
        With --yes, no terminal or a declined account confirmation, only project additions are removed and the native command is printed.
        Project removal keeps account settings, including telemetry off, and prints manual change commands.
        Shared file deletion and PATH edits need separate live choices. Changes aren't rolled back automatically.
        """;
}
