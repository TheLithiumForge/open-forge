using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Route.Shared.NextActions;

internal static class RouteNextCommand
{
    internal static CliNextAction? InWorkspace(CliNextAction? next, string? workspace, bool explicitWorkspace)
    {
        if (next is null || !explicitWorkspace || workspace is null || next.Kind != CliNextActionKind.Command)
        {
            return next;
        }
        var needsWorkspace = next.Command is "open-forge doctor" or "open-forge cleanup" or "open-forge cleanup --dry-run" or "open-forge update"
            or "open-forge route list --depth=all" or "open-forge find --tag Template"
            || next.Command.StartsWith("open-forge route init ", StringComparison.Ordinal)
            || next.Command.StartsWith("open-forge extension remove ", StringComparison.Ordinal);
        return needsWorkspace
            ? CliRetryCommand.InWorkspace(next, workspace, explicitWorkspace)
            : next;
    }
}
