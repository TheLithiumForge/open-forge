using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal sealed record ToolDetectionRequest(ToolCatalogueEntry Entry, string WorkingDirectory);
internal sealed record ToolChannelPlanResult(
    ToolChannelResolution? Resolution,
    IReadOnlyList<ToolPackageManagerFacts> ManagerChoices,
    ToolNpmClient? DefaultManager,
    IReadOnlyList<string> Problems,
    bool UsedNetwork);

internal interface IToolVersionService
{
    ToolSpec? ReadSpec(string value);
    ToolVersionRange? ReadRange(string value);
    bool Satisfies(string version, ToolVersionSelection selection);
}

internal interface IToolChannelService
{
    Task<ToolDetectionResult> DetectAsync(ToolDetectionRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ToolPackageManagerFacts>> DetectManagersAsync(string workspacePath, CancellationToken cancellationToken);
    Task<ToolChannelPlanResult> PlanAsync(ToolChannelRequest request, CancellationToken cancellationToken);
    Task<ToolChannelInstallResult> InstallAsync(ToolChannelInstallRequest request, CancellationToken cancellationToken);
    Task<ToolManagedFilesRemoveResult> RemoveAsync(ToolManagedFilesRemoveRequest request, CancellationToken cancellationToken);
}

internal interface IToolConfigService
{
    Task<ToolConfigPlan> PlanAsync(ToolConfigRequest request, CancellationToken cancellationToken);
    Task<ToolConfigApplyResult> ApplyAsync(ToolConfigPlan plan, CancellationToken cancellationToken);
    Task<ToolConfigRemoveResult> RemoveAsync(ToolConfigRemoveRequest request, CancellationToken cancellationToken);
}

internal interface IToolRecordService
{
    Task<ToolRecordsResult> ReadAsync(ToolRecordsRequest request, CancellationToken cancellationToken);
    Task<ToolRecordWriteResult> WriteAsync(ToolRecordWriteRequest request, CancellationToken cancellationToken);
}

internal interface IToolProcessHost
{
    Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken);
}

internal interface IToolDownloadHost
{
    Task<ToolDownloadResult> DownloadAsync(ToolDownloadRequest request, CancellationToken cancellationToken);
}

internal interface IToolPathHost
{
    Task<ToolPathFacts> ObserveAsync(string homePath, CancellationToken cancellationToken);
    Task<ToolPathSetupResult> AddAsync(ToolPathSetupRequest request, CancellationToken cancellationToken);
    Task<ToolPathRemoveResult> RemoveAsync(ToolPathRemoveRequest request, CancellationToken cancellationToken);
}
