namespace OpenForge.Cli.OutputText.Tool.Remove;

internal static class ToolRemoveText
{
    internal static string Syntax() => "open-forge tool remove <id> [options]";
    internal static string Examples() => "open-forge tool remove codegraph --dry-run\nopen-forge tool remove codegraph --yes\nopen-forge tool remove codebase-memory --yes";
    internal static string Behavior() => """
        Removes the project's declaration, lock entry and recorded Open Forge additions. CodeGraph's own project uninstall runs first.
        Codebase Memory removal stays local. Afterwards an interactive run asks whether to run its own uninstaller, which removes
        it system-wide from its versioned tool folder and every agent app. Open Forge's bin stays on PATH. The default keeps it.
        With --yes or no terminal, that uninstaller never runs and its command is printed.
        Project removal keeps account settings, including telemetry off, and prints manual change commands.
        Shared file deletion needs a separate live choice. Remove never edits PATH.
        After the last tool in bin, it prints the PATH entry or marked shell block to remove. Changes aren't rolled back automatically.
        """;
}
