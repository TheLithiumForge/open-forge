using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal enum ToolRecordReadState { Missing, Available, Unavailable, Invalid }

internal sealed record ToolRecordsRequest(string WorkspacePath, string HomePath);

internal sealed record ToolRecordsResult
{
    public required ToolRecordReadState PortableState { get; init; }
    public ToolPortableRecords? Portable { get; init; }
    public required ToolRecordReadState MachineState { get; init; }
    public required IReadOnlyDictionary<string, ToolMachineState> Machine { get; init; }
    public required ToolRecordReadState WorkspaceMachineState { get; init; }
    public ToolWorkspaceMachineState? WorkspaceMachine { get; init; }
    public required ToolRecordReadState RuntimeRecordsState { get; init; }
    public required IReadOnlyDictionary<string, ToolNodeRuntimeRecord> Runtimes { get; init; }
    public required ToolRecordReadState PathRecordState { get; init; }
    public ToolPathState? Path { get; init; }
    public required IReadOnlyList<string> Problems { get; init; }
    public required IReadOnlyDictionary<string, string?> ExpectedFileSha256 { get; init; }
}

internal sealed record ToolRecordWriteRequest
{
    public required ToolRecordsRequest Locations { get; init; }
    public required ToolRecordsResult Expected { get; init; }
    public required ToolPortableRecords Portable { get; init; }
    public required IReadOnlyList<ToolMachineState> Machine { get; init; }
    public ToolWorkspaceMachineState? WorkspaceMachine { get; init; }
    public required IReadOnlyList<ToolNodeRuntimeRecord> Runtimes { get; init; }
    public ToolPathState? Path { get; init; }
}

internal sealed record ToolRecordWriteResult(IReadOnlyList<string> WrittenPaths, IReadOnlyList<string> Unfinished, string? Failure);
