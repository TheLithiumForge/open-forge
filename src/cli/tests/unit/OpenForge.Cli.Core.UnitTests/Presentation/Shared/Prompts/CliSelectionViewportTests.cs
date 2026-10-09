using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Prompts.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Prompts;

public sealed class CliSelectionViewportTests
{
    [Theory(DisplayName = "Single selection scrolls the full inventory inside the current viewport"), Trait("Boundary", "Output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    [InlineData(80, 24)]
    [InlineData(40, 12)]
    public async Task SingleInventoryRemainsNavigable(int width, int height)
    {
        var keys = Enumerable.Repeat(new CliKeyStroke(CliKey.Down), 19).Append(new(CliKey.Enter));
        var script = ScriptedCliTerminal.Keys(keys.Select(key => (CliKeyStroke?)key));
        script.Viewport = new(width, height);
        var choices = Choices(20);

        var reply = await new CliPrompts(script.Terminal).SelectAsync(
            new CliSelectQuestion<string>("Which package?", choices), new(true), CancellationToken.None);

        Assert.Equal(choices[19].Value, reply.Value);
        Assert.Equal(20, script.ClearCalls);
        Assert.All(script.Frames, frame => AssertBounded(frame, width, height));
        Assert.Contains("Choice 20/20", script.Frames[^1], StringComparison.Ordinal);
        Assert.Contains("> 20. package-20", script.Frames[^1], StringComparison.Ordinal);
        Assert.DoesNotContain("Details for package-01", script.Frames[^1], StringComparison.Ordinal);
        Assert.All(choices, choice => Assert.Contains(script.Frames, frame => frame.Contains(choice.Label, StringComparison.Ordinal)));
    }

    [Theory(DisplayName = "Multi-selection keeps focus dependency marks and controls in bounded frames"), Trait("Boundary", "Output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    [InlineData(80, 24)]
    [InlineData(40, 12)]
    public async Task MultiSelectionKeepsDependencyState(int width, int height)
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Space), new(CliKey.Down), new(CliKey.Space), new(CliKey.Enter)]);
        script.Viewport = new(width, height);
        var question = new CliMultiSelectQuestion<string>("Which packages?", Choices(25),
            [new("package-01", "package-02"), new("package-02", "package-25")], new HashSet<string>(), CliSelectionAction.Install);

        var reply = await new CliPrompts(script.Terminal).MultiSelectAsync(question, new(true), CancellationToken.None);

        Assert.Equal(["package-01"], reply.Value.Chosen);
        Assert.Equal(["package-02", "package-25"], reply.Value.Required);
        Assert.All(script.Frames, frame => AssertBounded(frame, width, height));
        Assert.Contains("> [*] package-02", script.Frames[^1], StringComparison.Ordinal);
        Assert.Contains("required by package-01", script.Frames[2], StringComparison.Ordinal);
        Assert.Contains("unchoose package-01 first.", script.Frames[^1].Replace(Environment.NewLine, " ", StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Contains("up/down move   space toggle   a all", script.Frames[^1], StringComparison.Ordinal);
        Assert.Contains("n none   enter next   esc cancel", script.Frames[^1], StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Resize refreshes the frame while preserving choice dependency and notice state"), Trait("Boundary", "Output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task ResizePreservesMultiState()
    {
        var script = ScriptedCliTerminal.Keys([
            new(CliKey.Space), new(CliKey.Down), new(CliKey.Space), new(CliKey.Resize), new(CliKey.Resize), new(CliKey.Enter),
        ]);
        script.BeforeKeyRead = call =>
        {
            if (call == 4) script.Viewport = new(40, 12);
            if (call == 5) script.Viewport = new(80, 24);
        };
        var question = new CliMultiSelectQuestion<string>("Which packages?", Choices(25), [new("package-01", "package-02")], new HashSet<string>(), CliSelectionAction.Install);

        var reply = await new CliPrompts(script.Terminal).MultiSelectAsync(question, new(true), CancellationToken.None);

        Assert.Equal(["package-01"], reply.Value.Chosen);
        Assert.Equal(["package-02"], reply.Value.Required);
        AssertBounded(script.Frames[4], 40, 12);
        AssertBounded(script.Frames[5], 80, 24);
        Assert.True(script.Frames[5].Split('\n').Length > script.Frames[4].Split('\n').Length);
        Assert.Contains("> [*] package-02", script.Frames[4], StringComparison.Ordinal);
        Assert.Contains("unchoose package-01 first.", script.Frames[4].Replace(Environment.NewLine, " ", StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Contains("unchoose package-01 first.", script.Frames[5].Replace(Environment.NewLine, " ", StringComparison.Ordinal), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Permission pages retain owner choices and every complete path including long suffixes"), Trait("Boundary", "Output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task PermissionPathsRemainCompletelyReviewable()
    {
        var longPath = "docs/" + new string('p', 75) + "/distinct-file.md";
        var paths = new[] { new CliPermissionPath(longPath, false) }
            .Concat(Enumerable.Range(1, 10).Select(index => new CliPermissionPath($"docs/file-{index:00}.md", false))).ToArray();
        var keys = Enumerable.Repeat(new CliKeyStroke(CliKey.PageDown), 20)
            .Concat([new(CliKey.PageUp), new(CliKey.PageDown), new(CliKey.Down), new(CliKey.Enter)]);
        var script = ScriptedCliTerminal.Keys(keys.Select(key => (CliKeyStroke?)key));
        script.Viewport = new(40, 12);

        var reply = await new CliPrompts(script.Terminal).PermissionAsync(new("toolkit", paths), new(true), CancellationToken.None);

        Assert.Equal(CliPermissionChoice.Once, reply.Value);
        Assert.All(script.Frames, frame =>
        {
            AssertBounded(frame, 40, 12);
            Assert.Contains("Allow changes outside .agents?", frame, StringComparison.Ordinal);
            Assert.Contains("pgup/pgdn review", frame, StringComparison.Ordinal);
            Assert.Contains("Allow", frame, StringComparison.Ordinal);
        });
        var reviewed = string.Concat(script.Frames.SelectMany(frame => frame.Split(Environment.NewLine)
            .Skip(3).TakeWhile(line => !line.StartsWith("Paths ", StringComparison.Ordinal))));
        Assert.Contains(string.Concat(paths.Select(path => path.Path)), reviewed, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Long permission owners remain identifiable and fully reviewable at the smallest supported views"), Trait("Boundary", "Output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    [InlineData(36, 10)]
    [InlineData(40, 12)]
    public async Task LongPermissionOwnerKeepsItsSuffixAndPaths(int width, int height)
    {
        var owner = "catalogue/" + new string('o', 80) + "/distinct-owner";
        var paths = new[] { new CliPermissionPath("docs/first.md", false), new CliPermissionPath("docs/" + new string('p', 80) + "/last.md", false) };
        var keys = Enumerable.Repeat(new CliKeyStroke(CliKey.PageDown), 20).Append(new(CliKey.Enter));
        var script = ScriptedCliTerminal.Keys(keys.Select(key => (CliKeyStroke?)key));
        script.Viewport = new(width, height);

        var reply = await new CliPrompts(script.Terminal).PermissionAsync(new(owner, paths), new(true), CancellationToken.None);

        Assert.Equal(CliPermissionChoice.Always, reply.Value);
        Assert.All(script.Frames, frame =>
        {
            AssertBounded(frame, width, height);
            Assert.Contains("/distinct-owner", frame, StringComparison.Ordinal);
            Assert.Contains("docs/first.md", frame, StringComparison.Ordinal);
            Assert.Contains("enter choose   esc cancel", frame, StringComparison.Ordinal);
        });
        var reviewed = string.Concat(script.Frames.SelectMany(frame => frame.Split(Environment.NewLine)
            .Skip(3).TakeWhile(line => !line.StartsWith("Paths ", StringComparison.Ordinal))));
        Assert.Contains(owner + string.Concat(paths.Select(path => path.Path)), reviewed, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "An overfilled selection frame fails before clearing or emitting a scrolling view"), Trait("Boundary", "Output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public void MandatoryRowsMustFitBeforeRendering()
    {
        var frame = new CliSelectionFrame
        {
            Question = "Question?",
            Rows = [new("first")],
            Focus = 0,
            Position = "Choice 1/1",
            Context = Enumerable.Repeat("Path context", 8).ToArray(),
            Notice = "Choose a package.",
            Controls = ["enter choose", "esc cancel"],
        };

        Assert.Null(CliSelectionFrameRenderer.Render(frame, new(40, 12), CliTextStyle.Plain));
    }

    [Theory(DisplayName = "Unavailable geometry or clearing uses numbered input without key reads"), Trait("Boundary", "Input"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    [InlineData("geometry")]
    [InlineData("tiny")]
    [InlineData("clearing")]
    public async Task UnavailableViewFallsBackToLines(string unavailable)
    {
        var script = new ScriptedCliTerminal(new(true, true, true), lines: ["2"]);
        if (unavailable == "geometry") script.Viewport = null;
        if (unavailable == "tiny") script.Viewport = new(20, 8);
        if (unavailable == "clearing") script.CanClear = false;

        var reply = await new CliPrompts(script.Terminal).SelectAsync(new CliSelectQuestion<string>("Choose?", Choices(3)), new(true), CancellationToken.None);

        Assert.Equal("package-02", reply.Value);
        Assert.Equal(0, script.KeyReadCalls);
        Assert.Equal(1, script.LineReadCalls);
        Assert.Empty(script.Frames);
        Assert.DoesNotContain('\u001b', script.Output.ToString());
    }

    [Fact(DisplayName = "Confirmation ignores resize without clearing its plan or repeating the question"), Trait("Boundary", "Output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task ConfirmationResizeLeavesReportVisible()
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Resize), new(CliKey.Resize), new(CliKey.Character, 'y')]);
        await script.Terminal.WriteAsync("Existing plan\n".AsMemory(), CancellationToken.None);

        var reply = await new CliPrompts(script.Terminal).ConfirmAsync(new("Apply? [y/N]"), new(true), CancellationToken.None);

        Assert.True(reply.Value);
        Assert.Equal(0, script.ClearCalls);
        Assert.Equal("Existing plan\nApply? [y/N]" + Environment.NewLine, script.Output.ToString());
    }

    private static CliChoice<string>[] Choices(int count)
        => Enumerable.Range(1, count).Select(index => new CliChoice<string>($"package-{index:00}", $"package-{index:00}",
            $"Details for package-{index:00}: " + new string('d', 120))).ToArray();

    private static void AssertBounded(string frame, int width, int height)
    {
        var rows = frame.Split(Environment.NewLine);
        Assert.Equal(string.Empty, rows[^1]);
        Assert.InRange(rows.Length - 1, 1, height - 1);
        Assert.All(rows, row => Assert.True(row.Length < width, row));
    }
}
