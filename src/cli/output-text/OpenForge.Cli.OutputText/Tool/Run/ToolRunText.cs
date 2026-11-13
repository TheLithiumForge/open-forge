namespace OpenForge.Cli.OutputText.Tool.Run;

public static class ToolRunText
{
    public const string MissingTitle = "Declared tool is missing";
    public const string ToolsLabel = "Tool installations";
    public const string InstallNextReason = "Restore the workspace's declared tool pins.";
    public static string MissingMessage(string id) => $"The declared tool {id} isn't installed for this workspace.";
    public const string Usage = "open-forge tool run [--workspace <dir>] <tool> [--] <args...>";
    public const string Examples = "open-forge tool run <tool> --help\nopen-forge tool run --workspace <dir> <tool> -- <args...>";
    public const string Behavior = "Put Open Forge options before the tool ID. Everything after the ID is child input, including --help.\n"
        + "Run selects --workspace, then CLAUDE_PROJECT_DIR, then searches upward from CWD. The child keeps CWD.\n"
        + "Only a declared, installed and verified tool pin can run. Each immutable member must match its recorded size and UTC timestamp.\n"
        + "One delimiter immediately after the ID is consumed. Arguments and raw process streams pass through without a shell or a result envelope.\n"
        + "The child shares the console or foreground process group. Run waits through console interrupts and returns the child's exit.\n"
        + "Parent-only SIGTERM, SIGHUP or verified MCP input-pipe closure waits one second, then stops the running process tree. Run returns 143 for SIGTERM or 129 for hangup.\n"
        + "A declared missing tool requires open-forge tool install. Interactive Run offers normal Install consent. Non-interactive Run never installs silently.";

    public static string Refusal(string suffix, string id, string cause, string? next)
    {
        var diagnostic = $"tool-run.{suffix}: {id}: {cause}";
        return next is null ? diagnostic : $"{diagnostic}\nNext: {next}";
    }
}
