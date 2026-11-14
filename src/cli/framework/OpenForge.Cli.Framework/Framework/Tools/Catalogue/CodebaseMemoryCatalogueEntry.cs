using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue;

internal static class CodebaseMemoryCatalogueEntry
{
    internal static ToolCatalogueEntry Create()
    {
        var assets = CodebaseMemoryReleaseAssets.Create();
        return new()
        {
            Identity = new()
            {
                Id = CodebaseMemoryDefinitions.Id,
                Name = "Codebase Memory",
                Origin = ToolOrigin.ThirdParty,
                Upstream = new($"https://github.com/{CodebaseMemoryDefinitions.Repository}"),
                Licence = "MIT",
            },
            Channels = ImmutableArray.Create<ToolChannel>(new ToolGitHubReleaseChannel
            {
                Id = ToolCatalogueDefinitions.GitHubChannelId,
                Kind = ToolChannelKind.GitHubRelease,
                Repository = CodebaseMemoryDefinitions.Repository,
                Assets = assets,
                Requirements = new() { Targets = assets.Select(asset => asset.Target).ToImmutableArray() },
            }),
            Detection = new()
            {
                Command = CodebaseMemoryDefinitions.Command,
                Arguments = ImmutableArray.Create(ToolCatalogueDefinitions.VersionArgument),
                Timeout = ToolCatalogueDefinitions.ProbeTimeout,
                MaximumOutputBytes = ToolCatalogueDefinitions.ProbeMaximumBytes,
                Environment = ImmutableDictionary<string, string?>.Empty,
                Parser = ToolVersionParser.StableSemVer,
            },
            Profiles = ImmutableArray.Create(new ToolVersionProfile
            {
                Id = CodebaseMemoryDefinitions.ProfileId,
                Range = new(0, 12),
                MinimumToolVersion = CodebaseMemoryDefinitions.Version,
                MinimumOpenForgeVersion = ToolCatalogueDefinitions.MinimumOpenForgeVersion,
                NativeHomePolicy = ToolNativeHomePolicy.HomeThenUserProfile,
                TestedVersions = ImmutableArray<string>.Empty,
                Setup = CodebaseMemoryNativeSetup.Create(uninstall: false),
                Uninstall = CodebaseMemoryNativeSetup.Create(uninstall: true),
                Changes = ImmutableArray.Create<ToolConfigChange>(new ToolMarkedBlockChange
                {
                    Id = ToolCatalogueDefinitions.ArchiveChangeId,
                    Scope = ToolEffectScope.Workspace,
                    InitialState = ToolInitialConfigState.UniqueMarkedBlock,
                    Description = "Exclude archived Open Forge memory from Codebase Memory indexing.",
                    PathTemplate = CodebaseMemoryDefinitions.IgnorePath,
                    StartMarker = CodebaseMemoryDefinitions.IgnoreStartMarker,
                    EndMarker = CodebaseMemoryDefinitions.IgnoreEndMarker,
                    ContentTemplate = CodebaseMemoryDefinitions.ArchivePattern,
                }),
            }),
        };
    }
}
