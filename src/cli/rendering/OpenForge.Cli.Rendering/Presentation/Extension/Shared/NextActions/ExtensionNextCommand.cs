using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Extension.Shared.NextActions;

internal static class ExtensionNextCommand
{
    private static readonly string[] SourceOperations = ["install", "update", "inspect", "list"];
    internal static CliNextAction? InSource(CliNextAction? next, string? source)
    {
        if (next is not { Kind: CliNextActionKind.Command } || source is null || next.Command.EndsWith(" --help", StringComparison.Ordinal)
            || next.Command.Contains(" --source ", StringComparison.Ordinal)) return next;
        var usesSource = SourceOperations
            .Any(operation => next.Command == $"open-forge extension {operation}"
                || next.Command.StartsWith($"open-forge extension {operation} ", StringComparison.Ordinal));
        return usesSource ? new CliNextAction($"{next.Command} --source {CliCommandText.Quote(source)}", next.Reason) { Kind = next.Kind } : next;
    }

    internal static CliFinding InSource(CliFinding finding, string? source)
        => finding with { Actions = finding.Actions.Select(action => InSource(action, source) ?? action).ToArray() };
}
