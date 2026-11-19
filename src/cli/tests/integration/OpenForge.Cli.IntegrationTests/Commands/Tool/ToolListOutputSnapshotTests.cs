using OpenForge.Cli.Core.Commands.Tool.Models;
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
        using var result = await ToolOutputCapture.MatchAsync(new("fresh", ToolCommandFixture.List()));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), result.Run.ExitCode);
        Assert.Equal(result.Before, result.Fixture.Files());
        Assert.Empty(result.Fixture.Processes.Effects);
    }

    [Fact(DisplayName = "List shows a configured, integrated and installed tool with its version")]
    public async Task ConfiguredAndInstalled()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("configured", ToolCommandFixture.List())
        {
            Arrange = static async fixture =>
            {
                Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete),
                    (await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, ToolCommandDefinitions.YesOption))).ExitCode);
                fixture.Processes.Installed(fixture.CodeGraph);
            },
        });

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), result.Run.ExitCode);
    }

    [Fact(DisplayName = "List still shows every tool and warns when the settings can't be read")]
    public async Task UnreadableSettings()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("unreadable-settings", ToolCommandFixture.List())
        {
            Arrange = static fixture =>
            {
                fixture.Write(WorkspaceSettingsDefinitions.RelativePath, "[");
                return Task.CompletedTask;
            },
        });

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Incomplete), result.Run.ExitCode);
    }
}
