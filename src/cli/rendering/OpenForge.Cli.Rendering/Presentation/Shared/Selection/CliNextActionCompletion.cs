using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Shared.Selection;

// Complete command-owned actions using the parsed request, before either output format is selected.
internal static class CliNextActionCompletion
{
    internal static CliReport<TData> Complete<TData>(CliReport<TData> report, CliRequestReplay? replay) where TData : class
        => report with
        {
            Next = Complete(report.Next, report.Workspace, replay),
            Findings = report.Findings.Select(finding => finding with
            {
                Actions = finding.Actions.Select(action => Complete(action, report.Workspace, replay) ?? action).ToArray(),
            }).ToArray(),
        };

    private static CliNextAction? Complete(CliNextAction? next, CliWorkspaceEcho? workspace, CliRequestReplay? replay)
    {
        if (next is null || next.Kind != CliNextActionKind.Command) return next;
        var command = next.Command;
        if (command.EndsWith(" --help", StringComparison.Ordinal)) return next;
        var replaysRequest = false;
        if (replay is not null)
        {
            var selectedCommand = command;
            if (next.UsesInteractiveRequest && replay.InteractiveCommand is { } interactive) command = interactive;
            else if (next.AppliesRequest && selectedCommand == $"{replay.Command} --automatic" && replay.AutomaticApplyCommand is { } automaticApply) command = automaticApply;
            else if (next.AppliesRequest && replay.ApplyCommand is { } apply) command = apply;
            else if (command == replay.Command || command == replay.SubjectCommand) command = replay.RequestCommand;
            else if (command == $"{replay.Command} --detail debug" || command == $"{replay.SubjectCommand} --detail debug") command = replay.DebugCommand;
            else if (command == $"{replay.Command} --automatic" || command == $"{replay.SubjectCommand} --automatic") command = replay.AutomaticCommand;
            else if ((command == $"{replay.Command} --prune --dry-run" || command == $"{replay.SubjectCommand} --prune --dry-run")
                && replay.PrunePreviewCommand is { } preview) command = preview;
            else if (command == $"{replay.SubjectCommand} --for <path>") command = $"{replay.RequestCommand} --for <path>";
            else if (command.StartsWith($"{replay.Command} ", StringComparison.Ordinal)
                && command.EndsWith(" --for <path>", StringComparison.Ordinal)
                && !command[..^13].Contains(" --", StringComparison.Ordinal) && replay.OptionArguments.Length > 0)
                command = $"{command[..^13]} {replay.OptionArguments} --for <path>";
            else if (command.StartsWith($"{replay.Command} ", StringComparison.Ordinal)
                && !command.Contains(" --", StringComparison.Ordinal) && replay.OptionArguments.Length > 0)
                command = $"{command} {replay.OptionArguments}";
            replaysRequest = command != selectedCommand || command == replay.RequestCommand || command == replay.DebugCommand
                || command == replay.AutomaticCommand || command == replay.PrunePreviewCommand || command == replay.ApplyCommand;
        }
        var action = command == next.Command ? next : new CliNextAction(command, next.Reason) { Kind = next.Kind };
        if (replaysRequest && replay?.WorkspaceSpecified == true) return action;
        if (command.Contains(" --workspace ", StringComparison.Ordinal)
            || command.Contains(" --workspace=", StringComparison.Ordinal)
            || command.Contains(" --workspace:", StringComparison.Ordinal)) return action;
        return CliRetryCommand.InWorkspace(action, workspace?.Path, workspace?.Explicit == true);
    }
}
