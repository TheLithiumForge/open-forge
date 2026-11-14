using System.Runtime.InteropServices;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;

internal sealed partial class ToolCompositionFixture
{
    private const string SupportedOpenForgeMinimumVersion = "0.1.0";

    private static ToolCatalogue CreateCatalogue(bool twoTools)
    {
        var ids = twoTools ? new[] { LastId, FirstId } : [FirstId];
        return new() { Entries = ids.ToDictionary(id => id, Entry, StringComparer.Ordinal), NodeRuntimes = [] };
    }

    private static ToolCatalogueEntry Entry(string id)
    {
        using var value = JsonDocument.Parse($"[\"{id}\"]");
        var target = new ToolTarget
        {
            OperatingSystem = CurrentOperatingSystem(),
            Cpu = RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64 ? ToolCpu.Arm64 : ToolCpu.X64,
        };
        return new()
        {
            Identity = new() { Id = id, Name = id, Origin = ToolOrigin.ThirdParty, Upstream = new("https://example.invalid/synthetic"), Licence = "Synthetic" },
            Detection = new()
            {
                Command = id,
                Arguments = [Probe],
                Parser = ToolVersionParser.StableSemVer,
                Environment = new Dictionary<string, string?>(),
                Timeout = TimeSpan.FromSeconds(1),
                MaximumOutputBytes = 256,
            },
            Channels = [new ToolGitHubReleaseChannel
            {
                Id = ChannelId, Kind = ToolChannelKind.GitHubRelease, Repository = "synthetic/tool",
                Requirements = new() { Targets = [] },
                Assets = [new()
                {
                    Version = Version, Target = target, Url = new("https://example.invalid/synthetic-file"), Sha256 = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(DownloadBytes)),
                    Archive = ToolArchiveKind.File, StripComponents = 0, EntryRelativePath = id, MaximumBytes = 1024,
                }],
            }],
            Profiles = [new()
            {
                Id = "synthetic-profile", Range = new(3, 2), TestedVersions = [Version], MinimumToolVersion = Version,
                MinimumOpenForgeVersion = SupportedOpenForgeMinimumVersion,
                NativeHomePolicy = ToolNativeHomePolicy.Node,
                Setup = Spec(id, remove: false), Uninstall = Spec(id, remove: true),
                Changes = [new ToolJsonValueChange
                {
                    Id = "synthetic-members", Scope = ToolEffectScope.Workspace, InitialState = ToolInitialConfigState.JsonArrayOrMissing,
                    Description = "Synthetic project membership", PathTemplate = ConfigurationDirectory + "/" + id + ".json",
                    Pointer = MembersPointer, Operation = ToolJsonValueOperation.Union, Value = value.RootElement.Clone(),
                }],
            }],
        };
    }

    private static ToolOperatingSystem CurrentOperatingSystem()
    {
        if (OperatingSystem.IsWindows()) return ToolOperatingSystem.Windows;
        if (OperatingSystem.IsMacOS()) return ToolOperatingSystem.MacOS;
        return ToolOperatingSystem.Linux;
    }

    private static ToolSetupSpec Spec(string id, bool remove) => new()
    {
        Arguments = [remove ? Uninstall : Setup, id],
        Environment = new Dictionary<string, string?>(),
        Timeout = TimeSpan.FromSeconds(1),
        MaximumOutputBytes = 1024,
        Effects = [new()
        {
            Id = "synthetic-native-configuration", Scope = ToolEffectScope.Account, Kind = ToolDeclaredEffectKind.Configuration,
            Description = "Synthetic account binary and agent configuration", PathTemplate = ToolRecipeDefinitions.NativeHomeToken + "/" + id + ".json",
        }],
        PossibleOutputs = [new()
        {
            Scope = ToolEffectScope.Account, Match = ToolNativeOutputMatch.Exact, Purpose = ToolNativeOutputPurpose.Backup,
            PathTemplate = ToolRecipeDefinitions.NativeHomeToken + "/" + id + ".json",
        }],
    };
}
