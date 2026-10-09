using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Prompts;

public sealed class CliMultiSelectionPromptsTests
{
    [Theory(DisplayName = "Multi-selection uses each action's chosen mark and the shared dependency mark")]
    [InlineData("Install", "+"), InlineData("Update", "+"), InlineData("Remove", "-")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task ActionMarksAndLegends(string actionName, string symbol)
    {
        var action = Enum.Parse<CliSelectionAction>(actionName);
        var script = ScriptedCliTerminal.Keys([new(CliKey.Space), new(CliKey.Enter)]);
        var reply = await new CliPrompts(script.Terminal).MultiSelectAsync(Question(action, dependencies: true), new(true), CancellationToken.None);
        Assert.Equal(["app"], reply.Value.Chosen);
        Assert.Equal(["runtime"], reply.Value.Required);
        Assert.Contains($"> [{symbol}] app", script.Frames[^1], StringComparison.Ordinal);
        Assert.Contains("[*] runtime", script.Frames[^1], StringComparison.Ordinal);
        Assert.DoesNotContain("[x]", script.Output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("1.", script.Frames[^1], StringComparison.Ordinal);
        CommandOutputSnapshot.MatchSnapshot(script.Frames[^1], actionName + ".key");
        var lines = ScriptedCliTerminal.Lines(["1"]);
        await new CliPrompts(lines.Terminal).MultiSelectAsync(Question(action, dependencies: true), new(true), CancellationToken.None);
        CommandOutputSnapshot.MatchSnapshot(lines.Output.ToString(), actionName + ".line");
    }

    [Theory(DisplayName = "Multi-selection omits dependency legends when the question has no dependencies")]
    [InlineData("Install"), InlineData("Update"), InlineData("Remove")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task NoDependencyLegend(string actionName)
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Escape)]);
        await new CliPrompts(script.Terminal).MultiSelectAsync(Question(Enum.Parse<CliSelectionAction>(actionName), dependencies: false), new(true), CancellationToken.None);
        Assert.DoesNotContain("* also", script.Output.ToString(), StringComparison.Ordinal);
        CommandOutputSnapshot.MatchSnapshot(Assert.Single(script.Frames), actionName + ".key");
    }

    [Fact(DisplayName = "Multi-selection rejects an undefined action before rendering")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task UndefinedAction()
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Escape)]);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await new CliPrompts(script.Terminal).MultiSelectAsync(Question((CliSelectionAction)99, dependencies: false), new(true), CancellationToken.None));
        Assert.Equal(0, script.WriteCalls);
    }

    private static CliMultiSelectQuestion<string> Question(CliSelectionAction action, bool dependencies)
        => new("Choose packages", [new("app", "app", "Application tools"), new("runtime", "runtime", "Runtime tools"),
                new("installed", "installed", "An existing package")],
            Dependencies(action, dependencies), new HashSet<string>(["installed"]), action,
            action == CliSelectionAction.Remove ? CliDependencyDirection.Dependents : CliDependencyDirection.Requires);

    private static IReadOnlyList<CliDependency<string>> Dependencies(CliSelectionAction action, bool dependencies)
    {
        if (!dependencies) return [];
        return action == CliSelectionAction.Remove ? [new("runtime", "app")] : [new("app", "runtime")];
    }
}
