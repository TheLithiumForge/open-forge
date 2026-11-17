using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal sealed class ToolChannelService : IToolChannelService
{
    private readonly IToolProcessHost _processes;
    private readonly ToolPackageManagerDetector _managers;
    private readonly ToolChannelPlanner _planner;
    private readonly ToolChannelInstaller _installer;
    private readonly ToolManagedFilesRemoval _removal;

    internal ToolChannelService(IToolProcessHost processes, IToolDownloadHost downloads, IToolBackupService backups,
        IToolRecordService records, IReadOnlyDictionary<string, string?> environment)
    {
        _processes = processes;
        _managers = new(processes, environment);
        _planner = new(processes, downloads, records, environment);
        _installer = new(processes, downloads, backups, records);
        _removal = new(records);
    }

    public async Task<ToolDetectionResult> DetectAsync(ToolDetectionRequest request, CancellationToken cancellationToken)
    {
        var result = new ToolDetectionResult { ToolId = request.Entry.Identity.Id };
        try
        {
            var recorded = request.RecordedState;
            if (recorded is not null && recorded.ToolId != result.ToolId) return result with { Failure = "The recorded installation belongs to another tool." };
            ToolResolvedCommand? command = null;
            var unsupported = false;
            if (recorded is not null && File.Exists(recorded.CommandPath)) command = new(recorded.CommandPath, recorded.Invocation);
            else
            {
                try { command = await _processes.ResolveAsync(request.Entry.Detection.Command, request.WorkingDirectory, cancellationToken).ConfigureAwait(false); }
                catch (NotSupportedException) { unsupported = true; }
            }
            ToolPackageManagerFacts? observedManager = null;
            IReadOnlyList<ToolPackageManagerFacts>? managers = null;
            if (command is null)
            {
                managers = await _managers.DetectAsync(request.WorkingDirectory, cancellationToken).ConfigureAwait(false);
                foreach (var manager in managers)
                {
                    if (manager.RuntimePath is null || manager.GlobalBinPath is null) continue;
                    var modules = await new ToolPackageManagerQueries(_processes).RootAsync(manager, request.WorkingDirectory, cancellationToken).ConfigureAwait(false);
                    if (modules is null) continue;
                    foreach (var channel in request.Entry.Channels.OfType<ToolNpmChannel>())
                    {
                        var package = System.IO.Path.Combine(modules, channel.Package.Replace('/', System.IO.Path.DirectorySeparatorChar));
                        if (ToolNpmPackageReader.Read(package, request.Entry.Detection.Command) is not { } manifest || !File.Exists(manifest.Entry)) continue;
                        var path = System.IO.Path.Combine(manager.GlobalBinPath, request.Entry.Detection.Command + (OperatingSystem.IsWindows() ? ".cmd" : ""));
                        if (!File.Exists(path)) continue;
                        command = new(path, new() { ExecutablePath = manager.RuntimePath, PrefixArguments = [manifest.Entry] });
                        observedManager = manager;
                        result = result with { ChannelId = channel.Id };
                        break;
                    }
                    if (command is not null) break;
                }
            }
            if (command is null) return unsupported ? result with { Failure = "An existing shell launcher couldn't be resolved to its declared executable. Keep it before choosing another installation." } : result;
            var probe = ToolPackageManagerDetector.Request(command.Invocation, request.Entry.Detection.Arguments, request.WorkingDirectory) with
            {
                Environment = request.Entry.Detection.Environment,
                Timeout = request.Entry.Detection.Timeout,
                MaximumOutputBytes = request.Entry.Detection.MaximumOutputBytes,
            };
            var process = await _processes.RunAsync(probe, cancellationToken).ConfigureAwait(false);
            var version = ToolPackageManagerDetector.Succeeded(process) ? ToolVersionService.ReadProbe(process.StandardOutput) : null;
            result = result with { CommandPath = command.CommandPath, Invocation = command.Invocation, Version = version };
            if (version is null) return result with { Failure = "The existing tool didn't return one bounded stable version." };
            if (recorded is not null && ToolSupplyPaths.Comparer.Equals(command.CommandPath, recorded.CommandPath))
                return result with { RecordedState = recorded, ChannelId = recorded.ChannelId, PackageManager = recorded.PackageManager, RuntimePath = recorded.RuntimePath };
            if (observedManager is not null) return result with { PackageManager = observedManager, RuntimePath = observedManager.RuntimePath };
            managers ??= await _managers.DetectAsync(request.WorkingDirectory, cancellationToken).ConfigureAwait(false);
            foreach (var manager in managers)
            {
                if (manager.GlobalBinPath is null || !ToolSupplyPaths.Comparer.Equals(System.IO.Path.GetDirectoryName(command.CommandPath), manager.GlobalBinPath)) continue;
                result = result with
                {
                    PackageManager = manager,
                    RuntimePath = manager.RuntimePath,
                    ChannelId = request.Entry.Channels.OfType<ToolNpmChannel>().FirstOrDefault()?.Id
                };
                break;
            }
            if (result.ChannelId is null) result = IdentifyRelease(request.Entry, result);
            if (result.ChannelId is null && request.Entry.Channels.Count == 1) result = result with { ChannelId = request.Entry.Channels[0].Id };
            return result;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or System.Text.Json.JsonException)
        {
            return result with { Failure = "The existing tool installation couldn't be checked." };
        }
    }

    private static ToolDetectionResult IdentifyRelease(ToolCatalogueEntry entry, ToolDetectionResult detection)
    {
        if (detection.Invocation is not { } invocation) return detection;
        foreach (var channel in entry.Channels.OfType<ToolGitHubReleaseChannel>())
            foreach (var asset in channel.Assets)
            {
                if (asset.BundledRuntime is not { } bundled || invocation.PrefixArguments.Count != bundled.Arguments.Count + 1
                    || !invocation.PrefixArguments.Take(bundled.Arguments.Count).SequenceEqual(bundled.Arguments)) continue;
                var script = invocation.PrefixArguments[^1];
                var relative = bundled.EntryRelativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
                if (!script.EndsWith(System.IO.Path.DirectorySeparatorChar + relative, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) continue;
                var root = script[..^relative.Length];
                var runtime = System.IO.Path.Combine(root, bundled.RuntimeRelativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
                if (ToolSupplyPaths.Comparer.Equals(runtime, invocation.ExecutablePath)) return detection with { ChannelId = channel.Id, RuntimePath = runtime };
            }
        return detection;
    }

    public Task<IReadOnlyList<ToolPackageManagerFacts>> DetectManagersAsync(string workspacePath, CancellationToken cancellationToken)
        => _managers.DetectAsync(workspacePath, cancellationToken);
    public Task<ToolChannelPlanResult> PlanAsync(ToolChannelRequest request, CancellationToken cancellationToken) => _planner.PlanAsync(request, cancellationToken);
    public Task<ToolChannelInstallResult> InstallAsync(ToolChannelInstallRequest request, CancellationToken cancellationToken) => _installer.InstallAsync(request.Plan, cancellationToken);
    public Task<ToolManagedFilesRemoveResult> RemoveAsync(ToolManagedFilesRemoveRequest request, CancellationToken cancellationToken) => _removal.RemoveAsync(request, cancellationToken);
}
