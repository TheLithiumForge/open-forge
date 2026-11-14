using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.UnitTests.Framework.Tools.Catalogue.Shared;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Catalogue;

public sealed class CodebaseMemoryCatalogueTests
{
    public static TheoryData<string, string, long> PublishedPins => ReviewedAssetPins.CodebaseMemory;

    [Theory(DisplayName = "Each Codebase Memory release pin matches its reviewed official asset"), MemberData(nameof(PublishedPins))]
    [Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void MatchesPublishedPin(string name, string digest, long bytes)
    {
        var asset = Assert.Single(Channel().Assets, value => Path.GetFileName(value.Url.AbsolutePath) == name);
        Assert.Equal(digest, asset.Sha256);
        Assert.Equal(bytes, asset.MaximumBytes);
        Assert.Equal(CodebaseMemoryDefinitions.Version, asset.Version);
        Assert.Equal(new Uri($"https://github.com/{CodebaseMemoryDefinitions.Repository}/releases/download/v{CodebaseMemoryDefinitions.Version}/{name}"), asset.Url);
        Assert.Null(asset.Attestation);
        Assert.Null(asset.BundledRuntime);
    }

    [Fact(DisplayName = "Codebase Memory uses only native GitHub assets with static Linux and no invented platform floors"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void DeclaresNativeTargets()
    {
        var entry = CodebaseMemoryCatalogueEntry.Create();
        var channel = Assert.IsType<ToolGitHubReleaseChannel>(Assert.Single(entry.Channels));
        Assert.Null(channel.Requirements.Runtime);
        Assert.Equal(6, channel.Assets.Count);
        Assert.Equal(6, channel.Assets.Select(asset => (asset.Target.OperatingSystem, asset.Target.Cpu)).Distinct().Count());
        foreach (var asset in channel.Assets)
        {
            var windows = asset.Target.OperatingSystem == ToolOperatingSystem.Windows;
            Assert.Equal(windows ? CodebaseMemoryDefinitions.WindowsBinary : CodebaseMemoryDefinitions.Command, asset.EntryRelativePath);
            Assert.Equal(windows ? ToolArchiveKind.Zip : ToolArchiveKind.TarGzip, asset.Archive);
            Assert.Equal(0, asset.StripComponents);
            Assert.Null(asset.Target.MinimumOperatingSystemVersion);
            Assert.Null(asset.Target.MinimumLinuxKernelVersion);
            Assert.Null(asset.Target.MinimumLibcVersion);
            Assert.Null(asset.Target.MinimumLibStdCppVersion);
            if (asset.Target.OperatingSystem == ToolOperatingSystem.Linux)
                Assert.Equal(ToolLinuxLibc.Static, asset.Target.LinuxLibc);
        }
    }

    [Fact(DisplayName = "Codebase Memory retains default client discovery and points native setup at the verified install directory"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void DeclaresNativeSetupAndPreview()
    {
        var profile = Profile();
        Assert.Equal(new[]
        {
            ToolCatalogueDefinitions.InstallCommand, ToolCatalogueDefinitions.YesArgument, CodebaseMemoryDefinitions.SkipBinaryArgument,
            CodebaseMemoryDefinitions.DirectoryArgument, ToolRecipeDefinitions.NativeInstallDirectoryToken,
        }, profile.Setup.Arguments);
        Assert.Equal(profile.Setup.Arguments.Append(ToolCatalogueDefinitions.DryRunArgument), profile.Setup.DryRunArguments);
        var plan = Assert.IsType<ToolNativePlanSpec>(profile.Setup.NativePlan);
        Assert.Equal(profile.Setup.Arguments.Append(CodebaseMemoryDefinitions.PlanArgument), plan.Arguments);
        Assert.Equal(ToolNativePlanParser.CbmInstallPlanV1, plan.Parser);
        Assert.True(plan.Timeout > TimeSpan.Zero);
        Assert.True(plan.MaximumOutputBytes > 0);
        Assert.Empty(profile.Setup.Environment);
        Assert.Equal(ToolNativeHomePolicy.HomeThenUserProfile, profile.NativeHomePolicy);
        Assert.Equal(new ToolVersionRange(0, 12), profile.Range);
        Assert.Equal(CodebaseMemoryDefinitions.Version, profile.MinimumToolVersion);
        Assert.Equal(ToolCatalogueDefinitions.MinimumOpenForgeVersion, profile.MinimumOpenForgeVersion);
        Assert.Equal(CodebaseMemoryDefinitions.Version, Assert.Single(profile.TestedVersions));
    }

    [Fact(DisplayName = "Codebase Memory declares account uninstall separately from install inventory and index deletion"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void DeclaresAccountUninstall()
    {
        var uninstall = Profile().Uninstall;
        Assert.Equal(new[]
        {
            ToolCatalogueDefinitions.UninstallCommand, ToolCatalogueDefinitions.YesArgument,
            CodebaseMemoryDefinitions.DirectoryArgument, ToolRecipeDefinitions.NativeInstallDirectoryToken,
        }, uninstall.Arguments);
        Assert.Equal(uninstall.Arguments.Append(ToolCatalogueDefinitions.DryRunArgument), uninstall.DryRunArguments);
        Assert.Null(uninstall.NativePlan);
        Assert.Empty(uninstall.Environment);
        Assert.Contains(uninstall.Effects, effect => effect.Kind == ToolDeclaredEffectKind.SharedFileDeletion && effect.Scope == ToolEffectScope.Account);
        Assert.Contains(uninstall.Effects, effect => effect.Kind == ToolDeclaredEffectKind.PathChange && effect.Scope == ToolEffectScope.Account);
        Assert.True(uninstall.Timeout > TimeSpan.Zero);
        Assert.True(uninstall.MaximumOutputBytes > 0);
    }

    [Fact(DisplayName = "Codebase Memory adds only the marked project archive exclusion"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void AddsOnlyArchiveIgnore()
    {
        var change = Assert.IsType<ToolMarkedBlockChange>(Assert.Single(Profile().Changes));
        Assert.Equal(ToolCatalogueDefinitions.ArchiveChangeId, change.Id);
        Assert.Equal(ToolEffectScope.Workspace, change.Scope);
        Assert.Equal(ToolInitialConfigState.UniqueMarkedBlock, change.InitialState);
        Assert.Equal(CodebaseMemoryDefinitions.IgnorePath, change.PathTemplate);
        Assert.Equal(CodebaseMemoryDefinitions.ArchivePattern, change.ContentTemplate);
        Assert.NotEqual(change.StartMarker, change.EndMarker);
        Assert.Null(change.WhenNotIgnored);
        var detection = CodebaseMemoryCatalogueEntry.Create().Detection;
        Assert.Equal(CodebaseMemoryDefinitions.Command, detection.Command);
        Assert.Equal(new[] { ToolCatalogueDefinitions.VersionArgument }, detection.Arguments);
        Assert.True(detection.Timeout > TimeSpan.Zero);
        Assert.True(detection.MaximumOutputBytes > 0);
        Assert.Empty(detection.Environment);
    }

    [Fact(DisplayName = "Codebase Memory native declarations include all built-in and registry client IDs"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void IncludesCompleteDiscoverySet()
    {
        string[] clients =
        [
            CodebaseMemoryDefinitions.Claude, CodebaseMemoryDefinitions.Codex, CodebaseMemoryDefinitions.Gemini, CodebaseMemoryDefinitions.Zed,
            CodebaseMemoryDefinitions.OpenCode, CodebaseMemoryDefinitions.Antigravity, CodebaseMemoryDefinitions.Aider, CodebaseMemoryDefinitions.Kilo,
            CodebaseMemoryDefinitions.VsCode, CodebaseMemoryDefinitions.Cursor, CodebaseMemoryDefinitions.Windsurf, CodebaseMemoryDefinitions.Augment,
            CodebaseMemoryDefinitions.OpenClaw, CodebaseMemoryDefinitions.Kiro, CodebaseMemoryDefinitions.Junie, CodebaseMemoryDefinitions.Hermes,
            CodebaseMemoryDefinitions.OpenHands, CodebaseMemoryDefinitions.Cline, CodebaseMemoryDefinitions.Warp, CodebaseMemoryDefinitions.Qwen,
            CodebaseMemoryDefinitions.Copilot, CodebaseMemoryDefinitions.Factory, CodebaseMemoryDefinitions.Crush, CodebaseMemoryDefinitions.Goose,
            CodebaseMemoryDefinitions.Vibe, CodebaseMemoryDefinitions.Grok, CodebaseMemoryDefinitions.Qoder, CodebaseMemoryDefinitions.Kimi,
            CodebaseMemoryDefinitions.GitLab, CodebaseMemoryDefinitions.Rovo, CodebaseMemoryDefinitions.Amp, CodebaseMemoryDefinitions.Devin,
            CodebaseMemoryDefinitions.Tabnine, CodebaseMemoryDefinitions.Continue, CodebaseMemoryDefinitions.VisualStudio, CodebaseMemoryDefinitions.Trae,
            CodebaseMemoryDefinitions.Roo, CodebaseMemoryDefinitions.AmazonQ, CodebaseMemoryDefinitions.CodeBuddy, CodebaseMemoryDefinitions.BobIde,
            CodebaseMemoryDefinitions.BobShell, CodebaseMemoryDefinitions.Pochi, CodebaseMemoryDefinitions.Pi, CodebaseMemoryDefinitions.Cody, CodebaseMemoryDefinitions.Omp,
        ];
        foreach (var setup in new[] { Profile().Setup, Profile().Uninstall })
        {
            var ids = setup.PossibleOutputs.Select(output => output.ClientId).OfType<string>().Distinct().Order(StringComparer.Ordinal);
            Assert.Equal(clients.Order(StringComparer.Ordinal), ids);
            Assert.All(setup.PossibleOutputs, output => Assert.Equal(ToolEffectScope.Account, output.Scope));
        }
    }

    [Fact(DisplayName = "Codebase Memory supplements native plans with profile, legacy and empty-directory declarations"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void SupplementsNativeInventory()
    {
        var setup = Profile().Setup;
        var outputs = setup.PossibleOutputs;
        Assert.Contains(outputs, output => output.ClientId == CodebaseMemoryDefinitions.OpenHands && output.Match == ToolNativeOutputMatch.ExistingPattern);
        Assert.Contains(outputs, output => output.ClientId == CodebaseMemoryDefinitions.VsCode && output.Match == ToolNativeOutputMatch.ExistingPattern);
        Assert.Contains(outputs, output => output.ClientId == CodebaseMemoryDefinitions.Kilo && output.OperatingSystem == ToolOperatingSystem.Windows);
        Assert.Contains(outputs, output => output.ClientId == CodebaseMemoryDefinitions.Kilo && output.OperatingSystem == ToolOperatingSystem.MacOS);
        Assert.Contains(outputs, output => output.ClientId == CodebaseMemoryDefinitions.Kilo && output.OperatingSystem == ToolOperatingSystem.Linux);
        Assert.Contains(outputs, output => output.ClientId == CodebaseMemoryDefinitions.Claude && output.Purpose == ToolNativeOutputPurpose.ReportOnly);
        Assert.Equal(4, outputs.Count(output => output.ClientId == CodebaseMemoryDefinitions.Cline
            && output.OperatingSystem == ToolOperatingSystem.Windows && output.Match == ToolNativeOutputMatch.ExistingPattern));
        Assert.Contains(outputs, output => output.PathTemplate == CodebaseMemoryNativeOutputs.Root(CodebaseMemoryDefinitions.CacheRoot) && output.Purpose == ToolNativeOutputPurpose.ReportOnly);
        Assert.Contains(outputs, output => output.PathTemplate == CodebaseMemoryNativeOutputs.Root(CodebaseMemoryDefinitions.RuntimeRoot) && output.Purpose == ToolNativeOutputPurpose.ReportOnly);
        var uninstallOnly = Profile().Uninstall.PossibleOutputs.Except(outputs).ToArray();
        Assert.Contains(uninstallOnly, output => output.ClientId == CodebaseMemoryDefinitions.Crush);
        Assert.Contains(uninstallOnly, output => output.ClientId is null && output.OperatingSystem == ToolOperatingSystem.Windows && output.Purpose == ToolNativeOutputPurpose.Backup);
    }

    private static ToolVersionProfile Profile() => Assert.Single(CodebaseMemoryCatalogueEntry.Create().Profiles);
    private static ToolGitHubReleaseChannel Channel() => Assert.Single(CodebaseMemoryCatalogueEntry.Create().Channels.OfType<ToolGitHubReleaseChannel>());
}
