using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Prompts;

public sealed class CliSelectionTextPromptsTests
{
    [Theory(DisplayName = "Selection text wraps whole words and only splits words wider than a row")]
    [InlineData("one two three", 7, "one two|three")]
    [InlineData("keep  both spaces", 10, "keep  both|spaces")]
    [InlineData("first abcdefghijkl last", 6, "first|abcdef|ghijkl|last")]
    [InlineData("abcdefghi x", 6, "abcdef|ghi x")]
    [InlineData("ab 😀 cd", 4, "ab|😀|cd")]
    [InlineData("one\ttwo three", 10, @"one\ttwo|three")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void WordWrapping(string text, int width, string expected)
        => Assert.Equal(expected.Split('|'), CliSelectionText.Wrap(text, width));

    [Theory(DisplayName = "Legend and control wrapping keeps complete items without edge spaces")]
    [InlineData("+ add   ~ add, keep contents out of Git   - leave out", 79, "+ add   ~ add, keep contents out of Git   - leave out")]
    [InlineData("+ add   ~ add, keep contents out of Git   - leave out", 39, "+ add   ~ add, keep contents out of Git|- leave out")]
    [InlineData("up/down move   + ~ - set   space next   enter done   esc cancel", 39, "up/down move   + ~ - set   space next|enter done   esc cancel")]
    [InlineData("up/down move   space toggle   a all   n none   enter next   esc cancel", 39, "up/down move   space toggle   a all|n none   enter next   esc cancel")]
    [InlineData("first item longer than a row   last", 15, "first item|longer than a|row|last")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ItemWrapping(string text, int width, string expected)
    {
        var lines = CliSelectionText.WrapItems(text, width);
        Assert.Equal(expected.Split('|'), lines);
        Assert.All(lines, line => Assert.Equal(line.Trim(), line));
    }

    [Theory(DisplayName = "Single selection aligns unmarked rows and advertises only available digit choices")]
    [InlineData(3, 80), InlineData(9, 80), InlineData(10, 80)]
    [InlineData(3, 40), InlineData(9, 40), InlineData(10, 40)]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task SingleSelectionControls(int count, int width)
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Enter)]);
        script.Viewport = new(width, width == 80 ? 24 : 12);
        var choices = Enumerable.Range(1, count).Select(index => new CliChoice<int>(index, $"item-{index}")).ToArray();
        await new CliPrompts(script.Terminal).SelectAsync(new CliSelectQuestion<int>("Choose an item", choices), new(true), CancellationToken.None);
        var frame = Assert.Single(script.Frames);
        var twoDigitNumbers = count > 9 && width == 80;
        Assert.Contains(twoDigitNumbers ? ">  1. item-1" : "> 1. item-1", frame, StringComparison.Ordinal);
        Assert.Contains(twoDigitNumbers ? "   2. item-2" : "  2. item-2", frame, StringComparison.Ordinal);
        var controls = count <= 9 ? $"up/down move   1-{count} choose   enter choose   esc cancel" : "up/down move   enter choose   esc cancel";
        var expected = width == 80 ? controls : count > 9
            ? $"up/down move   enter choose{Environment.NewLine}esc cancel"
            : $"up/down move   1-{count} choose{Environment.NewLine}enter choose   esc cancel";
        Assert.EndsWith(expected + Environment.NewLine, frame, StringComparison.Ordinal);
    }
}
