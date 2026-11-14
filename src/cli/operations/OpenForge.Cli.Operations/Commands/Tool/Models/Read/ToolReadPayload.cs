namespace OpenForge.Cli.Core.Commands.Tool.Models.Read;

internal sealed record ToolReadPayload(IReadOnlyList<ToolReadTool> Tools);
internal sealed record ToolReadTool
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Origin { get; init; }
    public required string Upstream { get; init; }
    public required string Licence { get; init; }
    public string? DeclaredRange { get; init; }
    public string? LockedVersion { get; init; }
    public string? DetectedVersion { get; init; }
    public string? DetectionFailure { get; init; }
    public string? Channel { get; init; }
    public string? State { get; init; }
    public string? MachineState { get; init; }
    public string? NativeRecordState { get; init; }
    public string? CommandPath { get; init; }
    public string? ToolPath { get; init; }
    public string? RuntimePath { get; init; }
    public string? BinPath { get; init; }
    public string? BinReadiness { get; init; }
    public string? ManagerGlobalBinPath { get; init; }
    public string? ManagerGlobalBinReadiness { get; init; }
    public required IReadOnlyList<ToolReadChannel> Channels { get; init; }
    public required IReadOnlyList<ToolReadProfile> Profiles { get; init; }
    public IReadOnlyList<ToolReadManager> Managers { get; init; } = [];
    public IReadOnlyList<ToolReadChange> Changes { get; init; } = [];
    public IReadOnlyList<ToolReadChange> Removals { get; init; } = [];
    public IReadOnlyList<ToolReadNativeSetting> NativeSettings { get; init; } = [];
    public IReadOnlyList<string> Issues { get; init; } = [];
}
internal sealed record ToolReadFinding(string Code, string? Id, string Cause, string? NextCommand);
internal sealed record ToolReadNativeSetting(string ChangeId, string NativeHomePath, string? Previous, string? Applied, IReadOnlyList<string> ManualChangeArguments);
