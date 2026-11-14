using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodeGraph;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.UnitTests.Framework.Tools.Catalogue.Shared;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Catalogue;

public sealed class CodeGraphCatalogueTests
{
    public static TheoryData<string, string, long> PublishedPins => ReviewedAssetPins.CodeGraph;

    [Theory(DisplayName = "Each CodeGraph release pin matches its reviewed official asset"), MemberData(nameof(PublishedPins))]
    [Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void MatchesPublishedPin(string name, string digest, long bytes)
    {
        var channel = GitHubChannel();
        var asset = Assert.Single(channel.Assets, value => Path.GetFileName(value.Url.AbsolutePath) == name);
        Assert.Equal(digest, asset.Sha256);
        Assert.Equal(bytes, asset.MaximumBytes);
        Assert.Equal(CodeGraphDefinitions.Version, asset.Version);
        Assert.Equal(new Uri($"https://github.com/{CodeGraphDefinitions.Repository}/releases/download/v{CodeGraphDefinitions.Version}/{name}"), asset.Url);
        Assert.Null(asset.Attestation);
    }

    [Fact(DisplayName = "CodeGraph separates the wrapper runtime from bundled Node on all six targets"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void SeparatesWrapperAndBundle()
    {
        var entry = CodeGraphCatalogueEntry.Create();
        var npm = Assert.Single(entry.Channels.OfType<ToolNpmChannel>());
        Assert.Equal(CodeGraphDefinitions.Package, npm.Package);
        Assert.True(OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths.PortableWorkspacePath.TryNormalize(npm.EntryRelativePath, out var entryPath));
        Assert.Equal(npm.EntryRelativePath, entryPath);
        Assert.Equal(new(ToolCatalogueDefinitions.NodeRuntime, CodeGraphDefinitions.WrapperNodeRange), npm.Requirements.Runtime);
        Assert.Empty(npm.MinimumClientVersions);
        Assert.Null(npm.RequiredClient);
        var github = Assert.Single(entry.Channels.OfType<ToolGitHubReleaseChannel>());
        Assert.Null(github.Requirements.Runtime);
        Assert.Equal(6, github.Assets.Count);
        Assert.Equal(6, github.Assets.Select(asset => (asset.Target.OperatingSystem, asset.Target.Cpu)).Distinct().Count());
        Assert.Equal(github.Requirements.Targets, npm.Requirements.Targets);
    }

    [Fact(DisplayName = "CodeGraph declares its real executable prefix and the bundled runtime target floors"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void DeclaresBundledInvocation()
    {
        foreach (var asset in GitHubChannel().Assets)
        {
            var runtime = Assert.IsType<ToolBundledRuntimeSpec>(asset.BundledRuntime);
            var windows = asset.Target.OperatingSystem == ToolOperatingSystem.Windows;
            Assert.Equal(ToolCatalogueDefinitions.NodeRuntime, runtime.Runtime);
            Assert.Equal(CodeGraphDefinitions.BundledNodeVersion, runtime.Version);
            Assert.Equal(windows ? CodeGraphDefinitions.WindowsRuntime : CodeGraphDefinitions.UnixRuntime, runtime.RuntimeRelativePath);
            Assert.Equal(CodeGraphDefinitions.ApplicationEntry, runtime.EntryRelativePath);
            Assert.Equal(new[] { CodeGraphDefinitions.LiftoffArgument, CodeGraphDefinitions.WarningArgument }, runtime.Arguments);
            Assert.Equal(windows ? CodeGraphDefinitions.WindowsLauncher : CodeGraphDefinitions.UnixLauncher, asset.EntryRelativePath);
            Assert.Equal(windows ? ToolArchiveKind.Zip : ToolArchiveKind.TarGzip, asset.Archive);
            Assert.Equal(1, asset.StripComponents);
            if (asset.Target.OperatingSystem == ToolOperatingSystem.Linux)
            {
                Assert.Equal(ToolLinuxLibc.Glibc, asset.Target.LinuxLibc);
                Assert.Equal("2.28", asset.Target.MinimumLibcVersion);
                Assert.Equal("4.18", asset.Target.MinimumLinuxKernelVersion);
                Assert.Equal("6.0.25", asset.Target.MinimumLibStdCppVersion);
            }
        }
    }

    [Fact(DisplayName = "CodeGraph version detection cannot invoke the wrapper download fallback"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void BoundsDetection()
    {
        var detection = CodeGraphCatalogueEntry.Create().Detection;
        Assert.Equal(CodeGraphDefinitions.Id, detection.Command);
        Assert.Equal(new[] { ToolCatalogueDefinitions.VersionArgument }, detection.Arguments);
        Assert.Equal(ToolVersionParser.StableSemVer, detection.Parser);
        Assert.Equal(ToolCatalogueDefinitions.ProbeTimeout, detection.Timeout);
        Assert.Equal(ToolCatalogueDefinitions.ProbeMaximumBytes, detection.MaximumOutputBytes);
        Assert.Equal(CodeGraphDefinitions.NoDownloadValue, Assert.Single(detection.Environment).Value);
        Assert.True(detection.Environment.ContainsKey(ToolNativeConfigDefinitions.CodeGraphNoDownloadVariable));
    }

    [Fact(DisplayName = "CodeGraph preserves the native local Claude and Codex setup and removal vectors"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void KeepsNativeVectors()
    {
        var profile = Profile();
        Assert.Equal(new[]
        {
            ToolCatalogueDefinitions.InstallCommand, ToolCatalogueDefinitions.YesArgument, CodeGraphDefinitions.LocalArgument,
            CodeGraphDefinitions.LocalLocation, CodeGraphDefinitions.TargetArgument, CodeGraphDefinitions.Targets,
        }, profile.Setup.Arguments);
        Assert.Equal(profile.Setup.Arguments.Skip(1).Prepend(ToolCatalogueDefinitions.UninstallCommand).Append(CodeGraphDefinitions.KeepCliArgument), profile.Uninstall.Arguments);
        Assert.Null(profile.Setup.NativePlan);
        Assert.Null(profile.Setup.DryRunArguments);
        Assert.Null(profile.Uninstall.NativePlan);
        Assert.Null(profile.Uninstall.DryRunArguments);
        Assert.Empty(profile.Setup.Environment);
        Assert.Empty(profile.Uninstall.Environment);
        Assert.Equal(ToolNativeHomePolicy.Node, profile.NativeHomePolicy);
        Assert.Equal(new(1, 6), profile.Range);
        Assert.Equal(CodeGraphDefinitions.Version, profile.MinimumToolVersion);
        Assert.Equal(ToolCatalogueDefinitions.MinimumOpenForgeVersion, profile.MinimumOpenForgeVersion);
        Assert.Empty(profile.TestedVersions);
    }

    [Fact(DisplayName = "CodeGraph telemetry declares a pre-install stored reader and separate bounded native status metadata"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void DeclaresStoredTelemetry()
    {
        var telemetry = Assert.Single(Profile().Changes.OfType<ToolNativeConfigChange>());
        Assert.Equal(ToolEffectScope.Account, telemetry.Scope);
        Assert.Equal(ToolInitialConfigState.ReadableNativeValue, telemetry.InitialState);
        Assert.Equal(ToolNativeConfigDefinitions.CodeGraphTelemetryAllowListId, telemetry.AllowListId);
        var stored = Assert.IsType<ToolNativeStoredValueSpec>(telemetry.StoredValue);
        Assert.Equal(CodeGraphDefinitions.TelemetryPath, stored.PathTemplate);
        Assert.Equal(ToolNativeStoredValueParser.CodeGraphTelemetryV1, stored.Parser);
        Assert.Equal(ToolNativeConfigDefinitions.EnabledValue, stored.MissingValue);
        Assert.True(stored.MaximumBytes > 0);
        Assert.Equal(ToolNativeConfigDefinitions.DisabledValue, telemetry.DesiredValue);
        Assert.Equal(new[] { CodeGraphDefinitions.TelemetryCommand, ToolNativeConfigDefinitions.DisabledValue }, telemetry.ApplyArguments);
        Assert.Equal(new[] { CodeGraphDefinitions.NoColorArgument, CodeGraphDefinitions.TelemetryCommand, CodeGraphDefinitions.TelemetryStatusCommand }, telemetry.ReadArguments);
        Assert.Equal(ToolNativeConfigDefinitions.CodeGraphTelemetryStatusParserId, telemetry.ValueParserId);
        Assert.True(telemetry.ReadTimeout > TimeSpan.Zero);
        Assert.True(telemetry.ReadMaximumOutputBytes > 0);
        var environment = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string?>>(telemetry.ReadEnvironment);
        Assert.Null(environment[ToolNativeConfigDefinitions.DoNotTrackVariable]);
        Assert.Null(environment[ToolNativeConfigDefinitions.CodeGraphTelemetryVariable]);
        Assert.Equal(CodeGraphDefinitions.NoDownloadValue, environment[ToolNativeConfigDefinitions.CodeGraphNoDownloadVariable]);
        Assert.Equal(new[] { CodeGraphDefinitions.TelemetryCommand, ToolRecipeDefinitions.PreviousValueToken }, telemetry.ManualRestoreArgumentsTemplate);
    }

    [Fact(DisplayName = "CodeGraph adds only telemetry, archive union and conditional Git ignore coverage"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void DeclaresOnlyDecidedAdditions()
    {
        var changes = Profile().Changes;
        Assert.Equal(new[] { CodeGraphDefinitions.TelemetryChangeId, ToolCatalogueDefinitions.ArchiveChangeId, CodeGraphDefinitions.IgnoreChangeId }, changes.Select(change => change.Id));
        var union = Assert.Single(changes.OfType<ToolJsonValueChange>());
        Assert.Equal(ToolJsonValueOperation.Union, union.Operation);
        Assert.Equal(CodeGraphDefinitions.ConfigurationPath, union.PathTemplate);
        Assert.Equal(CodeGraphDefinitions.ExcludePointer, union.Pointer);
        Assert.Equal(ToolInitialConfigState.JsonArrayOrMissing, union.InitialState);
        Assert.True(union.Value.HasValue);
        Assert.Equal(CodeGraphDefinitions.ArchivePattern, Assert.Single(union.Value.Value.EnumerateArray()).GetString());
        var ignore = Assert.Single(changes.OfType<ToolMarkedBlockChange>());
        Assert.Equal(CodeGraphDefinitions.IgnorePath, ignore.PathTemplate);
        Assert.Equal(new(CodeGraphDefinitions.IndexDirectory), ignore.WhenNotIgnored);
        Assert.Equal(CodeGraphDefinitions.IndexDirectory, ignore.ContentTemplate);
        Assert.NotEqual(ignore.StartMarker, ignore.EndMarker);
        Assert.Empty(changes.OfType<ToolManagedFileChange>());
    }

    [Fact(DisplayName = "CodeGraph declares native instruction, config, legacy and malformed-JSON backup files"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void DeclaresNativeBackups()
    {
        var expected = new[]
        {
            CodeGraphDefinitions.ClaudeMcpPath, CodeGraphDefinitions.ClaudeSettingsPath, CodeGraphDefinitions.ClaudeInstructionsPath,
            CodeGraphDefinitions.CodexConfigurationPath, CodeGraphDefinitions.CodexInstructionsPath, CodeGraphDefinitions.LegacyClaudePath,
            CodeGraphDefinitions.ClaudeMcpPath + CodeGraphDefinitions.NativeBackupSuffix,
            CodeGraphDefinitions.ClaudeSettingsPath + CodeGraphDefinitions.NativeBackupSuffix,
            CodeGraphDefinitions.LegacyClaudePath + CodeGraphDefinitions.NativeBackupSuffix,
        }.Order(StringComparer.Ordinal);
        foreach (var setup in new[] { Profile().Setup, Profile().Uninstall })
        {
            var backups = setup.PossibleOutputs.Where(output => output.Purpose == ToolNativeOutputPurpose.Backup).ToArray();
            Assert.Equal(expected, backups.Select(output => output.PathTemplate).Order(StringComparer.Ordinal));
            Assert.All(backups, output => Assert.Equal(ToolEffectScope.Workspace, output.Scope));
            Assert.Contains(setup.PossibleOutputs, output => output.PathTemplate == CodeGraphDefinitions.TelemetryPath && output.Purpose == ToolNativeOutputPurpose.ReportOnly);
            Assert.Contains(setup.Effects, effect => effect.Kind == ToolDeclaredEffectKind.Telemetry && effect.Scope == ToolEffectScope.Account);
            Assert.True(setup.Timeout > TimeSpan.Zero);
            Assert.True(setup.MaximumOutputBytes > 0);
        }
    }

    private static ToolVersionProfile Profile() => Assert.Single(CodeGraphCatalogueEntry.Create().Profiles);
    private static ToolGitHubReleaseChannel GitHubChannel() => Assert.Single(CodeGraphCatalogueEntry.Create().Channels.OfType<ToolGitHubReleaseChannel>());
}
