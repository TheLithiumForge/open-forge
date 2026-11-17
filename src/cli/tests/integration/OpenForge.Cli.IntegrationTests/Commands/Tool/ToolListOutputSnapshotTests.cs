using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

[Trait("Feature", "tool-list"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
public sealed class ToolListOutputSnapshotTests
{
    [Fact(DisplayName = "List groups the catalogue by category and changes nothing when no tool is installed")]
    public async Task FreshWorkspace()
    {
        using var fixture = new ToolCommandFixture();
        var before = fixture.Files();

        var text = await fixture.RunAsync(ToolCommandFixture.List());
        var standard = await fixture.RunAsync(ToolCommandFixture.List(ToolCommandFixture.Standard));
        var json = await fixture.RunAsync(ToolCommandFixture.List(ToolCommandFixture.Json));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), text.ExitCode);
        Assert.Equal(before, fixture.Files());
        Assert.Empty(fixture.Processes.Effects);
        ToolCommandFixture.Match(text.Output, "fresh");
        ToolCommandFixture.Match(standard.Output, "fresh.standard");
        ToolCommandFixture.Match(json.Output, "fresh.json");
    }

    [Fact(DisplayName = "List shows a configured, integrated and installed tool with its version")]
    public async Task ConfiguredAndInstalled()
    {
        using var fixture = new ToolCommandFixture();
        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete),
            (await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, Core.Commands.Tool.ToolCommandDefinitions.YesOption))).ExitCode);
        fixture.Processes.Installed(fixture.CodeGraph);

        var text = await fixture.RunAsync(ToolCommandFixture.List());
        var json = await fixture.RunAsync(ToolCommandFixture.List(ToolCommandFixture.Json));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), text.ExitCode);
        ToolCommandFixture.Match(text.Output, "configured");
        ToolCommandFixture.Match(json.Output, "configured.json");
    }

    [Fact(DisplayName = "List still shows every tool and warns when the settings can't be read")]
    public async Task UnreadableSettings()
    {
        using var fixture = new ToolCommandFixture();
        fixture.Write(WorkspaceSettingsDefinitions.RelativePath, "[");

        var text = await fixture.RunAsync(ToolCommandFixture.List());

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Incomplete), text.ExitCode);
        ToolCommandFixture.Match(text.Output, "unreadable-settings");
    }
}
