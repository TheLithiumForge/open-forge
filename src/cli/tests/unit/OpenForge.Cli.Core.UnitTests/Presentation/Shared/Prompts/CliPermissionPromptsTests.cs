using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Prompts;

public sealed class CliPermissionPromptsTests
{
    [Theory(DisplayName = "Permission choices explain saved grants run-only grants and cancellation in both input modes")]
    [InlineData(0), InlineData(1), InlineData(2)]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task ChoiceScreens(int choice)
    {
        var keys = Enumerable.Repeat<CliKeyStroke?>(new(CliKey.Down), choice).Append(new(CliKey.Enter));
        var script = ScriptedCliTerminal.Keys(keys);
        var reply = await new CliPrompts(script.Terminal).PermissionAsync(Question(), new(true), CancellationToken.None);
        Assert.Equal(choice == 2 ? CliPromptState.Cancelled : CliPromptState.Answered, reply.State);
        if (choice < 2) Assert.Equal(choice == 0 ? CliPermissionChoice.Always : CliPermissionChoice.Once, reply.Value);
        CommandOutputSnapshot.MatchSnapshot(script.Frames[^1], $"permission.{choice}.key.80x24");
        var line = ScriptedCliTerminal.Lines([new[] { "always", "once", "cancel" }[choice]]);
        var lineReply = await new CliPrompts(line.Terminal).PermissionAsync(Question(), new(true), CancellationToken.None);
        Assert.Equal(reply.State, lineReply.State);
        CommandOutputSnapshot.MatchSnapshot(line.Output.ToString(), $"permission.{choice}.line");
    }

    [Fact(DisplayName = "Invalid permission line answers explain the accepted words before asking again")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task InvalidPermissionLine()
    {
        var script = ScriptedCliTerminal.Lines(["yes", "once"]);
        var reply = await new CliPrompts(script.Terminal).PermissionAsync(Question(), new(true), CancellationToken.None);
        Assert.Equal(CliPermissionChoice.Once, reply.Value);
        Assert.Equal(2, script.LineReadCalls);
        CommandOutputSnapshot.MatchSnapshot(script.Output.ToString(), "permission.invalid.line");
    }

    [Theory(DisplayName = "Invalid confirmation answers gain guidance only in line mode")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task InvalidConfirmation(bool keys)
    {
        var script = keys ? ScriptedCliTerminal.Keys([new(CliKey.Character, 'x'), new(CliKey.Character, 'y')])
            : ScriptedCliTerminal.Lines(["maybe", "yes"]);
        var reply = await new CliPrompts(script.Terminal).ConfirmAsync(new(CliPromptWording.Confirm()), new(true), CancellationToken.None);
        Assert.True(reply.Value);
        Assert.Equal(0, script.ClearCalls);
        CommandOutputSnapshot.MatchSnapshot(script.Output.ToString(), keys ? "confirmation.invalid.key.80x24" : "confirmation.invalid.line");
    }

    [Fact(DisplayName = "Long permission owners keep the compact question and owner context")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task CompactHeading()
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Enter)]);
        var reply = await new CliPrompts(script.Terminal).PermissionAsync(Question() with { Owner = "catalogue/extensions/development-toolkit" }, new(true), CancellationToken.None);
        Assert.Equal(CliPermissionChoice.Always, reply.Value);
        CommandOutputSnapshot.MatchSnapshot(Assert.Single(script.Frames), "permission.compact.key.80x24");
    }

    private static CliPermissionQuestion Question() => new("toolkit", [new("docs", true), new(".gitignore", false)]);
}
