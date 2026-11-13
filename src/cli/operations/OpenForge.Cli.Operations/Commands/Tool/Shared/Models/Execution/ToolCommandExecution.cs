using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Execution;

internal sealed record ToolCommandExecution(ToolRunRecord Record, ToolProcessRequest[] Commands);
internal sealed record ToolCommandExecutionResult(ToolRunRecord Record, int CompletedCommands, string? PublicationCause)
{
    internal bool Succeeded => Record.State == ToolCommandState.Exited && PublicationCause is null;
}
internal delegate ValueTask ToolCommandJournal(ToolRunRecord record, CancellationToken token);
