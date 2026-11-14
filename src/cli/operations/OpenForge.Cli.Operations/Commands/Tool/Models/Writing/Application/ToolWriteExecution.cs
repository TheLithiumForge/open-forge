using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;

internal sealed class ToolWriteExecution(ToolRecordsResult records)
{
    internal ToolRecordsResult Records { get; set; } = records;
    internal List<ToolWriteFinding> Findings { get; } = [];
    internal List<ToolWriteOutcome> Outcomes { get; } = [];
    internal List<ToolBackupCompletionRequest> Backups { get; } = [];
    internal List<(string ToolId, ToolWriteCommandView Command)> ManualCommands { get; } = [];
    internal List<string> PathInstructions { get; } = [];
    internal ToolWriteGrantView? Grants { get; set; }
    internal bool Stopped { get; set; }
}
