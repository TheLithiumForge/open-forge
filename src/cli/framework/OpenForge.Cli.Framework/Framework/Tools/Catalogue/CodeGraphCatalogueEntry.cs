using System.Collections.Immutable;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodeGraph;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue;

internal static class CodeGraphCatalogueEntry
{
    internal static ToolCatalogueEntry Create()
    {
        var assets = CodeGraphReleaseAssets.Create();
        var targets = assets.Select(asset => asset.Target).ToImmutableArray();
        return new()
        {
            Identity = new()
            {
                Id = CodeGraphDefinitions.Id,
                Name = "CodeGraph",
                Origin = ToolOrigin.ThirdParty,
                Upstream = new($"https://github.com/{CodeGraphDefinitions.Repository}"),
                Licence = "MIT",
            },
            Channels = ImmutableArray.Create<ToolChannel>(
                new ToolNpmChannel
                {
                    Id = ToolCatalogueDefinitions.NpmChannelId,
                    Kind = ToolChannelKind.Npm,
                    Package = CodeGraphDefinitions.Package,
                    EntryRelativePath = CodeGraphDefinitions.NpmEntryPath,
                    MinimumClientVersions = ImmutableDictionary<ToolNpmClient, string>.Empty,
                    Requirements = new() { Runtime = new(ToolCatalogueDefinitions.NodeRuntime, CodeGraphDefinitions.WrapperNodeRange), Targets = targets },
                },
                new ToolGitHubReleaseChannel
                {
                    Id = ToolCatalogueDefinitions.GitHubChannelId,
                    Kind = ToolChannelKind.GitHubRelease,
                    Repository = CodeGraphDefinitions.Repository,
                    Assets = assets,
                    Requirements = new() { Targets = targets },
                }),
            Detection = new()
            {
                Command = CodeGraphDefinitions.Id,
                Arguments = ImmutableArray.Create(ToolCatalogueDefinitions.VersionArgument),
                Timeout = ToolCatalogueDefinitions.ProbeTimeout,
                MaximumOutputBytes = ToolCatalogueDefinitions.ProbeMaximumBytes,
                Environment = ImmutableDictionary<string, string?>.Empty.Add(ToolNativeConfigDefinitions.CodeGraphNoDownloadVariable, CodeGraphDefinitions.NoDownloadValue),
                Parser = ToolVersionParser.StableSemVer,
            },
            Profiles = ImmutableArray.Create(new ToolVersionProfile
            {
                Id = CodeGraphDefinitions.ProfileId,
                Range = new(1, 6),
                MinimumToolVersion = CodeGraphDefinitions.Version,
                MinimumOpenForgeVersion = ToolCatalogueDefinitions.MinimumOpenForgeVersion,
                NativeHomePolicy = ToolNativeHomePolicy.Node,
                TestedVersions = ImmutableArray.Create(CodeGraphDefinitions.Version),
                Setup = CodeGraphNativeSetup.Create(uninstall: false),
                Uninstall = CodeGraphNativeSetup.Create(uninstall: true),
                Changes = Changes(),
            }),
        };
    }

    private static ImmutableArray<ToolConfigChange> Changes()
    {
        using var exclude = JsonDocument.Parse($"""["{CodeGraphDefinitions.ArchivePattern}"]""");
        return
        [
            new ToolNativeConfigChange
            {
                Id = CodeGraphDefinitions.TelemetryChangeId,
                Scope = ToolEffectScope.Account,
                InitialState = ToolInitialConfigState.ReadableNativeValue,
                Description = "Turn stored account telemetry off. This deletes the upstream identity and unsent queues. Project removal keeps this setting and prints its manual change command.",
                AllowListId = ToolNativeConfigDefinitions.CodeGraphTelemetryAllowListId,
                ApplyArguments = ImmutableArray.Create(CodeGraphDefinitions.TelemetryCommand, ToolNativeConfigDefinitions.DisabledValue),
                ReadArguments = ImmutableArray.Create(CodeGraphDefinitions.NoColorArgument, CodeGraphDefinitions.TelemetryCommand, CodeGraphDefinitions.TelemetryStatusCommand),
                ReadEnvironment = ImmutableDictionary<string, string?>.Empty
                    .Add(ToolNativeConfigDefinitions.DoNotTrackVariable, null)
                    .Add(ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, null)
                    .Add(ToolNativeConfigDefinitions.CodeGraphNoDownloadVariable, CodeGraphDefinitions.NoDownloadValue),
                ValueParserId = ToolNativeConfigDefinitions.CodeGraphTelemetryStatusParserId,
                ReadTimeout = ToolCatalogueDefinitions.ProbeTimeout,
                ReadMaximumOutputBytes = ToolCatalogueDefinitions.ProbeMaximumBytes,
                StoredValue = new()
                {
                    PathTemplate = CodeGraphDefinitions.TelemetryPath,
                    Parser = ToolNativeStoredValueParser.CodeGraphTelemetryV1,
                    MissingValue = ToolNativeConfigDefinitions.EnabledValue,
                    MaximumBytes = ToolCatalogueDefinitions.ProbeMaximumBytes,
                },
                DesiredValue = ToolNativeConfigDefinitions.DisabledValue,
                ManualRestoreArgumentsTemplate = ImmutableArray.Create(CodeGraphDefinitions.TelemetryCommand, ToolRecipeDefinitions.PreviousValueToken),
            },
            new ToolJsonValueChange
            {
                Id = ToolCatalogueDefinitions.ArchiveChangeId,
                Scope = ToolEffectScope.Workspace,
                InitialState = ToolInitialConfigState.JsonArrayOrMissing,
                Description = "Exclude archived Open Forge memory from the project graph.",
                PathTemplate = CodeGraphDefinitions.ConfigurationPath,
                Pointer = CodeGraphDefinitions.ExcludePointer,
                Operation = ToolJsonValueOperation.Union,
                Value = exclude.RootElement.Clone(),
            },
            new ToolMarkedBlockChange
            {
                Id = CodeGraphDefinitions.IgnoreChangeId,
                Scope = ToolEffectScope.Workspace,
                InitialState = ToolInitialConfigState.UniqueMarkedBlock,
                Description = "Ignore CodeGraph index data when existing Git rules don't cover it.",
                PathTemplate = CodeGraphDefinitions.IgnorePath,
                StartMarker = CodeGraphDefinitions.IgnoreStartMarker,
                EndMarker = CodeGraphDefinitions.IgnoreEndMarker,
                ContentTemplate = CodeGraphDefinitions.IndexDirectory,
                WhenNotIgnored = new(CodeGraphDefinitions.IndexDirectory),
            },
        ];
    }
}
