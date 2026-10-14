using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Presentation.Install.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install.Shared.Prompts;

public sealed class InstallSetupPromptsTests
{
    [Theory(DisplayName = "Custom shows all ten route choices in navigable bounded views")]
    [InlineData(80, 24), InlineData(40, 12)]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task CustomViewport(int width, int height)
    {
        var keys = Enumerable.Repeat<CliKeyStroke?>(new(CliKey.Down), 9).Append(new(CliKey.Enter));
        var script = ScriptedCliTerminal.Keys(keys);
        script.Viewport = new(width, height);
        var question = Question(installed: false);
        var reply = await InstallSetupQuestions.RoutesAsync(new(script.Terminal), question, new(true), CancellationToken.None);
        Assert.Equal(question.Routes, reply.Value);
        Assert.All(script.Frames, frame =>
        {
            var lines = frame.Split(Environment.NewLine);
            Assert.InRange(lines.Length - 1, 1, height - 1);
            Assert.All(lines, line => Assert.True(line.Length < width, line));
            Assert.Contains("esc cancel", frame, StringComparison.Ordinal);
        });
        foreach (var row in question.Routes) Assert.Contains(script.Frames, frame => frame.Contains(row.Id, StringComparison.Ordinal));
        if (width == 80) Assert.DoesNotContain("Choice ", script.Frames[0], StringComparison.Ordinal);
        else Assert.Contains("Choice 10/10", script.Frames[^1], StringComparison.Ordinal);
        CommandOutputSnapshot.MatchSnapshot(script.Frames[0], $"custom.{width}x{height}.first");
        CommandOutputSnapshot.MatchSnapshot(script.Frames[^1], $"custom.{width}x{height}.last");
    }

    [Theory(DisplayName = "Custom focused details describe fresh installed and private route choices")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task FocusedDetails(bool installed)
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Character, '~'), new(CliKey.Character, '-'), new(CliKey.Enter)]);
        var reply = await InstallSetupQuestions.RoutesAsync(new(script.Terminal), Question(installed), new(true), CancellationToken.None);
        Assert.Equal(InstallRouteAction.Remove, reply.Value[0].Action);
        for (var index = 0; index < script.Frames.Count; index++)
            CommandOutputSnapshot.MatchSnapshot(script.Frames[index], $"custom.{installed}.{index}");
    }

    [Fact(DisplayName = "Custom line edits map to final route actions and preserve an explicit override")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task LineEditsAndLockedOverride()
    {
        var script = ScriptedCliTerminal.Lines(["2+ 9~", "1-", ""]);
        var question = Question(installed: false) with { Locks = [new("directives", "add")] };
        var reply = await InstallSetupQuestions.RoutesAsync(new(script.Terminal), question, new(true), CancellationToken.None);
        Assert.Equal(InstallRouteAction.Add, reply.Value[0].Action);
        Assert.Equal(InstallRouteAction.Add, reply.Value[1].Action);
        Assert.Equal(InstallRouteAction.GitIgnore, reply.Value[8].Action);
        CommandOutputSnapshot.MatchSnapshot(script.Output.ToString(), "custom.line.edits-and-lock");
    }

    [Theory(DisplayName = "Custom reports locked override context in bounded key mode and leaves its mark unchanged")]
    [InlineData(80, 24), InlineData(40, 12)]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task LockedKeyScreen(int width, int height)
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Character, '-'), new(CliKey.Enter)]);
        script.Viewport = new(width, height);
        var reply = await InstallSetupQuestions.RoutesAsync(new(script.Terminal), Question(installed: true) with { Locks = [new("directives", "add")] }, new(true), CancellationToken.None);
        Assert.Equal(InstallRouteAction.Add, reply.Value[0].Action);
        Assert.InRange(script.Frames[^1].Split(Environment.NewLine).Length - 1, 1, height - 1);
        Assert.All(script.Frames[^1].Split(Environment.NewLine), line => Assert.True(line.Length < width));
        CommandOutputSnapshot.MatchSnapshot(script.Frames[^1], $"custom.key.locked.{width}x{height}");
    }

    [Fact(DisplayName = "Custom line mode places its legend under the question and its edit instruction after all rows")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task CustomLineScreen()
    {
        var script = ScriptedCliTerminal.Lines([""]);
        var question = Question(installed: false);
        var reply = await InstallSetupQuestions.RoutesAsync(new(script.Terminal), question, new(true), CancellationToken.None);
        Assert.Equal(question.Routes, reply.Value);
        Assert.StartsWith("Choose what Open Forge sets up" + Environment.NewLine + "+ add   ~ add, keep contents out of Git   - leave out", script.Output.ToString(), StringComparison.Ordinal);
        var outputLines = script.Output.ToString().Split(Environment.NewLine);
        Assert.Single(outputLines, line => line == "~ keeps the entrypoint shared through Git. New contents are Git-ignored.");
        Assert.Single(outputLines, line => line == "Files already tracked by Git stay tracked.");
        Assert.Single(outputLines, line => line == "- leaves out supplied defaults. Existing files stay and remain routable.");
        Assert.Single(outputLines, line => line == "Type cancel to stop without changes.");
        CommandOutputSnapshot.MatchSnapshot(script.Output.ToString(), "custom.line");
    }

    [Theory(DisplayName = "Preset and frontmatter lists show aligned summaries with focused details only once")]
    [InlineData("preset"), InlineData("full-core"), InlineData("frontmatter"), InlineData("configure-root"), InlineData("configure-scoped")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task SetupScreens(string screen)
    {
        var keys = ScriptedCliTerminal.Keys(screen == "full-core" ? [new(CliKey.Down), new(CliKey.Enter)] : [new(CliKey.Enter)]);
        var lines = ScriptedCliTerminal.Lines(["1"]);
        if (screen is "preset" or "full-core")
        {
            await new CliPrompts(keys.Terminal).SelectAsync(InstallSetupQuestions.Preset(InstallPreset.Essentials), new(true), CancellationToken.None);
            await new CliPrompts(lines.Terminal).SelectAsync(InstallSetupQuestions.Preset(InstallPreset.Essentials), new(true), CancellationToken.None);
        }
        else
        {
            var question = new InstallFrontmatterQuestion(screen == "configure-scoped" ? "scoped" : "root") { Installed = screen.StartsWith("configure", StringComparison.Ordinal) };
            await new CliPrompts(keys.Terminal).SelectAsync(InstallSetupQuestions.Frontmatter(question), new(true), CancellationToken.None);
            await new CliPrompts(lines.Terminal).SelectAsync(InstallSetupQuestions.Frontmatter(question), new(true), CancellationToken.None);
        }
        CommandOutputSnapshot.MatchSnapshot(keys.Frames[^1], screen + ".key.80x24");
        CommandOutputSnapshot.MatchSnapshot(lines.Output.ToString(), screen + ".line");
    }

    [Theory(DisplayName = "Custom adapter preserves cancellation and unavailable replies")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task AdapterPreservesNonAnswers(bool allowed)
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Escape)]);
        var reply = await InstallSetupQuestions.RoutesAsync(new(script.Terminal), Question(installed: false), new(allowed), CancellationToken.None);
        Assert.Equal(allowed ? CliPromptState.Cancelled : CliPromptState.Unavailable, reply.State);
    }

    private static InstallRouteQuestion Question(bool installed)
        => new(InstallConfigurationChoices.Defaults(InstallPreset.Essentials), ImmutableArray<InstallRouteLock>.Empty, installed);
}
