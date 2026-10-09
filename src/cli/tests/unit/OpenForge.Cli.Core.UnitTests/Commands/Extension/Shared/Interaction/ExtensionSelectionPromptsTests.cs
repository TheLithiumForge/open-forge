using OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Shared.Interaction;

public sealed class ExtensionSelectionPromptsTests
{
    [Theory(DisplayName = "Extension pickers name the action and explain the dependency in the correct direction")]
    [InlineData("Install"), InlineData("Update"), InlineData("Remove")]
    [Trait("Feature", "extension-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task SelectionScreens(string name)
    {
        var action = Enum.Parse<CliSelectionAction>(name);
        var question = Question(action);
        CliKeyStroke?[] keys = action == CliSelectionAction.Remove
            ? [new(CliKey.Down), new(CliKey.Space), new(CliKey.Up), new(CliKey.Space), new(CliKey.Enter)]
            : [new(CliKey.Space), new(CliKey.Down), new(CliKey.Space), new(CliKey.Enter)];
        var script = ScriptedCliTerminal.Keys(keys);
        var reply = await new CliPrompts(script.Terminal).MultiSelectAsync(question, new(true), CancellationToken.None);
        Assert.Equal([action == CliSelectionAction.Remove ? "workflows" : "planning"], reply.Value.Chosen);
        Assert.Equal([action == CliSelectionAction.Remove ? "planning" : "workflows"], reply.Value.Required);
        if (action == CliSelectionAction.Remove)
            Assert.Contains("Removing workflows also removes planning.", script.Frames[^1], StringComparison.Ordinal);
        CommandOutputSnapshot.MatchSnapshot(script.Frames[0], name + ".initial.key.80x24");
        CommandOutputSnapshot.MatchSnapshot(script.Frames[^1], name + ".required.key.80x24");
        var lines = ScriptedCliTerminal.Lines([action == CliSelectionAction.Remove ? "2" : "1"]);
        var lineReply = await new CliPrompts(lines.Terminal).MultiSelectAsync(question, new(true), CancellationToken.None);
        Assert.Equal(reply.Value.Chosen, lineReply.Value.Chosen);
        Assert.Equal(reply.Value.Required, lineReply.Value.Required);
        if (action == CliSelectionAction.Remove)
            Assert.Contains("Removing workflows also removes planning.", lines.Output.ToString(), StringComparison.Ordinal);
        CommandOutputSnapshot.MatchSnapshot(lines.Output.ToString(), name + ".line");
    }

    private static CliMultiSelectQuestion<string> Question(CliSelectionAction action)
    {
        var question = action switch
        {
            CliSelectionAction.Install => ExtensionInstallWording.Selection(),
            CliSelectionAction.Update => ExtensionUpdateWording.Selection(),
            CliSelectionAction.Remove => ExtensionRemoveWording.Selection(),
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };
        return new(question, [new("planning", "planning", "Plans, tasks and decisions"), new("workflows", "workflows", "Reusable workflows")],
            [new("planning", "workflows")], new HashSet<string>(), action,
            action == CliSelectionAction.Remove ? CliDependencyDirection.Dependents : CliDependencyDirection.Requires);
    }
}
