using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Retry;

internal static class RouteRetryCommand
{
    internal static string FromArguments(IEnumerable<string> arguments)
        => CliRetryCommand.Build(arguments);

    internal static CliNextAction? Complete(CliNextAction? next, string command, string requestCommand, bool confirmationRequired = false)
    {
        if (next is null)
        {
            return null;
        }
        if (confirmationRequired)
        {
            return new CliNextAction($"{requestCommand} --automatic", next.Reason) { Kind = next.Kind };
        }
        return next.Command == command || next.Command == $"{command} --detail debug"
            ? new CliNextAction(requestCommand, next.Reason) { Kind = next.Kind }
            : next;
    }

    internal static string Build(List<string> arguments, CliWorkspace workspace, bool dryRun)
    {
        if (workspace.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace)
        {
            Add(arguments, "--workspace", workspace.LexicalRoot);
        }
        if (dryRun)
        {
            arguments.Add("--dry-run");
        }
        return FromArguments(arguments);
    }

    internal static void Add(List<string> arguments, string flag, string? value)
        => CliRetryCommand.Add(arguments, flag, value);
}
