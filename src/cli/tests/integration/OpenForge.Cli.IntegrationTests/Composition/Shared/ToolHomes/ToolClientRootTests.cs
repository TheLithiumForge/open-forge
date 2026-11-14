using OpenForge.Cli.Composition.Shared.ToolHomes;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using static OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory.CodebaseMemoryDefinitions;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes;

public sealed class ToolClientRootTests
{
    [Theory(DisplayName = "All catalogue binding keys have reviewed paths or explicit unavailable facts on every platform"),
     InlineData((int)ToolOperatingSystem.Windows), InlineData((int)ToolOperatingSystem.MacOS), InlineData((int)ToolOperatingSystem.Linux),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void CatalogueCoverage(int platform)
    {
        var os = (ToolOperatingSystem)platform;
        using var fixture = new ToolHomeFixture();
        var existing = ExistingOnly(fixture);
        var before = Directory.GetFileSystemEntries(fixture.Owned.Path, "*", SearchOption.AllDirectories).Order().ToArray();
        var facts = fixture.Provider(os).Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(ToolHomePathOracle.RequiredKeys(), facts.Clients.Bindings.Keys.Concat(facts.Clients.Unavailable.Keys).Order(StringComparer.Ordinal));
        foreach (var pair in ToolHomePathOracle.FixedConfigurations.Concat(ToolHomePathOracle.PlatformConfigurations(os)))
            Assert.Equal(fixture.NativePath(pair.Value), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(pair.Key)]);
        foreach (var pair in existing) Assert.Equal(pair.Value, facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(pair.Key)]);
        var visualStudio = ToolHomePathOracle.ConfigurationKey(VisualStudio);
        Assert.Equal(os == ToolOperatingSystem.Windows, facts.Clients.Files.ContainsKey(visualStudio));
        Assert.Equal(os != ToolOperatingSystem.Windows, facts.Clients.Unavailable.ContainsKey(visualStudio));
        Assert.Equal(os == ToolOperatingSystem.Windows ? 0 : 1, facts.Clients.Unavailable.Count);
        Assert.All(facts.Clients.Bindings.Values, path => Assert.True(Path.IsPathFullyQualified(path)));
        Assert.Equal(before, Directory.GetFileSystemEntries(fixture.Owned.Path, "*", SearchOption.AllDirectories).Order().ToArray());
    }

    public static TheoryData<string, int> ExistingChoiceCases()
    {
        var result = new TheoryData<string, int>();
        foreach (var pair in ToolHomePathOracle.ExistingChoices)
            for (var index = 0; index < pair.Value.Length; index++) result.Add(pair.Key, index);
        return result;
    }

    [Theory(DisplayName = "Native existing-file preference keeps the first active filename"), MemberData(nameof(ExistingChoiceCases)),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void ExistingChoices(string client, int first)
    {
        using var fixture = new ToolHomeFixture();
        var choices = ToolHomePathOracle.ExistingChoices[client];
        foreach (var suffix in choices.Skip(first)) fixture.File(suffix);
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(fixture.NativePath(choices[first]), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(client)]);
    }

    [Theory(DisplayName = "Windows Hermes keeps legacy home until the native directory exists"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void HermesExistingDirectory(bool nativeExists)
    {
        using var fixture = new ToolHomeFixture();
        var legacy = fixture.Directory(".hermes");
        var native = fixture.NativePath("AppData/Local/hermes");
        if (nativeExists) fixture.Directory("AppData/Local/hermes");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(nativeExists ? native : legacy, facts.Clients.Directories[Hermes]);
    }

    [Fact(DisplayName = "Native environment redirects file and directory bindings without moving unrelated client roots"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void EnvironmentOverrides()
    {
        using var fixture = new ToolHomeFixture();
        var directories = new Dictionary<string, string>
        {
            [Claude] = ToolHomeEnvironment.ClaudeConfigDirectory,
            [Codex] = ToolHomeEnvironment.CodexHome,
            [Kiro] = ToolHomeEnvironment.KiroHome,
            [Hermes] = ToolHomeEnvironment.HermesHome,
            [Qwen] = ToolHomeEnvironment.QwenHome,
            [Copilot] = ToolHomeEnvironment.CopilotHome,
            [Vibe] = ToolHomeEnvironment.VibeHome,
            [Grok] = ToolHomeEnvironment.GrokHome,
            [ClineDataRoot] = ToolHomeEnvironment.ClineDataDirectory,
            [Kimi] = ToolHomeEnvironment.KimiHome,
            [Omp] = ToolHomeEnvironment.PiAgentDirectory,
        };
        foreach (var pair in directories) fixture.Environment[pair.Value] = fixture.NativePath("overrides/" + pair.Key);
        fixture.Environment[ToolHomeEnvironment.KiroHome] = "~/overrides/" + Kiro;
        fixture.Environment[ToolHomeEnvironment.OpenCodeConfig] = fixture.NativePath("overrides/open-code-file.jsonc");
        fixture.Environment[ToolHomeEnvironment.CrushConfig] = fixture.NativePath("overrides/crush-file.json");
        fixture.Environment[ToolHomeEnvironment.GlabConfigDirectory] = fixture.NativePath("overrides/glab");
        fixture.Environment[ToolHomeEnvironment.CacheDirectory] = fixture.NativePath("overrides/cache");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        foreach (var pair in directories) Assert.Equal(fixture.NativePath("overrides/" + pair.Key), facts.Clients.Directories[pair.Key]);
        Assert.Equal(facts.Clients.Directories[Claude], facts.Clients.Directories[ClaudeUserRoot]);
        Assert.Equal(fixture.Environment[ToolHomeEnvironment.OpenCodeConfig], facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(OpenCode)]);
        Assert.Equal(fixture.Environment[ToolHomeEnvironment.CrushConfig], facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(Crush)]);
        Assert.Equal(fixture.NativePath("overrides/glab/duo/mcp.json"), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(GitLab)]);
        Assert.Equal(fixture.NativePath("overrides/glab/duo/hooks.json"), facts.Clients.Files[GitLabHooksRoot]);
        Assert.Equal(fixture.NativePath(ToolHomePathOracle.FixedConfigurations[Cline]), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(Cline)]);
        Assert.Equal(fixture.Environment[ToolHomeEnvironment.CacheDirectory], facts.Clients.Directories[CacheRoot]);
        Assert.Equal(fixture.Runtime, facts.Clients.Directories[RuntimeRoot]);
    }

    [Theory(DisplayName = "APPDATA and XDG choices stay separate from home-based native paths"),
     InlineData((int)ToolOperatingSystem.Windows), InlineData((int)ToolOperatingSystem.Linux), InlineData((int)ToolOperatingSystem.MacOS),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void PlatformOverrides(int platform)
    {
        var os = (ToolOperatingSystem)platform;
        using var fixture = new ToolHomeFixture();
        var appData = fixture.Directory("redirected-appdata");
        var xdg = fixture.Directory("redirected-xdg");
        fixture.Environment[ToolHomeEnvironment.AppData] = appData;
        fixture.Environment[ToolHomeEnvironment.XdgConfigHome] = xdg;
        var facts = fixture.Provider(os).Resolve(ToolHomeFixture.Profile(Id));
        if (os == ToolOperatingSystem.Windows)
        {
            Assert.Equal(Path.Combine(appData, "Code", "User"), facts.Clients.Directories[VsCode]);
            Assert.Equal(Path.Combine(appData, "devin"), facts.Clients.Directories[Devin]);
            Assert.Equal(Path.Combine(appData, "GitLab", "duo", "mcp.json"), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(GitLab)]);
        }
        else
        {
            Assert.Equal(Path.Combine(xdg, "gitlab", "duo", "mcp.json"), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(GitLab)]);
            Assert.Equal(fixture.NativePath(".config/devin"), facts.Clients.Directories[Devin]);
        }
        if (os == ToolOperatingSystem.Linux)
        {
            Assert.Equal(Path.Combine(xdg, "Code", "User"), facts.Clients.Directories[VsCode]);
            Assert.Equal(Path.Combine(xdg, "zed", "settings.json"), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(Zed)]);
        }
    }

    [Fact(DisplayName = "Conditional native clients keep unavailable bindings until their required config exists"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void ConditionalPaths()
    {
        using var fixture = new ToolHomeFixture();
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        foreach (var id in new[] { Continue, Trae, Roo, Cody })
            Assert.True(facts.Clients.Unavailable.ContainsKey(ToolHomePathOracle.ConfigurationKey(id)));
        var existing = ExistingOnly(fixture);
        facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        foreach (var pair in existing) Assert.Equal(pair.Value, facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(pair.Key)]);
    }

    [Fact(DisplayName = "Native expanded-home overrides fall back while raw relative overrides stay unavailable"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void InvalidOverrides()
    {
        using var fixture = new ToolHomeFixture();
        fixture.Environment[ToolHomeEnvironment.KiroHome] = "relative";
        fixture.Environment[ToolHomeEnvironment.CodexHome] = "relative";
        fixture.Environment[ToolHomeEnvironment.KimiHome] = "relative";
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(fixture.NativePath(".kiro"), facts.Clients.Directories[Kiro]);
        Assert.Equal(fixture.NativePath(".kimi-code"), facts.Clients.Directories[Kimi]);
        Assert.True(facts.Clients.Unavailable.ContainsKey(Codex));
        Assert.True(facts.Clients.Unavailable.ContainsKey(ToolHomePathOracle.ConfigurationKey(Codex)));
    }

    [Fact(DisplayName = "OMP profile has native precedence over its explicit agent directory"), Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void OmpProfile()
    {
        using var fixture = new ToolHomeFixture();
        fixture.Environment[ToolHomeEnvironment.PiAgentDirectory] = fixture.NativePath("other-agent");
        fixture.Environment[ToolHomeEnvironment.OmpProfile] = "synthetic_profile";
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(fixture.NativePath(".omp/profiles/synthetic_profile/agent"), facts.Clients.Directories[Omp]);
    }

    [Fact(DisplayName = "An invalid OMP profile keeps the native default instead of falling through to an agent override"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void OmpInvalidProfile()
    {
        using var fixture = new ToolHomeFixture();
        fixture.Environment[ToolHomeEnvironment.PiAgentDirectory] = fixture.NativePath("other-agent");
        fixture.Environment[ToolHomeEnvironment.OmpProfile] = "../invalid";
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(Path.GetDirectoryName(fixture.NativePath(ToolHomePathOracle.FixedConfigurations[Omp])), facts.Clients.Directories[Omp]);
    }

    private static Dictionary<string, string> ExistingOnly(ToolHomeFixture fixture)
    {
        var variables = new Dictionary<string, string>
        {
            [Continue] = ToolHomeEnvironment.ContinueConfig,
            [Trae] = ToolHomeEnvironment.TraeConfig,
            [Roo] = ToolHomeEnvironment.RooConfig,
            [Cody] = ToolHomeEnvironment.CodyConfig,
        };
        var result = new Dictionary<string, string>();
        foreach (var pair in variables)
        {
            var path = fixture.File("optional/" + pair.Key + ".config");
            fixture.Environment[pair.Value] = path;
            result.Add(pair.Key, path);
        }
        return result;
    }
}
