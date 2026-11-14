using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;
using OpenForge.Cli.Core.Framework.Tools.Shared.Runtimes;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal sealed class ToolChannelInstaller(IToolProcessHost processes, IToolDownloadHost downloads, IToolBackupService backups, IToolRecordService records)
{
    internal async Task<ToolChannelInstallResult> InstallAsync(ToolChannelResolution plan, CancellationToken token)
    {
        var completed = new List<string>();
        var pending = new List<ToolBackupCompletionRequest>();
        var runtimes = new List<ToolNodeRuntimeRecord>();
        var integrity = new List<ToolIntegrityEvidence>();
        ToolMachineState? state = null;
        string? npmVersion = null;
        try
        {
            var execution = plan.Execution;
            if (execution.Commands.Count == 0) return Result("The channel plan has no final version verification.");
            if (plan.PrivateNode is { } sharedNode && execution.Extractions.Count == 0)
            {
                var current = await records.ReadAsync(new(execution.WorkspacePath, execution.HomePath), token).ConfigureAwait(false);
                if (current.RuntimeRecordsState != ToolRecordReadState.Available || current.Problems.Count > 0
                    || !current.Runtimes.TryGetValue(sharedNode.Version, out var receipt)
                    || !ToolPrivateNodeInstaller.Matches(sharedNode, receipt, execution.HomePath))
                    return Result("The shared private runtime receipt changed after planning.");
                var root = ToolSupplyPaths.Runtime(execution.HomePath, sharedNode.Version);
                var nodePath = ToolSupplyPaths.Under(root, sharedNode.Archive.EntryRelativePath);
                var npmPath = ToolSupplyPaths.Under(root, sharedNode.NpmRelativePath);
                ToolSupplyPaths.CheckOrdinary(nodePath);
                ToolSupplyPaths.CheckOrdinary(npmPath);
                if (!File.Exists(nodePath) || !File.Exists(npmPath)) return Result("The shared runtime files changed after planning.");
            }
            if (plan.Existing is { Version: { } oldVersion, Invocation: { } oldInvocation })
            {
                var final = execution.Commands[^1];
                var probeArguments = final.Arguments.Skip(execution.Invocation.PrefixArguments.Count).ToArray();
                var observed = await processes.RunAsync(final with { ExecutablePath = oldInvocation.ExecutablePath, Arguments = [.. oldInvocation.PrefixArguments, .. probeArguments] }, token).ConfigureAwait(false);
                if (!ToolPackageManagerDetector.Succeeded(observed) || ToolVersionService.ReadProbe(observed.StandardOutput) != oldVersion)
                    return Result("The existing installation changed after channel planning.");
            }
            if (plan.ReuseExisting)
            {
                var probe = await processes.RunAsync(execution.Commands[^1], token).ConfigureAwait(false);
                if (!VerifiedVersion(probe, plan.Version)) return Result("The reused installation didn't verify its planned version.");
                var prior = plan.Existing?.RecordedState;
                state = new()
                {
                    SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
                    ToolId = plan.ToolId,
                    Version = plan.Version,
                    ChannelId = plan.ChannelId,
                    CommandPath = execution.CommandPath,
                    Invocation = execution.Invocation,
                    RuntimePath = execution.RuntimePath,
                    ToolPath = execution.ToolPath,
                    PackageManager = plan.PackageManager,
                    PlacedByOpenForge = prior?.PlacedByOpenForge == true,
                    Launchers = prior?.Launchers ?? [],
                    Integrity = prior?.Version == plan.Version ? prior.Integrity : [],
                };
                completed.Add("existing-installation-verified");
                return Result(null);
            }
            foreach (var download in execution.Downloads)
            {
                ToolSupplyPaths.CheckOrdinary(download.DestinationPath);
                var parent = System.IO.Path.GetDirectoryName(download.DestinationPath) ?? throw new IOException("The download has no parent.");
                Directory.CreateDirectory(parent);
                var result = await downloads.DownloadAsync(download, token).ConfigureAwait(false);
                if (result.VerifiedPath != download.DestinationPath || result.Integrity is null) return Result(result.Failure ?? "The planned download didn't verify.");
                integrity.Add(result.Integrity);
                completed.Add(download.DestinationPath);
            }
            foreach (var extraction in execution.Extractions)
            {
                ToolSupplyPaths.CheckOrdinary(extraction.DestinationPath);
                if (Directory.Exists(extraction.DestinationPath)) return Result("An extraction destination appeared after planning.");
                Directory.CreateDirectory(extraction.DestinationPath);
                var prepared = extraction;
                if (extraction.Preparation == ToolArchivePreparation.OmitLinks)
                    prepared = await ToolNodeArchivePreparation.PrepareAsync(extraction, extraction.ArchivePath + ".prepared", token).ConfigureAwait(false);
                var result = await ToolArchiveExtractor.ExtractAsync(prepared, token).ConfigureAwait(false);
                if (result.Failure is not null) return Result(result.Failure);
                completed.Add(extraction.DestinationPath);
            }
            for (var index = 0; index < execution.Commands.Count - 1; index++)
            {
                var command = execution.Commands[index];
                var process = await processes.RunAsync(command, token).ConfigureAwait(false);
                if (!ToolPackageManagerDetector.Succeeded(process)) return Result("A planned native package command failed. The channel won't change and no second installation will run.");
                if (plan.PrivateNode is { } node && index == 0)
                {
                    if (!VerifiedVersion(process, node.Version)) return Result("Private Node didn't verify its exact pinned version.");
                    runtimes.Add(ToolPrivateNodeInstaller.Record(node, execution.HomePath));
                }
                if (plan.PrivateNode is not null && index == 1)
                {
                    npmVersion = ToolVersionService.ReadProbe(process.StandardOutput);
                    if (npmVersion is null) return Result("Private Node's npm didn't return a stable version.");
                }
                completed.Add($"package-command-{index + 1}");
            }
            var written = await new ToolLauncherWriter(backups).WriteAsync(new(execution.Launchers), pending, token).ConfigureAwait(false);
            completed.AddRange(written.WrittenPaths);
            if (written.Failure is not null) return Result(written.Failure);
            var verification = await processes.RunAsync(execution.Commands[^1], token).ConfigureAwait(false);
            if (!VerifiedVersion(verification, plan.Version)) return Result("The installed tool didn't verify the exact planned version. Keep the completed effects and backups.");
            var manager = plan.PackageManager;
            if (plan.PrivateNode is { } runtime)
            {
                if (npmVersion is null || execution.RuntimePath is null || execution.ToolPath is null) return Result("Private npm facts are incomplete.");
                var prefixBin = execution.ToolPath;
                if (runtime.Archive.Target.OperatingSystem != ToolOperatingSystem.Windows) prefixBin = System.IO.Path.Combine(prefixBin, ToolRecordDefinitions.BinDirectoryName);
                manager = new()
                {
                    Client = ToolNpmClient.Npm,
                    Version = npmVersion,
                    RuntimeVersion = runtime.Version,
                    RuntimePath = execution.RuntimePath,
                    Invocation = new() { ExecutablePath = execution.RuntimePath, PrefixArguments = [ToolSupplyPaths.Under(ToolSupplyPaths.Runtime(execution.HomePath, runtime.Version), runtime.NpmRelativePath)] },
                    GlobalBinPath = prefixBin,
                    GlobalBinReadiness = ToolPathReadiness.Missing,
                    SupportsGlobalInstall = true,
                };
                integrity.Add(ToolPrivateNodeInstaller.Record(runtime, execution.HomePath).Integrity);
            }
            if (manager is not null) integrity.Add(new() { Kind = ToolVerificationKind.PackageManager, Source = $"{manager.Client} {manager.Version}: {plan.ToolId}@{plan.Version}" });
            state = new()
            {
                SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
                ToolId = plan.ToolId,
                Version = plan.Version,
                ChannelId = plan.ChannelId,
                CommandPath = execution.CommandPath,
                Invocation = execution.Invocation,
                ToolPath = execution.ToolPath,
                RuntimePath = execution.RuntimePath,
                PlacedByOpenForge = execution.Launchers.Count > 0,
                Launchers = execution.Launchers,
                PackageManager = manager,
                Integrity = integrity,
            };
            completed.Add("installed-version-verified");
            return Result(null);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or OperationCanceledException)
        {
            return Result("Channel installation stopped. Keep the completed effects, downloads and backups for checking.");
        }

        ToolChannelInstallResult Result(string? failure) => new(state, completed, failure) { PendingBackups = pending, Runtimes = runtimes };
    }

    private static bool VerifiedVersion(ToolProcessResult process, string version)
        => ToolPackageManagerDetector.Succeeded(process) && ToolVersionService.ReadProbe(process.StandardOutput) == version;

}
