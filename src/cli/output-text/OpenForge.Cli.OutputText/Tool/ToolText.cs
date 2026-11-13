namespace OpenForge.Cli.OutputText.Tool;

public static class ToolText
{
    public static string GroupUsage() => "open-forge tool <command> [options]";
    public static string CommandHelp() => "Run open-forge tool <command> --help for syntax and examples.";
    public static string ListUsage() => "open-forge tool list [--role <role>] [--source <path>] [global flags]";
    public static string InspectUsage() => "open-forge tool inspect <id> [--source <path>] [--tool-version <exact>] [global flags]";
    public static string StatusUsage() => "open-forge tool status [<id>] [global flags]";
    public static string[] ListExamples() => ["open-forge tool list", "open-forge tool list --role code-navigation --format json"];
    public static string[] InspectExamples() => ["open-forge tool inspect example-tool --source ./manifest.json",
        "open-forge tool inspect example-tool --source ./manifest.json --tool-version 1.0.0"];
    public static string[] StatusExamples() => ["open-forge tool status --format json", "open-forge tool status example-tool"];
    public static string[] ReadBehavior() => ["Reads local facts without downloads, child processes or writes.", "Unavailable observations keep their cause and independently readable facts.", "Tool-owned installers handle agent configuration."];
    public static string ListHeadline() => "Read available definitions and recorded tool installs.";
    public static string InspectHeadline() => "Read the selected tool definition and its requirements.";
    public static string StatusHeadline() => "Read passive tool installation and configuration evidence.";
    public static string FindingTitle() => "Tool observation";
    public static string ReadNext() => "Resolve the reported cause, then rerun the same read command.";
    public static string Available() => "Available";
    public static string Installed() => "Installed";
    public static string Empty() => "None recorded.";
    public static string Coverage() => "Coverage";
    public static string Reviewed(bool reviewed) => reviewed ? "reviewed" : "unreviewed";
    public static string Roles() => "Roles";
    public static string Policy() => "Declared process policy";
    public static string Limits() => "Limits";
    public static string DefinitionUnavailable() => "The selected definition is unavailable.";
    public static string InstructionsUnavailable(string cause) => "External instructions unavailable: " + cause;
}
