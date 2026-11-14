using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition.Models;
using System.Text.Json;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Composition.Shared.ToolComposition;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

public sealed class ToolCompositionInteractionIntegrationTests
{
    [Theory(DisplayName = "Actual Root optional selection distinguishes account approval from decline and keeps native-first cleanup"), InlineData(true), InlineData(false), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task AccountChoice(bool approve)
    {
        using var fixture = new ToolCompositionFixture();
        Assert.Equal(0, (await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--format", "json"])).ExitCode);
        fixture.Events.Clear();
        var run = await fixture.RunAsync(["tool", "remove", ToolCompositionFixture.FirstId], "y\n" + (approve ? "2\n" : "1\n"));
        Assert.Equal(0, run.ExitCode);
        Assert.Equal(approve ? 1 : 0, fixture.Events.Count(item => item == ToolHostEvent.Uninstall));
        Assert.Equal(approve, fixture.NativeSawProjectAddition);
        Assert.False(fixture.HasMember());
        Assert.Empty(fixture.Backups());
        var transcript = JsonSerializer.Serialize(fixture.Normalize(run.Prompts).Split('\n'), ToolCompositionSnapshotJsonContext.Default.StringArray);
        CommandOutputSnapshot.MatchSnapshot(transcript, approve ? "approved.json" : "declined.json");
    }

    [Theory(DisplayName = "Unattended Remove skips account uninstall and cleans only Open Forge additions"), InlineData(false), InlineData(true), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task UnattendedRemoval(bool json)
    {
        using var fixture = new ToolCompositionFixture();
        Assert.Equal(0, (await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--format", "json"])).ExitCode);
        fixture.Events.Clear();
        string[] format = json ? ["--format", "json"] : [];
        var run = await fixture.RunAsync(["tool", "remove", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, .. format]);
        Assert.Equal(0, run.ExitCode);
        Assert.Empty(run.Prompts);
        Assert.DoesNotContain(ToolHostEvent.Uninstall, fixture.Events);
        Assert.False(fixture.HasMember());
        Assert.True(File.Exists(fixture.NativeFile()));
        Assert.Equal(0, fixture.PathEffects);
    }

    [Fact(DisplayName = "Cancelling the actual Root plan confirmation performs no native or project effects"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task CancelPlan()
    {
        using var fixture = new ToolCompositionFixture();
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId], string.Empty);
        Assert.Equal(130, run.ExitCode);
        Assert.Equal(before, fixture.Snapshot());
        Assert.DoesNotContain(ToolHostEvent.Setup, fixture.Events);
    }

    [Fact(DisplayName = "Without a terminal plan consent is required and optional effects stay unselected"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task NoTerminal()
    {
        using var fixture = new ToolCompositionFixture();
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId]);
        Assert.Equal(4, run.ExitCode);
        Assert.Empty(run.Prompts);
        Assert.Equal(before, fixture.Snapshot());
        Assert.Equal(0, fixture.PathEffects);
    }

    [Fact(DisplayName = "Root typed manager and PATH adapters return actual answers and track only live PATH consent"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task TypedAdapters()
    {
        using var fixture = new ToolCompositionFixture();
        using var input = new StringReader("2\n2\n");
        using var output = new StringWriter();
        var interaction = CliCompositionRoot.CreateInteraction(new CliCompositionInputs
        { StandardInput = input, PromptOutput = output, StandardInputRedirected = false, PromptOutputRedirected = false });
        var adapter = new ToolCompositionInteraction(interaction.Prompts);
        var prompts = adapter.Create();
        var manager = await prompts.ChooseManager(new(ToolCompositionFixture.FirstId,
            [new(1, "synthetic-one", "1.0.0", null, "ready"), new(2, "synthetic-two", "2.0.0", null, "ready")]), new(true), TestContext.Current.CancellationToken);
        Assert.Equal(CliPromptState.Answered, manager.State);
        Assert.Equal(2, manager.Value);
        Assert.False(adapter.HasLivePathConsent);
        var path = await prompts.ChoosePath(new(Path.Combine(fixture.Home, Core.Framework.Tools.Models.Records.ToolRecordDefinitions.BinDirectoryName), [], UsesShellFiles: false), new(true), TestContext.Current.CancellationToken);
        Assert.Equal(CliPromptState.Answered, path.State);
        Assert.True(path.Value.AddForMe);
        Assert.True(adapter.HasLivePathConsent);
    }
}
