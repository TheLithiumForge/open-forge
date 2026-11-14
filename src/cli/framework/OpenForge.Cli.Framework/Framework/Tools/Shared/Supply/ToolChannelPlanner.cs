using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal sealed partial class ToolChannelPlanner(IToolProcessHost processes, IToolDownloadHost downloads, IToolRecordService records, IReadOnlyDictionary<string, string?> environment)
{
    private readonly ToolVersionService _versions = new();
    internal async Task<ToolChannelPlanResult> PlanAsync(ToolChannelRequest request, CancellationToken token)
    {
        try
        {
            if (request.Entry.Channels.Count == 0) return Blocked("The tool has no published channel.");
            if (request.ChannelOverride is not null && !request.Entry.Channels.Any(channel => channel.Id == request.ChannelOverride)) return Blocked("The requested channel isn't advertised.");
            if (request.Existing.Failure is not null) return Blocked(request.Existing.Failure);
            var existing = request.Existing;
            if (existing.CommandPath is not null && existing.Version is null) return Blocked("The existing installation needs a verified version before replacement.");
            if (existing.Version is { } installed)
            {
                if (ToolVersionService.MatchProfile(request.Entry, installed) is null) return Blocked("The existing installation is outside catalogue support. Keep it and choose a manual native change.");
                if (request.Selection.Kind == ToolVersionSelectionKind.Exact && _versions.Satisfies(installed, request.Selection)
                    && (request.ChannelOverride is null || request.ChannelOverride == existing.ChannelId))
                    return Reuse(request);
                if (request.ChannelOverride is { } replacement && existing.ChannelId is { } original && replacement != original)
                    return Blocked("Changing this installed tool's channel needs native removal first. A second installation won't be created.");
                if (existing.ChannelId is null)
                    return Blocked("The existing installation's source is unknown. Keep it and identify its native update command before replacement.");
            }
            if (request.Selection.Kind == ToolVersionSelectionKind.Exact && (request.Selection.ExactVersion is not { } exact || ToolVersionService.MatchProfile(request.Entry, exact) is null))
                return Blocked("The exact version is outside the reviewed profile or below its floor.");
            var channels = request.Entry.Channels.Where(channel => request.ChannelOverride is null || channel.Id == request.ChannelOverride).ToArray();
            if (existing.ChannelId is { } responsible) channels = channels.Where(channel => channel.Id == responsible).ToArray();
            foreach (var channel in channels.OfType<ToolNpmChannel>())
            {
                if (!ToolTargetPolicy.Supports(channel.Requirements, request.Target)) continue;
                var planned = await NpmAsync(request, channel, token).ConfigureAwait(false);
                if (planned is not null) return planned;
            }
            foreach (var channel in channels.OfType<ToolGitHubReleaseChannel>())
            {
                if (!ToolTargetPolicy.Supports(channel.Requirements, request.Target)) continue;
                var asset = channel.Assets.Where(asset => ToolTargetPolicy.Supports(asset.Target, request.Target) && _versions.Satisfies(asset.Version, request.Selection)
                    && ToolVersionService.MatchProfile(request.Entry, asset.Version) is not null).OrderByDescending(asset => ToolVersionService.ReadStable(asset.Version)).FirstOrDefault();
                if (asset is null) continue;
                if (asset.Version == request.Existing.Version) return Reuse(request);
                if (!ToolArtifactIntegrity.IsSha256(asset.Sha256)) return Blocked("This direct asset needs a pinned SHA-256. Attestation-only verification isn't available.");
                return GitHub(request, channel, asset);
            }
            return Blocked("No advertised channel meets the version, runtime and platform requirements.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or System.Text.Json.JsonException)
        {
            return Blocked("Channel planning couldn't verify its paths or native package-manager facts.");
        }
    }

    private static ToolChannelPlanResult Reuse(ToolChannelRequest request)
    {
        var existing = request.Existing;
        if (existing.CommandPath is null || existing.Invocation is null || existing.Version is null) return Blocked("The existing installation lacks an absolute invocation.");
        var channel = existing.ChannelId;
        if (channel is null && request.Entry.Channels.Count == 1) channel = request.Entry.Channels[0].Id;
        if (channel is null) return Blocked("The existing installation's published channel couldn't be established.");
        var bin = ToolSupplyPaths.Bin(request.HomePath);
        return new(new()
        {
            ToolId = request.Entry.Identity.Id,
            Version = existing.Version,
            ChannelId = channel,
            ReuseExisting = true,
            Existing = existing,
            PackageManager = existing.PackageManager,
            Effects = [],
            Execution = new()
            {
                HomePath = request.HomePath,
                WorkspacePath = request.WorkspacePath,
                BinPath = bin,
                NativeInstallDirectory = System.IO.Path.GetDirectoryName(existing.CommandPath) ?? bin,
                CommandPath = existing.CommandPath,
                Invocation = existing.Invocation,
                RuntimePath = existing.RuntimePath,
                ToolPath = existing.RecordedState?.ToolPath,
                Downloads = [],
                Extractions = [],
                Commands = [Probe(request, existing.Invocation)],
                Launchers = [],
            },
        }, [], existing.PackageManager?.Client, [], false);
    }

    private ToolChannelPlanResult GitHub(ToolChannelRequest request, ToolGitHubReleaseChannel channel, ToolReleaseAsset asset)
    {
        var root = ToolSupplyPaths.Tool(request.HomePath, request.Entry.Identity.Id, asset.Version);
        if (Directory.Exists(root)) return Blocked("The version's tool directory already exists without matching active installation facts. Check it before retrying.");
        var archive = root + ".download";
        if (File.Exists(archive)) return Blocked("A prior download occupies the planned destination. Check it before retrying.");
        var bin = ToolSupplyPaths.Bin(request.HomePath);
        var entry = ToolSupplyPaths.Under(root, asset.EntryRelativePath);
        ToolCommandTarget invocation = new() { ExecutablePath = entry, PrefixArguments = [] };
        string? runtime = null;
        if (asset.BundledRuntime is { } bundled)
        {
            runtime = ToolSupplyPaths.Under(root, bundled.RuntimeRelativePath);
            invocation = new() { ExecutablePath = runtime, PrefixArguments = [.. bundled.Arguments, ToolSupplyPaths.Under(root, bundled.EntryRelativePath)] };
        }
        var extension = "";
        if (request.Target.OperatingSystem == ToolOperatingSystem.Windows) extension = asset.BundledRuntime is null ? ".exe" : ".cmd";
        var launcherPath = ToolSupplyPaths.Under(bin, request.Entry.Detection.Command + extension);
        var launcher = new ToolLauncherSpec
        {
            ToolId = request.Entry.Identity.Id,
            LauncherPath = launcherPath,
            EntryPath = entry,
            Kind = LauncherKind(request.Target, asset.BundledRuntime is null),
            TargetKind = asset.BundledRuntime is null ? ToolLauncherTargetKind.NativeBinary : ToolLauncherTargetKind.ReleaseLauncher,
            Arguments = [],
        };
        if (ToolSupplyPaths.PreflightFile(launcherPath) is { } problem) return Blocked(problem);
        if (asset.BundledRuntime is null) invocation = new() { ExecutablePath = launcherPath, PrefixArguments = [] };
        return new(new()
        {
            ToolId = request.Entry.Identity.Id,
            ChannelId = channel.Id,
            Version = asset.Version,
            ReuseExisting = false,
            Existing = request.Existing,
            Asset = asset,
            Effects = Effects(request, asset.Version),
            Execution = new()
            {
                HomePath = request.HomePath,
                WorkspacePath = request.WorkspacePath,
                BinPath = bin,
                NativeInstallDirectory = bin,
                CommandPath = launcherPath,
                Invocation = invocation,
                RuntimePath = runtime,
                ToolPath = root,
                Downloads = [new(asset, archive)],
                Extractions = [new(asset, archive, root, ToolArchivePreparation.Original)],
                Launchers = [launcher],
                Commands = [Probe(request, invocation)],
            },
        }, [], null, [], false);
    }

    private static ToolLauncherKind LauncherKind(ToolTarget target, bool native)
    {
        if (native) return ToolLauncherKind.NativeExecutable;
        return target.OperatingSystem == ToolOperatingSystem.Windows ? ToolLauncherKind.WindowsCmd : ToolLauncherKind.PosixSh;
    }

    private static ToolProcessRequest Probe(ToolChannelRequest request, ToolCommandTarget invocation)
        => ToolPackageManagerDetector.Request(invocation, request.Entry.Detection.Arguments, request.WorkspacePath) with
        {
            Environment = request.Entry.Detection.Environment,
            Timeout = request.Entry.Detection.Timeout,
            MaximumOutputBytes = request.Entry.Detection.MaximumOutputBytes,
        };

    private static IReadOnlyList<ToolDeclaredEffect> Effects(ToolChannelRequest request, string version)
        => [new() { Id = "published-installation", Scope = ToolEffectScope.Account, Kind = ToolDeclaredEffectKind.Configuration,
            Description = $"Install {request.Entry.Identity.Name} {version} for this account." }, .. ToolVersionService.MatchProfile(request.Entry, version)?.Setup.Effects ?? []];

    private static ToolChannelPlanResult Blocked(string problem, bool network = false) => new(null, [], null, [problem], network);
}
