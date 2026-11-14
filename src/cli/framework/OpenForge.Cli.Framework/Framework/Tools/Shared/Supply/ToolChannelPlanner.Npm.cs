using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;
using OpenForge.Cli.Core.Framework.Tools.Shared.Runtimes;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal sealed partial class ToolChannelPlanner
{
    private async Task<ToolChannelPlanResult?> NpmAsync(ToolChannelRequest request, ToolNpmChannel channel, CancellationToken token)
    {
        var privateExisting = IsPrivateNpm(request);
        IReadOnlyList<ToolPackageManagerFacts> candidates = privateExisting ? [] : ToolPackageManagerPolicy.Candidates(channel, request.AvailableManagers);
        var chosen = ToolPackageManagerPolicy.Default(request, candidates);
        if (!privateExisting && request.Existing.PackageManager is { } responsible)
        {
            candidates = candidates.Where(manager => manager.Client == responsible.Client && manager.GlobalBinPath is not null
                && ToolSupplyPaths.Comparer.Equals(manager.GlobalBinPath, responsible.GlobalBinPath)).ToArray();
            if (candidates.Count == 0) return Blocked("The existing installation's manager or global prefix no longer meets its prerequisites. Keep that installation and change its prerequisites first.");
            chosen = responsible.Client;
        }
        else if (!privateExisting && request.Existing.Version is not null)
            return Blocked("The existing npm installation has no verified responsible manager. Keep it before choosing another installation.");
        if (chosen is null && candidates.Count > 0) return new(null, candidates, null, [], false);
        var manager = candidates.FirstOrDefault(candidate => candidate.Client == chosen);
        ToolPrivateNodeSpec? node = null;
        if (manager is null)
        {
            if (channel.RequiredClient is not null and not ToolNpmClient.Npm) return null;
            if (channel.Requirements.Runtime is not { Runtime: ToolNpmPackageReader.NodeRuntime } requirement) return null;
            node = request.AvailableNodeRuntimes.Where(runtime => ToolPrivateNodeInstaller.IsReviewed(runtime) && ToolRuntimeRange.Satisfies(runtime.Version, requirement.VersionRange)
                && ToolTargetPolicy.Supports(runtime.Archive.Target, request.Target)).OrderByDescending(runtime => ToolVersionService.ReadStable(runtime.Version)).FirstOrDefault();
            if (node is null) return null;
        }
        var metadataClient = manager;
        if (metadataClient is null && privateExisting) metadataClient = request.Existing.PackageManager;
        metadataClient ??= request.AvailableManagers.FirstOrDefault(candidate => candidate.Client != ToolNpmClient.YarnModern);
        var network = false;
        string? version = request.Selection.ExactVersion;
        var relativeEntry = channel.EntryRelativePath;
        string? publishedNodeRange = null;
        if (request.Selection.Kind != ToolVersionSelectionKind.Exact)
        {
            if (!request.AllowLatestLookup) return Blocked("Resolving the latest supported npm version needs a disclosed registry lookup.");
            network = true;
            IReadOnlyList<string> versions;
            ToolNpmMetadataResult? metadata = null;
            if (metadataClient is null)
            {
                metadata = await downloads.ReadNpmMetadataAsync(new(channel, request.WorkspacePath), token).ConfigureAwait(false);
                if (metadata.Failure is not null) return Blocked(metadata.Failure, network);
                versions = metadata.Versions.Select(value => value.Version).ToArray();
            }
            else versions = await new ToolPackageManagerQueries(processes).VersionsAsync(metadataClient, channel.Package, request.WorkspacePath, token).ConfigureAwait(false);
            version = versions.Where(value => _versions.Satisfies(value, request.Selection) && ToolVersionService.MatchProfile(request.Entry, value) is not null)
                .OrderByDescending(ToolVersionService.ReadStable).FirstOrDefault();
            if (metadata?.Versions.FirstOrDefault(value => value.Version == version) is { } published)
            {
                relativeEntry = published.EntryRelativePath;
                publishedNodeRange = published.NodeRange;
            }
        }
        if (version is null || ToolVersionService.MatchProfile(request.Entry, version) is null) return Blocked("No published stable npm version meets the reviewed profile and selector.", network);
        if (version == request.Existing.Version) return Reuse(request) with { UsedNetwork = network };
        if (publishedNodeRange is not null)
        {
            if (manager is not null && !ToolRuntimeRange.Satisfies(manager.RuntimeVersion, publishedNodeRange))
                return Blocked("The selected manager's Node doesn't meet the published version's runtime requirement.", network);
            if (node is not null && channel.Requirements.Runtime is { } runtimeRequirement)
            {
                node = request.AvailableNodeRuntimes.Where(runtime => ToolPrivateNodeInstaller.IsReviewed(runtime)
                    && ToolRuntimeRange.Satisfies(runtime.Version, publishedNodeRange)
                    && ToolRuntimeRange.Satisfies(runtime.Version, runtimeRequirement.VersionRange)
                    && ToolTargetPolicy.Supports(runtime.Archive.Target, request.Target))
                    .OrderByDescending(runtime => ToolVersionService.ReadStable(runtime.Version)).FirstOrDefault();
                if (node is null) return Blocked("No approved private Node meets both the channel and published version's runtime bounds.", network);
            }
        }
        if (relativeEntry.StartsWith("./", StringComparison.Ordinal)) relativeEntry = relativeEntry[2..];
        if (!PortableWorkspacePath.TryNormalize(relativeEntry, out var entryPath))
            return Blocked("The package entry isn't a contained package-relative path.", network);
        return await NpmExecutionAsync(new() { Request = request, Channel = channel, Version = version, Entry = entryPath, Manager = manager, Node = node, UsedNetwork = network }, token).ConfigureAwait(false);
    }

    private static bool IsPrivateNpm(ToolChannelRequest request)
    {
        if (request.Existing.RecordedState is not { PlacedByOpenForge: true, ToolPath: { } tool, RuntimePath: { } runtime } prior
            || prior.Version != request.Existing.Version || request.Existing.PackageManager is not { Client: ToolNpmClient.Npm } manager
            || !ToolSupplyPaths.Comparer.Equals(runtime, manager.RuntimePath)) return false;
        return ToolSupplyPaths.Comparer.Equals(tool, ToolSupplyPaths.Tool(request.HomePath, request.Entry.Identity.Id, prior.Version))
            && PhysicalContainment.Contains(ToolSupplyPaths.Under(request.HomePath, ToolRecordDefinitions.RuntimesDirectoryName, ToolRecordDefinitions.NodeDirectoryName), runtime);
    }

    private async Task<ToolChannelPlanResult> NpmExecutionAsync(ToolNpmPlanInput input, CancellationToken token)
    {
        var request = input.Request;
        var channel = input.Channel;
        var version = input.Version;
        var relativeEntry = input.Entry;
        var manager = input.Manager;
        var node = input.Node;
        var network = input.UsedNetwork;
        var bin = ToolSupplyPaths.Bin(request.HomePath);
        var commands = new List<ToolProcessRequest>();
        var downloads = new List<ToolDownloadRequest>();
        var extractions = new List<ToolExtractionRequest>();
        var launchers = new List<ToolLauncherSpec>();
        var toolRoot = ToolSupplyPaths.Tool(request.HomePath, request.Entry.Identity.Id, version);
        string runtime;
        string entry;
        string commandPath;
        string nativeDirectory;
        if (manager is not null)
        {
            runtime = manager.RuntimePath ?? throw new IOException("The manager has no verified Node.");
            var modules = await new ToolPackageManagerQueries(processes).RootAsync(manager, request.WorkspacePath, token).ConfigureAwait(false)
                ?? throw new IOException("The manager's global root changed.");
            entry = System.IO.Path.Combine(modules, channel.Package.Replace('/', System.IO.Path.DirectorySeparatorChar), relativeEntry.Replace('/', System.IO.Path.DirectorySeparatorChar));
            nativeDirectory = manager.GlobalBinPath ?? throw new IOException("The manager has no global bin.");
            commandPath = System.IO.Path.Combine(nativeDirectory, request.Entry.Detection.Command + (request.Target.OperatingSystem == ToolOperatingSystem.Windows ? ".cmd" : ""));
            commands.Add(ToolPackageManagerDetector.Request(manager.Invocation, ToolPackageManagerPolicy.InstallArguments(manager.Client, channel.Package, version), request.WorkspacePath)
                with
            { Timeout = TimeSpan.FromMinutes(5), MaximumOutputBytes = 1024 * 1024 });
        }
        else
        {
            if (node is null) throw new IOException("The private runtime wasn't selected.");
            var root = ToolSupplyPaths.Runtime(request.HomePath, node.Version);
            runtime = ToolSupplyPaths.Under(root, node.Archive.EntryRelativePath);
            var npm = ToolSupplyPaths.Under(root, node.NpmRelativePath);
            var observed = await records.ReadAsync(new(request.WorkspacePath, request.HomePath), token).ConfigureAwait(false);
            if (observed.RuntimeRecordsState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable) return Blocked("The private runtime records couldn't be checked.", network);
            if (Directory.Exists(root))
            {
                if (!observed.Runtimes.TryGetValue(node.Version, out var prior) || !ToolPrivateNodeInstaller.Matches(node, prior, request.HomePath))
                    return Blocked("The existing private runtime lacks matching signed-checksum receipts.", network);
            }
            else
            {
                var archive = root + ".download";
                if (File.Exists(archive)) return Blocked("A prior private-runtime download occupies the planned path.", network);
                downloads.Add(new(node.Archive, archive));
                extractions.Add(new(node.Archive, archive, root, ToolArchivePreparation.OmitLinks));
            }
            if (Directory.Exists(toolRoot)) return Blocked("The private package prefix already exists. Check it before retrying.", network);
            entry = ToolSupplyPaths.Under(PackageRoot(toolRoot, channel.Package, request.Target), relativeEntry);
            commandPath = ToolSupplyPaths.Under(bin, request.Entry.Detection.Command + (request.Target.OperatingSystem == ToolOperatingSystem.Windows ? ".cmd" : ""));
            nativeDirectory = bin;
            var runtimeInvocation = new ToolCommandTarget { ExecutablePath = runtime, PrefixArguments = [] };
            commands.Add(ToolPackageManagerDetector.Request(runtimeInvocation, ["--version"], request.WorkspacePath));
            var npmInvocation = new ToolCommandTarget { ExecutablePath = runtime, PrefixArguments = [npm] };
            commands.Add(ToolPackageManagerDetector.Request(npmInvocation, ["--version"], request.WorkspacePath));
            var basePath = environment.FirstOrDefault(pair => pair.Key.Equals("PATH", StringComparison.OrdinalIgnoreCase)).Value;
            var nodeDirectory = System.IO.Path.GetDirectoryName(runtime) ?? throw new IOException("The runtime has no directory.");
            commands.Add(ToolPackageManagerDetector.Request(npmInvocation, ["install", "--global", "--prefix", toolRoot, $"{channel.Package}@{version}"], request.WorkspacePath)
                with
            { Environment = new Dictionary<string, string?> { ["PATH"] = $"{nodeDirectory}{System.IO.Path.PathSeparator}{basePath}" }, Timeout = TimeSpan.FromMinutes(5), MaximumOutputBytes = 1024 * 1024 });
            launchers.Add(new()
            {
                ToolId = request.Entry.Identity.Id,
                Kind = LauncherKind(request.Target, native: false),
                TargetKind = ToolLauncherTargetKind.RuntimeEntry,
                LauncherPath = commandPath,
                RuntimePath = runtime,
                EntryPath = entry,
                Arguments = []
            });
            if (ToolSupplyPaths.PreflightFile(commandPath) is { } conflict) return Blocked(conflict, network);
        }
        var invocation = new ToolCommandTarget { ExecutablePath = runtime, PrefixArguments = [entry] };
        commands.Add(Probe(request, invocation));
        return new(new()
        {
            ToolId = request.Entry.Identity.Id,
            ChannelId = channel.Id,
            Version = version,
            ReuseExisting = false,
            Existing = request.Existing,
            PackageManager = manager,
            PrivateNode = node,
            Effects = Effects(request, version),
            Execution = new()
            {
                HomePath = request.HomePath,
                WorkspacePath = request.WorkspacePath,
                BinPath = bin,
                NativeInstallDirectory = nativeDirectory,
                CommandPath = commandPath,
                Invocation = invocation,
                RuntimePath = runtime,
                ToolPath = manager is null ? toolRoot : null,
                Downloads = downloads,
                Extractions = extractions,
                Commands = commands,
                Launchers = launchers
            },
        }, [], manager?.Client, [], network);
    }

    private static string PackageRoot(string prefix, string package, ToolTarget target)
    {
        if (target.OperatingSystem != ToolOperatingSystem.Windows) prefix = System.IO.Path.Combine(prefix, "lib");
        return System.IO.Path.Combine(prefix, ToolNpmPackageReader.ModulesDirectory, package.Replace('/', System.IO.Path.DirectorySeparatorChar));
    }
}
