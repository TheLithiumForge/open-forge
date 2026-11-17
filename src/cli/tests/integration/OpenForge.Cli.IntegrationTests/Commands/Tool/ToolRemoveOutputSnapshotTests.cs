using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Tool;
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
        using var fixture = await InstalledAsync(fixture => fixture.CodeGraph);
        var preview = await fixture.RunAsync(ToolCommandFixture.Remove(fixture.CodeGraph.Id, DryRun));

        var run = await fixture.RunAsync(ToolCommandFixture.Remove(fixture.CodeGraph.Id, Yes));
        var json = await fixture.RunAsync(ToolCommandFixture.Remove(fixture.CodeGraph.Id, [Yes, .. ToolCommandFixture.Json]));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        Assert.Null(fixture.Read(CodeGraphEntry.ConfigurationPath));
        Assert.Empty(fixture.ConfiguredTools());
        Assert.Empty(fixture.Processes.Effects);
        ToolCommandFixture.Match(preview.Output, "dry-run");
        ToolCommandFixture.Match(run.Output, "removed");
        ToolCommandFixture.Match(json.Output, "nothing-left.json");
    }

    [Fact(DisplayName = "Answering yes runs the tool's own uninstall after Open Forge's changes are removed")]
    public async Task InteractiveUninstall()
    {
        using var fixture = await InstalledAsync(fixture => fixture.CodebaseMemory);

        var run = await fixture.RunAsync(ToolCommandFixture.Remove(fixture.CodebaseMemory.Id), "2" + Environment.NewLine);

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        Assert.Equal(fixture.CodebaseMemory.Uninstall.Commands.Select(command => command.Text), fixture.Processes.Effects);
        ToolInstallOutputSnapshotTests.AssertIntegrated(fixture, fixture.CodebaseMemory, ToolIntegrationState.Absent);
        ToolCommandFixture.Match(run.Prompts, "uninstall.prompt");
        ToolCommandFixture.Match(run.Output, "uninstalled");
    }

    [Fact(DisplayName = "Enter keeps the tool installed")]
    public async Task EnterKeepsTool()
    {
        using var fixture = await InstalledAsync(fixture => fixture.CodeGraph);

        var run = await fixture.RunAsync(ToolCommandFixture.Remove(fixture.CodeGraph.Id), Environment.NewLine);

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        Assert.Empty(fixture.Processes.Effects);
    }

    [Fact(DisplayName = "Remove keeps unrelated content in the tool's own file")]
    public async Task KeepsUnrelatedContent()
    {
        using var fixture = new ToolCommandFixture();
        fixture.Write(CodeGraphEntry.ConfigurationPath, new JsonObject { [UnrelatedMember] = true }.ToJsonString());
        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete),
            (await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, Yes))).ExitCode);

        var run = await fixture.RunAsync(ToolCommandFixture.Remove(fixture.CodeGraph.Id, Yes));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        var remaining = JsonNode.Parse(fixture.Read(CodeGraphEntry.ConfigurationPath)!)!.AsObject();
        Assert.True(remaining[UnrelatedMember]!.GetValue<bool>());
        ToolInstallOutputSnapshotTests.AssertIntegrated(fixture, fixture.CodeGraph, ToolIntegrationState.Absent);
    }

    [Fact(DisplayName = "Remove reports nothing to do for a tool that isn't set up or installed")]
    public async Task NothingToRemove()
    {
        using var fixture = new ToolCommandFixture();
        var before = fixture.Files();

        var run = await fixture.RunAsync(ToolCommandFixture.Remove(fixture.CodeGraph.Id, Yes));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        Assert.Equal(before, fixture.Files());
        ToolCommandFixture.Match(run.Output, "nothing");
    }

    [Fact(DisplayName = "An unknown tool ID is invalid input")]
    public async Task UnknownTool()
    {
        using var fixture = new ToolCommandFixture();

        var run = await fixture.RunAsync(ToolCommandFixture.Remove(nameof(UnknownTool), Yes));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Invalid), run.ExitCode);
        ToolCommandFixture.Match(run.Output, "unknown");
    }

    private static async Task<ToolCommandFixture> InstalledAsync(Func<ToolCommandFixture, ToolEntry> select)
    {
        var fixture = new ToolCommandFixture();
        var entry = select(fixture);
        var install = await fixture.RunAsync(ToolCommandFixture.Install(entry.Id, Yes));
        Assert.True(install.ExitCode == ToolCommandFixture.Exit(CliSemanticStatus.Complete), install.Output);
        fixture.Processes.Installed(entry);
        fixture.Processes.Commands.Clear();
        return fixture;
    }
}
