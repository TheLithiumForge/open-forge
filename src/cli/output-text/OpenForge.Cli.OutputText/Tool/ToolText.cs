using System.Globalization;

namespace OpenForge.Cli.OutputText.Tool;

internal static class ToolText
{
    // @OpenForgeText tool.help.group-syntax
    internal static string GroupSyntax() => "open-forge tool <command> [options]";

    // @OpenForgeText tool.help.group-examples
    internal static string GroupExamples() => "open-forge tool list\nopen-forge tool install codegraph --dry-run\nopen-forge tool install";

    // @OpenForgeText tool.help.group-behavior
    internal static string GroupBehavior() => """
        List shows the supported tools, how each one installs and whether this project uses it.
        Install runs the tool's own install and setup commands, then adds Open Forge's small changes. It shows every command and asks first.
        To update a tool, use its own update command or install it again.
        """;

    // @OpenForgeText tool.help.list-syntax
    internal static string ListSyntax() => "open-forge tool list [options]";

    // @OpenForgeText tool.help.list-examples
    internal static string ListExamples() => "open-forge tool list\nopen-forge tool list --format json";

    // @OpenForgeText tool.help.list-behavior
    internal static string ListBehavior() => """
        Shows each supported tool by category, with its install method, whether this project uses it and the version installed on this machine.
        Runs each tool's version command and changes nothing.
        """;

    // @OpenForgeText tool.help.install-syntax
    internal static string InstallSyntax() => "open-forge tool install [<id>] [--yes] [--dry-run] [options]";

    // @OpenForgeText tool.help.install-examples
    internal static string InstallExamples() => "open-forge tool install codegraph\nopen-forge tool install codegraph --dry-run\nopen-forge tool install";

    // @OpenForgeText tool.help.install-behavior
    internal static string InstallBehavior() => """
        With an ID, installs the tool when it's missing, runs its own project setup and adds Open Forge's changes.
        Without an ID, installs the tools this project uses that are missing on this machine. Use it after cloning a project.
        Shows the exact commands and changes, then asks. Checks npm or the install script's runner first and says what to install when one is missing.
        To update a tool, use its own update command or install it again.
        """;

    // @OpenForgeText tool.help.remove-syntax
    internal static string RemoveSyntax() => "open-forge tool remove <id> [--yes] [--dry-run] [options]";

    // @OpenForgeText tool.help.remove-examples
    internal static string RemoveExamples() => "open-forge tool remove codegraph\nopen-forge tool remove codegraph --dry-run";

    // @OpenForgeText tool.help.remove-behavior
    internal static string RemoveBehavior() => """
        Removes Open Forge's changes for the tool and takes it out of this project's tools.
        Then shows what the tool's own uninstall does and asks whether to run it too. The default is no, and --yes keeps the tool installed.
        """;

    internal static string RelatedLeaf(string leaf) => $"tool {leaf} --help";

    // @OpenForgeText tool.list.headline
    internal static string ListHeadline(int count) => count == 1 ? "Found 1 supported tool." : $"Found {count} supported tools.";

    // @OpenForgeText tool.list.state
    internal static string State(bool configured, bool installed, string? version)
    {
        var install = installed ? version is null ? "installed" : $"installed {version}" : "not installed";
        return configured ? $"configured, {install}" : install;
    }

    // @OpenForgeText tool.list.details
    internal static string Details(string name, string licence, string upstream) => $"{name}, {licence}, {upstream}";

    // @OpenForgeText tool.plan.install-heading
    internal static string InstallHeading(string name, bool preview, bool installed) => (preview, installed) switch
    {
        (true, false) => $"Open Forge will run these commands to install {name}:",
        (true, true) => $"Open Forge will run these commands to set up {name}:",
        (false, _) => $"Open Forge ran these commands for {name}:",
    };

    // @OpenForgeText tool.plan.already-installed
    internal static string AlreadyInstalled(string name, string? version)
        => string.IsNullOrEmpty(version) ? $"{name} is already installed." : $"{name} {version} is already installed.";

    // @OpenForgeText tool.plan.changes-heading
    internal static string ChangesHeading(bool preview, bool removal) => (preview, removal) switch
    {
        (true, false) => "Then Open Forge will make these changes:",
        (false, false) => "Then Open Forge made these changes:",
        (true, true) => "Open Forge will remove these changes:",
        (false, true) => "Open Forge removed these changes:",
    };

    // @OpenForgeText tool.plan.changes-not-made
    internal static string ChangesNotMade() => "Open Forge didn't make these changes:";

    // @OpenForgeText tool.plan.add-tool
    internal static string AddTool(string id) => $"Add {id} to this project's tools";

    // @OpenForgeText tool.plan.remove-tool
    internal static string RemoveTool(string id) => $"Remove {id} from this project's tools";

    // @OpenForgeText tool.plan.uninstall-heading
    internal static string UninstallHeading(string name) => $"{name}'s own uninstall:";

