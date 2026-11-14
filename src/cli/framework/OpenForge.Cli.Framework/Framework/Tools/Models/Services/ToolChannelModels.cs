using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal sealed record ToolDetectionResult
{
    public required string ToolId { get; init; }
    public string? CommandPath { get; init; }
    public ToolCommandTarget? Invocation { get; init; }
    public string? Version { get; init; }
    public string? ChannelId { get; init; }
    public string? RuntimePath { get; init; }
    public ToolMachineState? RecordedState { get; init; }
    public ToolPackageManagerFacts? PackageManager { get; init; }
    public string? Failure { get; init; }
}

internal sealed record ToolChannelRequest
{
    public required ToolCatalogueEntry Entry { get; init; }
    public required string WorkspacePath { get; init; }
    public required string HomePath { get; init; }
    public required ToolVersionSelection Selection { get; init; }
    public required ToolDetectionResult Existing { get; init; }
    public required ToolTarget Target { get; init; }
    public required IReadOnlyList<ToolNpmClient> ProjectManagers { get; init; }
    public required ToolManagerChoiceMode ManagerChoice { get; init; }
    public required bool AllowLatestLookup { get; init; }
    public required IReadOnlyList<ToolPackageManagerFacts> AvailableManagers { get; init; }
    public required IReadOnlyList<ToolPrivateNodeSpec> AvailableNodeRuntimes { get; init; }
    public string? ChannelOverride { get; init; }
    public ToolNpmClient? ChosenManager { get; init; }
}

internal sealed record ToolChannelResolution
{
    public required string ToolId { get; init; }
    public required string ChannelId { get; init; }
    public required string Version { get; init; }
    public required bool ReuseExisting { get; init; }
    public ToolDetectionResult? Existing { get; init; }
    public required ToolChannelExecutionPlan Execution { get; init; }
    public ToolPackageManagerFacts? PackageManager { get; init; }
    public ToolPrivateNodeSpec? PrivateNode { get; init; }
    public ToolReleaseAsset? Asset { get; init; }
    public required IReadOnlyList<ToolDeclaredEffect> Effects { get; init; }
}

internal sealed record ToolChannelExecutionPlan
{
    public required string HomePath { get; init; }
    public required string WorkspacePath { get; init; }
    public required string BinPath { get; init; }
    public required string NativeInstallDirectory { get; init; }
    public required string CommandPath { get; init; }
    public required ToolCommandTarget Invocation { get; init; }
    public string? ToolPath { get; init; }
    public string? RuntimePath { get; init; }
    public required IReadOnlyList<ToolDownloadRequest> Downloads { get; init; }
    public required IReadOnlyList<ToolExtractionRequest> Extractions { get; init; }
    public required IReadOnlyList<ToolProcessRequest> Commands { get; init; }
    public required IReadOnlyList<ToolLauncherSpec> Launchers { get; init; }
}

internal sealed record ToolChannelInstallRequest(ToolChannelResolution Plan);
internal sealed record ToolChannelInstallResult(ToolMachineState? State, IReadOnlyList<string> CompletedEffects, string? Failure)
{
    public required IReadOnlyList<ToolBackupCompletionRequest> PendingBackups { get; init; }
    public required IReadOnlyList<ToolNodeRuntimeRecord> Runtimes { get; init; }
}
