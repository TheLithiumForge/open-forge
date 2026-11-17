using System.Collections.Frozen;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;

internal static class ToolPackageManagerPolicy
{
    internal const string CorepackNetworkVariable = "COREPACK_ENABLE_NETWORK";
    internal const string CorepackLatestVariable = "COREPACK_DEFAULT_TO_LATEST";
    internal const string CorepackAutoPinVariable = "COREPACK_ENABLE_AUTO_PIN";
    internal const string CorepackEnvFileVariable = "COREPACK_ENV_FILE";
    internal const string Disabled = "0";
    internal static readonly IReadOnlyDictionary<string, string?> ReadEnvironment = new Dictionary<string, string?>
    {
        [CorepackNetworkVariable] = Disabled,
        [CorepackLatestVariable] = Disabled,
        [CorepackAutoPinVariable] = Disabled,
        [CorepackEnvFileVariable] = Disabled,
    }.ToFrozenDictionary(StringComparer.Ordinal);
    internal static readonly IReadOnlyList<ToolNpmClient> ClientOrder = [ToolNpmClient.Npm, ToolNpmClient.Pnpm, ToolNpmClient.YarnClassic, ToolNpmClient.Bun];

    internal static IReadOnlyList<ToolPackageManagerFacts> Candidates(ToolNpmChannel channel, IReadOnlyList<ToolPackageManagerFacts> managers)
        => ClientOrder.SelectMany(client => managers.Where(manager => manager.Client == client && IsUsable(channel, manager))).ToArray();

    internal static ToolNpmClient? Default(ToolChannelRequest request, IReadOnlyList<ToolPackageManagerFacts> candidates)
    {
        if (request.ChosenManager is { } chosen) return candidates.Any(manager => manager.Client == chosen) ? chosen : null;
        var preferred = candidates.Where(manager => request.ProjectManagers.Contains(manager.Client)).ToArray();
        if (preferred.Length == 1) return preferred[0].Client;
        if (candidates.Count == 1) return candidates[0].Client;
        if (request.ManagerChoice != ToolManagerChoiceMode.Default) return null;
        foreach (var client in ClientOrder)
            if (candidates.Any(manager => manager.Client == client)) return client;
        return null;
    }

    internal static bool IsUsable(ToolNpmChannel channel, ToolPackageManagerFacts manager)
    {
        if (!manager.SupportsGlobalInstall || manager.Client == ToolNpmClient.YarnModern
            || channel.RequiredClient is { } required && manager.Client != required
            || ToolVersionService.ReadStable(manager.Version) is not { } actual)
            return false;
        if (channel.MinimumClientVersions.TryGetValue(manager.Client, out var minimum)
            && (ToolVersionService.ReadStable(minimum) is not { } floor || actual < floor)) return false;
        return channel.Requirements.Runtime is not { } runtime || SatisfiesRuntime(manager, runtime);
    }

    private static bool SatisfiesRuntime(ToolPackageManagerFacts manager, ToolRuntimeRequirement requirement)
    {
        if (requirement.Runtime != "node" || manager.RuntimePath is null || ToolVersionService.ReadStable(manager.RuntimeVersion) is not { } actual) return false;
        return ToolRuntimeRange.Satisfies(manager.RuntimeVersion, requirement.VersionRange);
    }

    internal static IReadOnlyList<string> VersionArguments => ["--version"];

    internal static IReadOnlyList<string> GlobalBinArguments(ToolNpmClient client)
        => client switch
        {
            ToolNpmClient.Npm => ["prefix", "--global"],
            ToolNpmClient.Pnpm => ["bin", "--global"],
            ToolNpmClient.YarnClassic => ["global", "bin"],
            ToolNpmClient.Bun => ["pm", "bin", "--global"],
            ToolNpmClient.YarnModern => throw new ArgumentException("Yarn Modern has no global installation.", nameof(client)),
            _ => throw new ArgumentOutOfRangeException(nameof(client)),
        };

    internal static IReadOnlyList<string> InstallArguments(ToolNpmClient client, string package, string version)
    {
        if (ToolVersionService.ReadStable(version) is null || string.IsNullOrWhiteSpace(package) || package.StartsWith('-') || package.Any(char.IsWhiteSpace))
            throw new ArgumentException("A manager install needs a package and exact stable version.");
        var spec = $"{package}@{version}";
        return client switch
        {
            ToolNpmClient.Npm => ["install", "--global", spec],
            ToolNpmClient.Pnpm or ToolNpmClient.Bun => ["add", "--global", spec],
            ToolNpmClient.YarnClassic => ["global", "add", spec],
            ToolNpmClient.YarnModern => throw new ArgumentException("Yarn Modern has no global installation.", nameof(client)),
            _ => throw new ArgumentOutOfRangeException(nameof(client)),
        };
    }
}
