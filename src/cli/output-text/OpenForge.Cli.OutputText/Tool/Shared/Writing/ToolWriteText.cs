namespace OpenForge.Cli.OutputText.Tool.Shared.Writing;

internal static class ToolWriteText
{
    internal static string Completed(string command, bool preview, int count)
        => preview ? $"Planned {command} for {count} {ToolCountLabel(count)}." : $"Completed {command} for {count} {ToolCountLabel(count)}.";
    internal static string ToolCountLabel(int count) => count == 1 ? "tool" : "tools";
    internal static string EnvironmentChange(string key, string? value) => value is null ? $"Environment: remove {key}" : $"Environment: {key}={value}";
    internal static string NothingToDo(string command) => $"Nothing to change for {command}.";
    internal static string CannotStart(string command) => $"Cannot start {command}.";
    internal static string Incomplete(string command) => $"{command} has unfinished work.";
    internal static string Failed(string command) => $"{command} stopped after a failure.";
    internal static string Cancelled(string command) => $"{command} was cancelled.";
    internal static string Warnings(string command) => $"{command} completed with warnings.";
    internal static string PlanQuestion() => "Apply the shown tool setup plan, including downloads and native effects?";
    internal static string AccountQuestion(string? id)
        => $"Run {id}'s account-wide uninstall? It removes its binary and all agent entries. The declared native PATH changes also apply.";
    internal static string DeleteQuestion(string? id) => $"Delete the shown shared files for {id}? Other projects can use them.";
    internal static string PathQuestion() => "Remove the recorded Open Forge PATH entries after the last tool?";
    internal static string Keep() => "Keep these account or shared effects unchanged";
    internal static string Apply() => "Apply this separately confirmed change";
    internal static string Manager(string id) => $"Choose the global package manager for {id}.";
    internal static string PathChoice(string bin) => $"Choose PATH setup for {bin}.";
    internal static string ManualPath() => "I'll do it myself";
    internal static string AddPath() => "Add it for me";
    internal static string ShellFiles() => "Choose the shell files to change.";
    internal static string CustomShellFile() => "Choose a custom shell file";
    internal static string CustomShellPath() => "Enter the absolute path to your custom shell file.";
    internal static string AbsolutePathRule() => "Enter one absolute file path.";
    internal static string ShellSyntax() => "Choose the custom file's shell syntax.";
    internal static string PosixShell() => "POSIX";
    internal static string FishShell() => "fish";
    internal static string Network() => "Latest-version resolution used the network.";
    internal static string Grant(string path, string state) => $"Workspace path grant {path}: {state}. This settings edit is separate from tool consent.";
    internal static string GrantOnly(bool preview) => preview ? "Planned workspace path grants." : "Saved workspace path grants.";
    internal static string EffectiveNative(string variable, string value) => $"Effective native setting from {variable}: {value}.";
    internal static string Replacement() => "This replaces an account-wide installation used by other projects.";
    internal static string KeptSetting(string id, string home, string? value)
        => $"Kept account setting {id} at {home}: {value}. Its manual change command is shown below.";
    internal static string Restart() => "Restart terminals and agent apps after changing PATH. Restarted apps haven't been verified.";
    internal static string Row(string name, string? value) => $"{name}: {value ?? "unavailable"}";
    internal static string Tool(string id, string? version, string? channel) => $"{id}@{version ?? "unresolved"} ({channel ?? "unresolved channel"})";
    internal static string Outcome(string id, string stage, string state) => $"{id}: {stage} {state}";
    internal static string GuardedBackup(string target, string backup, string? hash)
        => $"Kept backup {backup}. Before restoring it to {target}, check that the current file still matches {hash ?? "the reported current state"}.";
    internal static string GuardedGit(string target, string objectId)
        => $"Git keeps the original {target} in {objectId}. Check the current file, then read those bytes with git show {objectId} before restoring it manually.";
    internal static string Finding(string code) => code switch
    {
        "invalid-input" => "Check the tool spec and options.",
        "records-unavailable" => "Tool records could not be read.",
        "unknown-tool" => "Choose a known tool ID.",
        "channel-invalid" => "Choose an advertised channel for a named tool.",
        "update-requires-record" => "Install or declare the tool before updating it.",
        "channel-blocked" => "The installation channel is blocked.",
        "profile-unavailable" => "The exact version has no supported profile.",
        "open-forge-version-unsupported" => "The running Open Forge version is below the profile's required version or cannot be compared.",
        "native-plan-blocked" => "The native setup plan is blocked.",
        "config-conflict" => "A recorded configuration change conflicts with current data.",
        "missing-consent" => "Review the plan in a terminal or pass --yes.",
        "manager-required" => "Choose a compatible package manager.",
        "native-failed" => "The native command failed.",
        "verification-failed" => "The resulting tool state could not be verified.",
        "config-failed" => "Configuration stopped after a failure.",
        "record-failed" => "Verified progress could not be saved.",
        "backup-failed" => "A backup needs manual attention.",
        "expected-state-changed" => "Files changed after planning. Review a fresh plan.",
        "permission-required" => "Grant the shown project paths before applying.",
        "interrupted" => "The tool operation was cancelled.",
        "unexpected-failure" => "The tool operation could not finish.",
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };
}
