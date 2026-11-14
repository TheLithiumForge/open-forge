using OpenForge.Cli.Composition.Shared.Tools.Path;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.Core.Framework.Tools.Shared.Runtimes;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Composition.Shared.Tools.Npm;

namespace OpenForge.Cli.Composition.Shared.Tools;

internal static class ToolHostFactory
{
    internal static IToolVersionService CreateVersions() => new ToolVersionService();
    internal static IToolProcessHost CreateProcesses(IReadOnlyDictionary<string, string?>? environment = null, ToolCatalogue? catalogue = null) => new ToolProcessHost(environment, catalogue);
    internal static IToolDownloadHost CreateDownloads(string resolvedUserHome, IReadOnlyDictionary<string, string?> environment)
        => new ToolHttpTransfer(new ToolNpmMetadataHost(new()
        {
            UserHome = resolvedUserHome,
            Variables = environment,
            DefaultProxy = HttpClient.DefaultProxy,
            AdditionalConfigPaths = ToolNpmMetadataPolicy.GlobalConfigPaths(resolvedUserHome, environment)
        }));
    internal static IToolChannelService CreateChannels(IToolProcessHost processes, IToolDownloadHost downloads, IToolBackupService backups,
        IToolRecordService records, IReadOnlyDictionary<string, string?> environment) => new ToolChannelService(processes, downloads, backups, records, environment);
    internal static IReadOnlyList<ToolPrivateNodeSpec> CreateNodeRuntimes() => ToolNodeRuntimeCatalogue.All;
    internal static Task<ToolTarget> ObserveTargetAsync(IToolProcessHost processes, string workspace, CancellationToken cancellationToken)
        => ToolTargetObserver.ObserveAsync(processes, workspace, cancellationToken);
    internal static IToolPathHost CreatePath(string resolvedShellHome, IToolBackupService backups, Func<bool> hasLiveConsent)
        => new ToolPathHost(new ToolUserPathEnvironment(), resolvedShellHome, backups, hasLiveConsent);
}
