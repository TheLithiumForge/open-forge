using System.Text.Json.Nodes;
using OpenForge.Cli.Composition.Shared.ToolHomes;
using static OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory.CodebaseMemoryDefinitions;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes;

public sealed class ToolClientConfigurationTests
{
    [Theory(DisplayName = "OpenClaw saved workspace has native precedence over environment and includes"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void OpenClawSavedWorkspace(bool include)
    {
        using var fixture = new ToolHomeFixture();
        var path = fixture.NativePath("chosen-workspace");
        var config = OpenClawConfiguration(path);
        if (include) config[ToolOpenClawRoots.IncludeProperty] = "synthetic-include.json";
        var file = fixture.File(ToolHomePathOracle.FixedConfigurations[OpenClaw], config.ToJsonString());
        var before = File.ReadAllBytes(file);
        fixture.Environment[ToolHomeEnvironment.OpenClawWorkspace] = fixture.NativePath("ignored-workspace");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(path, facts.Clients.Directories[OpenClawWorkspaceRoot]);
        Assert.Equal(before, File.ReadAllBytes(file));
        Assert.False(Directory.Exists(path));
    }

    [Theory(DisplayName = "OpenClaw unavailable config never guesses a workspace or follows includes"),
     InlineData(0), InlineData(1), InlineData(2), InlineData(3), InlineData(4),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void OpenClawUnavailable(int scenario)
    {
        using var fixture = new ToolHomeFixture();
        var text = scenario switch
        {
            0 => new JsonObject { [ToolOpenClawRoots.IncludeProperty] = "unfollowed.json" }.ToJsonString(),
            1 => "{invalid",
            2 => OpenClawConfiguration("relative-workspace").ToJsonString(),
            3 => "[]",
            4 => "{" + ToolOpenClawRoots.AgentsProperty + ": {}}",
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var file = fixture.File(ToolHomePathOracle.FixedConfigurations[OpenClaw], text);
        fixture.Environment[ToolHomeEnvironment.OpenClawWorkspace] = fixture.NativePath("unused-fallback");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.True(facts.Clients.Unavailable.ContainsKey(OpenClawWorkspaceRoot));
        Assert.False(facts.Clients.Directories.ContainsKey(OpenClawWorkspaceRoot));
        Assert.Equal(text, File.ReadAllText(file));
    }

    [Fact(DisplayName = "OpenClaw profile state and workspace paths honor its internal home and overrides"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void OpenClawOverrides()
    {
        using var fixture = new ToolHomeFixture();
        fixture.Environment[ToolHomeEnvironment.OpenClawHome] = "~/other-home";
        fixture.Environment[ToolHomeEnvironment.OpenClawProfile] = "synthetic";
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(fixture.NativePath("other-home/.openclaw-synthetic/" + ToolOpenClawRoots.ConfigurationFile), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(OpenClaw)]);
        Assert.Equal(fixture.NativePath("other-home/.openclaw/workspace-synthetic"), facts.Clients.Directories[OpenClawWorkspaceRoot]);
        fixture.Environment[ToolHomeEnvironment.OpenClawState] = "~/other-state";
        fixture.Environment[ToolHomeEnvironment.OpenClawWorkspace] = "~/other-workspace";
        facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(fixture.NativePath("other-home/other-state/" + ToolOpenClawRoots.ConfigurationFile), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(OpenClaw)]);
        Assert.Equal(fixture.NativePath("other-home/other-workspace"), facts.Clients.Directories[OpenClawWorkspaceRoot]);
        fixture.Environment[ToolHomeEnvironment.OpenClawConfig] = fixture.File("arbitrary-openclaw.json", "{/* comment */}");
        facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(fixture.Environment[ToolHomeEnvironment.OpenClawConfig], facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(OpenClaw)]);
    }

    [Fact(DisplayName = "OpenClaw duplicate path keys are unavailable instead of selecting an arbitrary value"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void OpenClawDuplicate()
    {
        using var fixture = new ToolHomeFixture();
        fixture.File(ToolHomePathOracle.FixedConfigurations[OpenClaw], $"{{\"{ToolOpenClawRoots.AgentsProperty}\":{{}},\"{ToolOpenClawRoots.AgentsProperty}\":{{}}}}");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.True(facts.Clients.Unavailable.ContainsKey(OpenClawWorkspaceRoot));
    }

    [Theory(DisplayName = "Rovo YAML path override is resolved only inside the native client root"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void RovoPath(bool contained)
    {
        using var fixture = new ToolHomeFixture();
        var expected = fixture.NativePath(".rovodev/owned-config.json");
        var value = contained ? "~/.rovodev/owned-config.json" : fixture.NativePath("outside-config.json");
        fixture.File(ToolClientConfigurationReader.RovoConfiguration,
            $"{ToolClientConfigurationReader.RovoMcp}:\n  {ToolClientConfigurationReader.RovoMcpPath}: '{value}'\n");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        var key = ToolHomePathOracle.ConfigurationKey(Rovo);
        if (contained) Assert.Equal(expected, facts.Clients.Files[key]);
        else Assert.True(facts.Clients.Unavailable.ContainsKey(key));
        Assert.False(File.Exists(expected));
    }

    [Theory(DisplayName = "Rovo malformed or aliased configuration leaves its MCP target unavailable"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void RovoUnavailable(bool alias)
    {
        using var fixture = new ToolHomeFixture();
        var value = alias ? "*missing-alias" : "[broken";
        fixture.File(ToolClientConfigurationReader.RovoConfiguration,
            $"{ToolClientConfigurationReader.RovoMcp}:\n  {ToolClientConfigurationReader.RovoMcpPath}: {value}\n");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.True(facts.Clients.Unavailable.ContainsKey(ToolHomePathOracle.ConfigurationKey(Rovo)));
    }

    [Fact(DisplayName = "File-valued and directory-valued injected bindings reject the wrong existing destination kind"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void BindingKinds()
    {
        using var fixture = new ToolHomeFixture();
        var key = ToolHomePathOracle.ConfigurationKey(Codex);
        var inputs = fixture.Inputs(new Dictionary<string, string>
        {
            [key] = fixture.Directory("wrong-file-kind"),
            [Claude] = fixture.File("wrong-directory-kind"),
        });
        var facts = fixture.Provider(inputs: inputs).Resolve(ToolHomeFixture.Profile(Id));
        Assert.True(facts.Clients.Unavailable.ContainsKey(key));
        Assert.True(facts.Clients.Unavailable.ContainsKey(Claude));
    }

    private static JsonObject OpenClawConfiguration(string workspace) => new()
    {
        [ToolOpenClawRoots.AgentsProperty] = new JsonObject
        {
            [ToolOpenClawRoots.DefaultsProperty] = new JsonObject { [ToolOpenClawRoots.WorkspaceProperty] = workspace },
        },
    };

    [Fact(DisplayName = "Empty Rovo YAML keeps the native default filename"), Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void EmptyRovo()
    {
        using var fixture = new ToolHomeFixture();
        fixture.File(ToolClientConfigurationReader.RovoConfiguration, string.Empty);
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(fixture.NativePath(ToolHomePathOracle.FixedConfigurations[Rovo]), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(Rovo)]);
    }
}
