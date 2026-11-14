using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Path;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;

internal sealed class ToolPackageManagerDetector(IToolProcessHost processes, IReadOnlyDictionary<string, string?> environment)
{
    internal const int OutputLimit = 65536;
    internal static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(10);
    private static readonly (string Command, ToolNpmClient Client)[] Clients = [("npm", ToolNpmClient.Npm), ("pnpm", ToolNpmClient.Pnpm), ("yarn", ToolNpmClient.YarnClassic), ("bun", ToolNpmClient.Bun)];

    internal async Task<IReadOnlyList<ToolPackageManagerFacts>> DetectAsync(string workspace, CancellationToken token)
    {
        var node = await ResolveAsync(ToolNpmPackageReader.NodeRuntime, workspace, token).ConfigureAwait(false);
        string? nodeVersion = null;
        if (node is not null) nodeVersion = await VersionAsync(node.Invocation, workspace, token).ConfigureAwait(false);
        var result = new List<ToolPackageManagerFacts>();
        foreach (var (command, declaredClient) in Clients)
        {
            var resolved = await ResolveAsync(command, workspace, token).ConfigureAwait(false);
            if (resolved is null) continue;
            var version = await VersionAsync(resolved.Invocation, workspace, token).ConfigureAwait(false);
            if (version is null || ToolVersionService.ReadStable(version) is not { } parsed) continue;
            var client = declaredClient;
            if (declaredClient == ToolNpmClient.YarnClassic && parsed.Major >= 2) client = ToolNpmClient.YarnModern;
            var clientNode = node?.Invocation.ExecutablePath;
            var clientNodeVersion = nodeVersion;
            if (resolved.Invocation.PrefixArguments.Count > 0)
            {
                clientNode = resolved.Invocation.ExecutablePath;
                if (!string.Equals(clientNode, node?.Invocation.ExecutablePath, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    clientNodeVersion = await VersionAsync(new() { ExecutablePath = clientNode, PrefixArguments = [] }, workspace, token).ConfigureAwait(false);
            }
            string? bin = null;
            var supports = client != ToolNpmClient.YarnModern;
            if (supports)
            {
                var probe = await RunAsync(resolved.Invocation, ToolPackageManagerPolicy.GlobalBinArguments(client), workspace, token).ConfigureAwait(false);
                var output = probe.StandardOutput.Trim();
                if (Succeeded(probe) && !output.Contains('\n') && System.IO.Path.IsPathFullyQualified(output))
                {
                    bin = output;
                    if (client == ToolNpmClient.Npm && !OperatingSystem.IsWindows()) bin = System.IO.Path.Combine(bin, ToolRecordDefinitions.BinDirectoryName);
                }
                supports = bin is not null;
            }
            result.Add(new()
            {
                Client = client,
                Version = version,
                Invocation = resolved.Invocation,
                RuntimePath = clientNode,
                RuntimeVersion = clientNodeVersion,
                GlobalBinPath = bin,
                SupportsGlobalInstall = supports && CanRunClient(resolved.Invocation, clientNodeVersion),
                GlobalBinReadiness = Readiness(bin),
            });
        }
        return result;
    }

    private ToolPathReadiness Readiness(string? bin)
    {
        if (bin is null) return ToolPathReadiness.Unavailable;
        var path = environment.FirstOrDefault(pair => pair.Key.Equals(ToolPathPolicy.PathVariable, StringComparison.OrdinalIgnoreCase)).Value;
        return ToolPathPolicy.Contains(path, bin, OperatingSystem.IsWindows()) ? ToolPathReadiness.Ready : ToolPathReadiness.Missing;
    }

    private async Task<ToolResolvedCommand?> ResolveAsync(string command, string workspace, CancellationToken token)
    {
        try { return await processes.ResolveAsync(command, workspace, token).ConfigureAwait(false); }
        catch (NotSupportedException) { return null; }
    }

    private static bool CanRunClient(ToolCommandTarget invocation, string? version)
    {
        try { return ClientRuntimeCompatible(invocation, version); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Text.Json.JsonException) { return false; }
    }

    private static bool ClientRuntimeCompatible(ToolCommandTarget invocation, string? version)
    {
        if (invocation.PrefixArguments.Count == 0) return true;
        if (version is null) return false;
        var entry = invocation.PrefixArguments.FirstOrDefault(File.Exists);
        if (entry is null) return false;
        for (var directory = System.IO.Path.GetDirectoryName(entry); directory is not null; directory = System.IO.Path.GetDirectoryName(directory))
        {
            var manifest = System.IO.Path.Combine(directory, ToolNpmPackageReader.ManifestName);
            if (!File.Exists(manifest)) continue;
            if (new FileInfo(manifest).Length > 1024 * 1024) return false;
            using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllBytes(manifest));
            if (!json.RootElement.TryGetProperty("engines", out var engines) || !engines.TryGetProperty("node", out var range) || range.ValueKind != System.Text.Json.JsonValueKind.String) return true;
            var text = range.GetString();
            if (text is null) return false;
            foreach (var alternative in text.Split("||", StringSplitOptions.TrimEntries))
            {
                if (alternative.StartsWith('^') && ToolVersionService.ReadStable(alternative[1..]) is { } lower
                    && ToolRuntimeRange.Satisfies(version, $">={lower} <{lower.Major + 1}")) return true;
                if (ToolRuntimeRange.Satisfies(version, alternative)) return true;
            }
            return false;
        }
        return true;
    }

    internal async Task<string?> VersionAsync(ToolCommandTarget invocation, string workspace, CancellationToken token)
    {
        var result = await RunAsync(invocation, ToolPackageManagerPolicy.VersionArguments, workspace, token).ConfigureAwait(false);
        return Succeeded(result) ? ToolVersionService.ReadProbe(result.StandardOutput) : null;
    }

    internal Task<ToolProcessResult> RunAsync(ToolCommandTarget invocation, IReadOnlyList<string> arguments, string workspace, CancellationToken token)
        => processes.RunAsync(ReadRequest(invocation, arguments, workspace), token);

    internal static ToolProcessRequest ReadRequest(ToolCommandTarget invocation, IReadOnlyList<string> arguments, string workspace)
        => Request(invocation, arguments, workspace) with { Environment = ToolPackageManagerPolicy.ReadEnvironment };

    internal static ToolProcessRequest Request(ToolCommandTarget invocation, IReadOnlyList<string> arguments, string workspace)
        => new()
        {
            ExecutablePath = invocation.ExecutablePath,
            Arguments = [.. invocation.PrefixArguments, .. arguments],
            WorkingDirectory = workspace,
            Environment = new Dictionary<string, string?>(),
            Timeout = ProbeTimeout,
            MaximumOutputBytes = OutputLimit,
        };

    internal static bool Succeeded(ToolProcessResult result) => result.ExitCode == 0 && !result.Cancelled && !result.TimedOut && result.Failure is null;

}
