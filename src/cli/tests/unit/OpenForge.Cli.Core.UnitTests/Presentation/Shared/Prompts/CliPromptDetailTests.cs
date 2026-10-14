using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Prompts;

public sealed class CliPromptDetailTests
{
    private const string Description = "Record ideas, investigate questions, preserve decisions, and organize tasks, plans, backlogs, and checkpoints.";

    [Theory(DisplayName = "Focused package details retain the full purpose before dependencies"), Trait("Boundary", "Output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    [InlineData(80, 24)]
    [InlineData(100, 30)]
    public async Task FocusedDescriptionAndClosure(int width, int height)
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Space), new(CliKey.Enter)]);
        script.Viewport = new(width, height);
        var question = new CliMultiSelectQuestion<string>("Choose Extensions to install",
            [new("planning", "planning", Description), new("workflows", "workflows", "Repeatable workflows")],
            [new("planning", "workflows")], new HashSet<string>(), CliSelectionAction.Install);
        var reply = await new CliPrompts(script.Terminal).MultiSelectAsync(question, new(true), CancellationToken.None);
        Assert.Equal(["planning"], reply.Value.Chosen);
        Assert.Equal(["workflows"], reply.Value.Required);
        var frame = script.Frames[0];
        var completeWords = string.Join(" ", frame.Split(Environment.NewLine).Select(line => line.Trim()));
        Assert.Contains(Description, completeWords, StringComparison.Ordinal);
        Assert.True(frame.IndexOf("Needs workflows.", StringComparison.Ordinal) > frame.IndexOf("Record ideas", StringComparison.Ordinal));
        CommandOutputSnapshot.MatchSnapshot(frame, $"focused-{width}x{height}");
    }

    [Fact(DisplayName = "Line selection shows description and list explanations once"), Trait("Boundary", "Output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task LineDescriptionsAndExplanations()
    {
        var script = ScriptedCliTerminal.Lines(["1"]);
        var question = new CliSelectQuestion<int>("Choose a setup",
            [new(1, "Small setup", Description: Description, Summary: "A few useful folders"), new(2, "All folders", "Every built-in folder")])
        {
            LineExplanations = ["Files already tracked by Git stay tracked."],
        };
        var reply = await new CliPrompts(script.Terminal).SelectAsync(question, new(true), CancellationToken.None);
        Assert.Equal(1, reply.Value);
        Assert.All(script.Output.ToString().Split(Environment.NewLine), line => Assert.True(line.Length < 80, line));
        CommandOutputSnapshot.MatchSnapshot(script.Output.ToString(), "single-line");
    }

    [Fact(DisplayName = "Marked lists explain marks once and advertise existing cancellation"), Trait("Boundary", "Output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task MarkedLineExplanationAndCancel()
    {
        var script = ScriptedCliTerminal.Lines(["cancel"]);
        var question = new CliMarkedListQuestion<string>("Choose folders", [new("+", "add"), new("-", "leave out")],
            [new("one", "One", "First folder", 0, null) { Description = Description }, new("two", "Two", "Second folder", 1, null)], static (_, _) => null)
        {
            LineExplanations = ["Existing files stay and remain routable."],
        };
        var reply = await new CliPrompts(script.Terminal).MarkedListAsync(question, new(true), CancellationToken.None);
        Assert.Equal(CliPromptState.Cancelled, reply.State);
        Assert.Equal(2, script.Output.ToString().Split("Existing files stay", StringSplitOptions.None).Length);
        CommandOutputSnapshot.MatchSnapshot(script.Output.ToString(), "marked-line");
    }
}
