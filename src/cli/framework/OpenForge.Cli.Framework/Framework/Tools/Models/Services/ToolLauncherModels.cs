using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal sealed record ToolLauncherWriteRequest(IReadOnlyList<ToolLauncherSpec> Launchers);
internal sealed record ToolLauncherWriteResult(IReadOnlyList<string> WrittenPaths, string? Failure);

internal sealed record ToolPathSetupRequest
{
    public required ToolPathFacts Facts { get; init; }
    public required ToolPathChoice Choice { get; init; }
    public required IReadOnlyList<ToolShellFile> ChosenShellFiles { get; init; }
    public ToolPathState? Previous { get; init; }
}

internal sealed record ToolPathSetupResult(ToolPathState? State, IReadOnlyList<string> ManualLines, bool RestartRequired, string? Failure);

internal sealed record ToolPathRemoveRequest(ToolPathState Expected);
internal sealed record ToolPathRemoveResult(IReadOnlyList<string> RemovedTargets, IReadOnlyList<string> KeptTargets, bool RestartRequired, string? Failure);

internal sealed record ToolManagedFilesRemoveRequest
{
    public required ToolMachineState Expected { get; init; }
    public required IReadOnlyList<string> ApprovedToolPaths { get; init; }
    public required IReadOnlyList<ToolNodeRuntimeRecord> ApprovedRuntimes { get; init; }
}

internal sealed record ToolManagedFilesRemoveResult(IReadOnlyList<string> RemovedPaths, IReadOnlyList<string> KeptPaths, string? Failure);