    // @OpenForgeText tool.plan.uninstall-yourself
    internal static string UninstallYourself() => "It wasn't run. To uninstall, run these commands yourself:";

    // @OpenForgeText tool.plan.exit
    internal static string Exit(int? code) => code is { } value
        ? string.Create(CultureInfo.InvariantCulture, $"It exited with code {value}. Its last output:")
        : "It couldn't start or didn't finish in time. Its last output:";

    // @OpenForgeText tool.plan.done
    internal static string Done() => "done";

    // @OpenForgeText tool.plan.failed
    internal static string Failed() => "failed";

    // @OpenForgeText tool.plan.skipped
    internal static string Skipped() => "skipped";

    // @OpenForgeText tool.plan.not-run
    internal static string NotRun() => "not run";

    // @OpenForgeText tool.plan.not-started
    internal static string NotStarted() => "not started";

    // @OpenForgeText tool.plan.question
    internal static string PlanQuestion() => "Continue? [y/N]";

    // @OpenForgeText tool.plan.uninstall-question
    internal static string UninstallQuestion() => "Run it too?";

    // @OpenForgeText tool.plan.no
    internal static string No() => "No, keep it installed";

    // @OpenForgeText tool.plan.yes
    internal static string Yes() => "Yes, run its uninstall";

    // @OpenForgeText tool.write.previewed
    internal static string Previewed(string command) => $"Previewed {command}. Nothing was run or changed.";

    // @OpenForgeText tool.write.review-plan
    internal static string ReviewPlan(string command) => $"Review the {command} plan.";

    // @OpenForgeText tool.write.finished
    internal static string Finished(string command) => $"Finished {command}.";

    // @OpenForgeText tool.write.nothing-to-do
    internal static string NothingToDo(string command) => $"Nothing to do for {command}.";

    // @OpenForgeText tool.write.no-tools
    internal static string NoTools() => "This project has no tools to install. Add one with open-forge tool install <id>.";

    // @OpenForgeText tool.write.failed
    internal static string StoppedAtFailure(string command) => $"{command} stopped at a failure.";

    // @OpenForgeText tool.write.blocked
    internal static string Blocked(string command) => $"{command} can't start yet.";

    // @OpenForgeText tool.write.cancelled
    internal static string Cancelled(string command) => $"{command} was cancelled. Nothing more was run or changed.";

    // @OpenForgeText tool.write.cannot-start
    internal static string CannotStart(string command) => $"{command} couldn't start.";

    // @OpenForgeText tool.finding.npm-missing
    internal static string NpmMissing(string name)
        => $"{name} needs npm, which isn't installed. Install Node.js from https://nodejs.org (npm comes with it), then run open-forge tool install again.";

    // @OpenForgeText tool.finding.powershell-missing
    internal static string PowerShellMissing(string name)
        => $"{name} needs Windows PowerShell, which isn't available. Make sure powershell runs in a terminal, then run open-forge tool install again.";

    // @OpenForgeText tool.finding.program-missing
    internal static string ProgramMissing(string name, string program)
        => $"{name} needs {program}, which isn't installed. Install {program} with your system's package manager, then run open-forge tool install again.";

    // @OpenForgeText tool.finding.command-failed
    internal static string CommandFailed(string command) => $"`{command}` failed.";

    // @OpenForgeText tool.finding.command-failed-next
    internal static string CommandFailedNext() => "Read its output above, fix the problem it names, then run the same Open Forge command again.";

    // @OpenForgeText tool.finding.settings-unavailable
    internal static string SettingsUnavailable(string path, string? cause) => $"{path} can't be used. {cause ?? "It couldn't be read."} Fix it, then run the command again.";

    // @OpenForgeText tool.finding.open-forge-missing
    internal static string OpenForgeMissing() => "This folder doesn't use Open Forge yet. Run open-forge install first, then run this command again.";

    // @OpenForgeText tool.finding.integration-conflict
    internal static string IntegrationConflict(string path, string? cause) => $"{path} can't be changed safely. {cause ?? "It couldn't be read."} Fix it, then run the command again.";

    // @OpenForgeText tool.finding.target-changed
    internal static string TargetChanged() => "A file changed while Open Forge was writing it. Nothing else was changed. Run the command again.";

    // @OpenForgeText tool.finding.write-failed
    internal static string WriteFailed(string? cause) => $"Open Forge couldn't write its changes. {cause ?? string.Empty}".TrimEnd();

    // @OpenForgeText tool.finding.unknown-tool
    internal static string UnknownTool(string id) => $"There is no supported tool called {id}. Run open-forge tool list to see the IDs.";

    // @OpenForgeText tool.finding.invalid-input
    internal static string InvalidInput(string? cause) => cause ?? "Check the command and its options.";

    // @OpenForgeText tool.finding.interrupted
    internal static string Interrupted() => "The command was cancelled.";

    // @OpenForgeText tool.finding.title
    internal static string FindingTitle() => "Check the tool command.";
}
