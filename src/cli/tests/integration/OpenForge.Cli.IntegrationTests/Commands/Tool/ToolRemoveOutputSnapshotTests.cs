using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Integration;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

[Trait("Feature", "tool-remove"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
public sealed class ToolRemoveOutputSnapshotTests
{
    private const string Yes = ToolCommandDefinitions.YesOption;
    private const string DryRun = ToolCommandDefinitions.DryRunOption;
    private const string UnrelatedMember = "unrelated";

    [Fact(DisplayName = "Remove with --yes undoes Open Forge's changes, keeps the tool installed and shows its uninstall commands")]
    public async Task RemovesChangesAndKeepsTool()
    {
        using var preview = await ToolOutputCapture.MatchAsync(new("remove-dry-run", ToolCommandFixture.Remove(CodeGraphEntry.Id, DryRun))
        {
            Arrange = fixture => InstalledAsync(fixture, fixture.CodeGraph),
        });
        using var result = await ToolOutputCapture.MatchAsync(new("removed", ToolCommandFixture.Remove(CodeGraphEntry.Id, Yes))
        {
            Arrange = fixture => InstalledAsync(fixture, fixture.CodeGraph),
        });
        using var again = await ToolOutputCapture.MatchAsync(new("nothing-left", ToolCommandFixture.Remove(CodeGraphEntry.Id, Yes))
        {
            Arrange = async fixture =>
            {
                await InstalledAsync(fixture, fixture.CodeGraph);
                Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete),
                    (await fixture.RunAsync(ToolCommandFixture.Remove(CodeGraphEntry.Id, Yes))).ExitCode);
            },
        });

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), result.Run.ExitCode);
        Assert.Null(result.Fixture.Read(CodeGraphEntry.ConfigurationPath));
        Assert.Empty(result.Fixture.ConfiguredTools());
        Assert.Empty(result.Fixture.Processes.Effects);
        Assert.Equal(again.Before, again.Fixture.Files());
    }

    [Fact(DisplayName = "Answering yes runs the tool's own uninstall after Open Forge's changes are removed")]
    public async Task InteractiveUninstall()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("uninstalled", ToolCommandFixture.Remove(CodebaseMemoryEntry.Id))
        {
            Arrange = fixture => InstalledAsync(fixture, fixture.CodebaseMemory),
            Answers = "2" + Environment.NewLine,
        });
        var fixture = result.Fixture;

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), result.Run.ExitCode);
        Assert.Equal(fixture.CodebaseMemory.Uninstall.Commands.Select(command => command.Text), fixture.Processes.Effects);
        ToolInstallOutputSnapshotTests.AssertIntegrated(fixture, fixture.CodebaseMemory, ToolIntegrationState.Absent);
        ToolOutputCapture.MatchPrompt(result.Run.Prompts, "uninstall-prompt", nameof(ToolRemoveOutputSnapshotTests));
    }

    [Fact(DisplayName = "Enter keeps the tool installed")]
    public async Task EnterKeepsTool()
    {
        using var fixture = new ToolCommandFixture();
        await InstalledAsync(fixture, fixture.CodeGraph);
        fixture.Processes.Commands.Clear();

        var run = await fixture.RunAsync(ToolCommandFixture.Remove(CodeGraphEntry.Id), Environment.NewLine);

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        Assert.Empty(fixture.Processes.Effects);
    }

    [Fact(DisplayName = "Remove keeps unrelated content in the tool's own file")]
    public async Task KeepsUnrelatedContent()
    {
        using var fixture = new ToolCommandFixture();
        fixture.Write(CodeGraphEntry.ConfigurationPath, new JsonObject { [UnrelatedMember] = true }.ToJsonString());
        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete),
            (await fixture.RunAsync(ToolCommandFixture.Install(CodeGraphEntry.Id, Yes))).ExitCode);

        var run = await fixture.RunAsync(ToolCommandFixture.Remove(CodeGraphEntry.Id, Yes));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        var remaining = JsonNode.Parse(fixture.Read(CodeGraphEntry.ConfigurationPath)!)!.AsObject();
        Assert.True(remaining[UnrelatedMember]!.GetValue<bool>());
        ToolInstallOutputSnapshotTests.AssertIntegrated(fixture, fixture.CodeGraph, ToolIntegrationState.Absent);
    }

    [Fact(DisplayName = "Remove reports nothing to do for a tool that isn't set up or installed")]
    public async Task NothingToRemove()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("nothing", ToolCommandFixture.Remove(CodeGraphEntry.Id, Yes)));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), result.Run.ExitCode);
        Assert.Equal(result.Before, result.Fixture.Files());
    }

    [Fact(DisplayName = "An unknown tool ID is invalid input")]
    public async Task UnknownTool()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("unknown", ToolCommandFixture.Remove(nameof(UnknownTool), Yes)));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Invalid), result.Run.ExitCode);
    }

    private static async Task InstalledAsync(ToolCommandFixture fixture, ToolEntry entry)
    {
        var install = await fixture.RunAsync(ToolCommandFixture.Install(entry.Id, Yes));
        Assert.True(install.ExitCode == ToolCommandFixture.Exit(CliSemanticStatus.Complete), install.Output + install.Error);
        fixture.Processes.Installed(entry);
    }
}
